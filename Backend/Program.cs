// Author: Hassan
// Date: 2025-11-25 (Updated for Serilog file logging)
// Description: ASP.NET Core Web API application entry point with DI, JWT, Swagger, CORS, and Serilog file logging

using Backend.Configuration;
using Backend.Data;
using Backend.Middleware;
using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Text;

// Configure Serilog from appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .CreateLogger();

try
{
    Log.Information("========================================");
    Log.Information("Starting VUTEQ Scanner API");
    Log.Information("========================================");

var builder = WebApplication.CreateBuilder(args);

// Use Serilog for logging
builder.Host.UseSerilog();

// ============================
// Configure Services
// ============================

// Add Entity Framework Core DbContext
builder.Services.AddDbContext<VuteqDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add configuration for JWT
builder.Services.Configure<JwtConfiguration>(builder.Configuration.GetSection("Jwt"));

// Add controllers
builder.Services.AddControllers();

// Add CORS policy for Next.js frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJsFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure JWT Authentication
var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtConfiguration>()
    ?? throw new InvalidOperationException("JWT configuration not found");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig.Issuer,
        ValidAudience = jwtConfig.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Register repositories (Scoped lifetime for per-request instances)
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IOfficeRepository, OfficeRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
builder.Services.AddScoped<IOrderUploadRepository, OrderUploadRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ISkidBuildRepository, SkidBuildRepository>();
builder.Services.AddScoped<IShipmentLoadRepository, ShipmentLoadRepository>();
builder.Services.AddScoped<IToyotaConfigRepository, ToyotaConfigRepository>();

// Register services (Scoped lifetime for per-request instances)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOfficeService, OfficeService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();

// Excel Parser Service (PDF support removed - Excel only)
builder.Services.AddScoped<IExcelParserService, ExcelParserService>();

// Toyota Validation Service
builder.Services.AddScoped<IToyotaValidationService, ToyotaValidationService>();

// Toyota API Service (OAuth + Skid Build + Shipment Load)
builder.Services.AddScoped<IToyotaApiService, ToyotaApiService>();

// Toyota API Configuration Service (Admin-only configuration management)
builder.Services.AddScoped<IToyotaConfigService, ToyotaConfigService>();

// HTTP Client Factory for API calls
builder.Services.AddHttpClient();

builder.Services.AddScoped<IOrderUploadService, OrderUploadService>();
builder.Services.AddScoped<IPlannedItemService, PlannedItemService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ISkidBuildService, SkidBuildService>();
builder.Services.AddScoped<IShipmentLoadService, ShipmentLoadService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VUTEQ Scanner APIs",
        Version = "v1",
        Description = "ASP.NET Core Web API for VUTEQ Scanner Application - Toyota Manufacturing Scanning System",
        Contact = new OpenApiContact
        {
            Name = "Hassan",
            Email = "hassan@vuteq.com"
        }
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
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

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// ============================
// Configure Middleware Pipeline
// ============================

var app = builder.Build();

// Apply EF Core migrations on startup (Code-First approach)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<VuteqDbContext>();
    try
    {
        app.Logger.LogInformation("Applying EF Core migrations...");
        dbContext.Database.Migrate();
        app.Logger.LogInformation("EF Core migrations applied successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error applying EF Core migrations");
        throw;
    }
}

// Enable static files for Swagger custom assets
app.UseStaticFiles();

// Enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "VUTEQ Scanner APIs v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at root
        options.DocumentTitle = "VUTEQ Scanner APIs";
        options.InjectStylesheet("/swagger/custom.css");
        options.HeadContent = "<link rel=\"icon\" type=\"image/x-icon\" href=\"/favicon.ico\" />";
    });
}

// Global error handling middleware
app.UseErrorHandling();

// NOTE: HTTPS redirection disabled for Docker development (HTTP only)
// app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowNextJsFrontend");

// Enable authentication
app.UseAuthentication();

// Enable authorization
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Log startup information
Log.Information("========================================");
Log.Information("VUTEQ Scanner API Configuration");
Log.Information("========================================");
Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
Log.Information("Database Server: {Server}", builder.Configuration.GetConnectionString("DefaultConnection")?.Split(";")[0]);
Log.Information("Logging to: {LogPath}", Path.Combine(Directory.GetCurrentDirectory(), "logs"));
Log.Information("Auth logs: {AuthLogPath}", Path.Combine(Directory.GetCurrentDirectory(), "logs", "auth"));
Log.Information("Error logs: {ErrorLogPath}", Path.Combine(Directory.GetCurrentDirectory(), "logs", "errors"));
Log.Information("========================================");
Log.Information("VUTEQ Scanner API started successfully");
Log.Information("========================================");

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.Information("========================================");
    Log.Information("VUTEQ Scanner API shutting down");
    Log.Information("========================================");
    Log.CloseAndFlush();
}
