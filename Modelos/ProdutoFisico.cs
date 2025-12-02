using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEcommerce.Modelos
{
    [Table("ProdutosFisicos")]
    public class ProdutoFisico : ProdutoBase
    {
        public decimal? Peso { get; set; }
        public decimal? Altura { get; set; }
        public decimal? Largura { get; set; }
        public decimal? Profundidade { get; set; }

        public virtual Estoque? Estoque { get; set; }

        // Construtores
        public ProdutoFisico() { }

        public ProdutoFisico(string nome, string descricao, decimal preco, string sku, int lojaId, int categoriaId)
            : base(nome, descricao, preco, sku, lojaId, categoriaId)
        {
        }

        public ProdutoFisico(string nome, string descricao, decimal preco, string sku, int lojaId, int categoriaId,
                           decimal? peso, decimal? altura, decimal? largura, decimal? profundidade)
            : base(nome, descricao, preco, sku, lojaId, categoriaId)
        {
            Peso = peso;
            Altura = altura;
            Largura = largura;
            Profundidade = profundidade;
        }

        public override string ObterTipoProduto() => "Físico";

        public void DefinirDimensoes(decimal altura, decimal largura, decimal profundidade)
        {
            if (altura <= 0 || largura <= 0 || profundidade <= 0)
                throw new ArgumentException("Dimensões devem ser maiores que zero");

            Altura = altura;
            Largura = largura;
            Profundidade = profundidade;
            Atualizar();
        }

        public string ObterDimensoes()
        {
            if (Altura.HasValue && Largura.HasValue && Profundidade.HasValue)
                return $"{Altura} x {Largura} x {Profundidade} cm";
            return "Dimensões não informadas";
        }

        public decimal? CalcularVolume()
        {
            if (Altura.HasValue && Largura.HasValue && Profundidade.HasValue)
                return Altura.Value * Largura.Value * Profundidade.Value;
            return null;
        }

        public bool PodeSerEnviado() => Peso.HasValue && Peso > 0;
    }
}