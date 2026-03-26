using BenchmarkDotNet.Running;
using Grpc.Net.Client;
using RESTvsGRPC.BenchmarkClient;
using RESTvsGRPC.Shared.gRPC;
using RESTvsGRPC.Shared.REST;
using System.Net.Http.Json;

    class Program
    {
    static async Task Main()
        {
            Console.WriteLine("--- SANITY CHECK ---");
            Console.WriteLine("Testing endpoints to ensure valid responses before benchmarking...");

            try
            {
            using var http = new HttpClient { BaseAddress = new Uri("http://localhost:7111") };
                var restRes = await http.PostAsJsonAsync("/api/weather/forecast", new RestWeatherRequest { DaysRequested = 1 });
                restRes.EnsureSuccessStatusCode();
                Console.WriteLine("✅ REST API is alive and returning valid HTTP 200 responses.");
            Console.WriteLine("REST JSON calculated size: " + (await restRes.Content.ReadAsByteArrayAsync()).Length);

            using var channel = GrpcChannel.ForAddress("http://localhost:50051");
                var grpcClient = new GrpcWeatherService.GrpcWeatherServiceClient(channel);
                var grpcRes = await grpcClient.GetWeatherForecastAsync(new GrpcWeatherRequest { DaysRequested = 1 });
                if (grpcRes != null && grpcRes.Forecasts.Count == 1)
            {
                    Console.WriteLine("✅ gRPC API is alive and returning valid Protobuf data.");
                Console.WriteLine("gRPC Protobuf calculated size: " + grpcRes.CalculateSize());
            }

                Console.WriteLine("Sanity check passed! Starting benchmarks in 3 seconds...\n");

                await Task.Delay(3000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SANITY CHECK FAILED! The servers are either not running or throwing errors: {ex.Message}");
                Console.WriteLine("Fix the servers and try again.");
            return;
            }

        BenchmarkRunner.Run<BenchmarkClient>();
    }
}