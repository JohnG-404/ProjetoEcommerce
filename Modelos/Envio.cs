namespace ProjetoEcommerce.Modelos
{
    public class Envio
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public string Transportadora { get; set; }
        public string TipoFrete { get; set; }
        public decimal ValorFrete { get; set; }
        public string Status { get; set; } = "Aguardando";
        public int? PrazoEntrega { get; set; }
        public string CodigoRastreamento { get; set; }
        public DateTime? DataEnvio { get; set; }
        public DateTime? DataPrevisaoEntrega { get; set; }

        // Propriedades de navegação
        public virtual Pedido Pedido { get; set; }
    }
}