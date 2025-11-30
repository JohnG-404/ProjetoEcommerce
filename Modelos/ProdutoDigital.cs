using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEcommerce.Modelos
{
    [Table("ProdutosDigitais")]
    public class ProdutoDigital
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public string UrlDownload { get; set; }
        public decimal TamanhoArquivoMB { get; set; }
        public string FormatoArquivo { get; set; }
        public int LimiteDownloads { get; set; } = 3;
        public DateTime? DataExpiracao { get; set; }

        // Propriedade de navegação
        public virtual Produto Produto { get; set; }

        // CONSTRUTOR CORRIGIDO - sem parâmetros para o EF
        public ProdutoDigital()
        {
        }

        // Construtor com parâmetros
        public ProdutoDigital(int produtoId, string urlDownload, decimal tamanhoArquivoMB, string formatoArquivo)
        {
            ProdutoId = produtoId;
            UrlDownload = urlDownload;
            TamanhoArquivoMB = tamanhoArquivoMB;
            FormatoArquivo = formatoArquivo;
        }

        public bool LinkValido()
        {
            return DataExpiracao == null || DataExpiracao > DateTime.Now;
        }

        public bool PodeFazerDownload(int downloadsRealizados)
        {
            return downloadsRealizados < LimiteDownloads && LinkValido();
        }

        // POLIMORFISMO - Método específico da classe derivada
        public string GerarLinkDownload(int usuarioId)
        {
            return $"{UrlDownload}?token={Guid.NewGuid()}&user={usuarioId}&expires={DateTime.Now.AddDays(1):yyyyMMdd}";
        }
    }
}