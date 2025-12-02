using ProjetoEcommerce.Interfaces;

namespace ProjetoEcommerce.Services
{
    public class EmailService : INotificacao
    {
        public string Destinatario { get; private set; }
        public string Mensagem { get; private set; }

        public EmailService()
        {
        }

        public EmailService(string destinatario, string mensagem)
        {
            Configurar(destinatario, mensagem);
        }

        public void Configurar(string destinatario, string mensagem)
        {
            Destinatario = destinatario;
            Mensagem = mensagem;
        }

        public bool ValidarDestinatario()
        {
            return !string.IsNullOrWhiteSpace(Destinatario) &&
                   Destinatario.Contains("@") &&
                   Destinatario.Contains(".");
        }

        public string Enviar()
        {
            if (!ValidarDestinatario())
                throw new InvalidOperationException("Destinatário de email inválido");

            Console.WriteLine($"📧 Enviando email para: {Destinatario}");
            Console.WriteLine($"📧 Mensagem: {Mensagem}");

            return $"Email enviado com sucesso para {Destinatario}";
        }
    }
}