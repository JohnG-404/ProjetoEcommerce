using ProjetoEcommerce.Interfaces;

namespace ProjetoEcommerce.Services
{
    public class NotificacaoService
    {
        private readonly List<INotificacao> _notificacoes;

        public NotificacaoService()
        {
            _notificacoes = new List<INotificacao>();
        }

        public void AdicionarNotificacao(INotificacao notificacao)
        {
            _notificacoes.Add(notificacao);
        }

        public void AdicionarNotificacaoEmail(string destinatario, string mensagem)
        {
            var email = new EmailService(destinatario, mensagem);
            _notificacoes.Add(email);
        }

        public void AdicionarNotificacaoSMS(string destinatario, string mensagem)
        {
            var sms = new SMSService(destinatario, mensagem);
            _notificacoes.Add(sms);
        }

        public List<string> EnviarTodasNotificacoes()
        {
            var resultados = new List<string>();

            foreach (var notificacao in _notificacoes)
            {
                try
                {
                    string resultado = notificacao.Enviar();
                    resultados.Add(resultado);
                }
                catch (Exception ex)
                {
                    resultados.Add($"❌ Erro ao enviar notificação: {ex.Message}");
                }
            }

            _notificacoes.Clear();
            return resultados;
        }

        public string EnviarNotificacao(INotificacao notificacao)
        {
            return notificacao.Enviar();
        }

        public string EnviarNotificacao(INotificacao notificacao, int tentativas)
        {
            for (int i = 0; i < tentativas; i++)
            {
                try
                {
                    return notificacao.Enviar();
                }
                catch
                {
                    if (i == tentativas - 1) throw;
                    Thread.Sleep(1000);
                }
            }
            return "Falha no envio após múltiplas tentativas";
        }
    }
}