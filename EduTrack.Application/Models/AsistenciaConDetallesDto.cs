using EduTrack.Core.Entities.Core;

namespace EduTrack.Application.Models
{
    public class AsistenciaConDetallesDto : Asistencia
    {
        public string nombre_completo_alumno { get; set; } = string.Empty;
        public Guid grado_id { get; set; }
        public string nombre_grado { get; set; } = string.Empty;
    }
}
