using System.ComponentModel.DataAnnotations;

namespace GestionAcademica.API.DTOs
{
    public class AsignacionCreateDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "EstudianteId debe ser mayor que 0.")]
        public int EstudianteId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CursoId debe ser mayor que 0.")]
        public int CursoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "DocenteId debe ser mayor que 0.")]
        public int DocenteId { get; set; }
    }
}
