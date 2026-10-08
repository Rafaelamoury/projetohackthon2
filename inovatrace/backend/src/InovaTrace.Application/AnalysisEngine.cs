using InovaTrace.Domain;

namespace InovaTrace.Application;

public sealed class AnalysisEngine : IAnalysisEngine
{
    private readonly MeasurementValidator _validator;
    private readonly FrascatiRubric _rubric;
    private readonly IClaimExtractionService _claims;
    private readonly ContradictionDetector _contradictions;

    public AnalysisEngine(
        MeasurementValidator validator,
        FrascatiRubric rubric,
        IClaimExtractionService claims,
        ContradictionDetector contradictions)
    {
        _validator = validator;
        _rubric = rubric;
        _claims = claims;
        _contradictions = contradictions;
    }

    public AnalysisRun Run(ProjectDossier dossier, DateTimeOffset now)
    {
        var project = dossier.Project;
        var math = _validator.Validate(project);
        var rubric = _rubric.Evaluate(project, dossier, math);
        var claims = _claims.Extract(dossier);
        var conclusion = claims.FirstOrDefault(c => c.Code.EndsWith("-CLM-ENT", StringComparison.Ordinal))?.Text;
        var contradictions = _contradictions.Detect(project, conclusion, math);

        var run = new AnalysisRun
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            CreatedAt = now,
            Recommendation = rubric.Classification,
            Summary = rubric.Summary,
            Limit = rubric.Limit,
            Confidence = rubric.Confidence,
            Lifecycle = AnalysisLifecycle.Recommended,
            Criteria = rubric.Criteria.ToList(),
            Claims = claims.ToList(),
            Contradictions = contradictions.ToList(),
            Missing = rubric.Missing.ToList(),
            MathChecks = math.ToList()
        };

        foreach (var child in run.Criteria) child.AnalysisRunId = run.Id;
        foreach (var child in run.Claims) child.AnalysisRunId = run.Id;
        foreach (var child in run.Contradictions) child.AnalysisRunId = run.Id;
        foreach (var child in run.Missing) child.AnalysisRunId = run.Id;
        foreach (var child in run.MathChecks) child.AnalysisRunId = run.Id;

        var confirmed = math.Count(m => m.Confirmed);
        run.Audit.Add(Entry(run, now, $"{math.Count} ensaios conferidos; {confirmed} confirmados pelo recálculo."));
        run.Audit.Add(Entry(run, now.AddSeconds(1), "Análise executada sem o gabarito histórico."));
        run.Audit.Add(Entry(run, now.AddSeconds(2), $"IA recomendou: {Labels.Classification(run.Recommendation)}."));
        return run;
    }

    private static AuditEntry Entry(AnalysisRun run, DateTimeOffset at, string message) => new()
    {
        Id = Guid.NewGuid(),
        AnalysisRunId = run.Id,
        At = at,
        Message = message
    };
}
