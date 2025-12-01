using EduTrack.Core.Entities.Core;

namespace EduTrack.Application.Models
{
    public class AlumnoConGradoDto : Alumno
    {
        public string nombre_grado { get; set; } = string.Empty;
    }
}
