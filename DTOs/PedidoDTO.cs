namespace ProjetoEcommerce.DTOs
{
    public class CriarPedidoDTO
    {
        public int ClienteId { get; set; }
        public int EnderecoEntregaId { get; set; }
        public string MetodoPagamento { get; set; }
        public string Observacao { get; set; }
    }

    public class PedidoResponseDTO
    {
        public int Id { get; set; }
        public string NumeroPedido { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; }
        public int LojaId { get; set; }
        public string LojaNome { get; set; }
        public EnderecoResponseDTO EnderecoEntrega { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal ValorFrete { get; set; }
        public decimal ValorDesconto { get; set; }
        public decimal ValorFinal { get; set; }
        public string Status { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public List<ItemPedidoResponseDTO> Itens { get; set; } = new();
        public PagamentoResponseDTO Pagamento { get; set; }
        public EnvioResponseDTO Envio { get; set; }
    }

    public class ItemPedidoResponseDTO
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; }
        public string TipoProduto { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class PagamentoResponseDTO
    {
        public string Metodo { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; }
        public string Referencia { get; set; }
    }

    public class EnvioResponseDTO
    {
        public string Transportadora { get; set; }
        public string TipoFrete { get; set; }
        public decimal ValorFrete { get; set; }
        public string Status { get; set; }
        public int? PrazoEntrega { get; set; }
        public string CodigoRastreamento { get; set; }
    }
    
        public class EnderecoResponseDTO
        {
            public int Id { get; set; } // ADICIONADO
            public string Rua { get; set; }
            public string Numero { get; set; }
            public string Complemento { get; set; }
            public string Bairro { get; set; }
            public string Cidade { get; set; }
            public string Estado { get; set; }
            public string CEP { get; set; }
            public string Pais { get; set; } // ADICIONADO
        }
    
}