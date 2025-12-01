namespace ProjetoEcommerce.Modelos
{
    public class Pagamento
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public string Metodo { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; } = "Pendente";
        public string Referencia { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        // Propriedades de navegação
        public virtual Pedido Pedido { get; set; }

        public Pagamento()
        {
            DataCriacao = DateTime.Now;
        }
    }
}