using System.ComponentModel.DataAnnotations;

namespace ProjetoEcommerce.Modelos
{
    public class Notificacao
    {
        public int Id { get; set; }

        [Required]
        public string Tipo { get; set; } // Email, SMS, Push

        [Required]
        public string Destinatario { get; set; }

        [Required]
        public string Mensagem { get; set; }

        public string Status { get; set; } = "Pendente";
        public DateTime DataEnvio { get; set; }
        public DateTime? DataConfirmacao { get; set; }
        public string Erro { get; set; }

        public int? UsuarioId { get; set; }
        public int? PedidoId { get; set; }

        // Propriedades de navegação
        public virtual Usuario Usuario { get; set; }
        public virtual Pedido Pedido { get; set; }

        public Notificacao()
        {
            DataEnvio = DateTime.Now;
        }

        public void MarcarComoEnviada()
        {
            Status = "Enviada";
            DataConfirmacao = DateTime.Now;
        }

        public void MarcarComoFalha(string erro = null)
        {
            Status = "Falha";
            Erro = erro;
        }
    }
}