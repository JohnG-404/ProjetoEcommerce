namespace ProjetoEcommerce.Modelos { 

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public int? ParentId { get; set; }

    // Propriedades de navegação
    public virtual Categoria Parent { get; set; }
    public virtual ICollection<Categoria> Subcategorias { get; set; } = new List<Categoria>();
    public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();
    }
}