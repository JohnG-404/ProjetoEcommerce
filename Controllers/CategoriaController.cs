using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEcommerce.DTOs;
using ProjetoEcommerce.Modelos;

namespace ProjetoEcommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaResponseDTO>>> GetCategorias()
        {
            try
            {
                var categorias = await _context.Categorias
                    .Include(c => c.Subcategorias)
                    .Where(c => c.ParentId == null)
                    .Select(c => new CategoriaResponseDTO
                    {
                        Id = c.Id,
                        Nome = c.Nome,
                        Descricao = c.Descricao,
                        Subcategorias = c.Subcategorias.Select(sc => new CategoriaResponseDTO
                        {
                            Id = sc.Id,
                            Nome = sc.Nome,
                            Descricao = sc.Descricao
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(categorias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CategoriaResponseDTO>> CriarCategoria([FromBody] CategoriaDTO categoriaDto)
        {
            try
            {
                var categoria = new Categoria
                {
                    Nome = categoriaDto.Nome,
                    Descricao = categoriaDto.Descricao,
                    ParentId = categoriaDto.ParentId
                };

                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();

                var response = new CategoriaResponseDTO
                {
                    Id = categoria.Id,
                    Nome = categoria.Nome,
                    Descricao = categoria.Descricao,
                    ParentId = categoria.ParentId
                };

                return CreatedAtAction("GetCategoria", new { id = categoria.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaResponseDTO>> GetCategoria(int id)
        {
            try
            {
                var categoria = await _context.Categorias
                    .Include(c => c.Subcategorias)
                    .Where(c => c.Id == id)
                    .Select(c => new CategoriaResponseDTO
                    {
                        Id = c.Id,
                        Nome = c.Nome,
                        Descricao = c.Descricao,
                        ParentId = c.ParentId,
                        Subcategorias = c.Subcategorias.Select(sc => new CategoriaResponseDTO
                        {
                            Id = sc.Id,
                            Nome = sc.Nome,
                            Descricao = sc.Descricao
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (categoria == null)
                {
                    return NotFound();
                }

                return Ok(categoria);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}