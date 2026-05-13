using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using invoices.api.Data.Context;
using invoices.api.Services.Implementations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Moq;
using RabbitMQ.Client;

namespace invoices.tests;

public class TestFactory : WebApplicationFactory<Program>
{
    private static readonly string JwtKey = "dev-only-not-a-real-secret-change-in-production!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove ALL DbContext-related registrations from Program.cs (Npgsql)
            var dbDescriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(DbContextOptions<TestAppDbContext>) ||
                d.ServiceType == typeof(AppDbContext) ||
                d.ServiceType.FullName?.Contains("DbContextOptions") == true
            ).ToList();
            foreach (var d in dbDescriptors) services.Remove(d);

            // Remove RabbitMQ real implementations
            var connDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IConnection));
            if (connDescriptor is not null) services.Remove(connDescriptor);
            var channelDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IChannel));
            if (channelDescriptor is not null) services.Remove(channelDescriptor);

            // Replace with SQLite in-memory (shared connection kept alive)
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            services.AddSingleton(connection);

            services.AddDbContext<TestAppDbContext>(options =>
                options.UseSqlite(connection));

            services.AddScoped<AppDbContext>(sp => sp.GetRequiredService<TestAppDbContext>());

            // Replace RabbitMQ with mocks
            services.AddSingleton<IConnection>(_ => Mock.Of<IConnection>());
            services.AddSingleton<IChannel>(_ => Mock.Of<IChannel>());

            // Remove InvoiceConsumer background service (it crashes with mock RabbitMQ)
            var hostedServiceDescriptor = services.SingleOrDefault(d =>
                d.ImplementationType == typeof(InvoiceConsumer));
            if (hostedServiceDescriptor is not null) services.Remove(hostedServiceDescriptor);
        });
    }

    public static string GenerateToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var token = new JwtSecurityToken(
            issuer: "invoices-api",
            audience: "invoices-front",
            claims: new[] { new Claim(ClaimTypes.Name, "test") },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
