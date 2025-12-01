using AutoMapper;
using EduTrack.Application.Interfaces;
using EduTrack.WebApi.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EduTrack.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class TablasController : ControllerBase
    {
        private readonly ILogger<TablasController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TablasController(ILogger<TablasController> logger, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene todos los roles.
        /// </summary>
        /// <returns>Lista de <see cref="TablaDto"/> con id y nombre.</returns>
        [ProducesResponseType(typeof(List<TablaDto>), (int)HttpStatusCode.OK)]
        [HttpGet("rol", Name = "Tablas_ObtenerRoles")]
        public async Task<IActionResult> ObtenerRoles()
        {
            try
            {
                var items = await _unitOfWork.Roles.GetAllAsync();
                var dtos = items.Select(i => new TablaDto { id = i.id, nombre = i.nombre }).ToList();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo rol");
                throw;
            }
        }

        /// <summary>
        /// Obtiene todos los grados.
        /// </summary>
        /// <returns>Lista de <see cref="TablaDto"/> con id y nombre.</returns>
        [ProducesResponseType(typeof(List<TablaDto>), (int)HttpStatusCode.OK)]
        [HttpGet("grado", Name = "Tablas_ObtenerGrados")]
        public async Task<IActionResult> ObtenerGrados()
        {
            try
            {
                var items = await _unitOfWork.Grados.GetAllAsync();
                var dtos = items.Select(i => new TablaDto { id = i.id.ToString(), nombre = i.nombre_grado }).ToList();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo grado");
                throw;
            }
        }
    }
}
