namespace ProjetoEcommerce.Interfaces
{
    public interface INotificacao
    {
        string Destinatario { get; }
        string Mensagem { get; }

        void Configurar(string destinatario, string mensagem);

        bool ValidarDestinatario();
        string Enviar();
    }
}