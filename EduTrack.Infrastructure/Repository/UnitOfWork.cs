using EduTrack.Application.Interfaces;
using EduTrack.Application.Interfaces.Core;

namespace EduTrack.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public const string DefaultConnection = "DefaultConnection";

        public UnitOfWork(
            IAlumnoRepository alumnos,
            IAsistenciaRepository asistencias,
            IConfiguracionRepository configuraciones,
            IGradoRepository grados,
            IRolRepository roles,
            ITipoConfiguracionRepository tipoConfiguraciones,
            IUsuarioRepository usuarios,
            IUsuarioRolRepository usuarioRoles)
        {
            Alumnos = alumnos;
            Asistencias = asistencias;
            Configuraciones = configuraciones;
            Grados = grados;
            Roles = roles;
            TipoConfiguraciones = tipoConfiguraciones;
            Usuarios = usuarios;
            UsuarioRoles = usuarioRoles;
        }

        public IAlumnoRepository Alumnos { get; }

        public IAsistenciaRepository Asistencias { get; }

        public IConfiguracionRepository Configuraciones { get; }

        public IGradoRepository Grados { get; }

        public IRolRepository Roles { get; }

        public ITipoConfiguracionRepository TipoConfiguraciones { get; }

        public IUsuarioRepository Usuarios { get; }

        public IUsuarioRolRepository UsuarioRoles { get; }
    }
}
