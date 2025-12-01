namespace EduTrack.Core.Entities.Core
{
    /// <summary>
    /// Tabla: rol
    /// Catálogo de roles del sistema (GUARDIA, SUPERVISOR, ADMIN, ...)
    /// </summary>
    public class Rol
    {
        public string id { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }
}
