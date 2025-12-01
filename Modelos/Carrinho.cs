namespace ProjetoEcommerce.Modelos
{
    public class Carrinho
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string SessaoId { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? AtualizadoEm { get; set; }
        public DateTime? Expiracao { get; set; } // 🔥 ADICIONADO

        // Propriedades de navegação
        public virtual Usuario Cliente { get; set; }
        public virtual ICollection<CarrinhoItem> Itens { get; set; } = new List<CarrinhoItem>();
    }
}