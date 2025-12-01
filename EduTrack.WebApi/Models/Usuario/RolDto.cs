namespace EduTrack.WebApi.Models.Usuario
{
    /// <summary>
    /// DTO que representa un rol asignable a un usuario.
    /// </summary>
    public class RolDto
    {
        public string id { get; set; } = default!;
        public string? nombre { get; set; }
    }
}
