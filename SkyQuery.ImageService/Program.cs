using SkyQuery.ImageService.Application.Interfaces;
using SkyQuery.ImageService.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// All httpclients
builder.Services.AddHttpClient("dataforsyningclient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("SkyQuery.ImageService/1.0");
});

// Dependency Injections
builder.Services.AddScoped<IDataforsyningService, DataforsyningService>();

// Registers DaprClient in DI
builder.Services.AddDaprClient();
builder.Services.AddControllers().AddDapr(); // vigtigt
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

// Dapr will send serialized event object vs. being raw CloudEvent
app.UseCloudEvents();

// needed for Dapr pub/sub routing
app.MapSubscribeHandler();

app.MapControllers();

app.Run();
