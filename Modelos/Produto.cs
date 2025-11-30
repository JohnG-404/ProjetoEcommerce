using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEcommerce.Modelos
{
    public class Produto
    {
        public int Id { get; set; }
        public int LojaId { get; set; }
        public int CategoriaId { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal Preco { get; set; }
        public decimal? Peso { get; set; }
        public string SKU { get; set; }

        // NOVA PROPRIEDADE
        public string Tipo { get; set; } = "Fisico"; // "Fisico" ou "Digital"

        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        // Propriedades de navegação
        public virtual Loja Loja { get; set; }
        public virtual Categoria Categoria { get; set; }
        public virtual Estoque Estoque { get; set; }

        // CONSTRUTOR SIMPLIFICADO
        public Produto()
        {
            // Construtor vazio para o EF
        }

        public Produto(string nome, string descricao, decimal preco, string sku, int lojaId, int categoriaId)
        {
            Nome = nome;
            Descricao = descricao;
            Preco = preco;
            SKU = sku;
            LojaId = lojaId;
            CategoriaId = categoriaId;
            Ativo = true;
            DataCriacao = DateTime.Now;
            Tipo = "Fisico";
        }

        // MÉTODOS COM ENCAPSULAMENTO
        public void AlterarPreco(decimal novoPreco)
        {
            if (novoPreco < 0)
                throw new ArgumentException("Preço não pode ser negativo");

            Preco = novoPreco;
            DataAtualizacao = DateTime.Now;
        }

        public void AlterarNome(string novoNome)
        {
            if (string.IsNullOrWhiteSpace(novoNome))
                throw new ArgumentException("Nome não pode ser vazio");

            Nome = novoNome.Trim();
            DataAtualizacao = DateTime.Now;
        }

        // POLIMORFISMO - Método virtual que pode ser sobrescrito
        public virtual bool Validar()
        {
            return !string.IsNullOrWhiteSpace(Nome) &&
                   !string.IsNullOrWhiteSpace(SKU) &&
                   Preco >= 0 &&
                   LojaId > 0 &&
                   CategoriaId > 0;
        }

        // SOBRECARGA DE MÉTODOS
        public bool TemEstoqueSuficiente()
        {
            return Estoque?.QuantidadeDisponivel > 0;
        }

        public bool TemEstoqueSuficiente(int quantidadeRequerida)
        {
            return Estoque?.QuantidadeDisponivel >= quantidadeRequerida;
        }

        public void DefinirComoDigital()
        {
            Tipo = "Digital";
            DataAtualizacao = DateTime.Now;
        }

        public void DefinirComoFisico()
        {
            Tipo = "Fisico";
            DataAtualizacao = DateTime.Now;
        }
    }
}