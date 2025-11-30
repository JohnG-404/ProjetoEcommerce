namespace ProjetoEcommerce.DTOs
{
    public class PedidoResponseDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; }
        public int LojaId { get; set; }
        public string LojaNome { get; set; }
        public decimal ValorTotal { get; set; }
        public string Status { get; set; }
        public DateTime DataCriacao { get; set; }
        public List<ItemPedidoResponseDTO> Itens { get; set; } = new();
        public PagamentoResponseDTO Pagamento { get; set; }
        public EnvioResponseDTO Envio { get; set; }
    }

    public class ItemPedidoResponseDTO
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class PagamentoResponseDTO
    {
        public string Tipo { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; }
    }

    public class EnvioResponseDTO
    {
        public string Tipo { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; }
        public string Rastreamento { get; set; }
    }

    public class CriarPedidoDTO
    {
        public int ClienteId { get; set; }
        public int EnderecoEntregaId { get; set; }
        public string MetodoPagamento { get; set; }
    }
}