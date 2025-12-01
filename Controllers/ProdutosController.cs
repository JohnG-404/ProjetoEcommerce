using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEcommerce.Modelos;
using ProjetoEcommerce.DTOs;
using ProjetoEcommerce.Services;

namespace ProjetoEcommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NotificacaoService _notificacaoService;

        public ProdutosController(AppDbContext context, NotificacaoService notificacaoService)
        {
            _context = context;
            _notificacaoService = notificacaoService;
        }
        // 🔥 DEBUG DO POST
        // 🔥 DEBUG DO POST MAIS DETALHADO
        [HttpPost("debug-stepbystep")]
        public async Task<ActionResult> DebugPostStepByStep([FromBody] ProdutoFisicoDTO request)
        {
            try
            {
                var debugSteps = new List<object>();

                // 🔥 PASSO 1: Validar dados básicos
                debugSteps.Add(new { Step = "1. Validação Dados", Status = "Iniciada", Data = request });

                if (string.IsNullOrWhiteSpace(request.Nome))
                    return BadRequest("Nome é obrigatório");

                if (string.IsNullOrWhiteSpace(request.SKU))
                    return BadRequest("SKU é obrigatório");

                debugSteps.Add(new { Step = "1. Validação Dados", Status = "Concluída" });

                // 🔥 PASSO 2: Consulta SIMPLES da Loja (sem includes)
                debugSteps.Add(new { Step = "2. Consulta Loja", Status = "Iniciada", LojaId = request.LojaId });

                var loja = await _context.Lojas
                    .Where(l => l.Id == request.LojaId)
                    .Select(l => new { l.Id, l.Nome })
                    .FirstOrDefaultAsync();

                debugSteps.Add(new { Step = "2. Consulta Loja", Status = loja != null ? "Encontrada" : "Não encontrada", Loja = loja });

                // 🔥 PASSO 3: Consulta SIMPLES da Categoria (sem includes)  
                debugSteps.Add(new { Step = "3. Consulta Categoria", Status = "Iniciada", CategoriaId = request.CategoriaId });

                var categoria = await _context.Categorias
                    .Where(c => c.Id == request.CategoriaId)
                    .Select(c => new { c.Id, c.Nome })
                    .FirstOrDefaultAsync();

                debugSteps.Add(new { Step = "3. Consulta Categoria", Status = categoria != null ? "Encontrada" : "Não encontrada", Categoria = categoria });

                // 🔥 PASSO 4: Consulta SIMPLES do SKU (sem includes)
                debugSteps.Add(new { Step = "4. Verificar SKU", Status = "Iniciada", SKU = request.SKU });

                var skuExistente = await _context.ProdutosBase
                    .Where(p => p.SKU == request.SKU)
                    .Select(p => new { p.Id, p.SKU })
                    .AnyAsync();

                debugSteps.Add(new { Step = "4. Verificar SKU", Status = skuExistente ? "Já existe" : "Disponível" });

                // 🔥 PASSO 5: Criar produto (apenas em memória)
                debugSteps.Add(new { Step = "5. Criar Produto", Status = "Iniciada" });

                var produto = new ProdutoFisico
                {
                    Nome = request.Nome.Trim(),
                    Descricao = request.Descricao?.Trim() ?? string.Empty,
                    Preco = request.Preco,
                    SKU = request.SKU.Trim(),
                    LojaId = request.LojaId,
                    CategoriaId = request.CategoriaId,
                    Peso = request.Peso
                };

                debugSteps.Add(new { Step = "5. Criar Produto", Status = "Concluída", Produto = new { produto.Nome, produto.SKU } });

                return Ok(new
                {
                    Message = "Debug POST Step-by-Step concluído",
                    Steps = debugSteps
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Erro no debug POST Step-by-Step",
                    Error = ex.Message,
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message
                });
            }
        }
        // GET: api/produtos - APENAS PRODUTOS FÍSICOS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProdutosFisicos()
        {
            try
            {
                // 🔥 CONSULTA MAIS SIMPLES E SEGURA
                var produtos = await _context.ProdutosFisicos
                    .Where(p => p.Ativo)
                    .Select(p => new
                    {
                        // 🔥 PROPRIEDADES BÁSICAS PRIMEIRO
                        p.Id,
                        p.Nome,
                        Descricao = p.Descricao ?? "",
                        p.Preco,
                        p.SKU,

                        // 🔥 PROPRIEDADES CALCULADAS
                        PrecoComImposto = p.CalcularPrecoComImposto(),

                        // 🔥 RELACIONAMENTOS COM VERIFICAÇÃO
                        Categoria = p.Categoria != null ? p.Categoria.Nome : "",
                        Loja = p.Loja != null ? p.Loja.Nome : "",

                        // 🔥 PROPRIEDADES ESPECÍFICAS
                        Peso = p.Peso,
                        Dimensoes = p.ObterDimensoes(),
                        Volume = p.CalcularVolume(),

                        // 🔥 ESTOQUE COM VERIFICAÇÃO
                        Estoque = p.Estoque != null ? p.Estoque.QuantidadeDisponivel : 0,

                        PodeEnviar = p.PodeSerEnviado(),
                        p.DataCriacao
                    })
                    .ToListAsync();

                return Ok(produtos);
            }
            catch (Exception ex)
            {
                // 🔥 LOG DETALHADO
                Console.WriteLine($"🔴 ERRO DETALHADO: {ex.Message}");
                Console.WriteLine($"🔴 STACK TRACE: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"🔴 INNER EXCEPTION: {ex.InnerException.Message}");
                }

                return StatusCode(500, $"Erro ao buscar produtos físicos: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<object>> PostProdutoFisico([FromBody] ProdutoFisicoDTO request)
        {
            try
            {
                // 🔥 VALIDAÇÃO BÁSICA (já testada no debug)
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

                // 🔥 VERIFICAÇÕES SIMPLES (já testadas no debug)
                var lojaExiste = await _context.Lojas.AnyAsync(l => l.Id == request.LojaId);
                if (!lojaExiste)
                    return BadRequest("Loja não encontrada");

                var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == request.CategoriaId);
                if (!categoriaExiste)
                    return BadRequest("Categoria não encontrada");

                var skuExistente = await _context.ProdutosBase.AnyAsync(p => p.SKU == request.SKU);
                if (skuExistente)
                    return BadRequest("SKU já cadastrado");

                // 🔥 CRIAR PRODUTO DE FORMA SEGURA
                var produto = new ProdutoFisico
                {
                    Nome = request.Nome.Trim(),
                    Descricao = request.Descricao?.Trim() ?? string.Empty,
                    Preco = request.Preco,
                    SKU = request.SKU.Trim(),
                    LojaId = request.LojaId,
                    CategoriaId = request.CategoriaId,
                    Peso = request.Peso
                };

                // Definir dimensões se fornecidas
                if (request.Altura.HasValue && request.Largura.HasValue && request.Profundidade.HasValue)
                {
                    produto.Altura = request.Altura.Value;
                    produto.Largura = request.Largura.Value;
                    produto.Profundidade = request.Profundidade.Value;
                }

                // 🔥 SALVAR APENAS O PRODUTO PRIMEIRO
                _context.ProdutosFisicos.Add(produto);
                await _context.SaveChangesAsync();

                // 🔥 DEPOIS CRIAR ESTOQUE (se necessário)
                if (request.QuantidadeEstoque > 0)
                {
                    var estoque = new Estoque
                    {
                        ProdutoFisicoId = produto.Id,
                        QuantidadeDisponivel = request.QuantidadeEstoque,
                        QuantidadeReservada = 0,
                        PontoRepor = request.PontoRepor,
                        EstoqueMinimo = 5,
                        UltimoMovimento = DateTime.Now
                    };

                    _context.Estoques.Add(estoque);
                    await _context.SaveChangesAsync();
                }

                // 🔥 RESPOSTA SIMPLES SEM CONSULTAS COMPLEXAS
                return Ok(new
                {
                    Message = "Produto criado com sucesso",
                    ProdutoId = produto.Id,
                    Nome = produto.Nome,
                    SKU = produto.SKU,
                    Preco = produto.Preco,
                    EstoqueCriado = request.QuantidadeEstoque > 0
                });
            }
            catch (Exception ex)
            {
                // 🔥 LOG DETALHADO
                Console.WriteLine($"🔴 ERRO NO POST: {ex.Message}");
                Console.WriteLine($"🔴 STACK TRACE: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"🔴 INNER EXCEPTION: {ex.InnerException.Message}");
                }

                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        // GET: api/produtos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoFisicoResponseDTO>> GetProdutoFisico(int id)
        {
            try
            {
                var produto = await _context.ProdutosFisicos
                    .Include(p => p.Categoria)
                    .Include(p => p.Loja)
                    .Include(p => p.Estoque)
                    .Where(p => p.Id == id && p.Ativo)
                    .Select(p => new ProdutoFisicoResponseDTO
                    {
                        Id = p.Id,
                        Nome = p.Nome,
                        Descricao = p.Descricao,
                        Preco = p.Preco,
                        PrecoComImposto = p.CalcularPrecoComImposto(),
                        Peso = p.Peso,
                        SKU = p.SKU,
                        Categoria = p.Categoria.Nome,
                        CategoriaId = p.CategoriaId,
                        Loja = p.Loja.Nome,
                        LojaId = p.LojaId,
                        Dimensoes = p.ObterDimensoes(),
                        Volume = p.CalcularVolume(),
                        Estoque = p.Estoque != null ? p.Estoque.QuantidadeDisponivel : 0,
                        PodeEnviar = p.PodeSerEnviado(),
                        DataCriacao = p.DataCriacao
                    })
                    .FirstOrDefaultAsync();

                if (produto == null)
                {
                    return NotFound("Produto físico não encontrado");
                }

                return produto;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar produto físico: {ex.Message}");
            }
        }

        // DELETE: api/produtos/5/permanente
        [HttpDelete("{id}/permanente")]
        public async Task<ActionResult> DeleteProdutoFisicoPermanente(int id)
        {
            try
            {
                var produto = await _context.ProdutosFisicos
                    .Include(p => p.Estoque)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (produto == null)
                {
                    return NotFound("Produto físico não encontrado");
                }

                // 🔥 POLIMORFISMO - Validar antes de deletar
                if (!produto.Validar())
                {
                    return BadRequest("Produto físico contém dados inválidos");
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Deletar estoque relacionado
                    if (produto.Estoque != null)
                    {
                        _context.Estoques.Remove(produto.Estoque);
                    }

                    // Deletar itens do carrinho
                    var itensCarrinho = await _context.CarrinhoItens
                        .Where(ci => ci.ProdutoFisicoId == id) // 🔥 Agora referencia ProdutoFisico
                        .ToListAsync();

                    if (itensCarrinho.Any())
                    {
                        _context.CarrinhoItens.RemoveRange(itensCarrinho);
                    }

                    // Deletar produto físico
                    _context.ProdutosFisicos.Remove(produto);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        message = "Produto físico deletado permanentemente com sucesso",
                        produtoId = id,
                        produtoNome = produto.Nome
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"Erro durante a transação: {ex.Message}", ex);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao deletar produto físico: {ex.Message}");
            }
        }

        // PUT: api/produtos/5/dimensoes
        [HttpPut("{id}/dimensoes")]
        public async Task<ActionResult> AtualizarDimensoes(int id, [FromBody] DimensoesDTO dimensoes)
        {
            try
            {
                var produto = await _context.ProdutosFisicos.FindAsync(id);
                if (produto == null)
                    return NotFound();

                // 🔥 ENCAPSULAMENTO - Usando método específico do ProdutoFisico
                produto.DefinirDimensoes(dimensoes.Altura, dimensoes.Largura, dimensoes.Profundidade);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Dimensões atualizadas com sucesso",
                    dimensoes = produto.ObterDimensoes(),
                    volume = produto.CalcularVolume()
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }

    // 🔥 DTOs ESPECÍFICOS para ProdutoFisico
}