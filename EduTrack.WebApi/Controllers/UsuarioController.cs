using AutoMapper;
using EduTrack.Application.Interfaces;
using EduTrack.WebApi.Models.Shared;
using EduTrack.WebApi.Models.Usuario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EduTrack.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly ILogger<UsuarioController> _logger;
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UsuarioController(IConfiguration config, ILogger<UsuarioController> logger, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _config = config;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Cambia la contraseña del usuario autenticado.
        /// </summary>
        /// <param name="claves">Objeto con las propiedades:
        /// - contrasena_actual: la contraseña actual del usuario.
        /// - contrasena_nueva: la nueva contraseña a establecer.</param>
        /// <returns>ActionResult con un `GenericResponseDto`. En caso de éxito devuelve `success = true` y un mensaje descriptivo.</returns>
        /// <remarks>
        /// Requiere que el usuario esté autenticado. Si la contraseña actual no coincide
        /// se lanzará una excepción y se registrará el error. El repositorio debe encargarse
        /// del hash de la nueva contraseña y de cualquier actualización de sello de seguridad.
        /// </remarks>
        [ProducesResponseType(typeof(GenericResponseDto), (int)HttpStatusCode.OK)]
        [HttpPatch("me/password", Name = "Usuarios_CambiarContrasena")]
        public async Task<IActionResult> CambiarContrasena([FromBody] CambiarContrasenaDto claves)
        {
            try
            {
                // Validar que la contraseña actual coincida con la almacenada para el usuario autenticado.
                if (!await _unitOfWork.Usuarios.ValidatePasswordAsync(User.Id(), claves.contrasena_actual))
                {
                    throw new Exception("La contraseña actual es incorrecta");
                }

                // Actualizar la contraseña por la nueva proporcionada (se espera que el repositorio haga el hashing correspondiente).
                await _unitOfWork.Usuarios.UpdatePasswordAsync(User.Id(), claves.contrasena_nueva);

                // Retornar respuesta genérica indicando éxito.
                return Ok(new GenericResponseDto()
                {
                    success = true,
                    message = "Contraseña actualizada correctamente"
                });
            }
            catch (Exception ex)
            {
                // Registrar el error y volver a lanzar para que el middleware de la API lo gestione.
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
