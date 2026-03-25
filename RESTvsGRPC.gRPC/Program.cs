var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();

var app = builder.Build();
app.MapGrpcService<RESTvsGRPC.gRPC.WeatherService>();
app.Run();
