namespace EduTrack.Core.Entities.Core
{
    /// <summary>
    /// Tabla: asistencia
    /// Registros de asistencia (entrada/salida) por alumno.
    /// </summary>
    public class Asistencia
    {
        public Guid id { get; set; }
        public Guid alumno_id { get; set; }
        public DateTimeOffset fecha_hora { get; set; }
    }
}
