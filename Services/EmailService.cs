using ProjetoEcommerce.Interfaces;

namespace ProjetoEcommerce.Services
{
    public class EmailService : INotificacao
    {
        public string Destinatario { get; private set; }
        public string Mensagem { get; private set; }

        // Construtor sem parâmetros para o DI
        public EmailService()
        {
        }

        // Construtor com parâmetros para uso manual
        public EmailService(string destinatario, string mensagem)
        {
            Configurar(destinatario, mensagem);
        }

        // ENCAPSULAMENTO - Método para configurar
        public void Configurar(string destinatario, string mensagem)
        {
            Destinatario = destinatario;
            Mensagem = mensagem;
        }

        // POLIMORFISMO - Implementação específica para email
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

            // Simulação de envio de email
            Console.WriteLine($"📧 Enviando email para: {Destinatario}");
            Console.WriteLine($"📧 Mensagem: {Mensagem}");

            return $"Email enviado com sucesso para {Destinatario}";
        }
    }
}