using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEcommerce.Modelos;

namespace ProjetoEcommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosBaseController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProdutosBaseController(AppDbContext context)
        {
            _context = context;
        }
      
        [HttpGet("debug")]
        public async Task<ActionResult> DebugProdutosBase()
        {
            try
            {
                var debugInfo = new List<object>();

                
                var produtosBase = await _context.ProdutosBase
                    .Take(5)
                    .ToListAsync();

                foreach (var p in produtosBase)
                {
                    debugInfo.Add(new
                    {
                        Id = p.Id,
                        Nome = p.Nome ?? "NULL",
                        SKU = p.SKU ?? "NULL",
                        Descricao = p.Descricao ?? "NULL",
                        LojaId = p.LojaId,
                        CategoriaId = p.CategoriaId
                    });
                }

                return Ok(new
                {
                    Message = "Debug Produtos Base",
                    Total = produtosBase.Count,
                    Data = debugInfo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Debug Error: {ex.Message}\n{ex.StackTrace}");
            }
        }
       
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTodosProdutos()
        {
            try
            {
                
                var debugInfo = new List<object>();

                
                var produtosBase = await _context.ProdutosBase
                    .Take(50) 
                    .ToListAsync();

                foreach (var p in produtosBase)
                {
                    debugInfo.Add(new
                    {
                        Id = p.Id,
                        Nome = p.Nome ?? "NULL",
                        SKU = p.SKU ?? "NULL",
                        Descricao = p.Descricao ?? "NULL",
                        Preco = p.Preco,
                        LojaId = p.LojaId,
                        CategoriaId = p.CategoriaId,
                        Tipo = "Base" 
                    });
                }

                return Ok(new
                {
                    Message = "Produtos Base (Consulta Segura)",
                    Total = produtosBase.Count,
                    Data = debugInfo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar produtos: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetProduto(int id)
        {
            try
            {
                var produtoFisico = await _context.ProdutosFisicos
                    .Include(p => p.Loja)
                    .Include(p => p.Categoria)
                    .Include(p => p.Estoque)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (produtoFisico != null)
                {
                    return Ok(new
                    {
                        Tipo = "Físico",
                        produtoFisico.Id,
                        produtoFisico.Nome,
                        Descricao = produtoFisico.Descricao ?? string.Empty, // 🔥 TRATAR NULL
                        produtoFisico.Preco,
                        produtoFisico.SKU,
                        PrecoComImposto = produtoFisico.CalcularPrecoComImposto(),
                        Categoria = produtoFisico.Categoria != null ? produtoFisico.Categoria.Nome : "Sem categoria", // 🔥 TRATAR NULL
                        Loja = produtoFisico.Loja != null ? produtoFisico.Loja.Nome : "Sem loja", // 🔥 TRATAR NULL
                        produtoFisico.Peso,
                        Dimensoes = produtoFisico.ObterDimensoes(),
                        Volume = produtoFisico.CalcularVolume(),
                        Estoque = produtoFisico.Estoque?.QuantidadeDisponivel ?? 0,
                        TipoProduto = produtoFisico.ObterTipoProduto()
                    });
                }

                var produtoDigital = await _context.ProdutosDigitais
                    .Include(p => p.Loja)
                    .Include(p => p.Categoria)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (produtoDigital != null)
                {
                    return Ok(new
                    {
                        Tipo = "Digital",
                        produtoDigital.Id,
                        produtoDigital.Nome,
                        Descricao = produtoDigital.Descricao ?? string.Empty, // 🔥 TRATAR NULL
                        produtoDigital.Preco,
                        produtoDigital.SKU,
                        PrecoComImposto = produtoDigital.CalcularPrecoComImposto(),
                        Categoria = produtoDigital.Categoria != null ? produtoDigital.Categoria.Nome : "Sem categoria", // 🔥 TRATAR NULL
                        Loja = produtoDigital.Loja != null ? produtoDigital.Loja.Nome : "Sem loja", // 🔥 TRATAR NULL
                        produtoDigital.TamanhoArquivoMB,
                        FormatoArquivo = produtoDigital.FormatoArquivo ?? string.Empty, // 🔥 TRATAR NULL
                        produtoDigital.LimiteDownloads,
                        ChaveLicenca = produtoDigital.ChaveLicenca ?? string.Empty, // 🔥 TRATAR NULL
                        InfoDownload = produtoDigital.ObterInformacoesDownload(true),
                        LinkValido = produtoDigital.LinkValido(),
                        TipoProduto = produtoDigital.ObterTipoProduto()
                    });
                }

                return NotFound("Produto não encontrado");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar produto: {ex.Message}");
            }
        }

        [HttpPost("calcular-imposto")]
        public ActionResult CalcularImposto([FromBody] List<int> produtosIds)
        {
            try
            {
                var produtos = new List<ProdutoBase>
                {
                    new ProdutoFisico
                    {
                        Nome = "Smartphone",
                        Descricao = "Smartphone Android",
                        Preco = 1500m,
                        SKU = "SM-001",
                        LojaId = 1,
                        CategoriaId = 1
                    },
                    new ProdutoDigital("E-book", "E-book de Programação", 50m, "EB-001", 1, 2,
                                     "http://download.com/ebook", 10m, "PDF")
                };

                var resultados = new List<object>();

                foreach (var produto in produtos)
                {
                    var imposto = produto.CalcularPrecoComImposto();

                    resultados.Add(new
                    {
                        Produto = produto.Nome,
                        Tipo = produto.ObterTipoProduto(),
                        PrecoOriginal = produto.Preco,
                        PrecoComImposto = imposto,
                        Diferenca = imposto - produto.Preco
                    });
                }

                return Ok(new
                {
                    Message = "Cálculo de impostos demonstrando polimorfismo",
                    Resultados = resultados
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro no cálculo: {ex.Message}");
            }
        }
    }
}