using ProjetoEcommerce.Interfaces;

namespace ProjetoEcommerce.Services
{
    public class SMSService : INotificacao
    {
        public string Destinatario { get; private set; }
        public string Mensagem { get; private set; }

        // Construtor sem parâmetros para o DI
        public SMSService()
        {
        }

        // Construtor com parâmetros para uso manual
        public SMSService(string destinatario, string mensagem)
        {
            Configurar(destinatario, mensagem);
        }

        // ENCAPSULAMENTO - Método para configurar
        public void Configurar(string destinatario, string mensagem)
        {
            Destinatario = destinatario;
            Mensagem = mensagem;
        }

        // POLIMORFISMO - Implementação específica para SMS
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

            // Simulação de envio de SMS
            Console.WriteLine($"📱 Enviando SMS para: {Destinatario}");
            Console.WriteLine($"📱 Mensagem: {Mensagem}");

            return $"SMS enviado com sucesso para {Destinatario}";
        }
    }
}