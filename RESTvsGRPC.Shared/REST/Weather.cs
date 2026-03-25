using System.Text.Json.Serialization;

namespace RESTvsGRPC.Shared.REST;

public class RestWeatherRequest
{
    public string LocationId { get; set; } = string.Empty;
    public int DaysRequested { get; set; }
    public bool IncludeAlerts { get; set; }
}

public class RestWeatherResponse
{
    public string LocationName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    // Equivalent to 'repeated GrpcDailyForecast'
    public List<RestDailyForecast> Forecasts { get; set; } = [];
    // Equivalent to 'map<string, string>'
    public Dictionary<string, string> StationMetadata { get; set; } = [];
}

public class RestDailyForecast
{
    // Equivalent to 'google.protobuf.Timestamp'
    public DateTimeOffset Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF { get; set; }
    public string Summary { get; set; } = string.Empty;
    public RestWeatherCondition Condition { get; set; }
    // Equivalent to 'optional double' (Nullable type in C#)
    public double? PrecipitationProb { get; set; }
    // Equivalent to 'repeated GrpcWeatherAlert'
    public List<RestWeatherAlert> Alerts { get; set; } = [];
}

// Industry standard to serialize enums as strings to preserve human-readabilty
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RestWeatherCondition
{
    // Manual numbering enum constants to guarantee order across modification
    Unspecified = 0,
    Sunny = 1,
    Cloudy = 2,
    Rainy = 3,
    Snowy = 4,
    Thunderstorm = 5
}

public class RestWeatherAlert
{
    public string AlertType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SeverityLevel { get; set; }
}