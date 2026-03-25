using Microsoft.AspNetCore.Mvc;
using RESTvsGRPC.Shared.REST;

namespace RESTvsGRPC.REST;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    [HttpPost("forecast")]
    public IActionResult GetExtendedForecast([FromBody] RestWeatherRequest request)
    {
        if (request.DaysRequested <= 0)
            return BadRequest("DaysRequested must be greater than 0.");

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

        for (int i = 0; i < request.DaysRequested; i++)
            response.Forecasts.Add(new RestDailyForecast
            {
                Date = DateTimeOffset.UtcNow.AddDays(i),
                TemperatureC = 22,
                TemperatureF = 71,
                Summary = "Clear skies",
                Condition = RestWeatherCondition.Sunny,
                PrecipitationProb = 0.05,
                Alerts = request.IncludeAlerts ?
                [
                    new RestWeatherAlert
                    {
                        AlertType = "Wind",
                        Description = "Mild gusts",
                        SeverityLevel = 1
                    }
                ] : []
            });

        return Ok(response);
    }
}