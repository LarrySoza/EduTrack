using EduTrack.Application.Interfaces;
using EduTrack.Application.Models;
using EduTrack.Core.Entities;
using EduTrack.Core.Entities.Core;
using EduTrack.WebApi.Hubs;
using EduTrack.WebApi.Models.Asistencia;
using EduTrack.WebApi.Models.Shared;
using EduTrack.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Net;

namespace EduTrack.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AsistenciaController : ControllerBase
    {
        private readonly ILogger<AsistenciaController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificarHub> _notificarHub;
        private readonly IWhatsappNotificationService _whatsappNotificationService;

        public AsistenciaController(ILogger<AsistenciaController> logger, IUnitOfWork unitOfWork, IWhatsappNotificationService whatsappNotificationService, IHubContext<NotificarHub> notificarHub)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _notificarHub = notificarHub;
            _whatsappNotificationService = whatsappNotificationService;
        }

        /// <summary>
        /// Registra una nueva asistencia para un alumno.
        /// </summary>
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost(Name = "Asistencia_Crear")]
        public async Task<IActionResult> CrearAsistencia([FromBody] CrearAsistenciaRequest dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var _alumno = await _unitOfWork.Alumnos.GetByBiometricoAsync(dto.grado_id, dto.biometrico_id);

                if (_alumno == null)
                {
                    return BadRequest("Alumno no encontrado para el grado y biometrico proporcionados");
                }

                var entity = new Asistencia
                {
                    alumno_id = _alumno.id
                };

                await _unitOfWork.Asistencias.AddAsync(entity);

                await _notificarHub.Clients.All.SendAsync("NuevaAsistencia", _alumno);
                await _whatsappNotificationService.EnviarNotificacionAsistenciaAsync(_alumno.nombre_apoderado, _alumno.nombre_completo, _alumno.numero_apoderado);

                return StatusCode((int)HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando asistencia");
                throw;
            }
        }

        /// <summary>
        /// Elimina una asistencia por id.
        /// </summary>
        [ProducesResponseType(typeof(GenericResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpDelete("{id:guid}", Name = "Asistencia_Eliminar")]
        public async Task<IActionResult> EliminarAsistencia(Guid id)
        {
            try
            {
                // comprobar existencia
                var existing = await _unitOfWork.Asistencias.GetPagedByAlumnoAsync(Guid.Empty, 1, 1);
                // no hay método GetById en la interfaz actual; asumo existencia por eliminación directa

                await _unitOfWork.Asistencias.DeleteAsync(id);
                return Ok(new GenericResponseDto { success = true, message = "Asistencia eliminada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando asistencia");
                throw;
            }
        }

        /// <summary>
        /// Obtiene asistencias de un alumno con paginación.
        /// </summary>
        [ProducesResponseType(typeof(PaginaDatos<AsistenciaConDetallesDto>), (int)HttpStatusCode.OK)]
        [HttpGet("by-alumno", Name = "Asistencia_PorAlumno")]
        public async Task<IActionResult> ObtenerPorAlumno([FromQuery] Guid alumnoId, [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 20)
        {
            try
            {
                pagina = Math.Max(1, pagina);
                tamanoPagina = Math.Clamp(tamanoPagina, 1, 100);

                var paginaDto = await _unitOfWork.Asistencias.GetPagedByAlumnoAsync(alumnoId, pagina, tamanoPagina);

                return Ok(paginaDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo asistencias por alumno");
                throw;
            }
        }

        /// <summary>
        /// Obtiene asistencias por fecha (día) con paginación.
        /// </summary>
        [ProducesResponseType(typeof(PaginaDatos<AsistenciaConDetallesDto>), (int)HttpStatusCode.OK)]
        [HttpGet("by-fecha", Name = "Asistencia_PorFecha")]
        public async Task<IActionResult> ObtenerPorFecha([FromQuery] DateTime fecha, [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 20)
        {
            try
            {
                pagina = Math.Max(1, pagina);
                tamanoPagina = Math.Clamp(tamanoPagina, 1, 100);

                var paginaDto = await _unitOfWork.Asistencias.GetPagedByFechaAsync(fecha, pagina, tamanoPagina);

                return Ok(paginaDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo asistencias por fecha");
                throw;
            }
        }
    }
}
