namespace ProjetoEcommerce.DTOs
{
    public class LojaDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string CNPJ { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
    }

    public class LojaResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string CNPJ { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
        public EnderecoResponseDTO? Endereco { get; set; }
    }
}