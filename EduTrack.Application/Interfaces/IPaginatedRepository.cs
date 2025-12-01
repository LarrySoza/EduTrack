using EduTrack.Core.Entities;

namespace EduTrack.Application.Interfaces
{
    /// <summary>
    /// Repositorio de solo lectura orientado a búsquedas y paginación para tablas con muchos registros.
    /// </summary>
    public interface IPaginatedRepository<T, TKey> where T : class
    {
        Task<T?> GetByIdAsync(TKey id);
        Task<PaginaDatos<T>> GetPagedAsync(string? search, int page = 1, int pageSize = 20);
    }
}
