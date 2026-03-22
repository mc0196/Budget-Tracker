using System.Net;
using System.Text;
using BudgetTracker.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetTracker.Tests.Integration;

/// <summary>
/// Integration tests for POST /api/import.
/// Uses WebApplicationFactory to spin up the full ASP.NET Core pipeline in-process.
/// Replaces the production SQLite DB with a per-test temp SQLite file so there is
/// no provider conflict (both use SQLite) and each test gets a clean isolated DB.
/// </summary>
public class ImportApiTests
{
    private static HttpClient CreateClient()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");

        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Swap the production SQLite connection for an isolated per-test file
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite($"Data Source={dbPath}"));
            });
        });

        var client = factory.CreateClient();

        // Ensure schema + seed data exist
        using var scope = factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();

        return client;
    }

    private static MultipartFormDataContent BuildCsvUpload(string csvContent, string fileName = "statement.csv")
    {
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(csvContent))
        {
            Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv") }
        }, "file", fileName);
        return content;
    }

    [Fact]
    public async Task PostImport_ValidCsv_Returns200WithRowCount()
    {
        var csv = """
            Date,Description,Amount
            15/01/2026,Salary,2500.00
            16/01/2026,Netflix,-15.99
            17/01/2026,Supermarket,-87.40
            """;

        var response = await CreateClient().PostAsync("/api/import", BuildCsvUpload(csv));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("rowsImported");
        body.Should().Contain("3");
    }

    [Fact]
    public async Task PostImport_DuplicateFile_Returns409Conflict()
    {
        var csv = """
            Date,Description,Amount
            15/01/2026,Salary,2500.00
            """;

        // Same client = same DB = same file hash
        var client = CreateClient();
        var first  = await client.PostAsync("/api/import", BuildCsvUpload(csv, "salary_jan.csv"));
        var second = await client.PostAsync("/api/import", BuildCsvUpload(csv, "salary_jan.csv"));

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostImport_UnsupportedFormat_Returns415()
    {
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x00, 0x01])
        {
            Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf") }
        }, "file", "statement.pdf");

        var response = await CreateClient().PostAsync("/api/import", content);

        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task PostImport_NoFile_Returns400()
    {
        var response = await CreateClient().PostAsync("/api/import", new MultipartFormDataContent());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
