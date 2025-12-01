using EduTrack.Application.Interfaces;
using EduTrack.WebApi.Configuration;
using EduTrack.WebApi.Hubs;
using EduTrack.WebApi.Models.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace EduTrack.WebApi
{
    public static class Extensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });
        }

        public static void ConfigureAuthJwt(this IServiceCollection services, IConfiguration config)
        {
            // Registrar configuracion Jwt para reloadOnChange
            services.Configure<JwtOptions>(config.GetSection("Jwt"));

            // Registrar el provider que crea y muta TokenValidationParameters
            services.AddSingleton<TokenValidationParametersProvider>();

            // Registrar configurador de JwtBearerOptions para asignar los TokenValidationParameters desde el provider
            services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

            // Registrar autenticacion (no asignar TokenValidationParameters aquí)
            var jwtClass = new JwtConfigManager(config);
            var cfg = jwtClass.LoadConfig();
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg.SecretKey));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer((obj) =>
            {
                // No asignar obj.TokenValidationParameters aquí porque lo hace ConfigureJwtBearerOptions
                obj.UseSecurityTokenValidators = true;
                obj.MapInboundClaims = false;

                obj.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        // Solo aplica para conexiones WebSocket como SignalR
                        var accessToken = context.Request.Query["access_token"];

                        // Aquí verifica si la solicitud es al Hub
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notificar"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },


                    OnTokenValidated = async context =>
                    {
                        //Debe existir el claim usuarioId
                        if (!context.Principal!.HasClaim(c => c.Type == JwtConfigManager.ClaimUsuarioId))
                        {
                            context.Fail($"No se encontró '{JwtConfigManager.ClaimUsuarioId}' en la sesión");
                        }

                        //Debe existir el ClaimSecurity
                        if (!context.Principal.HasClaim(c => c.Type == JwtConfigManager.ClaimSecurity))
                        {
                            context.Fail($"No se encontró '{JwtConfigManager.ClaimSecurity}' en la sesión");
                        }

                        //Valida que el sello de seguridad sea el mismo del usuario
                        var _usuarioId = context.Principal.Claims.First(c => c.Type == JwtConfigManager.ClaimUsuarioId).Value;
                        var _stampSecurity = context.Principal.Claims.First(c => c.Type == JwtConfigManager.ClaimSecurity).Value;

                        if (!Guid.TryParse(_stampSecurity, out var stampSecurityGuid))
                            throw new Exception($"{JwtConfigManager.ClaimSecurity} no es un UUID válido");

                        if (!Guid.TryParse(_usuarioId, out var usuarioGuid))
                            throw new Exception($"{JwtConfigManager.ClaimUsuarioId} no es un UUID válido");

                        // --- RESOLVER IUnitOfWork DESDE DI ---
                        var unitOfWork = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
                        var _usuario = await unitOfWork.Usuarios.GetByIdAsync(usuarioGuid);

                        if (_usuario == null)
                        {
                            context.Fail("Usuario no encontrado");
                            return;
                        }

                        if (_usuario.sello_seguridad != stampSecurityGuid || _usuario.deleted_at != null)
                        {
                            context.Fail("No Autorizado");
                        }
                    }
                };
            });
        }

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var commentsFileNameApi = Assembly.GetExecutingAssembly().GetName().Name + ".xml";
            var commentsFileApi = Path.Combine(baseDirectory, commentsFileNameApi);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "EduTrack Api",
                    Version = "v1"
                });

                if (File.Exists(commentsFileApi))
                {
                    c.IncludeXmlComments(commentsFileApi);
                }

                c.MapType<decimal>(() => new OpenApiSchema { Type = "number", Format = "decimal" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Scheme = "bearer",
                    Description = "Please insert JWT token into field"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });
        }

        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(a => a.Run(async context =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var error = context.Features.Get<IExceptionHandlerPathFeature>();

                if (error?.Error is ApiExceptionDto)
                {
                    var ex = error.Error as ApiExceptionDto;
                    await context.Response.WriteAsync(ex?.ToString() ?? "Error desconocido");
                }
                else
                {
                    await context.Response.WriteAsync(new ApiExceptionDto(error?.Error?.Message ?? "Error desconocido").ToString());
                }
            }));
        }

        public static void ConfigureHubs(this IEndpointRouteBuilder app)
        {
            app.MapHub<NotificarHub>("/hubs/notificar");
        }

        public static Guid Id(this ClaimsPrincipal user)
        {
            var _usuarioId = user?.Claims?.FirstOrDefault(c => c.Type.Equals(JwtConfigManager.ClaimUsuarioId, StringComparison.OrdinalIgnoreCase))?.Value;

            if (_usuarioId != null)
            {
                if (Guid.TryParse(_usuarioId, out var guid))
                    return guid;
            }

            throw new Exception("No se encontró el ID de usuario en el token.");
        }

        public static string UserName(this ClaimsPrincipal user)
        {
            var _nombreUsuario = user?.Claims?.FirstOrDefault(c => c.Type.Equals(JwtRegisteredClaimNames.Sub, StringComparison.OrdinalIgnoreCase))?.Value;

            if (_nombreUsuario != null)
            {
                return _nombreUsuario;
            }

            throw new Exception("No se encontró el Nombre de usuario en el token.");
        }

        public static Guid SesionId(this ClaimsPrincipal user)
        {
            var _sesionId = user?.Claims?.FirstOrDefault(c => c.Type.Equals(JwtConfigManager.ClaimSesionId, StringComparison.OrdinalIgnoreCase))?.Value;

            if (_sesionId != null)
            {
                if (Guid.TryParse(_sesionId, out var guid))
                    return guid;
            }

            throw new Exception("No se encontró el ID de sesion en el token.");
        }

        public static bool IsGuardia(this ClaimsPrincipal user)
        {
            var roles = user?.Claims?.Where(c => c.Type.Equals(ClaimTypes.Role, StringComparison.OrdinalIgnoreCase)).Select(c => c.Value).ToList();
            if (roles != null)
            {
                return roles.Contains("GUARDIA");
            }
            return false;
        }

        public static bool IsSupervisor(this ClaimsPrincipal user)
        {
            var roles = user?.Claims?.Where(c => c.Type.Equals(ClaimTypes.Role, StringComparison.OrdinalIgnoreCase)).Select(c => c.Value).ToList();
            if (roles != null)
            {
                return roles.Contains("SUPERVISOR");
            }
            return false;
        }
    }
}
