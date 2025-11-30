namespace ProjetoEcommerce.DTOs
{
    public class CategoriaDTO
    {
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public int? ParentId { get; set; }
    }

    public class CategoriaResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public int? ParentId { get; set; }
        public List<CategoriaResponseDTO> Subcategorias { get; set; } = new();
    }
}