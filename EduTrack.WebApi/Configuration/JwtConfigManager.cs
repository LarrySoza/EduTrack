using System.Text.Json;
using System.Text.Json.Nodes;

namespace EduTrack.WebApi.Configuration
{
    public class JwtConfigManager
    {
        public const string ClaimSecurity = "stampSecurity";
        public const string ClaimUsuarioId = "usuarioId";
        public const string ClaimSesionId = "sesionId";

        private readonly IConfiguration _config;

        public JwtConfigManager(IConfiguration config)
        {
            _config = config;
        }

        public JwtOptions LoadConfig()
        {
            var _defaultKey = "n`h}G01K{I{#B[4S$_^m=ISiC:7wy[?1";

            // Leer la configuración de JWT directamente desde IConfiguration (appsettings / env)
            var jwtSection = _config.GetSection("Jwt");
            var _configJwt = jwtSection.Get<JwtOptions>() ?? new JwtOptions
            {
                Issuer = _config["Jwt:Issuer"] ?? "Api",
                Audience = _config["Jwt:Audience"] ?? "https://gaspersoft.com",
                SecretKey = _config["Jwt:SecretKey"] ?? _defaultKey,
                LifetimeSeconds = int.TryParse(_config["Jwt:LifetimeSeconds"], out var _l) ? _l : 3600
            };

            if (_configJwt.SecretKey.Length < 32)
            {
                _configJwt.SecretKey = _defaultKey;
            }

            return _configJwt;
        }

        public void UpdateConfig(JwtOptions config)
        {
            if (config == null)
            {
                throw new Exception("Configuracion no valida");
            }

            if (string.IsNullOrEmpty(config.Issuer))
            {
                throw new Exception("Issuer no valido");
            }

            if (string.IsNullOrEmpty(config.Audience))
            {
                throw new Exception("Audience no valido");
            }

            if (config.LifetimeSeconds < 300)
            {
                throw new Exception("Se permite minimo 300 segundos");
            }

            if (config.SecretKey?.Length < 32)
            {
                throw new Exception("SecretKey debe tener al menso 32 caracteres");
            }

            // Determinar la ruta de appsettings.json en el content root
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

            if (!File.Exists(appSettingsPath))
            {
                throw new FileNotFoundException("No se encontro el archivo appsettings.json", appSettingsPath);
            }

            // Leer y parsear el JSON
            var jsonText = File.ReadAllText(appSettingsPath);

            JsonObject? root;
            if (string.IsNullOrWhiteSpace(jsonText))
            {
                root = new JsonObject();
            }
            else
            {
                var node = JsonNode.Parse(jsonText);
                root = node as JsonObject ?? new JsonObject();
            }

            // Actualizar o crear la seccion Jwt
            var jwtNode = root["Jwt"] as JsonObject ?? new JsonObject();
            jwtNode["Issuer"] = config.Issuer;
            jwtNode["Audience"] = config.Audience;
            jwtNode["SecretKey"] = config.SecretKey;
            jwtNode["LifetimeSeconds"] = config.LifetimeSeconds;

            root["Jwt"] = jwtNode;

            var options = new JsonSerializerOptions { WriteIndented = true };
            var newJson = root.ToJsonString(options);

            // Escribir de vuelta el archivo
            File.WriteAllText(appSettingsPath, newJson);
        }
    }
}
