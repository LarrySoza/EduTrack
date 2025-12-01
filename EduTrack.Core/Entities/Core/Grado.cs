namespace EduTrack.Core.Entities.Core
{
    /// <summary>
    /// Tabla: grado
    /// Catálogo de grados/curso.
    /// </summary>
    public class Grado
    {
        public Guid id { get; set; }
        public string nombre_grado { get; set; } = string.Empty;
    }
}
