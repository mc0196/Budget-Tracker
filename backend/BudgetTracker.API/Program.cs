using BudgetTracker.API.Middleware;
using BudgetTracker.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ─── Services ────────────────────────────────────────────────────────────────

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings for readable API responses
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Budget Tracker API", Version = "v1" });
});

// Register all Infrastructure + Application services via the extension method
builder.Services.AddInfrastructure(builder.Configuration);

// CORS — allow the Next.js frontend (adjust origin in production)
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ─── App pipeline ────────────────────────────────────────────────────────────

var app = builder.Build();

// Auto-apply EF Core migrations on startup.
// In production you'd prefer a dedicated migration step, but this is fine for an MVP.
await app.Services.ApplyMigrationsAsync();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.MapControllers();

app.Run();

// Expose the Program class for integration test WebApplicationFactory
public partial class Program { }
