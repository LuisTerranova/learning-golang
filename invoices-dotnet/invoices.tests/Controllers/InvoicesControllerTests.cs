using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using invoices.api.Data.Context;
using invoices.core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace invoices.tests.Controllers;

public class InvoicesControllerTests : IClassFixture<TestFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly TestFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public InvoicesControllerTests(TestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestFactory.GenerateToken());
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };
    }

    public async Task InitializeAsync()
    {
        // Clean up test data before each test to ensure isolation
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Remove invoices and raw invoices (in correct FK order)
        db.Invoices.RemoveRange(db.Invoices);
        await db.SaveChangesAsync();
        db.RawInvoices.RemoveRange(db.RawInvoices);
        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private AppDbContext GetDb()
    {
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    private static Invoice CreateInvoice(Guid? id = null, DateTime? date = null, string? key = null)
    {
        var raw = new RawInvoice
        {
            Id = Guid.NewGuid(),
            FileName = "test.pdf",
        };
        return new Invoice
        {
            Id = id ?? Guid.NewGuid(),
            RawId = raw.Id,
            RawInvoice = raw,
            Date = date,
            AccessKey = key,
        };
    }

    private async Task AddWithRawAsync(AppDbContext db, params Invoice[] invoices)
    {
        foreach (var inv in invoices)
        {
            if (inv.RawInvoice is not null)
                db.RawInvoices.Add(inv.RawInvoice);
        }
        db.Invoices.AddRange(invoices);
        await db.SaveChangesAsync();
    }

    // ─── GET /api/invoices ───

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithInvoices()
    {
        using var db = GetDb();
        await AddWithRawAsync(db, CreateInvoice(key: "AK001"));

        var response = await _client.GetAsync("/api/invoices?page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var invoices = await response.Content.ReadFromJsonAsync<List<Invoice>>(_jsonOptions);
        invoices.Should().NotBeNull();
        invoices!.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmpty_WhenNoInvoices()
    {
        var response = await _client.GetAsync("/api/invoices?page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var invoices = await response.Content.ReadFromJsonAsync<List<Invoice>>(_jsonOptions);
        invoices.Should().BeEmpty();
    }

    // ─── GET /api/invoices/{id} ───

    [Fact]
    public async Task GetById_ShouldReturnInvoice_WhenFound()
    {
        var id = Guid.NewGuid();
        using var db = GetDb();
        await AddWithRawAsync(db, CreateInvoice(id, key: "DETAIL001"));

        var response = await _client.GetAsync($"/api/invoices/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var invoice = await response.Content.ReadFromJsonAsync<Invoice>(_jsonOptions);
        invoice.Should().NotBeNull();
        invoice!.AccessKey.Should().Be("DETAIL001");
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenMissing()
    {
        var response = await _client.GetAsync($"/api/invoices/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── GET /api/invoices/count ───

    [Fact]
    public async Task GetCount_ShouldReturnTotal()
    {
        using var db = GetDb();
        await AddWithRawAsync(db, CreateInvoice(), CreateInvoice());

        var response = await _client.GetAsync("/api/invoices/count");
        var count = await response.Content.ReadFromJsonAsync<int>();
        count.Should().Be(2);
    }

    // ─── GET /api/invoices/groups ───

    [Fact]
    public async Task GetGroups_ShouldReturnGroupedCounts()
    {
        using var db = GetDb();
        await AddWithRawAsync(db,
            CreateInvoice(date: new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)),
            CreateInvoice(date: new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc)),
            CreateInvoice(date: new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc))
        );

        var response = await _client.GetAsync("/api/invoices/groups");
        var groups = await response.Content.ReadFromJsonAsync<List<YearMonthGroup>>(_jsonOptions);

        groups.Should().HaveCount(2);
        groups.Should().Contain(g => g.Year == 2026 && g.Month == 1 && g.Count == 2);
        groups.Should().Contain(g => g.Year == 2026 && g.Month == 2 && g.Count == 1);
    }

    // ─── GET /api/invoices/by-month ───

    [Fact]
    public async Task GetByMonth_ShouldReturnInvoicesForMonth()
    {
        using var db = GetDb();
        await AddWithRawAsync(db,
            CreateInvoice(date: new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), key: "JAN"),
            CreateInvoice(date: new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc), key: "FEB")
        );

        var response = await _client.GetAsync("/api/invoices/by-month?year=2026&month=1");
        var invoices = await response.Content.ReadFromJsonAsync<List<Invoice>>(_jsonOptions);

        invoices.Should().HaveCount(1);
        invoices![0].AccessKey.Should().Be("JAN");
    }

    // ─── POST /api/invoices/batch-delete ───

    [Fact]
    public async Task BatchDelete_ShouldRemoveInvoices()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        using var db = GetDb();
        await AddWithRawAsync(db,
            CreateInvoice(id1),
            CreateInvoice(id2),
            CreateInvoice()
        );

        var response = await _client.PostAsJsonAsync("/api/invoices/batch-delete",
            new BatchDeleteRequest { Ids = new List<Guid> { id1, id2 } });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var count = await db.Invoices.CountAsync();
        count.Should().Be(1);
    }

    // ─── Unauthenticated ───

    [Fact]
    public async Task GetAll_ShouldReturnUnauthorized_WithoutToken()
    {
        var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync("/api/invoices");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
