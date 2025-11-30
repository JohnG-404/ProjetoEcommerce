namespace ProjetoEcommerce.DTOs
{
    public class ProdutoDTO
    {
        public string Tipo { get; set; } = "Fisico"; // "Fisico" ou "Digital"
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal Preco { get; set; }
        public decimal? Peso { get; set; }
        public string SKU { get; set; }
        public int LojaId { get; set; }
        public int CategoriaId { get; set; }
        public int QuantidadeEstoque { get; set; }
        public int PontoRepor { get; set; } = 0;

        // Propriedades específicas para produto digital
        public string UrlDownload { get; set; }
        public decimal TamanhoArquivoMB { get; set; }
        public string FormatoArquivo { get; set; }
    }
    public class ProdutoUpdateDTO
    {
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal Preco { get; set; }
        public decimal? Peso { get; set; }
        public string SKU { get; set; }
        public int CategoriaId { get; set; }
        public bool Ativo { get; set; }
    }
}