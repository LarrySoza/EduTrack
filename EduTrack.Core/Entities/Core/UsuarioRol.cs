namespace EduTrack.Core.Entities.Core
{
    /// <summary>
    /// Table: usuario_rol
    /// N:N relationship between users and roles.
    /// </summary>
    public class UsuarioRol
    {
        public Guid usuario_id { get; set; }
        public string rol_id { get; set; } = string.Empty;
    }
}
