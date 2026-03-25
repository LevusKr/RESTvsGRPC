using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Grpc.Net.Client;
using RESTvsGRPC.Shared;
using SharedContracts.RestModels;
using System.Net.Http.Json;

namespace BenchmarkClient
{
    // 1. Tell BenchmarkDotNet to include P90, P95, and P99 in the results table
    [Config(typeof(Config))]
    [MemoryDiagnoser] // Keeps measuring RAM
    public class ApiBenchmark
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                AddColumn(StatisticColumn.P90);
                AddColumn(StatisticColumn.P95);
                AddColumn(StatisticColumn.P100);
            }
        }

        private HttpClient _httpClient = null!;
        private GrpcChannel _grpcChannel = null!;
        private GrpcWeatherService.GrpcWeatherServiceClient _grpcClient = null!;

        private RestWeatherRequest _restRequest = null!;
        private GrpcWeatherRequest _grpcRequest = null!;

        // 2. The MAGIC happens here: The benchmark will run 4 separate times!
        [Params(10, 100, 1000, 10000)]
        public int DaysRequested { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7111") };
            _grpcChannel = GrpcChannel.ForAddress("https://localhost:7120");
            _grpcClient = new GrpcWeatherService.GrpcWeatherServiceClient(_grpcChannel);

            // Use the [Params] property to dynamically set the payload size for each run
            _restRequest = new RestWeatherRequest { LocationId = "FRA", DaysRequested = this.DaysRequested, IncludeAlerts = true };
            _grpcRequest = new GrpcWeatherRequest { LocationId = "FRA", DaysRequested = this.DaysRequested, IncludeAlerts = true };
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _httpClient.Dispose();
            _grpcChannel.Dispose();
        }

        [Benchmark(Baseline = true)]
        public async Task<RestWeatherResponse?> Rest_PostRequest()
        {
            var response = await _httpClient.PostAsJsonAsync("/api/weather/forecast", _restRequest);
            response.EnsureSuccessStatusCode(); // This guarantees we aren't measuring 500 Internal Server Errors
            return await response.Content.ReadFromJsonAsync<RestWeatherResponse>();
        }

        [Benchmark]
        public async Task<GrpcWeatherResponse> Grpc_PostRequest()
        {
            return await _grpcClient.GetWeatherForecastAsync(_grpcRequest);
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("--- SANITY CHECK ---");
            Console.WriteLine("Testing endpoints to ensure valid responses before benchmarking...");

            try
            {
                // Quick REST Test
                using var http = new HttpClient { BaseAddress = new Uri("https://localhost:7111") };
                var restRes = await http.PostAsJsonAsync("/api/weather/forecast", new RestWeatherRequest { DaysRequested = 1 });
                restRes.EnsureSuccessStatusCode();
                Console.WriteLine("✅ REST API is alive and returning valid HTTP 200 responses.");

                // Quick gRPC Test
                using var channel = GrpcChannel.ForAddress("https://localhost:7120");
                var grpcClient = new GrpcWeatherService.GrpcWeatherServiceClient(channel);
                var grpcRes = await grpcClient.GetWeatherForecastAsync(new GrpcWeatherRequest { DaysRequested = 1 });
                if (grpcRes != null && grpcRes.Forecasts.Count == 1)
                    Console.WriteLine("✅ gRPC API is alive and returning valid Protobuf data.");

                Console.WriteLine("Sanity check passed! Starting benchmarks in 3 seconds...\n");
                await Task.Delay(3000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SANITY CHECK FAILED! The servers are either not running or throwing errors: {ex.Message}");
                Console.WriteLine("Fix the servers and try again.");
                return; // Stop the program, don't run the benchmark on broken servers
            }

            // Run the actual benchmark
            var summary = BenchmarkRunner.Run<ApiBenchmark>();
        }
    }
}