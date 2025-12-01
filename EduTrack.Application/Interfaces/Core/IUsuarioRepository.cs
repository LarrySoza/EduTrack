using EduTrack.Core.Entities;
using EduTrack.Core.Entities.Core;

namespace EduTrack.Application.Interfaces.Core
{
    // Repositorio para `usuario` (operaciones completas), clave Guid
    public interface IUsuarioRepository
    {
        Task UpdateAsync(Usuario entity, List<Rol> roles);

        Task DeleteAsync(Guid id);

        /// <summary>
        /// Obtener usuario por su nombre de usuario (login).
        /// </summary>
        Task<Usuario?> GetByNameAsync(string nombreUsuario);

        /// <summary>
        /// Valida que la contraseña plana `clave` coincida con el hash almacenado para el usuario.
        /// Implementación típica: recuperar el hash y usar la utilidad de hashing/verificación.
        /// Devuelve true si la clave es válida.
        /// </summary>
        Task<bool> ValidatePasswordAsync(Guid usuarioId, string clave);

        /// <summary>
        /// Actualiza la contraseña del usuario (recibe la clave plana).
        /// Implementación típica: generar hash, guardar en base de datos y actualizar sello_seguridad si procede.
        /// </summary>
        Task UpdatePasswordAsync(Guid usuarioId, string clave);

        /// <summary>
        /// Registra un nuevo usuario junto con sus roles asignados.
        /// </summary>
        Task<Guid> AddAsync(Usuario usuario, List<Rol> roles);

        Task<Usuario?> GetByIdAsync(Guid id);

        /// <summary>
        /// Búsqueda paginada de usuarios con opción de incluir los roles asignados en una sola consulta.
        /// Evita el problema N+1 cuando `includeRoles` es true cargando los roles para todos los usuarios retornados
        /// en una única consulta adicional.
        /// </summary>
        Task<PaginaDatos<Usuario>> GetPagedAsync(string? search, int page = 1, int pageSize = 20, bool includeRoles = false);
    }
}
