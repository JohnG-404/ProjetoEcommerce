using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEcommerce.DTOs;
using ProjetoEcommerce.Modelos;
using ProjetoEcommerce.Services;

namespace ProjetoEcommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosDigitaisController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NotificacaoService _notificacaoService;

        public ProdutosDigitaisController(AppDbContext context, NotificacaoService notificacaoService)
        {
            _context = context;
            _notificacaoService = notificacaoService;
        }
        [HttpGet("debug")]
        public async Task<ActionResult> DebugProdutosDigitais()
        {
            try
            {
                var debugInfo = new List<object>();

                // Testar consulta básica sem Includes
                var produtosBasico = await _context.ProdutosDigitais
                    .Take(5)
                    .ToListAsync();

                foreach (var p in produtosBasico)
                {
                    debugInfo.Add(new
                    {
                        Id = p.Id,
                        Nome = p.Nome ?? "NULL",
                        SKU = p.SKU ?? "NULL",
                        Descricao = p.Descricao ?? "NULL",
                        UrlDownload = p.UrlDownload ?? "NULL",
                        FormatoArquivo = p.FormatoArquivo ?? "NULL",
                        TamanhoArquivoMB = p.TamanhoArquivoMB?.ToString() ?? "NULL"
                    });
                }

                return Ok(new
                {
                    Message = "Debug Produtos Digitais",
                    Total = produtosBasico.Count,
                    Data = debugInfo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Debug Error: {ex.Message}\n{ex.StackTrace}");
            }
        }
        // GET: api/produtos-digitais
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProdutosDigitais()
        {
            try
            {
                // 🔥 USAR O MESMO CÓDIGO SEGURO DO DEBUG
                var debugInfo = new List<object>();

                // Testar consulta básica sem Includes
                var produtosBasico = await _context.ProdutosDigitais
                    .Take(50)
                    .ToListAsync();

                foreach (var p in produtosBasico)
                {
                    debugInfo.Add(new
                    {
                        Id = p.Id,
                        Nome = p.Nome ?? "NULL",
                        SKU = p.SKU ?? "NULL",
                        Descricao = p.Descricao ?? "NULL",
                        Preco = p.Preco,
                        UrlDownload = p.UrlDownload ?? "NULL",
                        FormatoArquivo = p.FormatoArquivo ?? "NULL",
                        TamanhoArquivoMB = p.TamanhoArquivoMB?.ToString() ?? "NULL",
                        LimiteDownloads = p.LimiteDownloads
                    });
                }

                return Ok(new
                {
                    Message = "Produtos Digitais (Consulta Segura)",
                    Total = produtosBasico.Count,
                    Data = debugInfo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar produtos digitais: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<ActionResult<object>> CriarProdutoDigital([FromBody] ProdutoDigitalDTO request)
        {
            try
            {
                // 🔥 VALIDAÇÃO BÁSICA
                if (string.IsNullOrWhiteSpace(request.Nome))
                    return BadRequest("Nome é obrigatório");

                if (string.IsNullOrWhiteSpace(request.SKU))
                    return BadRequest("SKU é obrigatório");

                if (request.Preco <= 0)
                    return BadRequest("Preço deve ser maior que zero");

                if (request.LojaId <= 0)
                    return BadRequest("LojaId inválido");

                if (request.CategoriaId <= 0)
                    return BadRequest("CategoriaId inválido");

                // 🔥 VERIFICAÇÕES SIMPLES
                var lojaExiste = await _context.Lojas.AnyAsync(l => l.Id == request.LojaId);
                if (!lojaExiste)
                    return BadRequest("Loja não encontrada");

                var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == request.CategoriaId);
                if (!categoriaExiste)
                    return BadRequest("Categoria não encontrada");

                var skuExistente = await _context.ProdutosBase.AnyAsync(p => p.SKU == request.SKU);
                if (skuExistente)
                    return BadRequest("SKU já cadastrado");

                // 🔥 CRIAR PRODUTO DIGITAL DE FORMA SEGURA
                var produto = new ProdutoDigital
                {
                    Nome = request.Nome.Trim(),
                    Descricao = request.Descricao?.Trim() ?? string.Empty,
                    Preco = request.Preco,
                    SKU = request.SKU.Trim(),
                    LojaId = request.LojaId,
                    CategoriaId = request.CategoriaId,
                    UrlDownload = request.UrlDownload?.Trim() ?? string.Empty,
                    TamanhoArquivoMB = request.TamanhoArquivoMB,
                    FormatoArquivo = request.FormatoArquivo?.Trim() ?? "PDF",
                    LimiteDownloads = request.LimiteDownloads,
                    DataExpiracao = request.DataExpiracao
                };

                // 🔥 SALVAR O PRODUTO
                _context.ProdutosDigitais.Add(produto);
                await _context.SaveChangesAsync();

                // 🔥 RESPOSTA SIMPLES
                return Ok(new
                {
                    Message = "Produto digital criado com sucesso",
                    ProdutoId = produto.Id,
                    Nome = produto.Nome,
                    SKU = produto.SKU,
                    Preco = produto.Preco,
                    UrlDownload = produto.UrlDownload,
                    ChaveLicenca = produto.ChaveLicenca
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 ERRO NO POST DIGITAL: {ex.Message}");
                Console.WriteLine($"🔴 STACK TRACE: {ex.StackTrace}");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoDigitalResponseDTO>> ObterProdutoDigital(int id)
        {
            try
            {
                var produtoDigital = await _context.ProdutosDigitais
                    .Include(pd => pd.Loja)
                    .Include(pd => pd.Categoria)
                    .Where(pd => pd.Id == id && pd.Ativo)
                    .Select(pd => new ProdutoDigitalResponseDTO
                    {
                        Id = pd.Id,
                        Nome = pd.Nome,
                        Descricao = pd.Descricao ?? string.Empty, // 🔥 CORREÇÃO: tratar NULL
                        Preco = pd.Preco,
                        PrecoComImposto = pd.CalcularPrecoComImposto(),
                        SKU = pd.SKU,
                        Categoria = pd.Categoria != null ? pd.Categoria.Nome : "Sem categoria", // 🔥 CORREÇÃO
                        Loja = pd.Loja != null ? pd.Loja.Nome : "Sem loja", // 🔥 CORREÇÃO
                        UrlDownload = pd.UrlDownload ?? string.Empty, // 🔥 CORREÇÃO
                        TamanhoArquivoMB = pd.TamanhoArquivoMB ?? 0, // 🔥 CORREÇÃO
                        FormatoArquivo = pd.FormatoArquivo ?? string.Empty, // 🔥 CORREÇÃO
                        LimiteDownloads = pd.LimiteDownloads,
                        ChaveLicenca = pd.ChaveLicenca ?? string.Empty, // 🔥 CORREÇÃO
                        DataExpiracao = pd.DataExpiracao,
                        DataCriacao = pd.DataCriacao,
                        LinkValido = pd.LinkValido(),
                        InfoDownload = pd.ObterInformacoesDownload(true)
                    })
                    .FirstOrDefaultAsync();

                if (produtoDigital == null)
                {
                    return NotFound("Produto digital não encontrado");
                }

                return produtoDigital;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar produto digital: {ex.Message}");
            }
        }

        // POST: api/produtos-digitais/5/renovar-licenca
        [HttpPost("{id}/renovar-licenca")]
        public async Task<ActionResult> RenovarLicenca(int id, [FromBody] int dias = 365)
        {
            try
            {
                var produto = await _context.ProdutosDigitais.FindAsync(id);
                if (produto == null)
                    return NotFound();

                produto.RenovarLicenca(dias);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Licença renovada com sucesso",
                    novaDataExpiracao = produto.DataExpiracao,
                    novaChaveLicenca = produto.ChaveLicenca
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }

}