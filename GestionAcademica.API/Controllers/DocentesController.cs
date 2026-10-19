using GestionAcademica.API.Data;
using GestionAcademica.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionAcademica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocentesController : ControllerBase
    {
        private readonly GestionAcademicaDbContext _context;

        public DocentesController(GestionAcademicaDbContext context)
        {
            _context = context;
        }

        // GET: api/Docentes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Docente>>> GetDocentes()
        {
            return Ok(await _context.Docentes.AsNoTracking().ToListAsync());
        }

        // GET: api/Docentes/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Docente>> GetDocente(int id)
        {
            var docente = await _context.Docentes.AsNoTracking()
                .FirstOrDefaultAsync(d => d.DocenteId == id);

            if (docente == null)
                return NotFound(new { mensaje = $"No existe el docente con ID {id}." });

            return Ok(docente);
        }

        // POST: api/Docentes
        [HttpPost]
        public async Task<ActionResult<Docente>> PostDocente(Docente docente)
        {
            docente.DocenteId = 0; // El ID lo genera la base de datos
            _context.Docentes.Add(docente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDocente), new { id = docente.DocenteId }, docente);
        }

        // PUT: api/Docentes/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutDocente(int id, Docente docente)
        {
            if (id != docente.DocenteId)
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el del cuerpo." });

            var existente = await _context.Docentes.FindAsync(id);
            if (existente == null)
                return NotFound(new { mensaje = $"No existe el docente con ID {id}." });

            existente.Nombres = docente.Nombres;
            existente.Apellidos = docente.Apellidos;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Docentes/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDocente(int id)
        {
            var docente = await _context.Docentes.FindAsync(id);
            if (docente == null)
                return NotFound(new { mensaje = $"No existe el docente con ID {id}." });

            bool tieneAsignaciones = await _context.Asignaciones.AnyAsync(a => a.DocenteId == id);
            if (tieneAsignaciones)
                return Conflict(new { mensaje = "No se puede eliminar el docente porque tiene asignaciones relacionadas." });

            _context.Docentes.Remove(docente);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
