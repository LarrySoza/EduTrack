using EduTrack.Application.Interfaces;
using EduTrack.Infrastructure;
using EduTrack.WebApi.Configuration;
using EduTrack.WebApi.Models.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EduTrack.WebApi.Services
{
    /// <summary>
    /// Servicio encargado de la autenticación y generación de tokens JWT para los usuarios.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        /// <summary>
        /// Crea una nueva instancia de <see cref="AuthService"/>.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo para acceso a repositorios.</param>
        /// <param name="config">Instancia de configuración de la aplicación.</param>
        public AuthService(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _config = config;
        }

        /// <summary>
        /// Construye el token JWT y el objeto de respuesta de inicio de sesión para un usuario dado.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario (sujeto) que será incluido en el token.</param>
        /// <param name="claimsUser">Identidad que contiene las reclamaciones adicionales del usuario.</param>
        /// <returns>Un <see cref="LoginResponseDto"/> con el token JWT, tipo y fecha de expiración.</returns>
        private Task<LoginResponseDto> GetLoginRequestDto(string nombreUsuario, ClaimsIdentity claimsUser)
        {
            var jwtClass = new JwtConfigManager(_config);
            var configJwt = jwtClass.LoadConfig();

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configJwt.SecretKey));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var lifetimeSeconds = configJwt.LifetimeSeconds;
            var expires = TimeSpan.FromSeconds(lifetimeSeconds);
            var iat = new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds();

            var claimsStandard = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, nombreUsuario),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, iat.ToString(), ClaimValueTypes.Integer64)
            };

            foreach (var item in claimsUser.Claims)
            {
                claimsStandard.Add(item);
            }

            var exp = now.Add(expires);

            var tokenOptions = new JwtSecurityToken(
            issuer: configJwt.Issuer,
            audience: configJwt.Audience,
            claims: claimsStandard,
            expires: exp,
            signingCredentials: signinCredentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return Task.FromResult(new LoginResponseDto
            {
                access_token = tokenString,
                token_type = "JWT",
                expires = exp
            });
        }

        /// <summary>
        /// Autentica a un usuario usando credenciales (nombre de usuario y contraseña) y genera un token JWT si son válidas.
        /// </summary>
        /// <param name="request">Objeto que contiene el nombre de usuario y la contraseña en texto plano.</param>
        /// <returns>
        /// Un <see cref="LoginResponseDto"/> con el token JWT si las credenciales son válidas; de lo contrario, <c>null</c>.
        /// </returns>
        public async Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request)
        {
            ClaimsIdentity? claimsUser = default;

            var usuario = await _unitOfWork.Usuarios.GetByNameAsync(request.nombre_usuario);

            if (usuario != null)
            {
                if (Crypto.VerifyHashedPassword(usuario.contrasena_hash!, request.contrasena))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(JwtConfigManager.ClaimUsuarioId, usuario.id.ToString()),
                        new Claim(JwtConfigManager.ClaimSecurity, usuario.sello_seguridad.ToString())
                    };

                    var roles = await _unitOfWork.UsuarioRoles.GetAllAsync(usuario.id);
                    foreach (var rol in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, rol.nombre!));
                    }

                    claimsUser = new ClaimsIdentity(claims.ToArray());
                }
            }

            if (claimsUser == null)
                return null;

            return await GetLoginRequestDto(request.nombre_usuario, claimsUser);
        }

        /// <summary>
        /// Autentica a un usuario por su nombre de usuario y crea un token JWT asociado a una sesión especificada.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario del usuario a autenticar.</param>
        /// <param name="sesionId">Identificador de sesión que se incluirá como reclamación en el token.</param>
        /// <returns>Un <see cref="LoginResponseDto"/> con el token JWT generado.</returns>
        /// <exception cref="Exception">Se lanza si no se encuentra el usuario especificado.</exception>
        public async Task<LoginResponseDto?> AuthenticateAsync(string nombreUsuario, Guid sesionId)
        {
            ClaimsIdentity? claimsUser = default;

            var usuario = await _unitOfWork.Usuarios.GetByNameAsync(nombreUsuario);

            if (usuario != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtConfigManager.ClaimUsuarioId, usuario.id.ToString()),
                    new Claim(JwtConfigManager.ClaimSecurity, usuario.sello_seguridad.ToString()),
                    new Claim(JwtConfigManager.ClaimSesionId, sesionId.ToString())
                };

                var roles = await _unitOfWork.UsuarioRoles.GetAllAsync(usuario.id);
                foreach (var rol in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, rol.nombre!));
                }

                claimsUser = new ClaimsIdentity(claims.ToArray());
            }

            if (claimsUser == null)
                throw new Exception("Usuario no encontrado");

            return await GetLoginRequestDto(nombreUsuario, claimsUser);
        }
    }
}
