using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEcommerce.DTOs;
using ProjetoEcommerce.Modelos;

namespace ProjetoEcommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LojasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LojasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LojaResponseDTO>>> GetLojas()
        {
            try
            {
                var lojas = await _context.Lojas
                    .Include(l => l.Endereco)
                    .Where(l => l.Ativo)
                    .Select(l => new LojaResponseDTO
                    {
                        Id = l.Id,
                        Nome = l.Nome,
                        Descricao = l.Descricao,
                        CNPJ = l.CNPJ,
                        Telefone = l.Telefone,
                        Ativo = l.Ativo,
                        DataCriacao = l.DataCriacao,
                        Endereco = l.Endereco != null ? new EnderecoResponseDTO
                        {
                            Id = l.Endereco.Id,
                            Rua = l.Endereco.Rua,
                            Numero = l.Endereco.Numero,
                            Complemento = l.Endereco.Complemento,
                            Bairro = l.Endereco.Bairro,
                            Cidade = l.Endereco.Cidade,
                            Estado = l.Endereco.Estado,
                            CEP = l.Endereco.CEP,
                            Pais = l.Endereco.Pais
                        } : null
                    })
                    .ToListAsync();

                return Ok(lojas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LojaResponseDTO>> GetLoja(int id)
        {
            try
            {
                var loja = await _context.Lojas
                    .Include(l => l.Endereco)
                    .Where(l => l.Id == id && l.Ativo)
                    .Select(l => new LojaResponseDTO
                    {
                        Id = l.Id,
                        Nome = l.Nome,
                        Descricao = l.Descricao,
                        CNPJ = l.CNPJ,
                        Telefone = l.Telefone,
                        Ativo = l.Ativo,
                        DataCriacao = l.DataCriacao,
                        Endereco = l.Endereco != null ? new EnderecoResponseDTO
                        {
                            Id = l.Endereco.Id,
                            Rua = l.Endereco.Rua,
                            Numero = l.Endereco.Numero,
                            Complemento = l.Endereco.Complemento,
                            Bairro = l.Endereco.Bairro,
                            Cidade = l.Endereco.Cidade,
                            Estado = l.Endereco.Estado,
                            CEP = l.Endereco.CEP,
                            Pais = l.Endereco.Pais
                        } : null
                    })
                    .FirstOrDefaultAsync();

                if (loja == null)
                {
                    return NotFound();
                }

                return Ok(loja);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<LojaResponseDTO>> CriarLoja([FromBody] LojaDTO lojaDto)
        {
            try
            {
                if (await _context.Lojas.AnyAsync(l => l.CNPJ == lojaDto.CNPJ))
                {
                    return BadRequest("CNPJ já cadastrado");
                }

                var loja = new Loja
                {
                    Nome = lojaDto.Nome,
                    Descricao = lojaDto.Descricao,
                    CNPJ = lojaDto.CNPJ,
                    Telefone = lojaDto.Telefone,
                    Ativo = true,
                    DataCriacao = DateTime.Now
                };

                _context.Lojas.Add(loja);
                await _context.SaveChangesAsync();

                // Criar caixa para a loja
                var caixa = new Caixa
                {
                    LojaId = loja.Id,
                    SaldoAtual = 0
                };
                _context.Caixas.Add(caixa);
                await _context.SaveChangesAsync();

                var response = new LojaResponseDTO
                {
                    Id = loja.Id,
                    Nome = loja.Nome,
                    Descricao = loja.Descricao,
                    CNPJ = loja.CNPJ,
                    Telefone = loja.Telefone,
                    Ativo = loja.Ativo,
                    DataCriacao = loja.DataCriacao
                };

                return CreatedAtAction("GetLoja", new { id = loja.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}