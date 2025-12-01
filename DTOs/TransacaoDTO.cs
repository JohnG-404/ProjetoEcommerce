namespace ProjetoEcommerce.DTOs
{
    public class TransacaoRequestDTO
    {
        public string Tipo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string MetodoPagamento { get; set; } = string.Empty;
        public string Observacao { get; set; } = string.Empty;
    }

    public class TransacaoResponseDTO
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public string MetodoPagamento { get; set; } = string.Empty;
        public string Observacao { get; set; } = string.Empty;
    }
}