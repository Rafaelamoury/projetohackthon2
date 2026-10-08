using InovaTrace.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InovaTrace.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInovaTraceInfrastructure(this IServiceCollection services, string connectionString, LanguageModelOptions languageModel)
    {
        services.AddDbContext<InovaTraceDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IDatasetImporter, HackathonDatasetImporter>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IDossierReader, FileDossierReader>();
        services.AddSingleton<IHistoricalCatalog, HistoricalCatalog>();
        services.AddSingleton(languageModel);
        services.AddHttpClient<ILanguageModel, CorporateLanguageModel>();
        return services;
    }

    public static IServiceCollection AddInovaTraceApplication(this IServiceCollection services, string datasetRoot)
    {
        services.AddScoped<MeasurementValidator>();
        services.AddScoped<FrascatiRubric>();
        services.AddScoped<ContradictionDetector>();
        services.AddScoped<IClaimExtractionService, ClaimExtractionService>();
        services.AddScoped<IAnalysisEngine, AnalysisEngine>();
        services.AddScoped(sp => new InovaTraceService(
            sp.GetRequiredService<IProjectRepository>(),
            sp.GetRequiredService<IDatasetImporter>(),
            sp.GetRequiredService<IDossierReader>(),
            sp.GetRequiredService<IAnalysisEngine>(),
            sp.GetRequiredService<IHistoricalCatalog>(),
            datasetRoot));
        return services;
    }
}
