namespace ProjetoEcommerce.DTOs
{
    public class LojaDTO
    {
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string CNPJ { get; set; }
        public string Telefone { get; set; }
    }

    public class LojaResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string CNPJ { get; set; }
        public string Telefone { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
        public EnderecoResponseDTO Endereco { get; set; }
    }
}