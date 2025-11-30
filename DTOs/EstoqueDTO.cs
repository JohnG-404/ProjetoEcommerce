namespace ProjetoEcommerce.DTOs
{
    public class EstoqueDTO
    {
        public int ProdutoId { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public int PontoRepor { get; set; }
        public int EstoqueMinimo { get; set; }
    }

    public class EstoqueResponseDTO
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; }
        public string LojaNome { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public int QuantidadeReservada { get; set; }
        public int EstoqueReal { get; set; }
        public int PontoRepor { get; set; }
        public int EstoqueMinimo { get; set; }
        public DateTime? UltimoMovimento { get; set; }
        public string Status { get; set; }
        public bool PrecisaRepor { get; set; }
    }

    public class EstoqueAlertaDTO
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; }
        public string LojaNome { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public int QuantidadeReservada { get; set; }
        public int EstoqueReal { get; set; }
        public int PontoRepor { get; set; }
        public int EstoqueMinimo { get; set; }
        public string Status { get; set; }
        public DateTime? UltimoMovimento { get; set; }
    }

    public class AjusteEstoqueDTO
    {
        public string Tipo { get; set; } // "entrada", "saida", "reserva", "liberar"
        public int Quantidade { get; set; }
        public string Motivo { get; set; }
    }
}