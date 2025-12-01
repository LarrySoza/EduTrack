namespace EduTrack.Application.Interfaces
{
    /// <summary>
    /// Repositorio de solo lectura para tablas pequeñas que permiten obtener todos los registros.
    /// </summary>
    public interface IReadAllRepository<T, TKey> where T : class
    {
        Task<T?> GetByIdAsync(TKey id);
        Task<IReadOnlyList<T>> GetAllAsync();
    }
}
