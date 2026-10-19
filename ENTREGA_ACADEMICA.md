# Entrega académica – API REST Gestión Académica (ASP.NET Core 8 + EF Core Code First)

## Relaciones entre entidades

- **Estudiante 1 : N Asignacion** – un estudiante puede tener muchas asignaciones; cada asignación pertenece a un único estudiante (`EstudianteId`).
- **Curso 1 : N Asignacion** – un curso puede aparecer en muchas asignaciones; cada asignación tiene un único curso (`CursoId`).
- **Docente 1 : N Asignacion** – un docente imparte muchas asignaciones; cada asignación tiene un único docente (`DocenteId`).
- `Asignacion` es la entidad central que relaciona a los tres. Las tres FK son obligatorias y se configuraron con Fluent API (`HasOne().WithMany().HasForeignKey()`) usando `DeleteBehavior.Restrict` (en SQL Server: `ON DELETE NO ACTION`), por lo que no se puede borrar un estudiante, curso o docente que tenga asignaciones (la API responde 409 Conflict).
- En las respuestas JSON se evitan referencias circulares con `[JsonIgnore]` en las colecciones y con DTOs en `AsignacionesController`.

## Modelos

### Curso.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GestionAcademica.API.Models
{
    public class Curso
    {
        [Key]
        public int CursoId { get; set; }

        [Required(ErrorMessage = "El nombre del curso es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        // RelaciÃ³n 1:N -> Un curso tiene muchas asignaciones
        [JsonIgnore]
        public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
    }
}
```

### Estudiante.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GestionAcademica.API.Models
{
    public class Estudiante
    {
        [Key]
        public int EstudianteId { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los nombres no pueden superar los 100 caracteres.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden superar los 100 caracteres.")]
        public string Apellidos { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La direcciÃ³n no puede superar los 200 caracteres.")]
        public string? Direccion { get; set; }

        // RelaciÃ³n 1:N -> Un estudiante tiene muchas asignaciones
        [JsonIgnore]
        public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
    }
}
```

### Docente.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GestionAcademica.API.Models
{
    public class Docente
    {
        [Key]
        public int DocenteId { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los nombres no pueden superar los 100 caracteres.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden superar los 100 caracteres.")]
        public string Apellidos { get; set; } = string.Empty;

        // RelaciÃ³n 1:N -> Un docente tiene muchas asignaciones
        [JsonIgnore]
        public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
    }
}
```

### Asignacion.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace GestionAcademica.API.Models
{
    public class Asignacion
    {
        [Key]
        public int AsignacionId { get; set; }

        // Claves forÃ¡neas
        public int EstudianteId { get; set; }
        public int CursoId { get; set; }
        public int DocenteId { get; set; }

        // Propiedades de navegaciÃ³n
        public Estudiante Estudiante { get; set; } = null!;
        public Curso Curso { get; set; } = null!;
        public Docente Docente { get; set; } = null!;
    }
}
```

## Controladores

### CursosController.cs

```csharp
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
```

### EstudiantesController.cs

```csharp
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
```

### DocentesController.cs

```csharp
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
```

### AsignacionesController.cs

```csharp
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
                return NotFound(new { mensaje = $"No existe la asignaciÃ³n con ID {id}." });

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
                return Conflict(new { mensaje = "Ya existe una asignaciÃ³n con el mismo estudiante, curso y docente." });

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
                return NotFound(new { mensaje = $"No existe la asignaciÃ³n con ID {id}." });

            var error = await ValidarRelacionesAsync(dto.EstudianteId, dto.CursoId, dto.DocenteId);
            if (error != null)
                return NotFound(new { mensaje = error });

            bool duplicada = await _context.Asignaciones.AnyAsync(a =>
                a.AsignacionId != id &&
                a.EstudianteId == dto.EstudianteId &&
                a.CursoId == dto.CursoId &&
                a.DocenteId == dto.DocenteId);
            if (duplicada)
                return Conflict(new { mensaje = "Ya existe otra asignaciÃ³n con el mismo estudiante, curso y docente." });

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
                return NotFound(new { mensaje = $"No existe la asignaciÃ³n con ID {id}." });

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
```

