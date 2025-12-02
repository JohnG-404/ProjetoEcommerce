namespace ProjetoEcommerce.Modelos
{
    public class ItemPedido
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int? ProdutoFisicoId { get; set; }
        public int? ProdutoDigitalId { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Desconto { get; set; }
        public string NomeProduto { get; set; }

        public virtual Pedido Pedido { get; set; }
        public virtual ProdutoFisico ProdutoFisico { get; set; }
        public virtual ProdutoDigital ProdutoDigital { get; set; }

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
            return Quantidade * (PrecoUnitario - Desconto);
        }
    }
}