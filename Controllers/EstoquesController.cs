using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEcommerce.DTOs;
using ProjetoEcommerce.Modelos;

namespace ProjetoEcommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstoquesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EstoquesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstoqueResponseDTO>>> GetEstoques()
        {
            try
            {
                var estoques = await _context.Estoques
                    .Include(e => e.Produto)
                    .ThenInclude(p => p.Loja)
                    .Select(e => new EstoqueResponseDTO
                    {
                        Id = e.Id,
                        ProdutoId = e.ProdutoId,
                        ProdutoNome = e.Produto.Nome,
                        LojaNome = e.Produto.Loja.Nome,
                        QuantidadeDisponivel = e.QuantidadeDisponivel,
                        QuantidadeReservada = e.QuantidadeReservada,
                        EstoqueReal = e.EstoqueReal(), // USANDO MÉTODO COM ENCAPSULAMENTO
                        PontoRepor = e.PontoRepor,
                        EstoqueMinimo = e.EstoqueMinimo,
                        UltimoMovimento = e.UltimoMovimento,
                        Status = e.StatusEstoque(), // USANDO MÉTODO COM POLIMORFISMO
                        PrecisaRepor = e.PrecisaRepor() // USANDO MÉTODO COM POLIMORFISMO
                    })
                    .ToListAsync();

                return Ok(estoques);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar estoques: {ex.Message}");
            }
        }

        [HttpGet("produto/{produtoId}")]
        public async Task<ActionResult<EstoqueResponseDTO>> GetEstoquePorProduto(int produtoId)
        {
            try
            {
                var estoque = await _context.Estoques
                    .Include(e => e.Produto)
                    .ThenInclude(p => p.Loja)
                    .Where(e => e.ProdutoId == produtoId)
                    .Select(e => new EstoqueResponseDTO
                    {
                        Id = e.Id,
                        ProdutoId = e.ProdutoId,
                        ProdutoNome = e.Produto.Nome,
                        LojaNome = e.Produto.Loja.Nome,
                        QuantidadeDisponivel = e.QuantidadeDisponivel,
                        QuantidadeReservada = e.QuantidadeReservada,
                        EstoqueReal = e.EstoqueReal(),
                        PontoRepor = e.PontoRepor,
                        EstoqueMinimo = e.EstoqueMinimo,
                        UltimoMovimento = e.UltimoMovimento,
                        Status = e.StatusEstoque(true), // SOBRECARGA - com quantidade
                        PrecisaRepor = e.PrecisaRepor()
                    })
                    .FirstOrDefaultAsync();

                if (estoque == null)
                {
                    return NotFound("Estoque do produto não encontrado");
                }

                return Ok(estoque);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar estoque: {ex.Message}");
            }
        }

        [HttpPut("{produtoId}/ajustar")]
        public async Task<ActionResult> AjustarEstoque(int produtoId, [FromBody] AjusteEstoqueDTO ajuste)
        {
            try
            {
                var estoque = await _context.Estoques
                    .Include(e => e.Produto)
                    .FirstOrDefaultAsync(e => e.ProdutoId == produtoId);

                if (estoque == null)
                {
                    return NotFound("Estoque do produto não encontrado");
                }

                // ENCAPSULAMENTO - Usando métodos da classe Estoque
                if (ajuste.Tipo == "entrada")
                {
                    estoque.AdicionarEstoque(ajuste.Quantidade);
                }
                else if (ajuste.Tipo == "saida")
                {
                    estoque.BaixarEstoque(ajuste.Quantidade);
                }
                else if (ajuste.Tipo == "reserva")
                {
                    estoque.Reservar(ajuste.Quantidade);
                }
                else if (ajuste.Tipo == "liberar")
                {
                    estoque.LiberarReserva(ajuste.Quantidade);
                }
                else
                {
                    return BadRequest("Tipo de ajuste inválido. Use: entrada, saida, reserva ou liberar");
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Estoque ajustado com sucesso",
                    produtoId = produtoId,
                    produtoNome = estoque.Produto.Nome,
                    quantidadeDisponivel = estoque.QuantidadeDisponivel,
                    quantidadeReservada = estoque.QuantidadeReservada,
                    estoqueReal = estoque.EstoqueReal(),
                    status = estoque.StatusEstoque()
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpPost("{produtoId}/reservar")]
        public async Task<ActionResult> ReservarEstoque(int produtoId, [FromBody] int quantidade)
        {
            try
            {
                var estoque = await _context.Estoques
                    .Include(e => e.Produto)
                    .FirstOrDefaultAsync(e => e.ProdutoId == produtoId);

                if (estoque == null)
                {
                    return NotFound("Estoque do produto não encontrado");
                }

                // ENCAPSULAMENTO - Usando método Reservar com validação
                estoque.Reservar(quantidade);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Estoque reservado com sucesso",
                    produtoId = produtoId,
                    produtoNome = estoque.Produto.Nome,
                    quantidadeReservada = quantidade,
                    estoqueReal = estoque.EstoqueReal()
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpGet("alerta-reposicao")]
        public async Task<ActionResult<IEnumerable<EstoqueAlertaDTO>>> GetEstoquesParaRepor()
        {
            try
            {
                var estoques = await _context.Estoques
                    .Include(e => e.Produto)
                    .ThenInclude(p => p.Loja)
                    .Where(e => e.PrecisaRepor()) // POLIMORFISMO - Usando método da classe
                    .Select(e => new EstoqueAlertaDTO
                    {
                        ProdutoId = e.ProdutoId,
                        ProdutoNome = e.Produto.Nome,
                        LojaNome = e.Produto.Loja.Nome,
                        QuantidadeDisponivel = e.QuantidadeDisponivel,
                        QuantidadeReservada = e.QuantidadeReservada,
                        EstoqueReal = e.EstoqueReal(),
                        PontoRepor = e.PontoRepor,
                        EstoqueMinimo = e.EstoqueMinimo,
                        Status = e.StatusEstoque(),
                        UltimoMovimento = e.UltimoMovimento
                    })
                    .ToListAsync();

                return Ok(estoques);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar estoques para reposição: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<EstoqueResponseDTO>> CriarEstoque([FromBody] EstoqueDTO estoqueDto)
        {
            try
            {
                // Verificar se produto existe
                var produto = await _context.Produtos.FindAsync(estoqueDto.ProdutoId);
                if (produto == null)
                {
                    return BadRequest("Produto não encontrado");
                }

                // Verificar se já existe estoque para este produto
                if (await _context.Estoques.AnyAsync(e => e.ProdutoId == estoqueDto.ProdutoId))
                {
                    return BadRequest("Já existe estoque cadastrado para este produto");
                }

                var estoque = new Estoque
                {
                    ProdutoId = estoqueDto.ProdutoId,
                    QuantidadeDisponivel = estoqueDto.QuantidadeDisponivel,
                    QuantidadeReservada = 0,
                    PontoRepor = estoqueDto.PontoRepor,
                    EstoqueMinimo = estoqueDto.EstoqueMinimo,
                    UltimoMovimento = DateTime.Now
                };

                _context.Estoques.Add(estoque);
                await _context.SaveChangesAsync();

                var response = new EstoqueResponseDTO
                {
                    Id = estoque.Id,
                    ProdutoId = estoque.ProdutoId,
                    ProdutoNome = produto.Nome,
                    QuantidadeDisponivel = estoque.QuantidadeDisponivel,
                    QuantidadeReservada = estoque.QuantidadeReservada,
                    EstoqueReal = estoque.EstoqueReal(),
                    PontoRepor = estoque.PontoRepor,
                    EstoqueMinimo = estoque.EstoqueMinimo,
                    UltimoMovimento = estoque.UltimoMovimento,
                    Status = estoque.StatusEstoque(),
                    PrecisaRepor = estoque.PrecisaRepor()
                };

                return CreatedAtAction(nameof(GetEstoquePorProduto), new { produtoId = estoque.ProdutoId }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}