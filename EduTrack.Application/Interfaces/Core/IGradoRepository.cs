using EduTrack.Core.Entities.Core;

namespace EduTrack.Application.Interfaces.Core
{
    // Repositorio read-only para el catálogo `grado`.
    public interface IGradoRepository : IReadAllRepository<Grado, Guid>
    {
    }
}
