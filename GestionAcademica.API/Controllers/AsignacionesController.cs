using GestionAcademica.API.Data;
using GestionAcademica.API.DTOs;
using GestionAcademica.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionAcademica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AsignacionesController : ControllerBase
    {
        private readonly GestionAcademicaDbContext _context;

        public AsignacionesController(GestionAcademicaDbContext context)
        {
            _context = context;
        }

        // GET: api/Asignaciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AsignacionResponseDto>>> GetAsignaciones()
        {
            var asignaciones = await _context.Asignaciones
                .AsNoTracking()
                .Include(a => a.Estudiante)
                .Include(a => a.Curso)
                .Include(a => a.Docente)
                .OrderBy(a => a.AsignacionId)
                .Select(a => MapToDto(a))
                .ToListAsync();

            return Ok(asignaciones);
        }

        // GET: api/Asignaciones/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AsignacionResponseDto>> GetAsignacion(int id)
        {
            var asignacion = await _context.Asignaciones
                .AsNoTracking()
                .Include(a => a.Estudiante)
                .Include(a => a.Curso)
                .Include(a => a.Docente)
                .FirstOrDefaultAsync(a => a.AsignacionId == id);

            if (asignacion == null)
                return NotFound(new { mensaje = $"No existe la asignación con ID {id}." });

            return Ok(MapToDto(asignacion));
        }

        // POST: api/Asignaciones
        [HttpPost]
        public async Task<ActionResult<AsignacionResponseDto>> PostAsignacion(AsignacionCreateDto dto)
        {
            var error = await ValidarRelacionesAsync(dto.EstudianteId, dto.CursoId, dto.DocenteId);
            if (error != null)
                return NotFound(new { mensaje = error });

            bool duplicada = await _context.Asignaciones.AnyAsync(a =>
                a.EstudianteId == dto.EstudianteId &&
                a.CursoId == dto.CursoId &&
                a.DocenteId == dto.DocenteId);
            if (duplicada)
                return Conflict(new { mensaje = "Ya existe una asignación con el mismo estudiante, curso y docente." });

            var asignacion = new Asignacion
            {
                EstudianteId = dto.EstudianteId,
                CursoId = dto.CursoId,
                DocenteId = dto.DocenteId
            };

            _context.Asignaciones.Add(asignacion);
            await _context.SaveChangesAsync();

            // Cargar las relaciones para devolver los nombres
            await _context.Entry(asignacion).Reference(a => a.Estudiante).LoadAsync();
            await _context.Entry(asignacion).Reference(a => a.Curso).LoadAsync();
            await _context.Entry(asignacion).Reference(a => a.Docente).LoadAsync();

            return CreatedAtAction(nameof(GetAsignacion), new { id = asignacion.AsignacionId }, MapToDto(asignacion));
        }

        // PUT: api/Asignaciones/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutAsignacion(int id, AsignacionUpdateDto dto)
        {
            if (id != dto.AsignacionId)
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el del cuerpo." });

            var asignacion = await _context.Asignaciones.FindAsync(id);
            if (asignacion == null)
                return NotFound(new { mensaje = $"No existe la asignación con ID {id}." });

            var error = await ValidarRelacionesAsync(dto.EstudianteId, dto.CursoId, dto.DocenteId);
            if (error != null)
                return NotFound(new { mensaje = error });

            bool duplicada = await _context.Asignaciones.AnyAsync(a =>
                a.AsignacionId != id &&
                a.EstudianteId == dto.EstudianteId &&
                a.CursoId == dto.CursoId &&
                a.DocenteId == dto.DocenteId);
            if (duplicada)
                return Conflict(new { mensaje = "Ya existe otra asignación con el mismo estudiante, curso y docente." });

            asignacion.EstudianteId = dto.EstudianteId;
            asignacion.CursoId = dto.CursoId;
            asignacion.DocenteId = dto.DocenteId;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Asignaciones/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsignacion(int id)
        {
            var asignacion = await _context.Asignaciones.FindAsync(id);
            if (asignacion == null)
                return NotFound(new { mensaje = $"No existe la asignación con ID {id}." });

            _context.Asignaciones.Remove(asignacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Verifica que existan el estudiante, el curso y el docente
        private async Task<string?> ValidarRelacionesAsync(int estudianteId, int cursoId, int docenteId)
        {
            if (!await _context.Estudiantes.AnyAsync(e => e.EstudianteId == estudianteId))
                return $"No existe el estudiante con ID {estudianteId}.";

            if (!await _context.Cursos.AnyAsync(c => c.CursoId == cursoId))
                return $"No existe el curso con ID {cursoId}.";

            if (!await _context.Docentes.AnyAsync(d => d.DocenteId == docenteId))
                return $"No existe el docente con ID {docenteId}.";

            return null;
        }

        // Convierte la entidad en DTO para evitar referencias circulares
        private static AsignacionResponseDto MapToDto(Asignacion a) => new AsignacionResponseDto
        {
            AsignacionId = a.AsignacionId,
            EstudianteId = a.EstudianteId,
            NombreEstudiante = a.Estudiante.Nombres + " " + a.Estudiante.Apellidos,
            CursoId = a.CursoId,
            NombreCurso = a.Curso.Nombre,
            DocenteId = a.DocenteId,
            NombreDocente = a.Docente.Nombres + " " + a.Docente.Apellidos
        };
    }
}
