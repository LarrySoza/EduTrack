using EduTrack.Application.Interfaces;
using EduTrack.WebApi.Models.Auth;
using EduTrack.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EduTrack.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;

        public LoginController(IConfiguration config, ILogger<LoginController> logger, IUnitOfWork unitOfWork, IAuthService authService)
        {
            _config = config;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _authService = authService;
        }

        /// <summary>
        /// Autentica un usuario y emite un token JWT si las credenciales son válidas.
        /// </summary>
        /// <param name="loginRequest">Objeto con las credenciales de inicio de sesión (por ejemplo, nombre de usuario y contraseña).</param>
        /// <returns>
        /// 200 OK con <see cref="LoginResponseDto"/> cuando la autenticación es exitosa.
        /// 401 Unauthorized cuando las credenciales no son válidas.
        /// 400 BadRequest cuando el request es inválido.
        /// </returns>
        /// <response code="200">Autenticación exitosa; devuelve el token y metadatos en <see cref="LoginResponseDto"/>.</response>
        /// <response code="401">Credenciales inválidas.</response>
        /// <response code="400">Solicitud con formato inválido o datos faltantes.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// <remarks>
        /// El token emitido incluye claims estándar (sub, jti, iat) y claims personalizados con el identificador
        /// del usuario y el sello de seguridad. Use siempre HTTPS para transmitir credenciales y proteja el token
        /// en el cliente (almacenamiento seguro, envío en Authorization header: "Bearer {token}").
        /// </remarks>
        [ProducesResponseType(typeof(LoginResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost(Name = "Auth_Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                var response = await _authService.AuthenticateAsync(loginRequest);
                if (response == null)
                {
                    return Unauthorized(null);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
