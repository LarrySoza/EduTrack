namespace EduTrack.WebApi.Services
{
    public interface IWhatsappNotificationService
    {
        Task<(bool success, string responseContent)> EnviarNotificacionAsistenciaAsync(string nombrePadre, string nombreHijo, string numeroCelular);
    }
}
