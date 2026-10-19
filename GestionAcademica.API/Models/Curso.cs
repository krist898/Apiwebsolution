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

        // Relación 1:N -> Un curso tiene muchas asignaciones
        [JsonIgnore]
        public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
    }
}
