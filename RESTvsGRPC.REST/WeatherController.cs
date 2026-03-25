using Microsoft.AspNetCore.Mvc;
using SharedContracts.RestModels;

namespace RestService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        // We use POST here because the request contains a complex body (LocationId, DaysRequested, etc.)
        [HttpPost("forecast")]
        public IActionResult GetExtendedForecast([FromBody] RestWeatherRequest request)
        {
            // 1. Validate request (Standard REST practice)
            if (request.DaysRequested <= 0)
                return BadRequest("DaysRequested must be greater than 0.");

            // 2. Generate identical mock data
            var response = new RestWeatherResponse
            {
                LocationName = "Frankfurt (DTIT HQ)",
                Latitude = 50.1109,
                Longitude = 8.6821,
                StationMetadata = new Dictionary<string, string>
                {
                    { "SensorType", "V2.1" },
                    { "MaintenanceDue", "false" }
                }
            };

            // Generate the requested amount of days
            for (int i = 0; i < request.DaysRequested; i++)
            {
                response.Forecasts.Add(new RestDailyForecast
                {
                    Date = DateTimeOffset.UtcNow.AddDays(i),
                    TemperatureC = 22,
                    TemperatureF = 71,
                    Summary = "Clear skies",
                    Condition = RestWeatherCondition.Sunny,
                    PrecipitationProb = 0.05,
                    Alerts = request.IncludeAlerts ? new List<RestWeatherAlert>
                    {
                        new RestWeatherAlert { AlertType = "Wind", Description = "Mild gusts", SeverityLevel = 1 }
                    } : new List<RestWeatherAlert>()
                });
            }

            // 3. Framework automatically serializes 'response' to JSON
            return Ok(response);
        }
    }
}