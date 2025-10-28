using Microsoft.EntityFrameworkCore;
using Polly;
using SkyQuery.ImageService.Application.Interfaces;
using SkyQuery.ImageService.Application.Interfaces.Persistence;
using SkyQuery.ImageService.Application.Services;
using SkyQuery.ImageService.Infrastructure.Data;
using SkyQuery.ImageService.Infrastructure.Persistence;

//Den bliver “importeret” via global using i .NET 8’s Aspire-pakker,
//så din lokale using-linje bliver markeret som ubrugt, selvom metoden reelt kommer fra
using Microsoft.Extensions.Http.Resilience; // Used for HttpClient resilience policies

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// All httpclients
builder.Services.AddHttpClient("dataforsyningclient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("SkyQuery.ImageService/1.0");
})
.AddStandardResilienceHandler(options =>
{
    // RETRY: eksponentiel backoff + jitter (jitter = +-10% af antallet)
    options.Retry.MaxRetryAttempts = 5;                 // 5 retries (6 forsøg i alt)
    options.Retry.Delay = TimeSpan.FromSeconds(2);      // base delay
    options.Retry.BackoffType = DelayBackoffType.Exponential; // 2s,4s,8s,16s,32s
    options.Retry.UseJitter = true;                     // spred spikes
});


// Connection string
var conn = builder.Configuration.GetConnectionString("DefaultConnection");

// Registrér SELVE TYPEN du injicerer i repo’et
builder.Services.AddDbContext<ImageServiceDbContext>(options =>
    options.UseSqlServer(conn));

// Dependency Injections
builder.Services.AddScoped<IDataforsyningService, DataforsyningService>();
builder.Services.AddScoped<IDataforsyningImageRepository, DataforsyningImageRepository>();

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
