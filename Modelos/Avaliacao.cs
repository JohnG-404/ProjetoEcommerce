namespace ProjetoEcommerce.Modelos
{
    public class Avaliacao
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public int ClienteId { get; set; }
        public int Nota { get; set; }
        public string Comentario { get; set; }
        public DateTime Data { get; set; }

        // Propriedades de navegação
        public virtual Produto Produto { get; set; }
        public virtual Usuario Cliente { get; set; }
    }
}