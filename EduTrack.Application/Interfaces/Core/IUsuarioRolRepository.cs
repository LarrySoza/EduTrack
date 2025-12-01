using EduTrack.Core.Entities.Core;

namespace EduTrack.Application.Interfaces.Core
{
    // Repositorio para la tabla usuario_rol (tiene auditoría)
    public interface IUsuarioRolRepository
    {
        Task<IReadOnlyList<Rol>> GetAllAsync(Guid usuario_id);
    }
}
