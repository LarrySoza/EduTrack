namespace EduTrack.WebApi.Models.Usuario
{
    public class ActualizarUsuarioRequest
    {
        public required string nombre_usuario { get; set; }
        public string? nombre_completo { get; set; }
        public string? email { get; set; }
        public string? telefono { get; set; }
        public string? tipo_documento_id { get; set; }
        public string? numero_documento { get; set; }
        public string? estado { get; set; }
        public required List<string> roles_id { get; set; }
    }
}
