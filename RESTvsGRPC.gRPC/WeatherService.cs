using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RESTvsGRPC.Shared.gRPC;

namespace RESTvsGRPC.gRPC;

    public class WeatherService : GrpcWeatherService.GrpcWeatherServiceBase
    {
        public override Task<GrpcWeatherResponse> GetWeatherForecast(GrpcWeatherRequest request, ServerCallContext context)
        {
            if (request.DaysRequested <= 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "DaysRequested must be greater than 0."));

            var response = new GrpcWeatherResponse
            {
                LocationName = "Frankfurt (DTIT HQ)",
                Latitude = 50.1109,
                Longitude = 8.6821,
            };

            response.StationMetadata.Add("SensorType", "V2.1");
            response.StationMetadata.Add("MaintenanceDue", "false");

            for (int i = 0; i < request.DaysRequested; i++)
            {
                var dailyForecast = new GrpcDailyForecast
                {
                    Date = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.AddDays(i)),
                    TemperatureC = 22,
                    TemperatureF = 71,
                    Summary = "Clear skies",
                    Condition = GrpcWeatherCondition.Sunny,
                    PrecipitationProb = 0.05
                };

                if (request.IncludeAlerts)
                    dailyForecast.Alerts.Add(new GrpcWeatherAlert
                    {
                        AlertType = "Wind",
                        Description = "Mild gusts",
                        SeverityLevel = 1
                    });

                response.Forecasts.Add(dailyForecast);
            }

            return Task.FromResult(response);
        }
    }
