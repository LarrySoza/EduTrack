using App.Infrastructure.Repository.Core;
using EduTrack.Application.Interfaces;
using EduTrack.Application.Interfaces.Core;
using EduTrack.Infrastructure.Database;
using EduTrack.Infrastructure.Repository;
using EduTrack.Infrastructure.Repository.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduTrack.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Registrar fábrica de conexiones (una instancia lee la cadena y la guarda)
            services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
            services.AddTransient<IAlumnoRepository, AlumnoRepository>();
            services.AddTransient<IAsistenciaRepository, AsistenciaRepository>();
            services.AddTransient<IConfiguracionRepository, ConfiguracionRepository>();
            services.AddTransient<IGradoRepository, GradoRepository>();
            services.AddTransient<IRolRepository, RolRepository>();
            services.AddTransient<ITipoConfiguracionRepository, TipoConfiguracionRepository>();
            services.AddTransient<IUsuarioRepository, UsuarioRepository>();
            services.AddTransient<IUsuarioRolRepository, UsuarioRolRepository>();

            // Registrar UnitOfWork como Scoped
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
