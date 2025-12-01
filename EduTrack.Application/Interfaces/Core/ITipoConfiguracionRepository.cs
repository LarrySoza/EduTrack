using EduTrack.Core.Entities.Core;

namespace EduTrack.Application.Interfaces.Core
{
    // Repositorio read-only para tipo_configuracion (catálogo)
    public interface ITipoConfiguracionRepository : IReadAllRepository<TipoConfiguracion, string>
    {
    }
}
