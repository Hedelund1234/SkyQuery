using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SkyQuery.AppGateway.Application.Interfaces;
using SkyQuery.AppGateway.Application.Services;
using SkyQuery.AppGateway.Infrastructure.TempStorage;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Registers DaprClient in DI
builder.Services.AddDaprClient();

//DI
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddSingleton<IImageStore, FileSystemImageStore>();



// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddControllers().AddDapr(); // vigtigt
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();



var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();



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

app.UseAuthentication();
app.UseAuthorization();


// Dapr will send serialized event object vs. being raw CloudEvent
app.UseCloudEvents();

// needed for Dapr pub/sub routing
app.MapSubscribeHandler();

app.MapControllers();

app.Run();
