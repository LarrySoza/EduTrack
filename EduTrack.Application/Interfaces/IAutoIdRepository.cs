namespace EduTrack.Application.Interfaces
{
    /// <summary>
    /// Interfaz genérica para repositorios que devuelven un id de tipo TKey al agregar una entidad.
    /// </summary>
    public interface IAutoIdRepository<T, TKey> : IPaginatedRepository<T, TKey> where T : class
    {
        Task<TKey> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(TKey id);
    }
}
