using api.painless.events.Entities;
using api.painless.events.Hubs;
using Microsoft.AspNetCore.SignalR;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using System.Buffers;

namespace api.painless.events.Core
{
    public sealed class SmtpWorker : BackgroundService
    {
        readonly SmtpServer.SmtpServer _smtpServer;

        public SmtpWorker(SmtpServer.SmtpServer smtpServer)
        {
            _smtpServer = smtpServer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //SmtpServer.ComponentModel.ServiceProvider sProvider = new SmtpServer.ComponentModel.ServiceProvider();
            //sProvider.Add((IUserAuthenticatorFactory)new SampleUserAuthenticator());
            //sProvider.Add(new ConsoleMessageStore(_configuration, _readContext, _writeContext, _memoryContext, _botHubContext, _presentationHubContext));
            return _smtpServer.StartAsync(stoppingToken);
        }
    }







    public class ConsoleMessageStore : MessageStore
    {

        private readonly IConfiguration _configuration;
        private IHubContext<SocketHub> _socketContext;
        private ReadContext _readContext;
        private WriteContext _writeContext;


        public ConsoleMessageStore(IConfiguration configuration, IHubContext<SocketHub> shub, ReadContext rContext, WriteContext wContext)
        {
            this._configuration = configuration;
            this._socketContext = shub;
            this._writeContext = wContext;
            this._readContext = rContext;
        }





        /// <summary>
        /// Save the given message to the underlying storage system.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="transaction">The SMTP message transaction to store.</param>
        /// <param name="buffer">The buffer that contains the message content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unique identifier that represents this message in the underlying message store.</returns>
        public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            await using var stream = new MemoryStream();
            var position = buffer.GetPosition(0);
            while (buffer.TryGet(ref position, out var memory))
            {
                await stream.WriteAsync(memory, cancellationToken);
            }
            stream.Position = 0;
            var message = await MimeKit.MimeMessage.LoadAsync(stream, cancellationToken);
            
            //await _writeContext.Users.AddAsync(new User { Username = message.To.ToString() });

            await _socketContext.Clients.All.SendAsync("ReceiveAnnouncement", "To:" + message.To + " Subject:" + message.Subject);

            Console.WriteLine(message.Subject);

            return SmtpResponse.Ok;
        }
    }




}
