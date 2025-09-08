using api.painless.events.Core;
using api.painless.events.Entities;
using api.painless.events.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace api.painless.events.Controllers
{

    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    [EnableRateLimiting("TenInTwenty")]
    public class DeviceController : Controller
    {


        private readonly IConfiguration _configuration;
        private ReadContext _readContext;
        private WriteContext _writeContext;
        private IHubContext<SocketHub> _socketContext;

        public DeviceController(IConfiguration configuration, ReadContext readContext, WriteContext writeContext, IHubContext<SocketHub> socketContext)
        {
            _configuration = configuration;
            _readContext = readContext;
            _writeContext = writeContext;
            _socketContext = socketContext;
        }




        [HttpGet("Init")]
        public async Task<IActionResult> Init(int Pin, int AccountId, int EventId)
        {
            // TODO: put endpoint behind authorization and check if user is allowed to assign device to account/event

            Device device = await (from d1 in _readContext.Devices
                                   where d1.Pin == Pin
                                   && d1.Enabled == 1
                                   && d1.Deleted == 0
                                   && d1.AccountId == 0
                                   && d1.EventId == 0
                                   select d1).FirstOrDefaultAsync() ?? new Device();
            if (device.Id == 0)
                return BadRequest("Invalid Pin");
            string[] activeServers = await (from n1 in _readContext.Nodes
                                            where n1.IsActive == 1
                                            select n1.Domain).ToArrayAsync();
            string activeServer = activeServers[device.EventId % activeServers.Length];
            device.Guid = System.Guid.NewGuid().ToString(); // Device always gets a new Guid for safety!
            device.AccountId = AccountId;
            device.EventId = EventId;
            device.LastLogon = DateTime.UtcNow;
            _writeContext.Devices.Update(device);
            await _writeContext.SaveChangesAsync();
            await _socketContext.Clients.Group("Device_" + device.Id).SendAsync("DeviceInitResponse", 0, device.Id, device.Guid, device.Pin, device.AccountId, device.EventId, activeServer, "");
            return Ok();
        }





    }
}
