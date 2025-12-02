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
        public async Task<ActionResult<IEnumerable<object>>> GetEstoques()
        {
            try
            {
                var estoques = await _context.Estoques
                    .Select(e => new
                    {
                        e.Id,
                        ProdutoFisicoId = e.ProdutoFisicoId,
                        QuantidadeDisponivel = e.QuantidadeDisponivel,
                        QuantidadeReservada = e.QuantidadeReservada,
                        EstoqueReal = e.QuantidadeDisponivel - e.QuantidadeReservada,
                        e.PontoRepor,
                        e.EstoqueMinimo,
                        UltimoMovimento = e.UltimoMovimento != null ? e.UltimoMovimento.Value.ToString("dd/MM/yyyy HH:mm") : "Nunca"
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Message = "Estoques carregados com sucesso",
                    Total = estoques.Count,
                    Data = estoques
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 ERRO GET ESTOQUES: {ex.Message}");
                return StatusCode(500, $"Erro ao buscar estoques: {ex.Message}");
            }
        }

        [HttpGet("produto/{produtoFisicoId}")]
        public async Task<ActionResult<object>> GetEstoquePorProduto(int produtoFisicoId)
        {
            try
            {
                var estoque = await _context.Estoques
                    .Where(e => e.ProdutoFisicoId == produtoFisicoId)
                    .Select(e => new
                    {
                        e.Id,
                        ProdutoFisicoId = e.ProdutoFisicoId,
                        QuantidadeDisponivel = e.QuantidadeDisponivel,
                        QuantidadeReservada = e.QuantidadeReservada,
                        EstoqueReal = e.QuantidadeDisponivel - e.QuantidadeReservada,
                        e.PontoRepor,
                        e.EstoqueMinimo,
                        UltimoMovimento = e.UltimoMovimento != null ? e.UltimoMovimento.Value.ToString("dd/MM/yyyy HH:mm") : "Nunca",
                        Status = e.QuantidadeDisponivel - e.QuantidadeReservada <= e.PontoRepor ? "Atenção" : "Normal"
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
                Console.WriteLine($"🔴 ERRO GET ESTOQUE POR PRODUTO: {ex.Message}");
                return StatusCode(500, $"Erro ao buscar estoque: {ex.Message}");
            }
        }

        [HttpPut("{produtoFisicoId}/ajustar")]
        public async Task<ActionResult> AjustarEstoque(int produtoFisicoId, [FromBody] AjusteEstoqueDTO ajuste)
        {
            try
            {
                var tiposValidos = new[] { "entrada", "saida", "reserva", "liberar" };
                if (!tiposValidos.Contains(ajuste.Tipo?.ToLower()))
                {
                    return BadRequest("Tipo de ajuste inválido. Use: entrada, saida, reserva ou liberar");
                }

                if (ajuste.Quantidade <= 0)
                {
                    return BadRequest("Quantidade deve ser maior que zero");
                }

                var estoque = await _context.Estoques
                    .FirstOrDefaultAsync(e => e.ProdutoFisicoId == produtoFisicoId);

                if (estoque == null)
                {
                    return NotFound("Estoque do produto não encontrado");
                }

                switch (ajuste.Tipo.ToLower())
                {
                    case "entrada":
                        estoque.QuantidadeDisponivel += ajuste.Quantidade;
                        break;
                    case "saida":
                        if (estoque.QuantidadeDisponivel < ajuste.Quantidade)
                            return BadRequest("Estoque insuficiente para saída");
                        estoque.QuantidadeDisponivel -= ajuste.Quantidade;
                        break;
                    case "reserva":
                        if (estoque.QuantidadeDisponivel - estoque.QuantidadeReservada < ajuste.Quantidade)
                            return BadRequest("Estoque disponível insuficiente para reserva");
                        estoque.QuantidadeReservada += ajuste.Quantidade;
                        break;
                    case "liberar":
                        if (estoque.QuantidadeReservada < ajuste.Quantidade)
                            return BadRequest("Quantidade de reserva insuficiente para liberar");
                        estoque.QuantidadeReservada -= ajuste.Quantidade;
                        break;
                }

                estoque.UltimoMovimento = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Estoque ajustado com sucesso",
                    produtoFisicoId = produtoFisicoId,
                    tipoAjuste = ajuste.Tipo,
                    quantidade = ajuste.Quantidade,
                    quantidadeDisponivel = estoque.QuantidadeDisponivel,
                    quantidadeReservada = estoque.QuantidadeReservada,
                    estoqueReal = estoque.QuantidadeDisponivel - estoque.QuantidadeReservada
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 ERRO AJUSTAR ESTOQUE: {ex.Message}");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpPost("{produtoId}/reservar")]
        public async Task<ActionResult> ReservarEstoque(int produtoId, [FromBody] int quantidade)
        {
            try
            {
                var estoque = await _context.Estoques
                    .Include(e => e.ProdutoFisico)
                    .FirstOrDefaultAsync(e => e.ProdutoFisicoId == produtoId);

                if (estoque == null)
                {
                    return NotFound("Estoque do produto não encontrado");
                }

                estoque.Reservar(quantidade);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Estoque reservado com sucesso",
                    produtoId = produtoId,
                    produtoNome = estoque.ProdutoFisico.Nome,
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
                    .Include(e => e.ProdutoFisico)
                    .ThenInclude(p => p.Loja)
                    .Where(e => e.PrecisaRepor())
                    .Select(e => new EstoqueAlertaDTO
                    {
                        ProdutoId = e.ProdutoFisicoId,
                        ProdutoNome = e.ProdutoFisico.Nome,
                        LojaNome = e.ProdutoFisico.Loja.Nome,
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
                var produtoFisico = await _context.ProdutosFisicos.FindAsync(estoqueDto.ProdutoFisicoId);
                if (produtoFisico == null)
                {
                    return BadRequest("Produto físico não encontrado");
                }

                if (await _context.Estoques.AnyAsync(e => e.ProdutoFisicoId == estoqueDto.ProdutoFisicoId))
                {
                    return BadRequest("Já existe estoque cadastrado para este produto físico");
                }

                var estoque = new Estoque
                {
                    ProdutoFisicoId = estoqueDto.ProdutoFisicoId,
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
                    ProdutoId = estoque.ProdutoFisicoId,
                    ProdutoNome = produtoFisico.Nome,
                    QuantidadeDisponivel = estoque.QuantidadeDisponivel,
                    QuantidadeReservada = estoque.QuantidadeReservada,
                    EstoqueReal = estoque.EstoqueReal(),
                    PontoRepor = estoque.PontoRepor,
                    EstoqueMinimo = estoque.EstoqueMinimo,
                    UltimoMovimento = estoque.UltimoMovimento,
                    Status = estoque.StatusEstoque(),
                    PrecisaRepor = estoque.PrecisaRepor()
                };

                return CreatedAtAction(nameof(GetEstoquePorProduto), new { produtoFisicoId = estoque.ProdutoFisicoId }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}