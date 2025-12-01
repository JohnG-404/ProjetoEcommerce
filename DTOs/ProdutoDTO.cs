namespace ProjetoEcommerce.DTOs
{
    public class ProdutoFisicoDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public decimal? Peso { get; set; }
        public string SKU { get; set; } = string.Empty;
        public int LojaId { get; set; }
        public int CategoriaId { get; set; }
        public int QuantidadeEstoque { get; set; }
        public int PontoRepor { get; set; } = 0;
        public decimal? Altura { get; set; }
        public decimal? Largura { get; set; }
        public decimal? Profundidade { get; set; }
    }

    public class ProdutoFisicoResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public decimal PrecoComImposto { get; set; }
        public decimal? Peso { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public string Loja { get; set; } = string.Empty;
        public int LojaId { get; set; }
        public string Dimensoes { get; set; } = string.Empty;
        public decimal? Volume { get; set; }
        public int Estoque { get; set; }
        public bool PodeEnviar { get; set; }
        public DateTime DataCriacao { get; set; }
    }

    public class ProdutoDigitalDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public string SKU { get; set; } = string.Empty;
        public int LojaId { get; set; }
        public int CategoriaId { get; set; }
        public string UrlDownload { get; set; } = string.Empty;
        public decimal TamanhoArquivoMB { get; set; }
        public string FormatoArquivo { get; set; } = string.Empty;
        public int LimiteDownloads { get; set; } = 3;
        public DateTime? DataExpiracao { get; set; }
    }

    public class ProdutoDigitalResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public decimal PrecoComImposto { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Loja { get; set; } = string.Empty;
        public string UrlDownload { get; set; } = string.Empty;
        public decimal TamanhoArquivoMB { get; set; }
        public string FormatoArquivo { get; set; } = string.Empty;
        public int LimiteDownloads { get; set; }
        public string ChaveLicenca { get; set; } = string.Empty;
        public DateTime? DataExpiracao { get; set; }
        public DateTime DataCriacao { get; set; }
        public bool LinkValido { get; set; }
        public string InfoDownload { get; set; } = string.Empty;
    }

    public class DimensoesDTO
    {
        public decimal Altura { get; set; }
        public decimal Largura { get; set; }
        public decimal Profundidade { get; set; }
    }
}