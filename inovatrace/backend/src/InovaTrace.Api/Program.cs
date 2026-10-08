using InovaTrace.Application;
using InovaTrace.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("InovaTrace")
    ?? throw new InvalidOperationException("Connection string InovaTrace ausente.");
var datasetRoot = builder.Configuration["Dataset:Root"];
if (string.IsNullOrWhiteSpace(datasetRoot))
{
    datasetRoot = FindDatasetRoot();
}

var language = builder.Configuration.GetSection("LanguageModel").Get<LanguageModelOptions>() ?? new LanguageModelOptions();
builder.Services.AddInovaTraceInfrastructure(connectionString, language);
builder.Services.AddInovaTraceApplication(datasetRoot);
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InovaTraceDbContext>();
    db.Database.Migrate();
    var service = scope.ServiceProvider.GetRequiredService<InovaTraceService>();
    await service.ImportCatalogIfEmptyAsync(CancellationToken.None);
}

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/projects", async (InovaTraceService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.ListAsync(cancellationToken)));

app.MapGet("/api/projects/{id}", async (string id, InovaTraceService service, CancellationToken cancellationToken) =>
{
    var project = await service.GetAsync(id, cancellationToken);
    return project is null ? Results.NotFound() : Results.Ok(project);
});

app.MapPost("/api/projects/{id}/analyses", async (string id, InovaTraceService service, CancellationToken cancellationToken) =>
{
    var analysis = await service.AnalyzeAsync(id, cancellationToken);
    return analysis is null ? Results.NotFound() : Results.Ok(analysis);
});

app.MapPost("/api/analyses/{id:guid}/review", async (Guid id, ReviewRequest request, InovaTraceService service, CancellationToken cancellationToken) =>
{
    try
    {
        var analysis = await service.ReviewAsync(id, request, cancellationToken);
        return analysis is null ? Results.NotFound() : Results.Ok(analysis);
    }
    catch (InvalidOperationException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
});

app.MapGet("/api/projects/{id}/report", async (string id, InovaTraceService service, CancellationToken cancellationToken) =>
{
    var report = await service.ReportAsync(id, cancellationToken);
    return report is null ? Results.NotFound() : Results.Ok(report);
});

app.MapPost("/api/calibration", async (InovaTraceService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.CalibrateAsync(cancellationToken)));

app.Run();

static string FindDatasetRoot()
{
    var current = new DirectoryInfo(AppContext.BaseDirectory);
    while (current is not null)
    {
        var candidate = Path.Combine(current.FullName, "dados-originais");
        if (Directory.Exists(candidate))
        {
            return candidate;
        }

        current = current.Parent;
    }

    throw new DirectoryNotFoundException("Pasta dados-originais não encontrada a partir do executável.");
}
