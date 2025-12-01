namespace EduTrack.WebApi.Models.Usuario
{
    public class CambiarContrasenaDto : NuevaContrasenaDto
    {
        public required string contrasena_actual { get; set; }
    }
}
