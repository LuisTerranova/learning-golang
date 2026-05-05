using invoices.api.Data.Context;
using invoices.api.Services.Implementations;
using invoices.core.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Services
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

builder.Services.AddHostedService<InvoiceConsumer>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();
