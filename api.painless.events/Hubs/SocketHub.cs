using api.painless.events.Core;
using api.painless.events.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Encodings;
using SignalRSwaggerGen.Attributes;
using System.Linq.Expressions;


namespace api.painless.events.Hubs
{
    [SignalRHub]
    public class SocketHub : Hub
    {

        private readonly IConfiguration _configuration;
        private readonly ReadContext _readContext;
        private readonly WriteContext _writeContext;

        public SocketHub(IConfiguration configuration, ReadContext readContext, WriteContext writeContext)
        {
            _configuration = configuration;
            _readContext = readContext;
            _writeContext = writeContext;
        }



        // Delete this later...
        public async Task SendAnnouncement(string message)
        {
            await Clients.All.SendAsync("ReceiveAnnouncement", message);
        }




        #region DEVICES

        public async Task DeviceInit(string Guid, int Version)
        {
            int ErrorValue = 0;
            if (Version < int.Parse(_configuration.GetSection("Appsettings:MinAppVersion").Value ?? "0"))
            {
                ErrorValue = 2;
                Guid = "";
            }
            if (Guid.Length > 0 && Guid.Length < 45)
            { 
                Device device = await (from d1 in _readContext.Devices
                                       where d1.Guid == Guid
                                       && d1.Enabled == 1
                                       && d1.Deleted == 0
                                       select d1).FirstOrDefaultAsync() ?? new Device();
                if (device.Id == 0)
                    Guid = "";
                else 
                {
                    // Device is valid
                    device.Guid = System.Guid.NewGuid().ToString(); // Device always gets a new Guid for safety!    
                    device.Version = Version;
                    if (device.EventId > 0) device.Pin = 0;
                    _writeContext.Devices.Update(device);
                    await _writeContext.SaveChangesAsync();

                    string[] activeServers = await (from n1 in _readContext.Nodes
                                                    where n1.IsActive == 1
                                                    select n1.Domain).ToArrayAsync();
                    string activeServer = (device.EventId == 0) ? "api.painess.events" : activeServers[device.EventId % activeServers.Length];
                    // Send the existing and valid device to the client
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Device_" + device.Id);
                    await Clients.Caller.SendAsync("DeviceInitResponse", ErrorValue, device.Id, device.Guid, device.Pin, device.AccountId, device.EventId, activeServer, ""); // last value is eventually for tokens   
                    return;
                }
            }
            // send a blank device to the client
            int Pin = new Random().Next(100001, 999998);
            while (await (from d1 in _readContext.Devices where d1.Pin == Pin select d1).AnyAsync())
            {
                Pin = new Random().Next(100001, 999998);
            }
            Device newDevice = new Device
            {
                Guid = System.Guid.NewGuid().ToString(),
                AccountId = 0,
                EventId = 0,
                Pin = Pin,
                Name = "New Device",
                Location = "",
                Version = Version,
                LastLogon = DateTime.UtcNow,
                Enabled = 1,
                Deleted = 0,
                Created = DateTime.UtcNow
            };
            _writeContext.Devices.Add(newDevice);
            await _writeContext.SaveChangesAsync();
            if (ErrorValue == 0) ErrorValue = 1;
            // Send the new device to the client
            await Groups.AddToGroupAsync(Context.ConnectionId, "Device_" + newDevice.Id);
            await Clients.Caller.SendAsync("DeviceInitResponse", ErrorValue, newDevice.Id, newDevice.Guid, newDevice.Pin, 0, 0, "api.painless.events", "");
        }


        #endregion DEVICES






    }
}
