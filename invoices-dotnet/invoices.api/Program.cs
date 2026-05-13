using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using invoices.api.Data.Context;
using invoices.api.Data.Repositories;
using invoices.api.Services.Implementations;
using invoices.core.Services.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// RabbitMQ
builder.Services.AddSingleton(sp =>
{
    var config = builder.Configuration.GetSection("RabbitMQ");
    var factory = new ConnectionFactory
    {
        HostName = config["HostName"] ?? "localhost",
        UserName = config["UserName"] ?? "guest",
        Password = config["Password"] ?? "guest",
    };

    const int maxRetries = 5;
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        }
        catch (Exception) when (i < maxRetries - 1)
        {
            Console.WriteLine($"RabbitMQ connection attempt {i + 1}/{maxRetries} failed. Retrying in 3s...");
            Thread.Sleep(3000);
        }
    }

    throw new InvalidOperationException("Failed to connect to RabbitMQ after multiple retries.");
});

builder.Services.AddSingleton<IChannel>(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    return connection.CreateChannelAsync().GetAwaiter().GetResult();
});

// Data
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHostedService<InvoiceConsumer>();

// JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
        };
    });

// Shared JSON options for RabbitMQ consumer & service
builder.Services.AddSingleton(new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
});

// Controllers + JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

// Apply migrations and seed admin user
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.InitializeAsync(db);
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
