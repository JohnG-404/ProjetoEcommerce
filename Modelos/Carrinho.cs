

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ProjetoEcommerce.Modelos
{

    public class Carrinho
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? AtualizadoEm { get; set; }

        // Propriedades de navegação
        public virtual Usuario Cliente { get; set; }
        public virtual ICollection<CarrinhoItem> Itens { get; set; } = new List<CarrinhoItem>();
    }
}