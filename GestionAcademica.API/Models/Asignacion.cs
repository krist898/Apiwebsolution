using System.ComponentModel.DataAnnotations;

namespace GestionAcademica.API.Models
{
    public class Asignacion
    {
        [Key]
        public int AsignacionId { get; set; }

        // Claves foráneas
        public int EstudianteId { get; set; }
        public int CursoId { get; set; }
        public int DocenteId { get; set; }

        // Propiedades de navegación
        public Estudiante Estudiante { get; set; } = null!;
        public Curso Curso { get; set; } = null!;
        public Docente Docente { get; set; } = null!;
    }
}
