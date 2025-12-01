using Microsoft.EntityFrameworkCore;
using ProjetoEcommerce.Services;
using ProjetoEcommerce.Interfaces;
using ProjetoEcommerce.Modelos;

var builder = WebApplication.CreateBuilder(args);

// Configuração para Docker
if (Environment.GetEnvironmentVariable("DOCKER_RUNNING") == "true")
{
    builder.Configuration.AddJsonFile("appsettings.Docker.json", optional: true);
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MySQL Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 🔥 NOVOS SERVIÇOS - Para POLIMORFISMO e INJEÇÃO DE DEPENDÊNCIA (CORRIGIDO)
builder.Services.AddScoped<NotificacaoService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<SMSService>();

// Registrar as interfaces para injeção de dependência
builder.Services.AddScoped<INotificacao, EmailService>(); // Implementação padrão para Email
builder.Services.AddScoped<INotificacao, SMSService>();   // Implementação alternativa para SMS

// Serviço para demonstrar polimorfismo com múltiplas implementações
builder.Services.AddScoped<IEnumerable<INotificacao>>(serviceProvider =>
{
    return new List<INotificacao>
    {
        serviceProvider.GetRequiredService<EmailService>(),
        serviceProvider.GetRequiredService<SMSService>()
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("DOCKER_RUNNING") == "true")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommerce API V1");
        c.RoutePrefix = "swagger"; // Acessível em /swagger
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// 🔥 INICIALIZAÇÃO DO BANCO DE DADOS - Com dados de exemplo para teste
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        // Garantir que o banco está criado e migrado
        context.Database.EnsureCreated();

        // Adicionar dados iniciais se for desenvolvimento
        if (app.Environment.IsDevelopment())
        {
            await DbInitializer.Initialize(context);
        }

        Console.WriteLine("✅ Banco de dados inicializado com sucesso!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Erro ao inicializar banco de dados");
    }
}

// 🔥 MIDDLEWARE DE TRATAMENTO DE EXCEÇÕES GLOBAL
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Erro não tratado na aplicação");

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var errorResponse = new
        {
            Message = "Ocorreu um erro interno no servidor",
            Detail = app.Environment.IsDevelopment() ? ex.Message : "Contate o administrador",
            Timestamp = DateTime.Now
        };

        await context.Response.WriteAsJsonAsync(errorResponse);
    }
});

// 🔥 ENDPOINTS MINIMAIS PARA TESTE DA API
app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        Message = "🚀 API Ecommerce funcionando!",
        Version = "1.0.0",
        Timestamp = DateTime.Now,
        Endpoints = new
        {
            Swagger = "/swagger",
            Produtos = "/api/Produtos",
            Usuarios = "/api/Usuarios",
            Carrinho = "/api/Carrinho",
            Pedidos = "/api/Pedidos",
            Notificacoes = "/api/Notificacoes"
        }
    });
});

app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        Status = "Healthy",
        Timestamp = DateTime.Now,
        Environment = app.Environment.EnvironmentName
    });
});

Console.WriteLine("🎉 Aplicação iniciada com sucesso!");
Console.WriteLine($"📊 Ambiente: {app.Environment.EnvironmentName}");
Console.WriteLine($"🔗 Swagger: {app.Urls.FirstOrDefault()}/swagger");
// Adicione isto antes de app.Run();
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ ERRO DETALHADO: {Message}", ex.Message);

        // Log mais detalhado para erros de cast
        if (ex.Message.Contains("Unable to cast object of type 'System.DBNull'"))
        {
            logger.LogError("🔍 ERRO DE CAST - Verifique campos NULL no banco de dados");
        }

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var errorResponse = new
        {
            Message = "Ocorreu um erro interno no servidor",
            Detail = app.Environment.IsDevelopment() ? ex.Message : "Contate o administrador",
            StackTrace = app.Environment.IsDevelopment() ? ex.StackTrace : null,
            Timestamp = DateTime.Now
        };

        await context.Response.WriteAsJsonAsync(errorResponse);
    }
});

app.Run();

// 🔥 CLASSE PARA INICIALIZAÇÃO DO BANCO DE DADOS
// 🔥 CLASSE PARA INICIALIZAÇÃO DO BANCO DE DADOS
public static class DbInitializer
{
    public static async Task Initialize(AppDbContext context)
    {
        // Verificar se já existem dados
        if (!await context.Usuarios.AnyAsync())
        {
            Console.WriteLine("📦 Inicializando banco de dados com dados de exemplo...");

            // 1. Criar categorias
            var categorias = new[]
            {
                new Categoria { Nome = "Eletrônicos", Descricao = "Produtos eletrônicos em geral", Ativo = true },
                new Categoria { Nome = "Celulares", Descricao = "Smartphones e telefones móveis", Ativo = true },
                new Categoria { Nome = "Informática", Descricao = "Computadores e acessórios", Ativo = true },
                new Categoria { Nome = "Livros Digitais", Descricao = "E-books e cursos online", Ativo = true }
            };
            await context.Categorias.AddRangeAsync(categorias);
            await context.SaveChangesAsync();

            // 2. Criar lojas
            var lojas = new[]
            {
                new Loja
                {
                    Nome = "TechStore Principal",
                    Descricao = "Loja principal de eletrônicos",
                    CNPJ = "12.345.678/0001-90",
                    Telefone = "(11) 3333-4444",
                    DataCriacao = DateTime.Now,
                    Ativo = true
                }
            };
            await context.Lojas.AddRangeAsync(lojas);
            await context.SaveChangesAsync();

            // 3. Criar caixas para as lojas
            var caixas = new[]
            {
                new Caixa
                {
                    LojaId = 1,
                    SaldoAtual = 10000.00m,
                    SaldoInicial = 10000.00m,
                    Status = "Aberto",
                    DataAbertura = DateTime.Now
                }
            };
            await context.Caixas.AddRangeAsync(caixas);
            await context.SaveChangesAsync();

            // 4. Criar usuários
            var usuarios = new[]
            {
                new Usuario
                {
                    Nome = "Administrador",
                    Email = "admin@loja.com",
                    Senha = "123456",
                    Telefone = "(11) 99999-9999",
                    Role = "Administrador",
                    DataCriacao = DateTime.Now,
                    Ativo = true
                },
                new Usuario
                {
                    Nome = "João Silva",
                    Email = "joao@email.com",
                    Senha = "123456",
                    Telefone = "(11) 88888-8888",
                    Role = "Cliente",
                    DataCriacao = DateTime.Now,
                    Ativo = true
                }
            };
            await context.Usuarios.AddRangeAsync(usuarios);
            await context.SaveChangesAsync();

            // 5. Criar produtos físicos
            var produtosFisicos = new[]
            {
                new ProdutoFisico
                {
                    Nome = "Smartphone Samsung Galaxy S21",
                    Descricao = "Smartphone Android 128GB, 8GB RAM",
                    Preco = 1899.99m,
                    SKU = "SM-GALAXY-S21",
                    LojaId = 1,
                    CategoriaId = 2,
                    Peso = 0.195m,
                    DataCriacao = DateTime.Now,
                    Ativo = true
                },
                new ProdutoFisico
                {
                    Nome = "Notebook Dell Inspiron",
                    Descricao = "Notebook 15.6\", Intel i5, 8GB RAM, 256GB SSD",
                    Preco = 2999.99m,
                    SKU = "DELL-INSPIRON-15",
                    LojaId = 1,
                    CategoriaId = 3,
                    Peso = 2.1m,
                    DataCriacao = DateTime.Now,
                    Ativo = true
                }
            };
            await context.ProdutosFisicos.AddRangeAsync(produtosFisicos);
            await context.SaveChangesAsync();

            // 6. Criar produtos digitais
            var produtosDigitais = new[]
            {
                new ProdutoDigital("E-book C# Avançado", "E-book completo sobre C# e .NET", 49.90m, "EBOOK-CSHARP-01", 1, 4,
                                  "https://download.loja.com/ebooks/csharp-avancado.pdf", 12.5m, "PDF")
                {
                    DataCriacao = DateTime.Now,
                    Ativo = true
                }
            };
            await context.ProdutosDigitais.AddRangeAsync(produtosDigitais);
            await context.SaveChangesAsync();

            // 7. Criar estoques (apenas para produtos físicos)
            var estoques = new[]
            {
                new Estoque
                {
                    ProdutoFisicoId = 1,
                    QuantidadeDisponivel = 50,
                    QuantidadeReservada = 0,
                    PontoRepor = 10,
                    EstoqueMinimo = 5,
                    UltimoMovimento = DateTime.Now
                },
                new Estoque
                {
                    ProdutoFisicoId = 2,
                    QuantidadeDisponivel = 30,
                    QuantidadeReservada = 0,
                    PontoRepor = 5,
                    EstoqueMinimo = 3,
                    UltimoMovimento = DateTime.Now
                }
            };
            await context.Estoques.AddRangeAsync(estoques);
            await context.SaveChangesAsync();

            // 8. Criar carrinho de exemplo
            var carrinho = new Carrinho
            {
                ClienteId = 2, // João Silva
                DataCriacao = DateTime.Now
            };
            await context.Carrinhos.AddAsync(carrinho);
            await context.SaveChangesAsync();

            // 9. Adicionar itens ao carrinho
            var itensCarrinho = new[]
            {
                new CarrinhoItem(1, 1, null, 1, 1899.99m) // CarrinhoId, ProdutoFisicoId, ProdutoDigitalId, Quantidade, Preco
                {
                    // DataAdicao é definida automaticamente no construtor
                }
            };
            await context.CarrinhoItens.AddRangeAsync(itensCarrinho);

            // 10. Criar transação de exemplo
            var transacoes = new[]
            {
                new Transacao
                {
                    CaixaId = 1,
                    Tipo = "Entrada",
                    Categoria = "Venda",
                    Valor = 1899.99m,
                    Descricao = "Venda de smartphone",
                    Data = DateTime.Now,
                    MetodoPagamento = "Cartão",
                    Observacao = "Venda inicial"
                }
            };
            await context.Transacoes.AddRangeAsync(transacoes);

            await context.SaveChangesAsync();

            Console.WriteLine("✅ Dados de exemplo inseridos com sucesso!");
            Console.WriteLine("📊 Resumo:");
            Console.WriteLine($"   - {categorias.Length} categorias");
            Console.WriteLine($"   - {lojas.Length} lojas");
            Console.WriteLine($"   - {caixas.Length} caixas");
            Console.WriteLine($"   - {usuarios.Length} usuários");
            Console.WriteLine($"   - {produtosFisicos.Length} produtos físicos");
            Console.WriteLine($"   - {produtosDigitais.Length} produtos digitais");
            Console.WriteLine($"   - {estoques.Length} estoques");
            Console.WriteLine($"   - {itensCarrinho.Length} itens no carrinho");
            Console.WriteLine($"   - {transacoes.Length} transações");
        }
        else
        {
            Console.WriteLine("✅ Banco de dados já contém dados. Inicialização ignorada.");
        }
    }
}