using api.painless.events.Hubs;
using Microsoft.AspNetCore.SignalR;
using Quartz;

namespace api.painless.events.Core
{
    public class CronDaily : IJob
    {

        private readonly IConfiguration _configuration;

        public CronDaily(IConfiguration configuration, IHubContext<SocketHub> shub)
        {
            _configuration = configuration;
        }


        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("CronDaily fired!");
        }


    }
}
