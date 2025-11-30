namespace ProjetoEcommerce.Modelos
{

    public class Pedido
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int LojaId { get; set; }
        public int EnderecoEntregaId { get; set; }
        public decimal ValorTotal { get; set; }
        public string Status { get; set; } = "Pendente";
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        // Propriedades de navegação
        public virtual Usuario Cliente { get; set; }
        public virtual Loja Loja { get; set; }
        public virtual Endereco EnderecoEntrega { get; set; }
        public virtual ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
        public virtual Pagamento Pagamento { get; set; }
        public virtual Envio Envio { get; set; }
    }
}