using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEcommerce.DTOs;
using ProjetoEcommerce.Modelos;

namespace ProjetoEcommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosDigitaisController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProdutosDigitaisController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoDigitalDTO>>> GetProdutosDigitais()
        {
            try
            {
                var produtos = await _context.ProdutosDigitais
                    .Include(pd => pd.Produto)
                    .Where(pd => pd.Produto.Ativo)
                    .Select(pd => new ProdutoDigitalDTO
                    {
                        Id = pd.Id,
                        ProdutoId = pd.ProdutoId,
                        Nome = pd.Produto.Nome,
                        Descricao = pd.Produto.Descricao,
                        Preco = pd.Produto.Preco,
                        UrlDownload = pd.UrlDownload,
                        TamanhoArquivoMB = pd.TamanhoArquivoMB,
                        FormatoArquivo = pd.FormatoArquivo,
                        LimiteDownloads = pd.LimiteDownloads
                    })
                    .ToListAsync();

                return Ok(produtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar produtos digitais: {ex.Message}");
            }
        }

        [HttpPost("download/{produtoId}")]
        public async Task<ActionResult> SolicitarDownload(int produtoId, [FromBody] int usuarioId)
        {
            try
            {
                var produtoDigital = await _context.ProdutosDigitais
                    .Include(pd => pd.Produto)
                    .FirstOrDefaultAsync(pd => pd.ProdutoId == produtoId);

                if (produtoDigital == null)
                    return NotFound("Produto digital não encontrado");

                if (!produtoDigital.LinkValido())
                    return BadRequest("Link de download expirado");

                // Aqui você implementaria a lógica de contagem de downloads
                var linkDownload = produtoDigital.UrlDownload;

                return Ok(new
                {
                    downloadUrl = linkDownload,
                    mensagem = "Download disponível por 24 horas",
                    formato = produtoDigital.FormatoArquivo,
                    tamanhoMB = produtoDigital.TamanhoArquivoMB
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao processar download: {ex.Message}");
            }
        }
    }
}

public class ProdutoDigitalDTO
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Preco { get; set; }
    public string UrlDownload { get; set; }
    public decimal TamanhoArquivoMB { get; set; }
    public string FormatoArquivo { get; set; }
    public int LimiteDownloads { get; set; }
}