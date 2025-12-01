using EduTrack.WebApi.Configuration;
using EduTrack.WebApi.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EduTrack.WebApi.Controllers.Admin
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = "ADMIN")]
    [ApiController]
    [Route("admin/[controller]")]
    public class JwtController : ControllerBase
    {
        private readonly ILogger<JwtController> _logger;
        private readonly IConfiguration _config;


        public JwtController(IConfiguration config, ILogger<JwtController> logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Leer la configuracion JWT
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(JwtOptions), (int)HttpStatusCode.OK)]
        [HttpGet("config", Name = "Admin_Jwt_LeerConfig")]
        public ActionResult LeerConfiguracionJwt()
        {
            try
            {
                var _jwtClass = new JwtConfigManager(_config);

                var _configJwt = _jwtClass.LoadConfig();

                return Ok(_configJwt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Actualizar la configuracion JWT
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(GenericResponseDto), (int)HttpStatusCode.OK)]
        [HttpPut("config", Name = "Admin_Jwt_ActualizarConfig")]
        public IActionResult ActualizarJwtConfig([FromBody] JwtOptions config)
        {
            try
            {
                var _jwtClass = new JwtConfigManager(_config);

                _jwtClass.UpdateConfig(config);

                return Ok(new GenericResponseDto
                {
                    success = true,
                    message = "Configuración actualizada correctamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
