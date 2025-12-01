namespace ProjetoEcommerce.Modelos
{
    public class Loja
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string CNPJ { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public int? EnderecoId { get; set; }
        public bool Ativo { get; set; } = true;
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        // 🔥 ATUALIZADO - Agora referencia ProdutoBase
        public virtual Endereco Endereco { get; set; }
        public virtual Caixa Caixa { get; set; }
        public virtual ICollection<ProdutoBase> ProdutosBase { get; set; } = new List<ProdutoBase>();
        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}