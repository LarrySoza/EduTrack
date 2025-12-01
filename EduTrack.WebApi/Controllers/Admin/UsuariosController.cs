using AutoMapper;
using EduTrack.Application.Interfaces;
using EduTrack.Core.Entities;
using EduTrack.Core.Entities.Core;
using EduTrack.Infrastructure;
using EduTrack.WebApi.Models.Shared;
using EduTrack.WebApi.Models.Usuario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EduTrack.WebApi.Controllers.Admin
{
    /// <summary>
    /// Controller para operaciones administrativas sobre usuarios.
    /// Acceso restringido a usuarios con el rol "ADMIN".
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [ApiController]
    [Route("admin/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly ILogger<UsuariosController> _logger;
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UsuariosController(IConfiguration config, ILogger<UsuariosController> logger, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _config = config;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene los datos públicos de un usuario por su identificador, incluyendo la lista de roles asignados.
        /// </summary>
        /// <param name="id">Identificador (GUID) del usuario.</param>
        /// <returns>
        /// - 200 OK con <see cref="UsuarioDto"/> cuando el usuario existe.
        /// - 404 Not Found cuando no se encuentra el usuario.
        /// </returns>
        /// <response code="200">Usuario encontrado y retornado como <see cref="UsuarioDto"/>.</response>
        /// <response code="404">Usuario no encontrado.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// <remarks>
        /// Este endpoint no expone información sensible (por ejemplo, contrasena_hash o sello_seguridad).
        /// Los roles se obtienen desde el repositorio de relaciones usuario-rol y se mapean a <see cref="RolDto"/>.
        /// </remarks>
        [ProducesResponseType(typeof(UsuarioDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpGet("{id:guid}", Name = "Admin_Usuarios_ObtenerPorId")]
        public async Task<IActionResult> GetUsuario(Guid id)
        {
            try
            {
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
                if (usuario == null) return NotFound();

                // Mapear entidad Usuario -> UsuarioDto
                var dto = _mapper.Map<UsuarioDto>(usuario);

                // Cargar roles y mapear a RolDto
                var roles = await _unitOfWork.UsuarioRoles.GetAllAsync(id);
                dto.roles = _mapper.Map<List<RolDto>>(roles);

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Lista usuarios con paginación. Permite filtrar por texto que coincida
        /// con el `nombre_usuario`, `email` o `numero_documento`.
        /// </summary>
        /// <param name="filtro">Texto de búsqueda opcional; si es nulo o vacío devuelve todos los registros paginados.</param>
        /// <param name="pagina">Número de página (mínimo1).</param>
        /// <param name="tamanoPagina">Tamaño de página (se limita entre1 y100).</param>
        /// <param name="includeRoles">Si es true, incluye la lista de roles asignados para cada usuario en la respuesta. Cuando es true, el repositorio intentará cargar los roles para todos los usuarios retornados en una sola consulta para evitar el problema N+1. Por defecto es false.</param>
        /// <returns>200 OK con un objeto <see cref="PaginaDatos{UsuarioDto}"/> que contiene la lista paginada de usuarios.</returns>
        /// <response code="200">Resultados paginados devueltos correctamente.</response>
        /// <remarks>
        /// Este endpoint delega la búsqueda al repositorio `IUsuarioRepository.FindAsync` a través de `IUnitOfWork.Usuarios`.
        /// Los resultados se mapean usando AutoMapper a `UsuarioDto` para evitar exponer información sensible.
        /// </remarks>
        [ProducesResponseType(typeof(PaginaDatos<UsuarioDto>), (int)HttpStatusCode.OK)]
        [HttpGet(Name = "Admin_Usuarios_ObtenerPagina")]
        public async Task<IActionResult> ObtenerPaginaUsuarios([FromQuery] string? filtro,
                                                       [FromQuery] int pagina = 1,
                                                       [FromQuery] int tamanoPagina = 20,
                                                       [FromQuery] bool includeRoles = false)
        {
            try
            {
                // Normalizar parámetros de paginación: asegurar página >=1 y tamanoPagina dentro de un rango razonable.
                pagina = Math.Max(1, pagina);
                tamanoPagina = Math.Clamp(tamanoPagina, 1, 100);

                // Delegar la búsqueda paginada al repositorio de usuarios.
                var resultado = await _unitOfWork.Usuarios.GetPagedAsync(filtro, pagina, tamanoPagina, includeRoles);

                // Mapear entidades Usuario -> UsuarioDto para la respuesta pública.
                var dataDto = _mapper.Map<List<UsuarioDto>>(resultado.data);

                // Construir objeto de paginación para la respuesta.
                var paginaDto = new PaginaDatos<UsuarioDto>
                {
                    total = resultado.total,
                    page = resultado.page,
                    pageSize = resultado.pageSize,
                    totalPages = resultado.totalPages,
                    data = dataDto
                };

                return Ok(paginaDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Crea un nuevo usuario con los datos proporcionados y retorna su identificador.
        /// </summary>
        /// <param name="dto">Datos para la creación del usuario.</param>
        /// <returns>El <see cref="Guid"/> del usuario creado.</returns>
        /// <response code="201">Usuario creado correctamente; devuelve el id (Guid) y Location hacia el recurso.</response>
        /// <response code="400">Modelo inválido.</response>
        [ProducesResponseType(typeof(GenericResponseIdDto<Guid>), (int)HttpStatusCode.OK)]
        [HttpPost(Name = "Admin_Usuarios_Crear")]
        public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioRequestDto dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var _usuario = _mapper.Map<Usuario>(dto);

                // generar hash y sello
                _usuario.contrasena_hash = Crypto.HashPassword(dto.contrasena);
                _usuario.sello_seguridad = Guid.NewGuid();
                _usuario.SetCreationAudit(DateTimeOffset.UtcNow, User.Id());

                // convertir roles (si vienen ids)
                var roles = (dto.roles_id ?? new List<string>()).Select(id => new Rol { id = id }).ToList();

                var id = await _unitOfWork.Usuarios.AddAsync(_usuario, roles);

                // CreatedAtRoute espera un objeto con los valores de ruta; pasar el Guid directamente provoca error de compilación.
                return CreatedAtRoute("Admin_Usuarios_ObtenerPorId", new { id = id }, new GenericResponseIdDto<Guid>(id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Actualiza la contraseña de un usuario identificado por su <paramref name="id"/>.
        /// Este endpoint es para uso administrativo; cambia la clave del usuario objetivo.
        /// </summary>
        /// <param name="id">Identificador (Guid) del usuario cuya contraseña será actualizada.</param>
        /// <param name="clave">Objeto que contiene la nueva contraseña (<see cref="NuevaContrasenaDto"/>).</param>
        /// <returns>Un <see cref="GenericResponseDto"/> indicando el resultado de la operación.</returns>
        /// <response code="200">Contraseña actualizada correctamente.</response>
        /// <response code="404">Usuario no encontrado (cuando la ruta no casó con un guid válido o no existe).</response>
        [ProducesResponseType(typeof(GenericResponseDto), (int)HttpStatusCode.OK)]
        [HttpPatch("{id:guid}/password", Name = "Admin_Usuarios_ActualizarContrasena")]
        public async Task<IActionResult> CambiarContrasena(Guid id, [FromBody] NuevaContrasenaDto clave)
        {
            try
            {
                await _unitOfWork.Usuarios.UpdatePasswordAsync(id, clave.contrasena_nueva);

                return Ok(new GenericResponseDto()
                {
                    success = true,
                    message = "Contraseña actualizada correctamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Actualiza los datos y roles de un usuario existente (administrativo).
        /// </summary>
        /// <param name="id">Identificador del usuario a actualizar.</param>
        /// <param name="dto">Datos para actualizar el usuario.</param>
        [ProducesResponseType(typeof(GenericResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpPut("{id:guid}", Name = "Admin_Usuarios_Actualizar")]
        public async Task<IActionResult> ActualizarUsuario(Guid id, [FromBody] ActualizarUsuarioRequest dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
                if (usuario == null) return NotFound();

                // Actualizar campos permitidos
                usuario.nombre_usuario = dto.nombre_usuario;
                usuario.email = dto.email;
                usuario.telefono = dto.telefono;
                usuario.tipo_documento_id = dto.tipo_documento_id;
                usuario.numero_documento = dto.numero_documento;

                if (!string.IsNullOrWhiteSpace(dto.estado))
                {
                    usuario.estado = dto.estado!;
                }

                // Auditar la actualización
                usuario.SetUpdateAudit(DateTimeOffset.UtcNow, User.Id());

                // Convertir roles
                var roles = (dto.roles_id ?? new List<string>()).Select(rid => new Rol { id = rid }).ToList();

                await _unitOfWork.Usuarios.UpdateAsync(usuario, roles);

                return Ok(new GenericResponseDto { success = true, message = "Usuario actualizado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Elimina (soft delete) un usuario por su identificador.
        /// </summary>
        /// <param name="id">Identificador (GUID) del usuario a eliminar.</param>
        /// <returns>200 OK si se eliminó correctamente,404 si no existe.</returns>
        [ProducesResponseType(typeof(GenericResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpDelete("{id:guid}", Name = "Admin_Usuarios_Eliminar")]
        public async Task<IActionResult> EliminarUsuario(Guid id)
        {
            try
            {
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
                if (usuario == null) return NotFound();

                await _unitOfWork.Usuarios.DeleteAsync(id);

                return Ok(new GenericResponseDto { success = true, message = "Usuario eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
