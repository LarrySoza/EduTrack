using EduTrack.Application.Interfaces.Core;

namespace EduTrack.Application.Interfaces
{
    public interface IUnitOfWork
    {
        IAlumnoRepository Alumnos { get; }
        IAsistenciaRepository Asistencias { get; }
        IConfiguracionRepository Configuraciones { get; }
        IGradoRepository Grados { get; }
        IRolRepository Roles { get; }
        ITipoConfiguracionRepository TipoConfiguraciones { get; }
        IUsuarioRepository Usuarios { get; }
        IUsuarioRolRepository UsuarioRoles { get; }
    }
}
