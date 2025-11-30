using System.ComponentModel.DataAnnotations;

namespace ProjetoEcommerce.Modelos
{
    public abstract class EntidadeBase
    {
        [Key]
        public int Id { get; protected set; }

        public DateTime DataCriacao { get; protected set; }
        public DateTime? DataAtualizacao { get; protected set; }
        public bool Ativo { get; protected set; } = true;

        protected EntidadeBase()
        {
            DataCriacao = DateTime.Now;
        }

        public virtual void Atualizar()
        {
            DataAtualizacao = DateTime.Now;
        }

        public virtual void Desativar()
        {
            Ativo = false;
            Atualizar();
        }

        public virtual void Ativar()
        {
            Ativo = true;
            Atualizar();
        }

        // Método abstrato para validação - POLIMORFISMO
        public abstract bool Validar();
    }
}