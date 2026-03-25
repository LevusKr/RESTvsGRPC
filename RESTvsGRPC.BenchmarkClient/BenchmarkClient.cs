using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using Grpc.Net.Client;
using RESTvsGRPC.Shared.gRPC;
using RESTvsGRPC.Shared.REST;
using System.Net.Http.Json;

namespace RESTvsGRPC.BenchmarkClient;

[Config(typeof(Config))]
[MemoryDiagnoser]
public class BenchmarkClient
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

    [Params(10, 100, 1000, 10000)]
    public int DaysRequested { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:7111") };
        _grpcChannel = GrpcChannel.ForAddress("http://localhost:50051");
        _grpcClient = new GrpcWeatherService.GrpcWeatherServiceClient(_grpcChannel);

        _restRequest = new RestWeatherRequest { LocationId = "FRA", DaysRequested = DaysRequested, IncludeAlerts = true };
        _grpcRequest = new GrpcWeatherRequest { LocationId = "FRA", DaysRequested = DaysRequested, IncludeAlerts = true };
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
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RestWeatherResponse>();
    }

    [Benchmark]
    public async Task<GrpcWeatherResponse> Grpc_PostRequest()
    {
        return await _grpcClient.GetWeatherForecastAsync(_grpcRequest);
    }
}