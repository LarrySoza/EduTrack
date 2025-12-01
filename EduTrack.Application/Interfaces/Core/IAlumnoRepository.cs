using EduTrack.Application.Models;
using EduTrack.Core.Entities;
using EduTrack.Core.Entities.Core;

namespace EduTrack.Application.Interfaces.Core
{
    // Repositorio para la entidad `alumno`.
    public interface IAlumnoRepository
    {
        Task<Guid> AddAsync(Alumno entity);
        Task UpdateAsync(Alumno entity);
        Task DeleteAsync(Guid id);
        Task<AlumnoConGradoDto?> GetByBiometricoAsync(Guid gradoId, int biometricoId);
        Task<AlumnoConGradoDto?> GetByIdAsync(Guid id);
        Task<PaginaDatos<AlumnoConGradoDto>> GetPagedAsync(string? search, int page = 1, int pageSize = 20);
    }
}
