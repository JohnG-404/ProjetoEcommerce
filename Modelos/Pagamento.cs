namespace ProjetoEcommerce.Modelos
{

    public class Pagamento
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public string Tipo { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; } = "Pendente";
        public string Referencia { get; set; }

        // Propriedade de navegação
        public virtual Pedido Pedido { get; set; }
    }
}