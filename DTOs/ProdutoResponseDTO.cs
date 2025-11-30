namespace ProjetoEcommerce.DTOs
{
    public class ProdutoResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal Preco { get; set; }
        public decimal? Peso { get; set; }
        public string SKU { get; set; }
        public string Categoria { get; set; }
        public int CategoriaId { get; set; }
        public string Loja { get; set; }
        public int LojaId { get; set; }
        public int Estoque { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
        public string Tipo { get; set; } // NOVA PROPRIEDADE
    }
}