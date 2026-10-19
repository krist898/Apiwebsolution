using GestionAcademica.API.Data;
using GestionAcademica.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionAcademica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstudiantesController : ControllerBase
    {
        private readonly GestionAcademicaDbContext _context;

        public EstudiantesController(GestionAcademicaDbContext context)
        {
            _context = context;
        }

        // GET: api/Estudiantes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Estudiante>>> GetEstudiantes()
        {
            return Ok(await _context.Estudiantes.AsNoTracking().ToListAsync());
        }

        // GET: api/Estudiantes/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Estudiante>> GetEstudiante(int id)
        {
            var estudiante = await _context.Estudiantes.AsNoTracking()
                .FirstOrDefaultAsync(e => e.EstudianteId == id);

            if (estudiante == null)
                return NotFound(new { mensaje = $"No existe el estudiante con ID {id}." });

            return Ok(estudiante);
        }

        // POST: api/Estudiantes
        [HttpPost]
        public async Task<ActionResult<Estudiante>> PostEstudiante(Estudiante estudiante)
        {
            estudiante.EstudianteId = 0; // El ID lo genera la base de datos
            _context.Estudiantes.Add(estudiante);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEstudiante), new { id = estudiante.EstudianteId }, estudiante);
        }

        // PUT: api/Estudiantes/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutEstudiante(int id, Estudiante estudiante)
        {
            if (id != estudiante.EstudianteId)
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el del cuerpo." });

            var existente = await _context.Estudiantes.FindAsync(id);
            if (existente == null)
                return NotFound(new { mensaje = $"No existe el estudiante con ID {id}." });

            existente.Nombres = estudiante.Nombres;
            existente.Apellidos = estudiante.Apellidos;
            existente.Direccion = estudiante.Direccion;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Estudiantes/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEstudiante(int id)
        {
            var estudiante = await _context.Estudiantes.FindAsync(id);
            if (estudiante == null)
                return NotFound(new { mensaje = $"No existe el estudiante con ID {id}." });

            bool tieneAsignaciones = await _context.Asignaciones.AnyAsync(a => a.EstudianteId == id);
            if (tieneAsignaciones)
                return Conflict(new { mensaje = "No se puede eliminar el estudiante porque tiene asignaciones relacionadas." });

            _context.Estudiantes.Remove(estudiante);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
