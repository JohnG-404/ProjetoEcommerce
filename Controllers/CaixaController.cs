using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEcommerce.DTOs;
using ProjetoEcommerce.Modelos;

namespace ProjetoEcommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CaixaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CaixaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("loja/{lojaId}")]
        public async Task<ActionResult<object>> GetCaixaLoja(int lojaId)
        {
            try
            {
                var caixa = await _context.Caixas
                    .Include(c => c.Loja)
                    .Include(c => c.Transacoes)
                    .FirstOrDefaultAsync(c => c.LojaId == lojaId);

                if (caixa == null)
                {
                    return NotFound("Caixa da loja não encontrado");
                }

                var response = new
                {
                    Loja = caixa.Loja.Nome,
                    SaldoAtual = caixa.SaldoAtual,
                    TotalTransacoes = caixa.Transacoes.Count,
                    UltimasTransacoes = caixa.Transacoes
                        .OrderByDescending(t => t.Data)
                        .Take(10)
                        .Select(t => new
                        {
                            t.Tipo,
                            t.Valor,
                            t.Descricao,
                            t.Data
                        })
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}