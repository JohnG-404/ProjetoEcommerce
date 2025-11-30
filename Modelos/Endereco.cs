namespace ProjetoEcommerce.Modelos
{

    public class Endereco
    {
        public int Id { get; set; }
        public int? UsuarioId { get; set; }
        public int? LojaId { get; set; }
        public string Rua { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string CEP { get; set; }
        public string Pais { get; set; } = "BR";

        // Propriedades de navegação
        public virtual Usuario Usuario { get; set; }
        public virtual Loja Loja { get; set; }
        public virtual ICollection<Pedido> PedidosEntrega { get; set; }
    }
}