namespace ProjetoEcommerce.Modelos
{
    public class Loja
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string CNPJ { get; set; }
        public string Telefone { get; set; }
        public int? EnderecoId { get; set; }
        // REMOVER: public int? CaixaId { get; set; } // Esta linha não deve existir
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }

        // Propriedades de navegação
        public virtual Endereco Endereco { get; set; }
        public virtual Caixa Caixa { get; set; } // Relacionamento 1:1, mas sem CaixaId na Loja
        public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();
        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}