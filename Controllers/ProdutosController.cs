using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEcommerce.DTOs;
using ProjetoEcommerce.Modelos;
using ProjetoEcommerce.Services;
using ProjetoEcommerce.Interfaces;

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

        // GET: api/produtos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoResponseDTO>>> GetProdutos()
        {
            try
            {
                var produtos = await _context.Produtos
                    .Where(p => p.Ativo)
                    .Include(p => p.Categoria)
                    .Include(p => p.Loja)
                    .Include(p => p.Estoque)
                    .Select(p => new ProdutoResponseDTO
                    {
                        Id = p.Id,
                        Nome = p.Nome,
                        Descricao = p.Descricao,
                        Preco = p.Preco,
                        Peso = p.Peso,
                        SKU = p.SKU,
                        Categoria = p.Categoria.Nome,
                        CategoriaId = p.CategoriaId,
                        Loja = p.Loja.Nome,
                        LojaId = p.LojaId,
                        Estoque = p.Estoque != null ? p.Estoque.QuantidadeDisponivel : 0,
                        Ativo = p.Ativo,
                        DataCriacao = p.DataCriacao,
                        Tipo = p.Tipo
                    })
                    .ToListAsync();

                return Ok(produtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar produtos: {ex.Message}");
            }
        }

        // GET: api/produtos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoResponseDTO>> GetProduto(int id)
        {
            try
            {
                var produto = await _context.Produtos
                    .Include(p => p.Categoria)
                    .Include(p => p.Loja)
                    .Include(p => p.Estoque)
                    .Where(p => p.Id == id)
                    .Select(p => new ProdutoResponseDTO
                    {
                        Id = p.Id,
                        Nome = p.Nome,
                        Descricao = p.Descricao,
                        Preco = p.Preco,
                        Peso = p.Peso,
                        SKU = p.SKU,
                        Categoria = p.Categoria.Nome,
                        CategoriaId = p.CategoriaId,
                        Loja = p.Loja.Nome,
                        LojaId = p.LojaId,
                        Estoque = p.Estoque != null ? p.Estoque.QuantidadeDisponivel : 0,
                        Ativo = p.Ativo,
                        DataCriacao = p.DataCriacao,
                        Tipo = p.Tipo
                    })
                    .FirstOrDefaultAsync();

                if (produto == null)
                {
                    return NotFound("Produto não encontrado");
                }

                return produto;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar produto: {ex.Message}");
            }
        }

        // POST: api/produtos
        [HttpPost]
        public async Task<ActionResult<ProdutoResponseDTO>> PostProduto([FromBody] ProdutoDTO request)
        {
            try
            {
                // Verificar se a loja existe
                var loja = await _context.Lojas.FindAsync(request.LojaId);
                if (loja == null)
                {
                    return BadRequest("Loja não encontrada");
                }

                // Verificar se a categoria existe
                var categoria = await _context.Categorias.FindAsync(request.CategoriaId);
                if (categoria == null)
                {
                    return BadRequest("Categoria não encontrada");
                }

                // Verificar se SKU já existe
                if (await _context.Produtos.AnyAsync(p => p.SKU == request.SKU))
                {
                    return BadRequest("SKU já cadastrado");
                }

                // HERANÇA SIMULADA - Criar produto base
                Produto produto;

                if (request.Tipo == "Digital")
                {
                    // ENCAPSULAMENTO - Usando construtor específico
                    produto = new Produto(request.Nome, request.Descricao, request.Preco, request.SKU, request.LojaId, request.CategoriaId);
                    produto.DefinirComoDigital(); // MÉTODO ESPECÍFICO
                }
                else
                {
                    produto = new Produto(request.Nome, request.Descricao, request.Preco, request.SKU, request.LojaId, request.CategoriaId);
                    produto.DefinirComoFisico(); // MÉTODO ESPECÍFICO
                }

                // POLIMORFISMO - Usando método Validar() que pode ter comportamentos diferentes
                if (!produto.Validar())
                {
                    return BadRequest("Dados do produto inválidos");
                }

                _context.Produtos.Add(produto);
                await _context.SaveChangesAsync();

                // Se for produto digital, criar registro na tabela específica
                if (request.Tipo == "Digital" && !string.IsNullOrEmpty(request.UrlDownload))
                {
                    var produtoDigital = new ProdutoDigital(
                        produto.Id,
                        request.UrlDownload,
                        request.TamanhoArquivoMB,
                        request.FormatoArquivo
                    );
                    _context.ProdutosDigitais.Add(produtoDigital);
                }

                // Criar estoque (apenas para produtos físicos)
                if (request.Tipo == "Fisico")
                {
                    var estoque = new Estoque
                    {
                        ProdutoId = produto.Id,
                        QuantidadeDisponivel = request.QuantidadeEstoque,
                        QuantidadeReservada = 0,
                        PontoRepor = request.PontoRepor,
                        EstoqueMinimo = 5,
                        UltimoMovimento = DateTime.Now
                    };
                    _context.Estoques.Add(estoque);
                }

                await _context.SaveChangesAsync();

                // POLIMORFISMO - Sistema de notificação (CORRIGIDO)
                var emailService = new EmailService("admin@loja.com", $"Novo produto {produto.Tipo} cadastrado: {produto.Nome}");
                var smsService = new SMSService("11999999999", $"Produto {produto.Nome} cadastrado com sucesso");

                _notificacaoService.AdicionarNotificacao(emailService);
                _notificacaoService.AdicionarNotificacao(smsService);

                // OU usar os métodos de sobrecarga (alternativa)
                _notificacaoService.AdicionarNotificacaoEmail("admin@loja.com", $"Novo produto {produto.Tipo} cadastrado: {produto.Nome}");
                _notificacaoService.AdicionarNotificacaoSMS("11999999999", $"Produto {produto.Nome} cadastrado com sucesso");

                var resultados = _notificacaoService.EnviarTodasNotificacoes();

                Console.WriteLine("🔔 Notificações enviadas:");
                foreach (var resultado in resultados)
                {
                    Console.WriteLine($"   - {resultado}");
                }

                var response = new ProdutoResponseDTO
                {
                    Id = produto.Id,
                    Nome = produto.Nome,
                    Descricao = produto.Descricao,
                    Preco = produto.Preco,
                    Peso = produto.Peso,
                    SKU = produto.SKU,
                    Categoria = categoria.Nome,
                    CategoriaId = categoria.Id,
                    Loja = loja.Nome,
                    LojaId = loja.Id,
                    Estoque = request.Tipo == "Fisico" ? request.QuantidadeEstoque : 0,
                    Ativo = produto.Ativo,
                    DataCriacao = produto.DataCriacao,
                    Tipo = produto.Tipo
                };

                return CreatedAtAction(nameof(GetProduto), new { id = produto.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        // PUT: api/produtos/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ProdutoResponseDTO>> PutProduto(int id, [FromBody] ProdutoUpdateDTO request)
        {
            try
            {
                var produto = await _context.Produtos
                    .Include(p => p.Categoria)
                    .Include(p => p.Loja)
                    .Include(p => p.Estoque)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (produto == null)
                {
                    return NotFound("Produto não encontrado");
                }

                // ENCAPSULAMENTO - Usando métodos para alterar propriedades
                produto.AlterarNome(request.Nome);
                produto.AlterarPreco(request.Preco);

                // Alterar outras propriedades diretamente (poderia ter métodos também)
                produto.Descricao = request.Descricao;
                produto.Peso = request.Peso;
                produto.SKU = request.SKU;
                produto.Ativo = request.Ativo;

                // Verificar se a categoria existe
                var categoria = await _context.Categorias.FindAsync(request.CategoriaId);
                if (categoria == null)
                {
                    return BadRequest("Categoria não encontrada");
                }
                produto.CategoriaId = request.CategoriaId;

                await _context.SaveChangesAsync();

                // Retornar resposta atualizada
                var response = new ProdutoResponseDTO
                {
                    Id = produto.Id,
                    Nome = produto.Nome,
                    Descricao = produto.Descricao,
                    Preco = produto.Preco,
                    Peso = produto.Peso,
                    SKU = produto.SKU,
                    Categoria = categoria.Nome,
                    CategoriaId = categoria.Id,
                    Loja = produto.Loja.Nome,
                    LojaId = produto.LojaId,
                    Estoque = produto.Estoque?.QuantidadeDisponivel ?? 0,
                    Ativo = produto.Ativo,
                    DataCriacao = produto.DataCriacao,
                    Tipo = produto.Tipo
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        // NOVO ENDPOINT: api/produtos/validos
        [HttpGet("validos")]
        public async Task<ActionResult<IEnumerable<ProdutoResponseDTO>>> GetProdutosValidos()
        {
            try
            {
                var produtos = await _context.Produtos
                    .Where(p => p.Ativo)
                    .Include(p => p.Categoria)
                    .Include(p => p.Loja)
                    .Include(p => p.Estoque)
                    .ToListAsync();

                // POLIMORFISMO - Filtrando usando método Validar()
                var produtosValidos = produtos.Where(p => p.Validar())
                    .Select(p => new ProdutoResponseDTO
                    {
                        Id = p.Id,
                        Nome = p.Nome,
                        Descricao = p.Descricao,
                        Preco = p.Preco,
                        Peso = p.Peso,
                        SKU = p.SKU,
                        Categoria = p.Categoria.Nome,
                        CategoriaId = p.CategoriaId,
                        Loja = p.Loja.Nome,
                        LojaId = p.LojaId,
                        Estoque = p.Estoque?.QuantidadeDisponivel ?? 0,
                        Ativo = p.Ativo,
                        DataCriacao = p.DataCriacao,
                        Tipo = p.Tipo
                    })
                    .ToList();

                return Ok(produtosValidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar produtos: {ex.Message}");
            }
        }

        // PUT: api/produtos/5/preco
        [HttpPut("{id}/preco")]
        public async Task<ActionResult> AtualizarPreco(int id, [FromBody] decimal novoPreco)
        {
            try
            {
                var produto = await _context.Produtos.FindAsync(id);
                if (produto == null)
                    return NotFound();

                // ENCAPSULAMENTO - Usando método para alterar preço com validação
                produto.AlterarPreco(novoPreco);

                await _context.SaveChangesAsync();

                return Ok(new { message = "Preço atualizado com sucesso" });
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

        // GET: api/produtos/categoria/5
        [HttpGet("categoria/{categoriaId}")]
        public async Task<ActionResult<IEnumerable<ProdutoResponseDTO>>> GetProdutosPorCategoria(int categoriaId)
        {
            try
            {
                var produtos = await _context.Produtos
                    .Where(p => p.CategoriaId == categoriaId && p.Ativo)
                    .Include(p => p.Categoria)
                    .Include(p => p.Loja)
                    .Include(p => p.Estoque)
                    .Select(p => new ProdutoResponseDTO
                    {
                        Id = p.Id,
                        Nome = p.Nome,
                        Descricao = p.Descricao,
                        Preco = p.Preco,
                        Peso = p.Peso,
                        SKU = p.SKU,
                        Categoria = p.Categoria.Nome,
                        CategoriaId = p.CategoriaId,
                        Loja = p.Loja.Nome,
                        LojaId = p.LojaId,
                        Estoque = p.Estoque != null ? p.Estoque.QuantidadeDisponivel : 0,
                        Ativo = p.Ativo,
                        DataCriacao = p.DataCriacao,
                        Tipo = p.Tipo
                    })
                    .ToListAsync();

                return Ok(produtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar produtos: {ex.Message}");
            }
        }

        // GET: api/produtos/digitais
        [HttpGet("digitais")]
        public async Task<ActionResult<IEnumerable<ProdutoResponseDTO>>> GetProdutosDigitais()
        {
            try
            {
                var produtos = await _context.Produtos
                    .Where(p => p.Tipo == "Digital" && p.Ativo)
                    .Include(p => p.Categoria)
                    .Include(p => p.Loja)
                    .Select(p => new ProdutoResponseDTO
                    {
                        Id = p.Id,
                        Nome = p.Nome,
                        Descricao = p.Descricao,
                        Preco = p.Preco,
                        SKU = p.SKU,
                        Categoria = p.Categoria.Nome,
                        CategoriaId = p.CategoriaId,
                        Loja = p.Loja.Nome,
                        LojaId = p.LojaId,
                        Ativo = p.Ativo,
                        DataCriacao = p.DataCriacao,
                        Tipo = p.Tipo
                    })
                    .ToListAsync();

                return Ok(produtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar produtos digitais: {ex.Message}");
            }
        }

        // DELETE: api/produtos/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduto(int id)
        {
            try
            {
                var produto = await _context.Produtos.FindAsync(id);
                if (produto == null)
                {
                    return NotFound("Produto não encontrado");
                }

                // Soft delete
                produto.Ativo = false;
                produto.DataAtualizacao = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Produto desativado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}