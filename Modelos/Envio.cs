namespace ProjetoEcommerce.Modelos
{

    public class Envio
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public string Tipo { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; } = "Aguardando";
        public string Rastreamento { get; set; }
        public int? TempoEstimadoDias { get; set; }

        // Propriedade de navegação
        public virtual Pedido Pedido { get; set; }
    }
}