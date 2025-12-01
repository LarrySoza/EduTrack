using EduTrack.Core.Entities.Core;

namespace EduTrack.Application.Interfaces.Core
{
    // Repositorio sólo lectura para el catálogo `rol` (datos iniciales)
    public interface IRolRepository : IReadAllRepository<Rol, string>
    {
    }
}
