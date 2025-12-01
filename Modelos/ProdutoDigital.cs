using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEcommerce.Modelos
{
    [Table("ProdutosDigitais")]
    public class ProdutoDigital : ProdutoBase
    {
        public string? UrlDownload { get; set; }
        public decimal? TamanhoArquivoMB { get; set; }
        public string? FormatoArquivo { get; set; }
        public int LimiteDownloads { get; set; } = 3;
        public DateTime? DataExpiracao { get; set; }
        public string? ChaveLicenca { get; set; }

        // Construtores
        public ProdutoDigital() { }

        public ProdutoDigital(string nome, string descricao, decimal preco, string sku, int lojaId, int categoriaId)
            : base(nome, descricao, preco, sku, lojaId, categoriaId)
        {
            ChaveLicenca = GerarChaveLicenca();
        }

        public ProdutoDigital(string nome, string descricao, decimal preco, string sku, int lojaId, int categoriaId,
                            string urlDownload, decimal tamanhoArquivoMB, string formatoArquivo)
            : base(nome, descricao, preco, sku, lojaId, categoriaId)
        {
            UrlDownload = urlDownload ?? throw new ArgumentNullException(nameof(urlDownload));
            TamanhoArquivoMB = tamanhoArquivoMB > 0 ? tamanhoArquivoMB : throw new ArgumentException("Tamanho do arquivo deve ser maior que zero");
            FormatoArquivo = formatoArquivo ?? throw new ArgumentNullException(nameof(formatoArquivo));
            ChaveLicenca = GerarChaveLicenca();
        }

        public override string ObterTipoProduto() => "Digital";

        public override bool Validar()
        {
            return base.Validar() &&
                   !string.IsNullOrWhiteSpace(UrlDownload) &&
                   TamanhoArquivoMB > 0 &&
                   !string.IsNullOrWhiteSpace(FormatoArquivo) &&
                   LimiteDownloads > 0;
        }

        public override decimal CalcularPrecoComImposto()
        {
            var precoBase = base.CalcularPrecoComImposto();
            return precoBase * 0.98m; // 2% de desconto para digitais
        }

        public bool LinkValido()
        {
            return DataExpiracao == null || DataExpiracao > DateTime.Now;
        }

        public bool PodeFazerDownload(int downloadsRealizados)
        {
            return downloadsRealizados < LimiteDownloads && LinkValido();
        }

        public string? GerarLinkDownload(int usuarioId)
        {
            if (!LinkValido() || string.IsNullOrEmpty(UrlDownload))
                return null;

            return $"{UrlDownload}?token={Guid.NewGuid()}&user={usuarioId}&licenca={ChaveLicenca}";
        }

        private string GerarChaveLicenca()
        {
            return $"LIC-{Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper()}";
        }

        public void RenovarLicenca(int dias = 365)
        {
            DataExpiracao = DateTime.Now.AddDays(dias);
            ChaveLicenca = GerarChaveLicenca();
            Atualizar(); // 🔥 CORRIGIDO: Método existe agora na classe base
        }

        public void AumentarLimiteDownloads(int novoLimite)
        {
            if (novoLimite <= LimiteDownloads)
                throw new ArgumentException("Novo limite deve ser maior que o atual");

            LimiteDownloads = novoLimite;
            Atualizar(); // 🔥 CORRIGIDO: Método existe agora na classe base
        }

        public string ObterInformacoesDownload()
        {
            return $"Arquivo: {FormatoArquivo ?? "N/A"} | Tamanho: {TamanhoArquivoMB?.ToString("F2") ?? "N/A"}MB";
        }

        public string ObterInformacoesDownload(bool incluirLimite)
        {
            var info = ObterInformacoesDownload();
            return incluirLimite ? $"{info} | Downloads: {LimiteDownloads}" : info;
        }
    }
}