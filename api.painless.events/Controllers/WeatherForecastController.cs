using api.painless.events.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace api.painless.events.Controllers
{
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    [EnableRateLimiting("TenInTwenty")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private ReadContext _readContext;
        private WriteContext _writeContext;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, WriteContext writeContext, ReadContext readContext)
        {
            _logger = logger;
            _writeContext = writeContext;
            _readContext = readContext;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }





    }
}
