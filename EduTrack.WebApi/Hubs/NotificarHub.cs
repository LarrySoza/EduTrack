using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EduTrack.WebApi.Hubs
{
    [Authorize]
    public class NotificarHub : Hub
    {
        private readonly ILogger<NotificarHub> _logger;
        public NotificarHub(ILogger<NotificarHub> logger) => _logger = logger;

        public override Task OnConnectedAsync()
        {
            var http = Context.GetHttpContext();
            _logger.LogInformation("Hub conectado: ConnId={id} PathBase={base} Path={path} Query={query}",
                Context.ConnectionId, http?.Request.PathBase, http?.Request.Path, http?.Request.QueryString);
            return base.OnConnectedAsync();
        }
    }
}
