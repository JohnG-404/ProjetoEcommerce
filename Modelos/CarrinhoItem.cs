namespace ProjetoEcommerce.Modelos
{
    public class CarrinhoItem
    {
        public int Id { get; set; }
        public int CarrinhoId { get; set; }
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public DateTime DataAdicao { get; set; } // NOVA PROPRIEDADE

        // Propriedades de navegação
        public virtual Carrinho Carrinho { get; set; }
        public virtual Produto Produto { get; set; }

        // ENCAPSULAMENTO - Construtor
        public CarrinhoItem()
        {
            DataAdicao = DateTime.Now;
        }

        public CarrinhoItem(int carrinhoId, int produtoId, int quantidade, decimal precoUnitario) : this()
        {
            CarrinhoId = carrinhoId;
            ProdutoId = produtoId;
            Quantidade = quantidade;
            PrecoUnitario = precoUnitario;
        }

        // MÉTODOS COM ENCAPSULAMENTO
        public void AtualizarQuantidade(int novaQuantidade)
        {
            if (novaQuantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero");

            Quantidade = novaQuantidade;
        }

        public void AumentarQuantidade(int quantidade = 1)
        {
            if (quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero");

            Quantidade += quantidade;
        }

        public void DiminuirQuantidade(int quantidade = 1)
        {
            if (quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero");

            if (Quantidade - quantidade < 1)
                throw new InvalidOperationException("Quantidade não pode ser menor que 1");

            Quantidade -= quantidade;
        }

        // POLIMORFISMO - Método virtual
        public virtual decimal CalcularSubtotal()
        {
            return Quantidade * PrecoUnitario;
        }

        // SOBRECARGA DE MÉTODOS
        public decimal CalcularSubtotal(bool aplicarDesconto)
        {
            var subtotal = CalcularSubtotal();
            return aplicarDesconto ? subtotal * 0.9m : subtotal; // 10% de desconto exemplo
        }

        public decimal CalcularSubtotal(decimal percentualDesconto)
        {
            var subtotal = CalcularSubtotal();
            return subtotal * (1 - percentualDesconto / 100);
        }

        public bool TemEstoqueDisponivel()
        {
            return Produto?.Estoque?.TemEstoqueSuficiente(Quantidade) ?? false;
        }

        public bool ItemEhValido()
        {
            return Produto != null &&
                   Produto.Ativo &&
                   Quantidade > 0 &&
                   PrecoUnitario >= 0 &&
                   TemEstoqueDisponivel();
        }

        // Método para verificar se o item foi adicionado recentemente
        public bool FoiAdicionadoRecentemente(int minutos = 30)
        {
            return DataAdicao > DateTime.Now.AddMinutes(-minutos);
        }
    }
}