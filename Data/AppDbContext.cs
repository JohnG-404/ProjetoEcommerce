using Microsoft.EntityFrameworkCore;
using ProjetoEcommerce.Modelos;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Loja> Lojas { get; set; }
    public DbSet<Endereco> Enderecos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Estoque> Estoques { get; set; }
    public DbSet<Carrinho> Carrinhos { get; set; }
    public DbSet<CarrinhoItem> CarrinhoItens { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<ItemPedido> ItensPedido { get; set; }
    public DbSet<Pagamento> Pagamentos { get; set; }
    public DbSet<Envio> Envios { get; set; }
    public DbSet<Caixa> Caixas { get; set; }
    public DbSet<Transacao> Transacoes { get; set; }
    public DbSet<Avaliacao> Avaliacoes { get; set; }
    public DbSet<ProdutoDigital> ProdutosDigitais { get; set; }
    public DbSet<Notificacao> Notificacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurações de chaves únicas
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Loja>()
            .HasIndex(l => l.CNPJ)
            .IsUnique();

        // RELACIONAMENTO CORRIGIDO: Loja -> Caixa (1:1)
        modelBuilder.Entity<Loja>()
            .HasOne(l => l.Caixa)
            .WithOne(c => c.Loja)
            .HasForeignKey<Caixa>(c => c.LojaId);

        // Relacionamento Loja -> Endereco
        modelBuilder.Entity<Loja>()
            .HasOne(l => l.Endereco)
            .WithOne(e => e.Loja)
            .HasForeignKey<Endereco>(e => e.LojaId)
            .IsRequired(false);

        // Relacionamento Usuario -> Endereco
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Endereco)
            .WithOne(e => e.Usuario)
            .HasForeignKey<Endereco>(e => e.UsuarioId)
            .IsRequired(false);

        // Relacionamento Produto -> Estoque
        modelBuilder.Entity<Produto>()
            .HasOne(p => p.Estoque)
            .WithOne(e => e.Produto)
            .HasForeignKey<Estoque>(e => e.ProdutoId);

        // Relacionamentos um-para-muitos
        modelBuilder.Entity<Produto>()
            .HasOne(p => p.Loja)
            .WithMany(l => l.Produtos)
            .HasForeignKey(p => p.LojaId);

        modelBuilder.Entity<Produto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Produtos)
            .HasForeignKey(p => p.CategoriaId);

        modelBuilder.Entity<Carrinho>()
            .HasMany(c => c.Itens)
            .WithOne(ci => ci.Carrinho)
            .HasForeignKey(ci => ci.CarrinhoId);

        modelBuilder.Entity<ProdutoDigital>()
            .HasOne(pd => pd.Produto)
            .WithOne() // Se quiser navegação bidirecional, ajuste conforme necessário
            .HasForeignKey<ProdutoDigital>(pd => pd.ProdutoId);

        // Configuração para Notificacao
        modelBuilder.Entity<Notificacao>()
            .HasOne(n => n.Usuario)
            .WithMany()
            .HasForeignKey(n => n.UsuarioId);

        modelBuilder.Entity<Notificacao>()
            .HasOne(n => n.Pedido)
            .WithMany()
            .HasForeignKey(n => n.PedidoId);

        modelBuilder.Entity<Produto>()
            .Property(p => p.Tipo)
            .HasConversion<string>()
            .HasMaxLength(10);

        modelBuilder.Entity<Produto>()
            .Property(p => p.Tipo)
            .HasMaxLength(10)
            .HasDefaultValue("Fisico"); 

        // Configurar delete behavior
        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        base.OnModelCreating(modelBuilder);
    }
}