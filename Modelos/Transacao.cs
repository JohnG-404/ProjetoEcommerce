namespace ProjetoEcommerce.Modelos
{
    public class Transacao
    {
        public int Id { get; set; }
        public int CaixaId { get; set; }
        public string Tipo { get; set; } // "Entrada" ou "Saida"
        public string Categoria { get; set; }
        public decimal Valor { get; set; }
        public string Descricao { get; set; }
        public DateTime Data { get; set; }
        public int? PedidoId { get; set; }
        public string MetodoPagamento { get; set; }
        public string Observacao { get; set; }

        // Propriedades de navegação
        public virtual Caixa Caixa { get; set; }
        public virtual Pedido Pedido { get; set; }

        public Transacao()
        {
            Data = DateTime.Now;
        }
    }
}