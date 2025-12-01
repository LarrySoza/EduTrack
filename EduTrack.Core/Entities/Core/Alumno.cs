namespace EduTrack.Core.Entities.Core
{
    /// <summary>
    /// Tabla: alumno
    /// Registro de alumnos y su relación con grado.
    /// </summary>
    public class Alumno
    {
        public Guid id { get; set; }
        public string nombre_completo { get; set; } = string.Empty;
        public Guid grado_id { get; set; }
        public int biometrico_id { get; set; }
        public string nombre_apoderado { get; set; } = string.Empty;
        public string numero_apoderado { get; set; } = string.Empty;
    }
}
