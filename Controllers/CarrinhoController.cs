using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEcommerce.DTOs;
using ProjetoEcommerce.Modelos;

namespace ProjetoEcommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarrinhoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CarrinhoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{clienteId}")]
        public async Task<ActionResult<CarrinhoResponseDTO>> GetCarrinho(int clienteId)
        {
            try
            {
                var carrinho = await _context.Carrinhos
                    .Include(c => c.Itens)
                        .ThenInclude(ci => ci.ProdutoFisico) // 🔥 ATUALIZADO
                    .Include(c => c.Itens)
                        .ThenInclude(ci => ci.ProdutoDigital) // 🔥 ATUALIZADO
                    .Include(c => c.Cliente)
                    .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

                if (carrinho == null)
                {
                    carrinho = new Carrinho { ClienteId = clienteId, DataCriacao = DateTime.Now };
                    _context.Carrinhos.Add(carrinho);
                    await _context.SaveChangesAsync();
                }

                var response = new CarrinhoResponseDTO
                {
                    Id = carrinho.Id,
                    ClienteId = carrinho.ClienteId,
                    ClienteNome = carrinho.Cliente?.Nome,
                    DataCriacao = carrinho.DataCriacao,
                    AtualizadoEm = carrinho.AtualizadoEm,
                    Itens = carrinho.Itens.Select(ci => new CarrinhoItemResponseDTO
                    {
                        Id = ci.Id,
                        // 🔥 ATUALIZADO - Verificar tipo de produto
                        ProdutoId = ci.ProdutoFisicoId ?? ci.ProdutoDigitalId ?? 0,
                        ProdutoNome = ci.ObterProduto()?.Nome, // 🔥 USAR MÉTODO
                        TipoProduto = ci.ObterTipoProduto(), // 🔥 ADICIONADO
                        Quantidade = ci.Quantidade,
                        PrecoUnitario = ci.PrecoUnitario,
                        Subtotal = ci.CalcularSubtotal()
                    }).ToList(),
                    Total = carrinho.Itens.Sum(i => i.CalcularSubtotal())
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpPost("adicionar-item")]
        public async Task<ActionResult> AdicionarItem([FromBody] AdicionarItemRequestDTO request)
        {
            try
            {
                var carrinho = await _context.Carrinhos
                    .FirstOrDefaultAsync(c => c.ClienteId == request.ClienteId);

                if (carrinho == null)
                {
                    carrinho = new Carrinho { ClienteId = request.ClienteId, DataCriacao = DateTime.Now };
                    _context.Carrinhos.Add(carrinho);
                    await _context.SaveChangesAsync();
                }

                // 🔥 ATUALIZADO - Verificar se é produto físico ou digital
                ProdutoBase produto = null;
                decimal preco = 0;

                if (request.TipoProduto == "Fisico")
                {
                    produto = await _context.ProdutosFisicos // 🔥 ATUALIZADO
                        .Include(p => p.Estoque)
                        .FirstOrDefaultAsync(p => p.Id == request.ProdutoId);
                }
                else if (request.TipoProduto == "Digital")
                {
                    produto = await _context.ProdutosDigitais // 🔥 ATUALIZADO
                        .FirstOrDefaultAsync(p => p.Id == request.ProdutoId);
                }

                if (produto == null)
                {
                    return NotFound("Produto não encontrado");
                }

                preco = produto.Preco;

                // Verificar estoque apenas para produtos físicos
                if (produto is ProdutoFisico produtoFisico)
                {
                    if (produtoFisico.Estoque == null || produtoFisico.Estoque.QuantidadeDisponivel < request.Quantidade)
                    {
                        return BadRequest("Quantidade indisponível em estoque");
                    }
                }

                // 🔥 ATUALIZADO - Buscar item existente considerando tipo
                CarrinhoItem itemExistente = null;

                if (request.TipoProduto == "Fisico")
                {
                    itemExistente = await _context.CarrinhoItens
                        .FirstOrDefaultAsync(ci => ci.CarrinhoId == carrinho.Id && ci.ProdutoFisicoId == request.ProdutoId);
                }
                else if (request.TipoProduto == "Digital")
                {
                    itemExistente = await _context.CarrinhoItens
                        .FirstOrDefaultAsync(ci => ci.CarrinhoId == carrinho.Id && ci.ProdutoDigitalId == request.ProdutoId);
                }

                if (itemExistente != null)
                {
                    itemExistente.Quantidade += request.Quantidade;
                }
                else
                {
                    // 🔥 ATUALIZADO - Criar item com tipo correto
                    var novoItem = new CarrinhoItem
                    {
                        CarrinhoId = carrinho.Id,
                        ProdutoFisicoId = request.TipoProduto == "Fisico" ? request.ProdutoId : null,
                        ProdutoDigitalId = request.TipoProduto == "Digital" ? request.ProdutoId : null,
                        Quantidade = request.Quantidade,
                        PrecoUnitario = preco
                    };
                    _context.CarrinhoItens.Add(novoItem);
                }

                carrinho.AtualizadoEm = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Item adicionado ao carrinho" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpDelete("remover-item/{itemId}")]
        public async Task<ActionResult> RemoverItem(int itemId)
        {
            try
            {
                var item = await _context.CarrinhoItens.FindAsync(itemId);
                if (item == null)
                {
                    return NotFound("Item não encontrado");
                }

                _context.CarrinhoItens.Remove(item);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Item removido do carrinho" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpPut("atualizar-quantidade/{itemId}")]
        public async Task<ActionResult> AtualizarQuantidade(int itemId, [FromBody] int quantidade)
        {
            try
            {
                if (quantidade <= 0)
                {
                    return BadRequest("Quantidade deve ser maior que zero");
                }

                var item = await _context.CarrinhoItens
                    .Include(i => i.ProdutoFisico)
                    .ThenInclude(p => p.Estoque)
                    .Include(i => i.ProdutoDigital)
                    .FirstOrDefaultAsync(i => i.Id == itemId);

                if (item == null)
                {
                    return NotFound("Item não encontrado");
                }

                // Verificar estoque apenas para produtos físicos
                if (item.ProdutoFisico != null && item.ProdutoFisico.Estoque?.QuantidadeDisponivel < quantidade)
                {
                    return BadRequest("Quantidade indisponível em estoque");
                }

                item.Quantidade = quantidade;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Quantidade atualizada" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }

    // 🔥 ATUALIZADO - DTO com tipo de produto
    public class AdicionarItemRequestDTO
    {
        public int ClienteId { get; set; }
        public int ProdutoId { get; set; }
        public string TipoProduto { get; set; } // "Fisico" ou "Digital"
        public int Quantidade { get; set; }
    }

    public class CarrinhoItemResponseDTO
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; }
        public string TipoProduto { get; set; } // 🔥 ADICIONADO
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class CarrinhoResponseDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? AtualizadoEm { get; set; }
        public List<CarrinhoItemResponseDTO> Itens { get; set; } = new();
        public decimal Total { get; set; }
    }
}