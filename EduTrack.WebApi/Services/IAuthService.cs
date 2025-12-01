using EduTrack.WebApi.Models.Auth;

namespace EduTrack.WebApi.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request);
        Task<LoginResponseDto?> AuthenticateAsync(string nombreUsuario, Guid sesionId);
    }
}
