namespace EduTrack.Application.Interfaces
{
    /// <summary>
    /// Interfaz genérica para repositorios que no devuelven un id al agregar una entidad.
    /// </summary>
    public interface IGenericRepository<T, TKey> : IReadAllRepository<T, TKey> where T : class
    {
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(TKey id);
    }
}
