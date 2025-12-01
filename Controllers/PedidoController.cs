using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEcommerce.DTOs;
using ProjetoEcommerce.Modelos;
using ProjetoEcommerce.Services;

namespace ProjetoEcommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NotificacaoService _notificacaoService;

        public PedidosController(AppDbContext context, NotificacaoService notificacaoService)
        {
            _context = context;
            _notificacaoService = notificacaoService;
        }

        [HttpPost]
        public async Task<ActionResult<PedidoResponseDTO>> CriarPedido([FromBody] CriarPedidoDTO request)
        {
            try
            {
                // Buscar carrinho do cliente
                var carrinho = await _context.Carrinhos
                    .Include(c => c.Itens)
                        .ThenInclude(ci => ci.ProdutoFisico)
                            .ThenInclude(pf => pf.Estoque)
                    .Include(c => c.Itens)
                        .ThenInclude(ci => ci.ProdutoDigital)
                    .Include(c => c.Cliente)
                    .FirstOrDefaultAsync(c => c.ClienteId == request.ClienteId);

                if (carrinho == null || !carrinho.Itens.Any())
                {
                    return BadRequest("Carrinho vazio");
                }

                // Verificar estoque para produtos físicos
                foreach (var item in carrinho.Itens.Where(i => i.ProdutoFisicoId.HasValue))
                {
                    var estoque = item.ProdutoFisico?.Estoque;
                    if (estoque == null || estoque.QuantidadeDisponivel < item.Quantidade)
                    {
                        return BadRequest($"Produto {item.ProdutoFisico?.Nome} sem estoque suficiente");
                    }
                }

                // Verificar endereço de entrega
                var enderecoEntrega = await _context.Enderecos
                    .FirstOrDefaultAsync(e => e.Id == request.EnderecoEntregaId && e.UsuarioId == request.ClienteId);

                if (enderecoEntrega == null)
                {
                    return BadRequest("Endereço de entrega não encontrado");
                }

                // Calcular totais
                var valorTotal = carrinho.Itens.Sum(i => i.CalcularSubtotal());
                var valorFrete = CalcularFrete(carrinho.Itens.ToList());
                var valorTotalComFrete = valorTotal + valorFrete;

                // Criar pedido
                var pedido = new Pedido
                {
                    NumeroPedido = GerarNumeroPedido(),
                    ClienteId = request.ClienteId,
                    LojaId = 1, // Por enquanto, loja padrão
                    EnderecoEntregaId = request.EnderecoEntregaId,
                    ValorTotal = valorTotal,
                    ValorFrete = valorFrete,
                    ValorDesconto = 0, // Pode ser calculado com promoções
                    Status = "Pendente",
                    DataCriacao = DateTime.Now,
                    Observacao = request.Observacao
                };

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                // Criar itens do pedido e atualizar estoque
                foreach (var item in carrinho.Itens)
                {
                    var produto = item.ObterProduto();
                    var itemPedido = new ItemPedido
                    {
                        PedidoId = pedido.Id,
                        ProdutoFisicoId = item.ProdutoFisicoId,
                        ProdutoDigitalId = item.ProdutoDigitalId,
                        Quantidade = item.Quantidade,
                        PrecoUnitario = item.PrecoUnitario,
                        NomeProduto = produto?.Nome ?? "Produto não encontrado"
                    };
                    _context.ItensPedido.Add(itemPedido);

                    // Atualizar estoque apenas para produtos físicos
                    if (item.ProdutoFisicoId.HasValue && item.ProdutoFisico?.Estoque != null)
                    {
                        var estoque = item.ProdutoFisico.Estoque;
                        estoque.QuantidadeDisponivel -= item.Quantidade;
                        estoque.UltimoMovimento = DateTime.Now;
                    }
                }

                // Criar pagamento
                var pagamento = new Pagamento
                {
                    PedidoId = pedido.Id,
                    Metodo = request.MetodoPagamento,
                    Valor = valorTotalComFrete,
                    Status = "Pendente",
                    Referencia = Guid.NewGuid().ToString(),
                    DataCriacao = DateTime.Now
                };
                _context.Pagamentos.Add(pagamento);

                // Criar envio (apenas para pedidos com produtos físicos)
                var temProdutosFisicos = carrinho.Itens.Any(i => i.ProdutoFisicoId.HasValue);
                if (temProdutosFisicos)
                {
                    var envio = new Envio
                    {
                        PedidoId = pedido.Id,
                        Transportadora = "Correios",
                        TipoFrete = "PAC",
                        ValorFrete = valorFrete,
                        PrazoEntrega = 7, // dias úteis
                        Status = "Aguardando",
                        DataEnvio = null
                    };
                    _context.Envios.Add(envio);
                }

                // Limpar carrinho
                _context.CarrinhoItens.RemoveRange(carrinho.Itens);
                carrinho.AtualizadoEm = DateTime.Now;

                await _context.SaveChangesAsync();

                // Criar transação no caixa
                var caixa = await _context.Caixas.FirstOrDefaultAsync(c => c.LojaId == pedido.LojaId && c.Status == "Aberto");
                if (caixa != null)
                {
                    var transacao = new Transacao
                    {
                        CaixaId = caixa.Id,
                        Tipo = "Entrada",
                        Categoria = "Venda",
                        Valor = valorTotalComFrete,
                        Descricao = $"Venda pedido {pedido.NumeroPedido}",
                        Data = DateTime.Now,
                        PedidoId = pedido.Id,
                        MetodoPagamento = request.MetodoPagamento,
                        Observacao = $"Cliente: {carrinho.Cliente?.Nome}"
                    };
                    _context.Transacoes.Add(transacao);

                    // Atualizar saldo do caixa
                    caixa.SaldoAtual += valorTotalComFrete;
                }

                await _context.SaveChangesAsync();

                // Notificações
                var emailCliente = new EmailService(carrinho.Cliente?.Email ?? "cliente@email.com",
                    $"Seu pedido {pedido.NumeroPedido} foi criado com sucesso! Valor: R$ {valorTotalComFrete:F2}");
                _notificacaoService.AdicionarNotificacao(emailCliente);

                var emailAdmin = new EmailService("admin@loja.com",
                    $"Novo pedido {pedido.NumeroPedido} - Cliente: {carrinho.Cliente?.Nome} - Valor: R$ {valorTotalComFrete:F2}");
                _notificacaoService.AdicionarNotificacao(emailAdmin);

                _notificacaoService.EnviarTodasNotificacoes();

                // Retornar pedido criado
                return await GetPedido(pedido.Id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PedidoResponseDTO>> GetPedido(int id)
        {
            try
            {
                var pedido = await _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.Loja)
                    .Include(p => p.EnderecoEntrega)
                    .Include(p => p.Itens)
                        .ThenInclude(ip => ip.ProdutoFisico)
                    .Include(p => p.Itens)
                        .ThenInclude(ip => ip.ProdutoDigital)
                    .Include(p => p.Pagamento)
                    .Include(p => p.Envio)
                    .Where(p => p.Id == id)
                    .Select(p => new PedidoResponseDTO
                    {
                        Id = p.Id,
                        NumeroPedido = p.NumeroPedido,
                        ClienteId = p.ClienteId,
                        ClienteNome = p.Cliente.Nome,
                        LojaId = p.LojaId,
                        LojaNome = p.Loja.Nome,
                        EnderecoEntrega = new EnderecoResponseDTO
                        {
                            Rua = p.EnderecoEntrega.Rua,
                            Numero = p.EnderecoEntrega.Numero,
                            Complemento = p.EnderecoEntrega.Complemento,
                            Bairro = p.EnderecoEntrega.Bairro,
                            Cidade = p.EnderecoEntrega.Cidade,
                            Estado = p.EnderecoEntrega.Estado,
                            CEP = p.EnderecoEntrega.CEP
                        },
                        ValorTotal = p.ValorTotal,
                        ValorFrete = p.ValorFrete,
                        ValorDesconto = p.ValorDesconto,
                        ValorFinal = p.ValorTotal + p.ValorFrete - p.ValorDesconto,
                        Status = p.Status,
                        DataCriacao = p.DataCriacao,
                        DataAtualizacao = p.DataAtualizacao,
                        Itens = p.Itens.Select(ip => new ItemPedidoResponseDTO
                        {
                            Id = ip.Id,
                            ProdutoId = ip.ProdutoFisicoId ?? ip.ProdutoDigitalId ?? 0,
                            ProdutoNome = ip.NomeProduto,
                            TipoProduto = ip.ProdutoFisicoId.HasValue ? "Físico" : "Digital",
                            Quantidade = ip.Quantidade,
                            PrecoUnitario = ip.PrecoUnitario,
                            Subtotal = ip.CalcularSubtotal()
                        }).ToList(),
                        Pagamento = p.Pagamento != null ? new PagamentoResponseDTO
                        {
                            Metodo = p.Pagamento.Metodo,
                            Valor = p.Pagamento.Valor,
                            Status = p.Pagamento.Status,
                            Referencia = p.Pagamento.Referencia
                        } : null,
                        Envio = p.Envio != null ? new EnvioResponseDTO
                        {
                            Transportadora = p.Envio.Transportadora,
                            TipoFrete = p.Envio.TipoFrete,
                            ValorFrete = p.Envio.ValorFrete,
                            Status = p.Envio.Status,
                            PrazoEntrega = p.Envio.PrazoEntrega,
                            CodigoRastreamento = p.Envio.CodigoRastreamento
                        } : null
                    })
                    .FirstOrDefaultAsync();

                if (pedido == null)
                {
                    return NotFound("Pedido não encontrado");
                }

                return Ok(pedido);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar pedido: {ex.Message}");
            }
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<PedidoResponseDTO>>> GetPedidosPorCliente(int clienteId)
        {
            try
            {
                var pedidos = await _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.Loja)
                    .Include(p => p.Pagamento)
                    .Include(p => p.Envio)
                    .Where(p => p.ClienteId == clienteId)
                    .OrderByDescending(p => p.DataCriacao)
                    .Select(p => new PedidoResponseDTO
                    {
                        Id = p.Id,
                        NumeroPedido = p.NumeroPedido,
                        ClienteId = p.ClienteId,
                        ClienteNome = p.Cliente.Nome,
                        LojaId = p.LojaId,
                        LojaNome = p.Loja.Nome,
                        ValorTotal = p.ValorTotal,
                        ValorFrete = p.ValorFrete,
                        ValorFinal = p.ValorTotal + p.ValorFrete - p.ValorDesconto,
                        Status = p.Status,
                        DataCriacao = p.DataCriacao,
                        Pagamento = p.Pagamento != null ? new PagamentoResponseDTO
                        {
                            Metodo = p.Pagamento.Metodo,
                            Valor = p.Pagamento.Valor,
                            Status = p.Pagamento.Status
                        } : null,
                        Envio = p.Envio != null ? new EnvioResponseDTO
                        {
                            Status = p.Envio.Status,
                            CodigoRastreamento = p.Envio.CodigoRastreamento
                        } : null
                    })
                    .ToListAsync();

                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar pedidos: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PedidoResponseDTO>>> GetTodosPedidos()
        {
            try
            {
                var pedidos = await _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.Loja)
                    .Include(p => p.Pagamento)
                    .OrderByDescending(p => p.DataCriacao)
                    .Select(p => new PedidoResponseDTO
                    {
                        Id = p.Id,
                        NumeroPedido = p.NumeroPedido,
                        ClienteId = p.ClienteId,
                        ClienteNome = p.Cliente.Nome,
                        LojaId = p.LojaId,
                        LojaNome = p.Loja.Nome,
                        ValorTotal = p.ValorTotal,
                        ValorFrete = p.ValorFrete,
                        ValorFinal = p.ValorTotal + p.ValorFrete - p.ValorDesconto,
                        Status = p.Status,
                        DataCriacao = p.DataCriacao,
                        Pagamento = p.Pagamento != null ? new PagamentoResponseDTO
                        {
                            Metodo = p.Pagamento.Metodo,
                            Status = p.Pagamento.Status
                        } : null
                    })
                    .ToListAsync();

                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar pedidos: {ex.Message}");
            }
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult> AtualizarStatus(int id, [FromBody] string status)
        {
            try
            {
                var pedido = await _context.Pedidos
                    .Include(p => p.Cliente)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (pedido == null)
                {
                    return NotFound("Pedido não encontrado");
                }

                var statusAnterior = pedido.Status;
                pedido.Status = status;
                pedido.DataAtualizacao = DateTime.Now;

                // Atualizar status do pagamento se o pedido for confirmado
                if (status == "Confirmado")
                {
                    var pagamento = await _context.Pagamentos.FirstOrDefaultAsync(p => p.PedidoId == id);
                    if (pagamento != null)
                    {
                        pagamento.Status = "Aprovado";
                        pagamento.DataAtualizacao = DateTime.Now;
                    }

                    // Se tiver envio, atualizar status
                    var envio = await _context.Envios.FirstOrDefaultAsync(e => e.PedidoId == id);
                    if (envio != null)
                    {
                        envio.Status = "Preparando";
                    }
                }

                // Se o pedido foi enviado, gerar código de rastreamento
                if (status == "Enviado")
                {
                    var envio = await _context.Envios.FirstOrDefaultAsync(e => e.PedidoId == id);
                    if (envio != null && string.IsNullOrEmpty(envio.CodigoRastreamento))
                    {
                        envio.CodigoRastreamento = $"BR{DateTime.Now:yyyyMMddHHmmss}{id}";
                        envio.DataEnvio = DateTime.Now;
                        envio.Status = "EmTransito";
                    }
                }

                await _context.SaveChangesAsync();

                // Notificar cliente sobre mudança de status
                var emailService = new EmailService(pedido.Cliente?.Email ?? "cliente@email.com",
                    $"Status do seu pedido {pedido.NumeroPedido} alterado: {statusAnterior} → {status}");
                _notificacaoService.AdicionarNotificacao(emailService);
                _notificacaoService.EnviarTodasNotificacoes();

                return Ok(new
                {
                    message = "Status atualizado com sucesso",
                    pedidoId = id,
                    statusAnterior = statusAnterior,
                    novoStatus = status
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpPut("{id}/pagamento")]
        public async Task<ActionResult> AtualizarPagamento(int id, [FromBody] AtualizarPagamentoDTO request)
        {
            try
            {
                var pagamento = await _context.Pagamentos.FirstOrDefaultAsync(p => p.PedidoId == id);
                if (pagamento == null)
                {
                    return NotFound("Pagamento do pedido não encontrado");
                }

                pagamento.Status = request.Status;
                pagamento.DataAtualizacao = DateTime.Now;

                // Se pagamento aprovado, atualizar status do pedido
                if (request.Status == "Aprovado")
                {
                    var pedido = await _context.Pedidos.FindAsync(id);
                    if (pedido != null)
                    {
                        pedido.Status = "Confirmado";
                        pedido.DataAtualizacao = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Status do pagamento atualizado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpPost("{id}/cancelar")]
        public async Task<ActionResult> CancelarPedido(int id)
        {
            try
            {
                var pedido = await _context.Pedidos
                    .Include(p => p.Itens)
                    .Include(p => p.Cliente)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (pedido == null)
                {
                    return NotFound("Pedido não encontrado");
                }

                if (pedido.Status == "Entregue")
                {
                    return BadRequest("Não é possível cancelar um pedido já entregue");
                }

                // Restaurar estoque dos produtos físicos
                foreach (var item in pedido.Itens.Where(i => i.ProdutoFisicoId.HasValue))
                {
                    var estoque = await _context.Estoques.FirstOrDefaultAsync(e => e.ProdutoFisicoId == item.ProdutoFisicoId);
                    if (estoque != null)
                    {
                        estoque.QuantidadeDisponivel += item.Quantidade;
                        estoque.UltimoMovimento = DateTime.Now;
                    }
                }

                // Atualizar status
                pedido.Status = "Cancelado";
                pedido.DataAtualizacao = DateTime.Now;

                // Atualizar pagamento
                var pagamento = await _context.Pagamentos.FirstOrDefaultAsync(p => p.PedidoId == id);
                if (pagamento != null)
                {
                    pagamento.Status = "Estornado";
                    pagamento.DataAtualizacao = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                // Notificar cliente
                var emailService = new EmailService(pedido.Cliente?.Email ?? "cliente@email.com",
                    $"Seu pedido {pedido.NumeroPedido} foi cancelado. Valor estornado: R$ {pedido.ValorTotal + pedido.ValorFrete:F2}");
                _notificacaoService.AdicionarNotificacao(emailService);
                _notificacaoService.EnviarTodasNotificacoes();

                return Ok(new { message = "Pedido cancelado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        // 🔥 MÉTODOS PRIVADOS AUXILIARES

        private string GerarNumeroPedido()
        {
            var data = DateTime.Now.ToString("yyyyMMdd");
            var ultimoPedido = _context.Pedidos
                .Where(p => p.NumeroPedido.StartsWith($"PED{data}"))
                .OrderByDescending(p => p.NumeroPedido)
                .FirstOrDefault();

            var sequencia = 1;
            if (ultimoPedido != null)
            {
                var ultimaSequencia = int.Parse(ultimoPedido.NumeroPedido.Substring(11));
                sequencia = ultimaSequencia + 1;
            }

            return $"PED{data}{sequencia:D4}";
        }

        private decimal CalcularFrete(List<CarrinhoItem> itens)
        {
            // Simulação de cálculo de frete
            var temProdutosFisicos = itens.Any(i => i.ProdutoFisicoId.HasValue);

            if (!temProdutosFisicos)
            {
                return 0; // Produtos digitais não têm frete
            }

            var pesoTotal = itens
                .Where(i => i.ProdutoFisico != null && i.ProdutoFisico.Peso.HasValue)
                .Sum(i => i.ProdutoFisico.Peso.Value * i.Quantidade);

            // Frete baseado no peso
            if (pesoTotal <= 1) return 15.00m;
            if (pesoTotal <= 5) return 25.00m;
            if (pesoTotal <= 10) return 35.00m;
            return 50.00m; // acima de 10kg
        }
    }

    
}