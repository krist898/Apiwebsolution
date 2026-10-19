using GestionAcademica.API.Data;
using GestionAcademica.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionAcademica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CursosController : ControllerBase
    {
        private readonly GestionAcademicaDbContext _context;

        public CursosController(GestionAcademicaDbContext context)
        {
            _context = context;
        }

        // GET: api/Cursos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Curso>>> GetCursos()
        {
            return Ok(await _context.Cursos.AsNoTracking().ToListAsync());
        }

        // GET: api/Cursos/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Curso>> GetCurso(int id)
        {
            var curso = await _context.Cursos.AsNoTracking()
                .FirstOrDefaultAsync(c => c.CursoId == id);

            if (curso == null)
                return NotFound(new { mensaje = $"No existe el curso con ID {id}." });

            return Ok(curso);
        }

        // POST: api/Cursos
        [HttpPost]
        public async Task<ActionResult<Curso>> PostCurso(Curso curso)
        {
            curso.CursoId = 0; // El ID lo genera la base de datos
            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCurso), new { id = curso.CursoId }, curso);
        }

        // PUT: api/Cursos/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutCurso(int id, Curso curso)
        {
            if (id != curso.CursoId)
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el del cuerpo." });

            var existente = await _context.Cursos.FindAsync(id);
            if (existente == null)
                return NotFound(new { mensaje = $"No existe el curso con ID {id}." });

            existente.Nombre = curso.Nombre;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Cursos/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCurso(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
                return NotFound(new { mensaje = $"No existe el curso con ID {id}." });

            bool tieneAsignaciones = await _context.Asignaciones.AnyAsync(a => a.CursoId == id);
            if (tieneAsignaciones)
                return Conflict(new { mensaje = "No se puede eliminar el curso porque tiene asignaciones relacionadas." });

            _context.Cursos.Remove(curso);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
