using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEcommerce.DTOs;
using ProjetoEcommerce.Modelos;

namespace ProjetoEcommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PedidosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<PedidoResponseDTO>> CriarPedido([FromBody] CriarPedidoDTO request)
        {
            try
            {
                var carrinho = await _context.Carrinhos
                    .Include(c => c.Itens)
                    .ThenInclude(i => i.Produto)
                    .FirstOrDefaultAsync(c => c.ClienteId == request.ClienteId);

                if (carrinho == null || !carrinho.Itens.Any())
                {
                    return BadRequest("Carrinho vazio");
                }

                // Verificar estoque
                foreach (var item in carrinho.Itens)
                {
                    var estoque = await _context.Estoques.FirstOrDefaultAsync(e => e.ProdutoId == item.ProdutoId);
                    if (estoque == null || estoque.QuantidadeDisponivel < item.Quantidade)
                    {
                        return BadRequest($"Produto {item.Produto.Nome} sem estoque suficiente");
                    }
                }

                // Criar pedido
                var pedido = new Pedido
                {
                    ClienteId = request.ClienteId,
                    LojaId = 1, // Por enquanto, loja padrão
                    EnderecoEntregaId = request.EnderecoEntregaId,
                    ValorTotal = carrinho.Itens.Sum(i => i.Quantidade * i.PrecoUnitario),
                    Status = "Pendente",
                    DataCriacao = DateTime.Now
                };

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                // Adicionar itens do pedido e atualizar estoque
                foreach (var item in carrinho.Itens)
                {
                    var itemPedido = new ItemPedido
                    {
                        PedidoId = pedido.Id,
                        ProdutoId = item.ProdutoId,
                        Quantidade = item.Quantidade,
                        PrecoUnitario = item.PrecoUnitario
                    };
                    _context.ItensPedido.Add(itemPedido);

                    // Atualizar estoque
                    var estoque = await _context.Estoques.FirstOrDefaultAsync(e => e.ProdutoId == item.ProdutoId);
                    if (estoque != null)
                    {
                        estoque.QuantidadeDisponivel -= item.Quantidade;
                        estoque.UltimoMovimento = DateTime.Now;
                    }
                }

                // Criar pagamento
                var pagamento = new Pagamento
                {
                    PedidoId = pedido.Id,
                    Tipo = request.MetodoPagamento,
                    Valor = pedido.ValorTotal,
                    Status = "Pendente",
                    Referencia = Guid.NewGuid().ToString()
                };
                _context.Pagamentos.Add(pagamento);

                // Criar envio
                var envio = new Envio
                {
                    PedidoId = pedido.Id,
                    Tipo = "Standard",
                    Valor = 0, // Frete grátis por enquanto
                    Status = "Aguardando",
                    TempoEstimadoDias = 7
                };
                _context.Envios.Add(envio);

                // Limpar carrinho
                _context.CarrinhoItens.RemoveRange(carrinho.Itens);
                await _context.SaveChangesAsync();

                // Retornar pedido criado
                return await GetPedido(pedido.Id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PedidoResponseDTO>> GetPedido(int id)
        {
            try
            {
                var pedido = await _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.Loja)
                    .Include(p => p.Itens)
                        .ThenInclude(i => i.Produto)
                    .Include(p => p.Pagamento)
                    .Include(p => p.Envio)
                    .Where(p => p.Id == id)
                    .Select(p => new PedidoResponseDTO
                    {
                        Id = p.Id,
                        ClienteId = p.ClienteId,
                        ClienteNome = p.Cliente.Nome,
                        LojaId = p.LojaId,
                        LojaNome = p.Loja.Nome,
                        ValorTotal = p.ValorTotal,
                        Status = p.Status,
                        DataCriacao = p.DataCriacao,
                        Itens = p.Itens.Select(i => new ItemPedidoResponseDTO
                        {
                            ProdutoId = i.ProdutoId,
                            ProdutoNome = i.Produto.Nome,
                            Quantidade = i.Quantidade,
                            PrecoUnitario = i.PrecoUnitario,
                            Subtotal = i.Quantidade * i.PrecoUnitario
                        }).ToList(),
                        Pagamento = p.Pagamento != null ? new PagamentoResponseDTO
                        {
                            Tipo = p.Pagamento.Tipo,
                            Valor = p.Pagamento.Valor,
                            Status = p.Pagamento.Status
                        } : null,
                        Envio = p.Envio != null ? new EnvioResponseDTO
                        {
                            Tipo = p.Envio.Tipo,
                            Valor = p.Envio.Valor,
                            Status = p.Envio.Status,
                            Rastreamento = p.Envio.Rastreamento
                        } : null
                    })
                    .FirstOrDefaultAsync();

                if (pedido == null)
                {
                    return NotFound();
                }

                return Ok(pedido);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<PedidoResponseDTO>>> GetPedidosPorCliente(int clienteId)
        {
            try
            {
                var pedidos = await _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.Loja)
                    .Include(p => p.Itens)
                    .Include(p => p.Pagamento)
                    .Where(p => p.ClienteId == clienteId)
                    .OrderByDescending(p => p.DataCriacao)
                    .Select(p => new PedidoResponseDTO
                    {
                        Id = p.Id,
                        ClienteId = p.ClienteId,
                        ClienteNome = p.Cliente.Nome,
                        LojaId = p.LojaId,
                        LojaNome = p.Loja.Nome,
                        ValorTotal = p.ValorTotal,
                        Status = p.Status,
                        DataCriacao = p.DataCriacao,
                        Pagamento = p.Pagamento != null ? new PagamentoResponseDTO
                        {
                            Tipo = p.Pagamento.Tipo,
                            Valor = p.Pagamento.Valor,
                            Status = p.Pagamento.Status
                        } : null
                    })
                    .ToListAsync();

                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult> AtualizarStatus(int id, [FromBody] string status)
        {
            try
            {
                var pedido = await _context.Pedidos.FindAsync(id);
                if (pedido == null)
                {
                    return NotFound();
                }

                pedido.Status = status;
                pedido.DataAtualizacao = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Status atualizado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}