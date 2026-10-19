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

        // Relación 1:N -> Un docente tiene muchas asignaciones
        [JsonIgnore]
        public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
    }
}
