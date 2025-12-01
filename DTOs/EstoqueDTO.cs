namespace ProjetoEcommerce.DTOs
{
    public class EstoqueDTO
    {
        public int ProdutoFisicoId { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public int PontoRepor { get; set; }
        public int EstoqueMinimo { get; set; }
    }

    public class EstoqueResponseDTO
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public string LojaNome { get; set; } = string.Empty;
        public int QuantidadeDisponivel { get; set; }
        public int QuantidadeReservada { get; set; }
        public int EstoqueReal { get; set; }
        public int PontoRepor { get; set; }
        public int EstoqueMinimo { get; set; }
        public DateTime? UltimoMovimento { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool PrecisaRepor { get; set; }
    }

    public class EstoqueAlertaDTO
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public string LojaNome { get; set; } = string.Empty;
        public int QuantidadeDisponivel { get; set; }
        public int QuantidadeReservada { get; set; }
        public int EstoqueReal { get; set; }
        public int PontoRepor { get; set; }
        public int EstoqueMinimo { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? UltimoMovimento { get; set; }
    }

    public class AjusteEstoqueDTO
    {
        public string Tipo { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }
}