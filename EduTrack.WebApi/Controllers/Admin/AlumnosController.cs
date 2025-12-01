using AutoMapper;
using EduTrack.Application.Interfaces;
using EduTrack.Application.Models;
using EduTrack.Core.Entities;
using EduTrack.Core.Entities.Core;
using EduTrack.WebApi.Models.Alumno;
using EduTrack.WebApi.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EduTrack.WebApi.Controllers.Admin
{
    /// <summary>
    /// Endpoints administrativos para gestionar alumnos.
    /// Acceso restringido a usuarios con el rol "ADMIN".
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [ApiController]
    [Route("admin/[controller]")]
    public class AlumnosController : ControllerBase
    {
        private readonly ILogger<AlumnosController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AlumnosController(ILogger<AlumnosController> logger, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene los datos públicos de un alumno por su identificador.
        /// </summary>
        /// <param name="id">Identificador (GUID) del alumno.</param>
        /// <returns>
        /// - 200 OK con <see cref="AlumnoConGradoDto"/> cuando el alumno existe.
        /// - 404 Not Found cuando no se encuentra el alumno.
        /// </returns>
        [ProducesResponseType(typeof(AlumnoConGradoDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpGet("{id:guid}", Name = "Admin_Alumnos_ObtenerPorId")]
        public async Task<IActionResult> GetAlumno(Guid id)
        {
            try
            {
                var alumno = await _unitOfWork.Alumnos.GetByIdAsync(id);
                if (alumno == null) return NotFound();
                return Ok(alumno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo alumno por id");
                throw;
            }
        }

        /// <summary>
        /// Lista alumnos con paginación y búsqueda por texto.
        /// </summary>
        /// <param name="filtro">Texto de búsqueda opcional que coincide con el nombre, apoderado o número.</param>
        /// <param name="pagina">Número de página (>=1).</param>
        /// <param name="tamanoPagina">Tamaño de página (1..100).</param>
        /// <returns>200 OK con un objeto <see cref="PaginaDatos{AlumnoConGradoDto}"/> que contiene la lista paginada.</returns>
        [ProducesResponseType(typeof(PaginaDatos<AlumnoConGradoDto>), (int)HttpStatusCode.OK)]
        [HttpGet(Name = "Admin_Alumnos_ObtenerPagina")]
        public async Task<IActionResult> ObtenerPaginaAlumnos([FromQuery] string? filtro,
                                                               [FromQuery] int pagina = 1,
                                                               [FromQuery] int tamanoPagina = 20)
        {
            try
            {
                pagina = Math.Max(1, pagina);
                tamanoPagina = Math.Clamp(tamanoPagina, 1, 100);

                var resultado = await _unitOfWork.Alumnos.GetPagedAsync(filtro, pagina, tamanoPagina);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pagina de alumnos");
                throw;
            }
        }

        /// <summary>
        /// Crea un nuevo alumno con los datos proporcionados.
        /// </summary>
        /// <param name="dto">Datos para la creación del alumno.</param>
        /// <returns>201 Created con el id del alumno creado.</returns>
        [ProducesResponseType(typeof(GenericResponseIdDto<Guid>), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost(Name = "Admin_Alumnos_Crear")]
        public async Task<IActionResult> CrearAlumno([FromBody] CrearAlumnoRequest dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var entity = _mapper.Map<Alumno>(dto);
                var id = await _unitOfWork.Alumnos.AddAsync(entity);
                return CreatedAtRoute("Admin_Alumnos_ObtenerPorId", new { id = id }, new GenericResponseIdDto<Guid>(id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando alumno");
                throw;
            }
        }

        /// <summary>
        /// Actualiza los datos de un alumno existente.
        /// </summary>
        /// <param name="id">Identificador del alumno a actualizar.</param>
        /// <param name="dto">Datos con los cambios a aplicar.</param>
        /// <returns>200 OK cuando la actualización fue exitosa, 404 si no existe el alumno.</returns>
        [ProducesResponseType(typeof(GenericResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpPut("{id:guid}", Name = "Admin_Alumnos_Actualizar")]
        public async Task<IActionResult> ActualizarAlumno(Guid id, [FromBody] ActualizarAlumnoRequest dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var alumno = await _unitOfWork.Alumnos.GetByIdAsync(id);
                if (alumno == null) return NotFound();

                // mapear cambios hacia la entidad core
                var updated = _mapper.Map<Alumno>(dto);
                // aplicar campos manualmente para evitar reemplazar id
                alumno.nombre_completo = updated.nombre_completo;
                alumno.grado_id = updated.grado_id;
                alumno.biometrico_id = updated.biometrico_id;
                alumno.nombre_apoderado = updated.nombre_apoderado;
                alumno.numero_apoderado = updated.numero_apoderado;

                await _unitOfWork.Alumnos.UpdateAsync(alumno);

                return Ok(new GenericResponseDto { success = true, message = "Alumno actualizado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando alumno");
                throw;
            }
        }

        /// <summary>
        /// Elimina un alumno por su identificador.
        /// </summary>
        /// <param name="id">Identificador (GUID) del alumno a eliminar.</param>
        /// <returns>200 OK si se eliminó correctamente, 404 si no existe.</returns>
        [ProducesResponseType(typeof(GenericResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpDelete("{id:guid}", Name = "Admin_Alumnos_Eliminar")]
        public async Task<IActionResult> EliminarAlumno(Guid id)
        {
            try
            {
                var alumno = await _unitOfWork.Alumnos.GetByIdAsync(id);
                if (alumno == null) return NotFound();

                await _unitOfWork.Alumnos.DeleteAsync(id);

                return Ok(new GenericResponseDto { success = true, message = "Alumno eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando alumno");
                throw;
            }
        }
    }
}
