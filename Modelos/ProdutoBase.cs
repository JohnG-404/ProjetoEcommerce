using System.ComponentModel.DataAnnotations;

namespace ProjetoEcommerce.Modelos
{
    public abstract class ProdutoBase
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public string SKU { get; set; } = string.Empty;
        public int LojaId { get; set; }
        public int CategoriaId { get; set; }
        public bool Ativo { get; set; } = true;
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        public virtual Loja? Loja { get; set; }
        public virtual Categoria? Categoria { get; set; }

        public ProdutoBase()
        {
            DataCriacao = DateTime.Now;
        }

        public ProdutoBase(string nome, string descricao, decimal preco, string sku, int lojaId, int categoriaId)
        {
            Nome = nome ?? throw new ArgumentNullException(nameof(nome));
            Descricao = descricao ?? string.Empty;
            Preco = preco >= 0 ? preco : throw new ArgumentException("Preço não pode ser negativo");
            SKU = sku ?? throw new ArgumentNullException(nameof(sku));
            LojaId = lojaId > 0 ? lojaId : throw new ArgumentException("LojaId inválido");
            CategoriaId = categoriaId > 0 ? categoriaId : throw new ArgumentException("CategoriaId inválido");
            DataCriacao = DateTime.Now;
            Ativo = true;
        }

        public abstract string ObterTipoProduto();

        public virtual bool Validar()
        {
            return !string.IsNullOrWhiteSpace(Nome) &&
                   !string.IsNullOrWhiteSpace(SKU) &&
                   Preco >= 0 &&
                   LojaId > 0 &&
                   CategoriaId > 0;
        }
        public void Atualizar()
        {
            DataAtualizacao = DateTime.Now;
        }

        public virtual decimal CalcularPrecoComImposto()
        {
            return Preco * 1.1m;
        }
    }
}