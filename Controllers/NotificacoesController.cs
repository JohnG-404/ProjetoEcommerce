using Microsoft.AspNetCore.Mvc;
using ProjetoEcommerce.Interfaces;
using ProjetoEcommerce.DTOs;
using ProjetoEcommerce.Services;

namespace ProjetoEcommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificacoesController : ControllerBase
    {
        private readonly NotificacaoService _notificacaoService;
        private readonly IEnumerable<INotificacao> _servicosNotificacao;

        public NotificacoesController(
            NotificacaoService notificacaoService,
            IEnumerable<INotificacao> servicosNotificacao)
        {
            _notificacaoService = notificacaoService;
            _servicosNotificacao = servicosNotificacao;
        }

        [HttpPost("teste")]
        public ActionResult TestarNotificacoes()
        {
            try
            {
                // POLIMORFISMO - Testando diferentes tipos de notificação
                var emailService = new EmailService("teste@email.com", "Esta é uma notificação de teste por email");
                var smsService = new SMSService("11999999999", "Esta é uma notificação de teste por SMS");

                _notificacaoService.AdicionarNotificacao(emailService);
                _notificacaoService.AdicionarNotificacao(smsService);

                var resultados = _notificacaoService.EnviarTodasNotificacoes();

                return Ok(new
                {
                    Message = "Teste de notificações concluído",
                    Resultados = resultados,
                    TotalNotificacoes = resultados.Count,
                    ServicosDisponiveis = _servicosNotificacao.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro no teste: {ex.Message}");
            }
        }

        [HttpPost("email")]
        public ActionResult EnviarEmail([FromBody] NotificacaoRequest request)
        {
            try
            {
                var emailService = new EmailService(request.Destinatario, request.Mensagem);
                var resultado = _notificacaoService.EnviarNotificacao(emailService);

                return Ok(new { Message = resultado });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao enviar email: {ex.Message}");
            }
        }

        [HttpPost("sms")]
        public ActionResult EnviarSMS([FromBody] NotificacaoRequest request)
        {
            try
            {
                var smsService = new SMSService(request.Destinatario, request.Mensagem);
                var resultado = _notificacaoService.EnviarNotificacao(smsService, 3); // SOBRECARGA com tentativas

                return Ok(new { Message = resultado });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao enviar SMS: {ex.Message}");
            }
        }

        [HttpGet("servicos")]
        public ActionResult ListarServicos()
        {
            var servicos = _servicosNotificacao
                .Select(s => new
                {
                    Tipo = s.GetType().Name,
                    Implementa = s.GetType().GetInterfaces().Select(i => i.Name)
                })
                .ToList();

            return Ok(servicos);
        }
    }
}