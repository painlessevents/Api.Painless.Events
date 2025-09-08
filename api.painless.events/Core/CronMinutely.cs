using api.painless.events.Hubs;
using Microsoft.AspNetCore.SignalR;
using Quartz;

namespace api.painless.events.Core
{
    public class CronMinutely : IJob
    {

        private readonly IConfiguration _configuration;
        private IHubContext<SocketHub> _socketContext;

        public CronMinutely(IConfiguration configuration, IHubContext<SocketHub> shub)
        {
            _configuration = configuration;
            _socketContext = shub;
        }


        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("CronMinutely fired!");
            await _socketContext.Clients.All.SendAsync("ReceiveAnnouncement", "Server time is " + DateTime.UtcNow.ToString("MM-dd-yyyy HH:mm:ss"));
        }


    }
}
