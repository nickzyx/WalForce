using WebServer.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFilter("Microsoft.AspNetCore.DataProtection", LogLevel.None);
builder.Logging.AddFilter("Microsoft.Extensions.Logging.EventLog", LogLevel.None);

builder.Services.AddWalForceApi(builder.Configuration);

var app = builder.Build();

app.UseWalForcePipeline();

app.MapWalForceApi();

app.Run();

public partial class Program;
