using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEcommerce.Modelos;

[ApiController]
[Route("api/[controller]")]
public class CarrinhoController : ControllerBase
{
    private readonly AppDbContext _context;

    public CarrinhoController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{clienteId}")]
    public async Task<ActionResult<Carrinho>> GetCarrinho(int clienteId)
    {
        var carrinho = await _context.Carrinhos
            .Include(c => c.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

        if (carrinho == null)
        {
            // Cria um novo carrinho se não existir
            carrinho = new Carrinho { ClienteId = clienteId };
            _context.Carrinhos.Add(carrinho);
            await _context.SaveChangesAsync();
        }

        return carrinho;
    }

    [HttpPost("adicionar-item")]
    public async Task<ActionResult> AdicionarItem([FromBody] AdicionarItemRequest request)
    {
        var carrinho = await _context.Carrinhos
            .FirstOrDefaultAsync(c => c.ClienteId == request.ClienteId);

        if (carrinho == null)
        {
            carrinho = new Carrinho { ClienteId = request.ClienteId };
            _context.Carrinhos.Add(carrinho);
            await _context.SaveChangesAsync();
        }

        var produto = await _context.Produtos.FindAsync(request.ProdutoId);
        if (produto == null)
        {
            return NotFound("Produto não encontrado");
        }

        var itemExistente = await _context.CarrinhoItens
            .FirstOrDefaultAsync(ci => ci.CarrinhoId == carrinho.Id && ci.ProdutoId == request.ProdutoId);

        if (itemExistente != null)
        {
            itemExistente.Quantidade += request.Quantidade;
        }
        else
        {
            var novoItem = new CarrinhoItem
            {
                CarrinhoId = carrinho.Id,
                ProdutoId = request.ProdutoId,
                Quantidade = request.Quantidade,
                PrecoUnitario = produto.Preco
            };
            _context.CarrinhoItens.Add(novoItem);
        }

        carrinho.AtualizadoEm = DateTime.Now;
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("finalizar-pedido/{clienteId}")]
    public async Task<ActionResult<Pedido>> FinalizarPedido(int clienteId)
    {
        var carrinho = await _context.Carrinhos
            .Include(c => c.Itens)
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

        if (carrinho == null || !carrinho.Itens.Any())
        {
            return BadRequest("Carrinho vazio");
        }

        // Cria o pedido
        var pedido = new Pedido
        {
            ClienteId = clienteId,
            LojaId = 1, // Assume uma loja padrão por enquanto
            ValorTotal = carrinho.Itens.Sum(i => i.Quantidade * i.PrecoUnitario),
            Status = "Pendente",
            DataCriacao = DateTime.Now
        };

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        // Adiciona itens do pedido
        foreach (var item in carrinho.Itens)
        {
            var itemPedido = new ItemPedido
            {
                PedidoId = pedido.Id,
                ProdutoId = item.ProdutoId,
                Quantidade = item.Quantidade,
                PrecoUnitario = item.PrecoUnitario
            };
            _context.ItensPedido.Add(itemPedido);
        }

        // Limpa o carrinho
        _context.CarrinhoItens.RemoveRange(carrinho.Itens);
        await _context.SaveChangesAsync();

        return pedido;
    }
}

public class AdicionarItemRequest
{
    public int ClienteId { get; set; }
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
}