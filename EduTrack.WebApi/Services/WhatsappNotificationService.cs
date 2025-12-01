using RestSharp;

namespace EduTrack.WebApi.Services
{
    public class WhatsappNotificationService : IWhatsappNotificationService
    {
        private readonly string _endpointUrl;
        private readonly string _username;
        private readonly string _password;

        public WhatsappNotificationService(IConfiguration configuration)
        {
            _endpointUrl = configuration["Whatsapp:EndpointUrl"] ?? throw new ArgumentNullException("Whatsapp:EndpointUrl");
            _username = configuration["Whatsapp:Username"] ?? string.Empty;
            _password = configuration["Whatsapp:Password"] ?? string.Empty;
        }

        public async Task<(bool success, string responseContent)> EnviarNotificacionAsistenciaAsync(string nombrePadre, string nombreHijo, string numeroCelular)
        {
            var options = new RestClientOptions(_endpointUrl)
            {
                ThrowOnAnyError = false,
                Timeout = new TimeSpan(0, 0, 10)
            };

            var client = new RestClient(options);
            var request = new RestRequest();

            if (!string.IsNullOrEmpty(_username))
            {
                var basic = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(_username + ":" + _password));
                request.AddHeader("Authorization", "Basic " + basic);
            }

            // Form data
            request.AddParameter("ot", "asistencia_registrada");
            request.AddParameter("p1", nombrePadre);
            request.AddParameter("p2", nombreHijo);
            request.AddParameter("number", numeroCelular);

            var response = await client.ExecutePostAsync(request);

            return (response.IsSuccessful, response.Content ?? string.Empty);
        }
    }
}
