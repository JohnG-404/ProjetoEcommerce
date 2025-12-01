using Microsoft.EntityFrameworkCore;
using ProjetoEcommerce.Modelos;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // 🔥 DBSETS COM HERANÇA TPT
    public DbSet<ProdutoBase> ProdutosBase { get; set; }
    public DbSet<ProdutoFisico> ProdutosFisicos { get; set; }
    public DbSet<ProdutoDigital> ProdutosDigitais { get; set; }

    // DBSETS EXISTENTES
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Loja> Lojas { get; set; }
    public DbSet<Endereco> Enderecos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Estoque> Estoques { get; set; }
    public DbSet<Carrinho> Carrinhos { get; set; }
    public DbSet<CarrinhoItem> CarrinhoItens { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<ItemPedido> ItensPedido { get; set; }
    public DbSet<Pagamento> Pagamentos { get; set; }
    public DbSet<Envio> Envios { get; set; }
    public DbSet<Caixa> Caixas { get; set; }
    public DbSet<Transacao> Transacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 🔥 CONFIGURAÇÃO CORRETA DE HERANÇA TPT
        modelBuilder.Entity<ProdutoBase>()
            .UseTptMappingStrategy();

        // 🔥 CONFIGURAÇÕES DE CHAVES ÚNICAS
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Loja>()
            .HasIndex(l => l.CNPJ)
            .IsUnique();

        modelBuilder.Entity<ProdutoBase>()
            .HasIndex(p => p.SKU)
            .IsUnique();

        // 🔥 RELACIONAMENTOS COM HERANÇA

        // ProdutoFisico -> Estoque (1:1)
        modelBuilder.Entity<ProdutoFisico>()
            .HasOne(pf => pf.Estoque)
            .WithOne(e => e.ProdutoFisico)
            .HasForeignKey<Estoque>(e => e.ProdutoFisicoId)
            .OnDelete(DeleteBehavior.Cascade);

        // ProdutoBase -> Loja (M:1)
        modelBuilder.Entity<ProdutoBase>()
            .HasOne(p => p.Loja)
            .WithMany(l => l.ProdutosBase)
            .HasForeignKey(p => p.LojaId)
            .OnDelete(DeleteBehavior.Restrict);

        // ProdutoBase -> Categoria (M:1)
        modelBuilder.Entity<ProdutoBase>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.ProdutosBase)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // 🔥 RELACIONAMENTOS DE CARRINHO ITEM (com herança)
        modelBuilder.Entity<CarrinhoItem>()
            .HasOne(ci => ci.ProdutoFisico)
            .WithMany()
            .HasForeignKey(ci => ci.ProdutoFisicoId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        modelBuilder.Entity<CarrinhoItem>()
            .HasOne(ci => ci.ProdutoDigital)
            .WithMany()
            .HasForeignKey(ci => ci.ProdutoDigitalId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // Validação: Um item deve referenciar OU produto físico OU digital
        modelBuilder.Entity<CarrinhoItem>()
            .HasCheckConstraint("CK_CarrinhoItem_TipoProduto",
                "([ProdutoFisicoId] IS NOT NULL AND [ProdutoDigitalId] IS NULL) OR ([ProdutoFisicoId] IS NULL AND [ProdutoDigitalId] IS NOT NULL)");

        // 🔥 RELACIONAMENTOS DE PEDIDO (com herança)
        modelBuilder.Entity<ItemPedido>()
            .HasOne(ip => ip.ProdutoFisico)
            .WithMany()
            .HasForeignKey(ip => ip.ProdutoFisicoId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        modelBuilder.Entity<ItemPedido>()
            .HasOne(ip => ip.ProdutoDigital)
            .WithMany()
            .HasForeignKey(ip => ip.ProdutoDigitalId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Check constraint para ItensPedido
        modelBuilder.Entity<ItemPedido>()
            .HasCheckConstraint("CK_ItensPedido_TipoProduto",
                "([ProdutoFisicoId] IS NOT NULL AND [ProdutoDigitalId] IS NULL) OR ([ProdutoFisicoId] IS NULL AND [ProdutoDigitalId] IS NOT NULL)");

        // 🔥 RELACIONAMENTOS EXISTENTES

        // Loja -> Endereco (1:1)
        modelBuilder.Entity<Loja>()
            .HasOne(l => l.Endereco)
            .WithOne(e => e.Loja)
            .HasForeignKey<Endereco>(e => e.LojaId)
            .IsRequired(false);

        // Usuario -> Endereco (1:1)
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Endereco)
            .WithOne(e => e.Usuario)
            .HasForeignKey<Endereco>(e => e.UsuarioId)
            .IsRequired(false);

        // Loja -> Caixa (1:1)
        modelBuilder.Entity<Loja>()
            .HasOne(l => l.Caixa)
            .WithOne(c => c.Loja)
            .HasForeignKey<Caixa>(c => c.LojaId);

        // Carrinho -> Usuario (M:1)
        modelBuilder.Entity<Carrinho>()
            .HasOne(c => c.Cliente)
            .WithMany(u => u.Carrinhos)
            .HasForeignKey(c => c.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Carrinho -> CarrinhoItem (1:M)
        modelBuilder.Entity<Carrinho>()
            .HasMany(c => c.Itens)
            .WithOne(ci => ci.Carrinho)
            .HasForeignKey(ci => ci.CarrinhoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Pedido -> Usuario (M:1)
        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Cliente)
            .WithMany(u => u.Pedidos)
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Pedido -> Loja (M:1)
        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Loja)
            .WithMany(l => l.Pedidos)
            .HasForeignKey(p => p.LojaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Pedido -> Endereco (M:1)
        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.EnderecoEntrega)
            .WithMany()
            .HasForeignKey(p => p.EnderecoEntregaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Pedido -> ItemPedido (1:M)
        modelBuilder.Entity<Pedido>()
            .HasMany(p => p.Itens)
            .WithOne(ip => ip.Pedido)
            .HasForeignKey(ip => ip.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Pedido -> Pagamento (1:1)
        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Pagamento)
            .WithOne(pg => pg.Pedido)
            .HasForeignKey<Pagamento>(pg => pg.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Pedido -> Envio (1:1)
        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Envio)
            .WithOne(e => e.Pedido)
            .HasForeignKey<Envio>(e => e.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Caixa -> Transacao (1:M)
        modelBuilder.Entity<Caixa>()
            .HasMany(c => c.Transacoes)
            .WithOne(t => t.Caixa)
            .HasForeignKey(t => t.CaixaId)
            .OnDelete(DeleteBehavior.Cascade);

        // 🔥 CONFIGURAÇÕES DE PROPRIEDADES

        // Configurar decimal para precisão
        modelBuilder.Entity<ProdutoBase>()
            .Property(p => p.Preco)
            .HasPrecision(15, 2);

        modelBuilder.Entity<ProdutoFisico>()
            .Property(p => p.Peso)
            .HasPrecision(10, 3);

        modelBuilder.Entity<ProdutoFisico>()
            .Property(p => p.Altura)
            .HasPrecision(8, 2);

        modelBuilder.Entity<ProdutoFisico>()
            .Property(p => p.Largura)
            .HasPrecision(8, 2);

        modelBuilder.Entity<ProdutoFisico>()
            .Property(p => p.Profundidade)
            .HasPrecision(8, 2);

        modelBuilder.Entity<ProdutoDigital>()
            .Property(p => p.TamanhoArquivoMB)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Estoque>()
            .Property(e => e.QuantidadeDisponivel)
            .HasDefaultValue(0);

        modelBuilder.Entity<Estoque>()
            .Property(e => e.QuantidadeReservada)
            .HasDefaultValue(0);

        modelBuilder.Entity<CarrinhoItem>()
            .Property(ci => ci.PrecoUnitario)
            .HasPrecision(15, 2);

        modelBuilder.Entity<Pedido>()
            .Property(p => p.ValorTotal)
            .HasPrecision(15, 2);

        modelBuilder.Entity<Pedido>()
            .Property(p => p.ValorFrete)
            .HasPrecision(15, 2);

        modelBuilder.Entity<Pedido>()
            .Property(p => p.ValorDesconto)
            .HasPrecision(15, 2);

        modelBuilder.Entity<ItemPedido>()
            .Property(ip => ip.PrecoUnitario)
            .HasPrecision(15, 2);

        modelBuilder.Entity<ItemPedido>()
            .Property(ip => ip.Desconto)
            .HasPrecision(15, 2);

        modelBuilder.Entity<Pagamento>()
            .Property(p => p.Valor)
            .HasPrecision(15, 2);

        modelBuilder.Entity<Envio>()
            .Property(e => e.ValorFrete)
            .HasPrecision(15, 2);

        modelBuilder.Entity<Caixa>()
            .Property(c => c.SaldoAtual)
            .HasPrecision(15, 2);

        modelBuilder.Entity<Caixa>()
            .Property(c => c.SaldoInicial)
            .HasPrecision(15, 2);

        modelBuilder.Entity<Transacao>()
            .Property(t => t.Valor)
            .HasPrecision(15, 2);

        // 🔥 CONFIGURAÇÕES DE VALORES PADRÃO

        modelBuilder.Entity<ProdutoBase>()
            .Property(p => p.Ativo)
            .HasDefaultValue(true);

        modelBuilder.Entity<ProdutoBase>()
            .Property(p => p.DataCriacao)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Usuario>()
            .Property(u => u.Ativo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Usuario>()
            .Property(u => u.Role)
            .HasDefaultValue("Cliente");

        modelBuilder.Entity<Loja>()
            .Property(l => l.Ativo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Categoria>()
            .Property(c => c.Ativo)
            .HasDefaultValue(true);

        modelBuilder.Entity<CarrinhoItem>()
            .Property(ci => ci.Quantidade)
            .HasDefaultValue(1);

        modelBuilder.Entity<CarrinhoItem>()
            .Property(ci => ci.DataAdicao)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Pedido>()
            .Property(p => p.Status)
            .HasDefaultValue("Pendente");

        modelBuilder.Entity<Pagamento>()
            .Property(p => p.Status)
            .HasDefaultValue("Pendente");

        modelBuilder.Entity<Envio>()
            .Property(e => e.Status)
            .HasDefaultValue("Aguardando");

        modelBuilder.Entity<Caixa>()
            .Property(c => c.Status)
            .HasDefaultValue("Aberto");

        modelBuilder.Entity<Caixa>()
            .Property(c => c.DataAbertura)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Transacao>()
            .Property(t => t.Data)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // 🔥 ÍNDICES PARA MELHOR PERFORMANCE

        modelBuilder.Entity<ProdutoBase>()
            .HasIndex(p => p.Ativo);

        modelBuilder.Entity<ProdutoBase>()
            .HasIndex(p => p.DataCriacao);

        modelBuilder.Entity<ProdutoFisico>()
            .HasIndex(p => p.Peso);

        modelBuilder.Entity<ProdutoDigital>()
            .HasIndex(p => p.FormatoArquivo);

        modelBuilder.Entity<ProdutoDigital>()
            .HasIndex(p => p.DataExpiracao);

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Ativo);

        modelBuilder.Entity<Loja>()
            .HasIndex(l => l.Ativo);

        modelBuilder.Entity<Carrinho>()
            .HasIndex(c => c.ClienteId);

        modelBuilder.Entity<Carrinho>()
            .HasIndex(c => c.Expiracao);

        modelBuilder.Entity<CarrinhoItem>()
            .HasIndex(ci => ci.ProdutoFisicoId);

        modelBuilder.Entity<CarrinhoItem>()
            .HasIndex(ci => ci.ProdutoDigitalId);

        modelBuilder.Entity<Pedido>()
            .HasIndex(p => p.ClienteId);

        modelBuilder.Entity<Pedido>()
            .HasIndex(p => p.Status);

        modelBuilder.Entity<Pedido>()
            .HasIndex(p => p.DataCriacao);

        modelBuilder.Entity<Pedido>()
            .HasIndex(p => p.NumeroPedido);

        modelBuilder.Entity<Estoque>()
            .HasIndex(e => e.QuantidadeDisponivel);

        modelBuilder.Entity<Estoque>()
            .HasIndex(e => e.QuantidadeReservada);

        modelBuilder.Entity<Caixa>()
            .HasIndex(c => c.Status);

        modelBuilder.Entity<Caixa>()
            .HasIndex(c => c.DataAbertura);

        modelBuilder.Entity<Transacao>()
            .HasIndex(t => t.Tipo);

        modelBuilder.Entity<Transacao>()
            .HasIndex(t => t.Data);

        modelBuilder.Entity<Transacao>()
            .HasIndex(t => t.CaixaId);

        // 🔥 CONFIGURAÇÃO DE DELETE BEHAVIOR GLOBAL
        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys())
            .Where(fk => !fk.IsOwnership &&
                        fk.DeleteBehavior == DeleteBehavior.Cascade))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        base.OnModelCreating(modelBuilder);
    }
}