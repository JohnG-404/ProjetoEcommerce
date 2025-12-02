namespace ProjetoEcommerce.Modelos
{
    public class CarrinhoItem
    {
        public int Id { get; set; }
        public int CarrinhoId { get; set; }
        public int? ProdutoFisicoId { get; set; }
        public int? ProdutoDigitalId { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public DateTime DataAdicao { get; set; }

        public virtual Carrinho Carrinho { get; set; }
        public virtual ProdutoFisico ProdutoFisico { get; set; }
        public virtual ProdutoDigital ProdutoDigital { get; set; }

        public CarrinhoItem()
        {
            DataAdicao = DateTime.Now;
        }

        public CarrinhoItem(int carrinhoId, int? produtoFisicoId, int? produtoDigitalId, int quantidade, decimal precoUnitario)
            : this()
        {
            CarrinhoId = carrinhoId;
            ProdutoFisicoId = produtoFisicoId;
            ProdutoDigitalId = produtoDigitalId;
            Quantidade = quantidade;
            PrecoUnitario = precoUnitario;
        }

        public ProdutoBase ObterProduto()
        {
            return (ProdutoBase)ProdutoFisico ?? ProdutoDigital;
        }

        public string ObterTipoProduto()
        {
            if (ProdutoFisicoId.HasValue) return "Físico";
            if (ProdutoDigitalId.HasValue) return "Digital";
            return "Desconhecido";
        }

        public decimal CalcularSubtotal()
        {
            return Quantidade * PrecoUnitario;
        }
    }
}