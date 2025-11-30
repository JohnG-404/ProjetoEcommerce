namespace ProjetoEcommerce.Modelos
{
    public abstract class ProdutoBase : EntidadeBase
    {
        public string Nome { get; protected set; }
        public string Descricao { get; protected set; }
        public decimal Preco { get; protected set; }
        public decimal? Peso { get; protected set; }
        public string SKU { get; protected set; }

        protected ProdutoBase(string nome, string descricao, decimal preco, string sku)
        {
            Nome = nome ?? throw new ArgumentNullException(nameof(nome));
            Descricao = descricao ?? throw new ArgumentNullException(nameof(descricao));
            Preco = preco >= 0 ? preco : throw new ArgumentException("Preço não pode ser negativo");
            SKU = sku ?? throw new ArgumentNullException(nameof(sku));
        }

        // ENCAPSULAMENTO - Métodos para modificar propriedades com validação
        public virtual void AlterarPreco(decimal novoPreco)
        {
            if (novoPreco < 0)
                throw new ArgumentException("Preço não pode ser negativo");

            Preco = novoPreco;
            Atualizar();
        }

        public virtual void AlterarNome(string novoNome)
        {
            if (string.IsNullOrWhiteSpace(novoNome))
                throw new ArgumentException("Nome não pode ser vazio");

            Nome = novoNome.Trim();
            Atualizar();
        }

        // POLIMORFISMO - Método abstrato implementado nas classes derivadas
        public override abstract bool Validar();
    }
}