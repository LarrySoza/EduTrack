using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace EduTrack.WebApi.Configuration
{
    public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly TokenValidationParametersProvider _provider;
        public ConfigureJwtBearerOptions(TokenValidationParametersProvider provider) => _provider = provider;

        public void Configure(JwtBearerOptions options) => Configure(Options.DefaultName, options);

        public void Configure(string? name, JwtBearerOptions options)
        {
            // Asignar la instancia que será mutada en caliente
            options.TokenValidationParameters = _provider.Parameters;
        }
    }
}
