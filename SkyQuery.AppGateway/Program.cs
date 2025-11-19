using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SkyQuery.AppGateway.Application.Interfaces;
using SkyQuery.AppGateway.Infrastructure.Services;
using SkyQuery.AppGateway.Infrastructure.TempStorage;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Dapr
builder.Services.AddDaprClient();

// DI
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddSingleton<IImageStore, FileSystemImageStore>();

// Controllers
builder.Services.AddControllers().AddDapr();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// OpenAPI
builder.Services.AddOpenApi();

// ===== CORS =====
const string PagesPolicy = "PagesPolicy";
var allowedOrigins = new[]
{
    "https://www.skyquery.hedef.dk",
    // Remove // below if domain is ever used without www
    // "https://skyquery.hedef.dk"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy(PagesPolicy, p =>
    {
        if (builder.Environment.IsDevelopment())
        {
            p.AllowAnyOrigin()
             .AllowAnyHeader()
             .AllowAnyMethod();
        }
        else
        {
            p.WithOrigins(allowedOrigins)
             .AllowAnyHeader()
             .AllowAnyMethod();
            // Brug IKKE .AllowCredentials() med Authorization-header/JWT – kun hvis du har cookies
        }
    });
});


// Auth
// Indlæs .env hvis den findes lokalt
if (builder.Environment.IsDevelopment()) Env.Load(); // valgfrit: Env.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

builder.Configuration.AddEnvironmentVariables();

// nu læser du som før
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


// BELOW IS USED FOR SWAGGER !!!!!!
// Swagger + JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Indtast 'Bearer {your JWT token}'"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer(); // USED FOR SWAGGER!!!!
builder.Services.AddSwaggerGen(); // USED FOR SWAGGER!!!!

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger(); // USED FOR SWAGGER!!!!
    app.UseSwaggerUI(); // USED FOR SWAGGER!!!!
}

// Kun redirect når ikke kaldt af Dapr-sidecar
var daprPresent = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_HTTP_PORT"));
if (!daprPresent)
{
    app.UseHttpsRedirection();
}


// ===== Aktiver CORS =====
app.UseRouting();
app.UseCors(PagesPolicy);


app.UseAuthentication();
app.UseAuthorization();


// Dapr will send serialized event object vs. being raw CloudEvent
app.UseCloudEvents();

// needed for Dapr pub/sub routing
app.MapSubscribeHandler();

app.MapControllers()
   .RequireCors(PagesPolicy);


app.Run();
