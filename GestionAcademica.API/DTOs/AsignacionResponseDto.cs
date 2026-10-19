namespace GestionAcademica.API.DTOs
{
    public class AsignacionResponseDto
    {
        public int AsignacionId { get; set; }

        public int EstudianteId { get; set; }
        public string NombreEstudiante { get; set; } = string.Empty;

        public int CursoId { get; set; }
        public string NombreCurso { get; set; } = string.Empty;

        public int DocenteId { get; set; }
        public string NombreDocente { get; set; } = string.Empty;
    }
}
