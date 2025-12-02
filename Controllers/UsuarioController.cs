using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEcommerce.DTOs;
using ProjetoEcommerce.Modelos;

namespace ProjetoEcommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("registrar")]
        public async Task<ActionResult<UsuarioResponseDTO>> Registrar(UsuarioDTO usuarioDto)
        {
            try
            {
                if (await _context.Usuarios.AnyAsync(u => u.Email == usuarioDto.Email))
                {
                    return BadRequest("Email já cadastrado");
                }

                var usuario = new Usuario
                {
                    Nome = usuarioDto.Nome,
                    Email = usuarioDto.Email,
                    Senha = usuarioDto.Senha, 
                    Telefone = usuarioDto.Telefone,
                    DataCriacao = DateTime.Now,
                    Role = "Cliente",
                    Ativo = true
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var response = new UsuarioResponseDTO
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    Telefone = usuario.Telefone,
                    Role = usuario.Role,
                    DataCriacao = usuario.DataCriacao
                };

                return CreatedAtAction("GetUsuario", new { id = usuario.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UsuarioResponseDTO>> Login(LoginDTO loginDto)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Senha == loginDto.Senha && u.Ativo);

                if (usuario == null)
                {
                    return Unauthorized("Email ou senha inválidos");
                }

                var response = new UsuarioResponseDTO
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    Telefone = usuario.Telefone,
                    Role = usuario.Role,
                    DataCriacao = usuario.DataCriacao
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioResponseDTO>> GetUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Where(u => u.Id == id && u.Ativo)
                    .Select(u => new UsuarioResponseDTO
                    {
                        Id = u.Id,
                        Nome = u.Nome,
                        Email = u.Email,
                        Telefone = u.Telefone,
                        Role = u.Role,
                        DataCriacao = u.DataCriacao
                    })
                    .FirstOrDefaultAsync();

                if (usuario == null)
                {
                    return NotFound();
                }

                return usuario;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDTO>>> GetUsuarios()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Where(u => u.Ativo)
                    .Select(u => new UsuarioResponseDTO
                    {
                        Id = u.Id,
                        Nome = u.Nome,
                        Email = u.Email,
                        Telefone = u.Telefone,
                        Role = u.Role,
                        DataCriacao = u.DataCriacao
                    })
                    .ToListAsync();

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}