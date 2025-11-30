namespace ProjetoEcommerce.Interfaces
{
    public interface INotificacao
    {
        string Destinatario { get; }
        string Mensagem { get; }

        // Método para configurar a notificação
        void Configurar(string destinatario, string mensagem);

        bool ValidarDestinatario();
        string Enviar();
    }
}