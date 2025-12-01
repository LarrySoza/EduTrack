using EduTrack.Infrastructure;
using EduTrack.WebApi.Configuration;
using EduTrack.WebApi.Services;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using NLog;
using NLog.Web;

namespace EduTrack.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
            logger.Debug("init main");

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Load optional secrets file (ignored in git)
                builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);

                builder.Services.ConfigureCors();
                builder.Services.AddInfrastructure(builder.Configuration);

                // Registrar servicio de autenticacion
                builder.Services.AddScoped<IAuthService, AuthService>();
                builder.Services.AddScoped<IWhatsappNotificationService, WhatsappNotificationService>();

                // Register AutoMapper using local extension
                builder.Services.AddAutoMapper(typeof(Program).Assembly);

                builder.Services.ConfigureAuthJwt(builder.Configuration);
                builder.Services.AddControllers(options =>
                {
                    options.Conventions.Add(new RouteTokenTransformerConvention(new LowercaseUrls()));
                });

                // NLog: Setup NLog for Dependency injection
                builder.Logging.ClearProviders();
                builder.Host.UseNLog();

                builder.Services.ConfigureSwagger();

                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();

                builder.Services.AddSignalR(options =>
                {
                    options.EnableDetailedErrors = true;
                });

                var app = builder.Build();

                // Establece el prefijo público /api
                app.UsePathBase("/api");

                // Configure the HTTP request pipeline.
                //if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseSwagger();
                    app.UseSwaggerUI(c => c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "EduTrack.WebApi v1"));
                }

                //Usar el manejador de error
                app.ConfigureExceptionHandler();

                app.UseRouting();

                app.UseCors("CorsPolicy");

                app.UseAuthentication();

                app.UseAuthorization();

                app.MapControllers();

                app.ConfigureHubs();

                app.Run();
            }
            catch (Exception ex)
            {
                // NLog: catch setup errors
                logger.Error(ex, "Programa detenido debido a excepción");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }
    }
}
