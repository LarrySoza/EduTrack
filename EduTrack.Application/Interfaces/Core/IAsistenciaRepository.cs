using EduTrack.Application.Models;
using EduTrack.Core.Entities;
using EduTrack.Core.Entities.Core;

namespace EduTrack.Application.Interfaces.Core
{
    // Repositorio para la tabla `asistencia`.
    public interface IAsistenciaRepository
    {
        Task AddAsync(Asistencia entity);
        Task DeleteAsync(Guid id);
        Task<PaginaDatos<AsistenciaConDetallesDto>> GetPagedByFechaAsync(DateTime fecha, int page = 1, int pageSize = 20);
        Task<PaginaDatos<AsistenciaConDetallesDto>> GetPagedByAlumnoAsync(Guid alumnoId, int page = 1, int pageSize = 20);
    }
}
