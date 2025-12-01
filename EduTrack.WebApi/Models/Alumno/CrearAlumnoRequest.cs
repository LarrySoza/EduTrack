namespace EduTrack.WebApi.Models.Alumno
{
    public class CrearAlumnoRequest
    {
        public string nombre_completo { get; set; } = string.Empty;
        public Guid grado_id { get; set; }
        public int biometrico_id { get; set; }
        public string nombre_apoderado { get; set; } = string.Empty;
        public string numero_apoderado { get; set; } = string.Empty;
    }
}
