namespace ProjetoEcommerce.DTOs
{
    public class CriarPedidoDTO
    {
        public int ClienteId { get; set; }
        public int EnderecoEntregaId { get; set; }
        public string MetodoPagamento { get; set; } = string.Empty;
        public string Observacao { get; set; } = string.Empty;
    }

    public class PedidoResponseDTO
    {
        public int Id { get; set; }
        public string NumeroPedido { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public int LojaId { get; set; }
        public string LojaNome { get; set; } = string.Empty;
        public EnderecoResponseDTO EnderecoEntrega { get; set; } = new();
        public decimal ValorTotal { get; set; }
        public decimal ValorFrete { get; set; }
        public decimal ValorDesconto { get; set; }
        public decimal ValorFinal { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public List<ItemPedidoResponseDTO> Itens { get; set; } = new();
        public PagamentoResponseDTO? Pagamento { get; set; }
        public EnvioResponseDTO? Envio { get; set; }
    }

    public class ItemPedidoResponseDTO
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public string TipoProduto { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class PagamentoResponseDTO
    {
        public string Metodo { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
    }

    public class EnvioResponseDTO
    {
        public string Transportadora { get; set; } = string.Empty;
        public string TipoFrete { get; set; } = string.Empty;
        public decimal ValorFrete { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? PrazoEntrega { get; set; }
        public string CodigoRastreamento { get; set; } = string.Empty;
    }

    public class AtualizarPagamentoDTO
    {
        public string Status { get; set; } = string.Empty;
    }
}