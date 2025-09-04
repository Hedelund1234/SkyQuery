using SkyQuery.AppGateway.Interfaces;
using SkyQuery.AppGateway.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Registers DaprClient in DI
builder.Services.AddDaprClient();

//DI
builder.Services.AddScoped<IImageService, ImageService>();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddControllers().AddDapr(); // vigtigt
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Kun redirect når ikke kaldt af Dapr-sidecar
var daprPresent = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_HTTP_PORT"));
if (!daprPresent)
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

// Dapr will send serialized event object vs. being raw CloudEvent
app.UseCloudEvents();

// needed for Dapr pub/sub routing
app.MapSubscribeHandler();

app.MapControllers();

app.Run();
