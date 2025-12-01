namespace EduTrack.WebApi.Models.Usuario
{
    /// <summary>
    /// DTO que expone la información del usuario.
    /// </summary>
    public class UsuarioDto
    {
        public Guid id { get; set; }
        public string nombre_usuario { get; set; } = default!;
        public string? nombre_completo { get; set; }
        public string? email { get; set; }
        public string? telefono { get; set; }
        public string? tipo_documento_id { get; set; }
        public string? numero_documento { get; set; }
        public string? estado { get; set; }
        public List<RolDto> roles { get; set; } = new();
    }
}
