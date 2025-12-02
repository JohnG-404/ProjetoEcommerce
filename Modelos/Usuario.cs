namespace ProjetoEcommerce.Modelos
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string Telefone { get; set; }
        public string Role { get; set; } = "Cliente";
        public DateTime DataCriacao { get; set; }
        public bool Ativo { get; set; } = true;

        public virtual Endereco Endereco { get; set; }
        public virtual ICollection<Carrinho> Carrinhos { get; set; } = new List<Carrinho>();
        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    }
}