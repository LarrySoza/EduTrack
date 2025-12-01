using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EduTrack.WebApi.Configuration
{
    public class TokenValidationParametersProvider
    {
        public TokenValidationParameters Parameters { get; }

        public TokenValidationParametersProvider(IOptionsMonitor<JwtOptions> jwtMonitor)
        {
            var cfg = jwtMonitor.CurrentValue;
            Parameters = Create(cfg);

            // Suscribirse a cambios y mutar la instancia existente
            jwtMonitor.OnChange(newCfg =>
            {
                // Actualizar los campos relevantes
                Parameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(newCfg.SecretKey));
                Parameters.ValidIssuer = newCfg.Issuer;
                Parameters.ValidAudience = newCfg.Audience;
                Parameters.ValidateIssuer = true;
                Parameters.ValidateAudience = true;
                Parameters.ValidateIssuerSigningKey = true;
                Parameters.ValidateLifetime = true;
                Parameters.ClockSkew = TimeSpan.Zero;
            });
        }

        private static TokenValidationParameters Create(JwtOptions cfg) => new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg.SecretKey)),
            ValidIssuer = cfg.Issuer,
            ValidAudience = cfg.Audience,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }
}
