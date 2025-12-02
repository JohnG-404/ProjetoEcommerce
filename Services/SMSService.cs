using ProjetoEcommerce.Interfaces;

namespace ProjetoEcommerce.Services
{
    public class SMSService : INotificacao
    {
        public string Destinatario { get; private set; }
        public string Mensagem { get; private set; }

        public SMSService()
        {
        }

        public SMSService(string destinatario, string mensagem)
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
                   Destinatario.Length >= 10 &&
                   Destinatario.All(char.IsDigit);
        }

        public string Enviar()
        {
            if (!ValidarDestinatario())
                throw new InvalidOperationException("Número de telefone inválido");

            Console.WriteLine($"📱 Enviando SMS para: {Destinatario}");
            Console.WriteLine($"📱 Mensagem: {Mensagem}");

            return $"SMS enviado com sucesso para {Destinatario}";
        }
    }
}