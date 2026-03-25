using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using RESTvsGRPC.Shared;

namespace GrpcService.Services
{
    // Inherits from the auto-generated Protobuf class
    public class WeatherService : GrpcWeatherService.GrpcWeatherServiceBase
    {
        // Method signature is strictly dictated by the contract
        public override Task<GrpcWeatherResponse> GetWeatherForecast(GrpcWeatherRequest request, ServerCallContext context)
        {
            // 1. Validate request (gRPC uses RpcException for errors)
            if (request.DaysRequested <= 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "DaysRequested must be greater than 0."));
            }

            // 2. Generate identical mock data
            var response = new GrpcWeatherResponse
            {
                LocationName = "Frankfurt (DTIT HQ)",
                Latitude = 50.1109,
                Longitude = 8.6821,
            };

            // Populate the dictionary (map<string, string>)
            response.StationMetadata.Add("SensorType", "V2.1");
            response.StationMetadata.Add("MaintenanceDue", "false");

            // Generate the requested amount of days
            for (int i = 0; i < request.DaysRequested; i++)
            {
                var dailyForecast = new GrpcDailyForecast
                {
                    // gRPC requires explicit conversion to its own Timestamp type
                    Date = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.AddDays(i)),
                    TemperatureC = 22,
                    TemperatureF = 71,
                    Summary = "Clear skies",
                    Condition = GrpcWeatherCondition.Sunny,
                    PrecipitationProb = 0.05
                };

                if (request.IncludeAlerts)
                {
                    dailyForecast.Alerts.Add(new GrpcWeatherAlert
                    {
                        AlertType = "Wind",
                        Description = "Mild gusts",
                        SeverityLevel = 1
                    });
                }

                // Add to the 'repeated' collection
                response.Forecasts.Add(dailyForecast);
            }

            // 3. Return as a Task (gRPC is inherently asynchronous)
            return Task.FromResult(response);
        }
    }
}