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
                    .Where(c => c.LojaId == lojaId)
                    .Select(c => new
                    {
                        c.Id,
                        c.LojaId,
                        c.SaldoAtual,
                        c.Status,
                        DataAbertura = c.DataAbertura.ToString("dd/MM/yyyy HH:mm"),
                        DataFechamento = c.DataFechamento != null ? c.DataFechamento.Value.ToString("dd/MM/yyyy HH:mm") : "Aberto",
                        c.SaldoInicial
                    })
                    .FirstOrDefaultAsync();

                if (caixa == null)
                {
                    return NotFound("Caixa da loja não encontrado");
                }

                // 🔥 CONSULTA SEGURA para transações
                var transacoes = await _context.Transacoes
                    .Where(t => t.CaixaId == caixa.Id)
                    .OrderByDescending(t => t.Data)
                    .Take(10)
                    .Select(t => new
                    {
                        t.Tipo,
                        t.Valor,
                        Descricao = t.Descricao ?? string.Empty,
                        Data = t.Data.ToString("dd/MM/yyyy HH:mm"),
                        MetodoPagamento = t.MetodoPagamento ?? string.Empty,
                        Observacao = t.Observacao ?? string.Empty
                    })
                    .ToListAsync();

                
                var loja = await _context.Lojas
                    .Where(l => l.Id == lojaId)
                    .Select(l => new { l.Nome })
                    .FirstOrDefaultAsync();

                var response = new
                {
                    Loja = loja != null ? loja.Nome : "Loja não encontrada",
                    SaldoAtual = caixa.SaldoAtual,
                    TotalTransacoes = transacoes.Count,
                    UltimasTransacoes = transacoes
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 ERRO GET CAIXA: {ex.Message}");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

       
        [HttpPost("loja/{lojaId}/abrir")]
        public async Task<ActionResult<object>> AbrirCaixa(int lojaId, [FromBody] decimal saldoInicial = 0)
        {
            try
            {
                
                var caixaExistente = await _context.Caixas
                    .FirstOrDefaultAsync(c => c.LojaId == lojaId && c.Status == "Aberto");

                if (caixaExistente != null)
                {
                    return BadRequest("Já existe um caixa aberto para esta loja");
                }

                var caixa = new Caixa(lojaId, saldoInicial)
                {
                    Status = "Aberto",
                    DataAbertura = DateTime.Now
                };

                _context.Caixas.Add(caixa);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Caixa aberto com sucesso",
                    CaixaId = caixa.Id,
                    LojaId = caixa.LojaId,
                    SaldoInicial = caixa.SaldoInicial,
                    DataAbertura = caixa.DataAbertura.ToString("dd/MM/yyyy HH:mm")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 ERRO ABRIR CAIXA: {ex.Message}");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        
        [HttpPost("loja/{lojaId}/fechar")]
        public async Task<ActionResult<object>> FecharCaixa(int lojaId)
        {
            try
            {
                var caixa = await _context.Caixas
                    .FirstOrDefaultAsync(c => c.LojaId == lojaId && c.Status == "Aberto");

                if (caixa == null)
                {
                    return NotFound("Caixa aberto não encontrado para esta loja");
                }

                caixa.FecharCaixa();
                await _context.SaveChangesAsync();

                
                var transacoesDia = await _context.Transacoes
                    .Where(t => t.CaixaId == caixa.Id && t.Data.Date == DateTime.Today)
                    .Select(t => new
                    {
                        t.Tipo,
                        t.Valor,
                        Descricao = t.Descricao ?? string.Empty,
                        Data = t.Data.ToString("dd/MM/yyyy HH:mm")
                    })
                    .ToListAsync();

                var totalEntradas = transacoesDia
                    .Where(t => t.Tipo == "Entrada")
                    .Sum(t => t.Valor);

                var totalSaidas = transacoesDia
                    .Where(t => t.Tipo == "Saida")
                    .Sum(t => t.Valor);

                return Ok(new
                {
                    Message = "Caixa fechado com sucesso",
                    CaixaId = caixa.Id,
                    DataFechamento = caixa.DataFechamento?.ToString("dd/MM/yyyy HH:mm"),
                    SaldoFinal = caixa.SaldoAtual,
                    TotalEntradas = totalEntradas,
                    TotalSaidas = totalSaidas,
                    SaldoDoDia = totalEntradas - totalSaidas,
                    TotalTransacoes = transacoesDia.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 ERRO FECHAR CAIXA: {ex.Message}");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }


        [HttpPost("loja/{lojaId}/transacao")]
        public async Task<ActionResult<object>> AdicionarTransacao(int lojaId, [FromBody] TransacaoRequestDTO request)
        {
            try
            {
                var caixa = await _context.Caixas
                    .FirstOrDefaultAsync(c => c.LojaId == lojaId && c.Status == "Aberto");

                if (caixa == null)
                {
                    return BadRequest("Não há caixa aberto para esta loja");
                }

                var transacao = new Transacao
                {
                    CaixaId = caixa.Id,
                    Tipo = request.Tipo,
                    Categoria = request.Categoria,
                    Valor = request.Valor,
                    Descricao = request.Descricao,
                    Data = DateTime.Now,
                    MetodoPagamento = request.MetodoPagamento,
                    Observacao = request.Observacao
                };

                if (request.Tipo == "Entrada")
                    caixa.SaldoAtual += request.Valor;
                else if (request.Tipo == "Saida")
                    caixa.SaldoAtual -= request.Valor;

                _context.Transacoes.Add(transacao);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Transação adicionada com sucesso",
                    TransacaoId = transacao.Id,
                    Tipo = transacao.Tipo,
                    Valor = transacao.Valor,
                    SaldoAtual = caixa.SaldoAtual,
                    Data = transacao.Data.ToString("dd/MM/yyyy HH:mm")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 ERRO ADICIONAR TRANSAÇÃO: {ex.Message}");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}