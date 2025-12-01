namespace ProjetoEcommerce.DTOs
{
    public class AdicionarItemRequestDTO
    {
        public int ClienteId { get; set; }
        public int ProdutoId { get; set; }
        public string TipoProduto { get; set; } = string.Empty;
        public int Quantidade { get; set; }
    }

    public class CarrinhoItemResponseDTO
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public string TipoProduto { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class CarrinhoResponseDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public DateTime? AtualizadoEm { get; set; }
        public List<CarrinhoItemResponseDTO> Itens { get; set; } = new();
        public decimal Total { get; set; }
    }
}