namespace EduTrack.WebApi.Models.Usuario
{
    /// <summary>
    /// DTO para la creación de un nuevo usuario.
    /// </summary>
    public class CrearUsuarioRequestDto
    {
        public required string nombre_usuario { get; set; }
        public string? nombre_completo { get; set; }
        public string? email { get; set; }
        public required string contrasena { get; set; }
        public string? telefono { get; set; }
        public string? tipo_documento_id { get; set; }
        public string? numero_documento { get; set; }
        public required List<string> roles_id { get; set; }
    }
}
