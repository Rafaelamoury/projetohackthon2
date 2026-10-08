using InovaTrace.Domain;

namespace InovaTrace.Application;

public sealed class ProjectDossier
{
    public required Project Project { get; init; }
    public required string MethodMarkdown { get; init; }
    public required string ReviewMarkdown { get; init; }
    public string InterviewText { get; init; } = "";
    public IReadOnlyDictionary<string, string> MethodSections { get; init; } = new Dictionary<string, string>();
    public IReadOnlyDictionary<string, string> ReviewSections { get; init; } = new Dictionary<string, string>();
}

public sealed class HistoricalLabel
{
    public string ProjectId { get; set; } = "";
    public string Classification { get; set; } = "";
    public IReadOnlyDictionary<CriterionType, string> CriterionStates { get; set; } = new Dictionary<CriterionType, string>();
}

public interface IDatasetImporter
{
    Task<Project> ImportAsync(string projectDirectory, CancellationToken cancellationToken);
    Task<IReadOnlyList<Project>> ImportCatalogAsync(string datasetRoot, CancellationToken cancellationToken);
}

public interface IProjectRepository
{
    Task<bool> AnyAsync(CancellationToken cancellationToken);
    Task AddRangeAsync(IEnumerable<Project> projects, CancellationToken cancellationToken);
    Task<IReadOnlyList<Project>> ListSummariesAsync(CancellationToken cancellationToken);
    Task<Project?> GetAsync(string id, CancellationToken cancellationToken);
    Task<Project?> GetTrackedAsync(string id, CancellationToken cancellationToken);
    Task<Project?> FindByAnalysisAsync(Guid analysisId, CancellationToken cancellationToken);
    void StageNewAnalysis(AnalysisRun run);
    void StageNewReview(AnalysisReview review, IReadOnlyList<AuditEntry> entries);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IDossierReader
{
    ProjectDossier Read(Project project);
}

public interface IHistoricalCatalog
{
    IReadOnlyDictionary<string, HistoricalLabel> Load(string datasetRoot);
}

public interface ILanguageModel
{
    bool IsConfigured { get; }
    Task<string> CompleteJsonAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken);
}

public interface IAnalysisEngine
{
    AnalysisRun Run(ProjectDossier dossier, DateTimeOffset now);
}

public interface IClaimExtractionService
{
    IReadOnlyList<Claim> Extract(ProjectDossier dossier);
}

public sealed record ProjectListItem(
    string Id,
    string Title,
    string Group,
    string Team,
    int DurationWeeks,
    string QueueStatus,
    string? Recommendation,
    Guid? AnalysisId,
    int WarningCount);

public sealed record EvidenceCounts(int Favorable, int Contrary, int Contradictory, int Absent);

public sealed record CriterionDto(
    string Criterion,
    string Label,
    string Status,
    string StatusLabel,
    double Confidence,
    string Reasoning,
    IReadOnlyList<string> ClaimCodes,
    IReadOnlyList<string> SupportingEvidenceIds,
    IReadOnlyList<string> ContraryEvidenceIds,
    IReadOnlyList<string> MissingEvidenceIds);

public sealed record ClaimDto(
    string Code,
    string? ActivityId,
    string Text,
    string Source,
    double Confidence,
    IReadOnlyList<string> EvidenceIds);

public sealed record ContradictionDto(
    string SourceA,
    string SourceB,
    string? ClaimA,
    string? ClaimB,
    string AffectedCriterion,
    string Severity,
    string Explanation);

public sealed record MissingDto(string AffectedCriterion, string Description, string NeededEvidence);

public sealed record MathCheckDto(string EssayId, string Metric, string Operation, string Declared, string Recalculated, bool Confirmed, string Detail);

public sealed record ReviewDto(string Choice, string FinalDecision, string Justification, string AnalystName, DateTimeOffset DecidedAt);

public sealed record AuditDto(DateTimeOffset At, string Message);

public sealed record ActivityDto(string Id, string Cycle, string Phase, string DeclaredNature, string Description, string Output, IReadOnlyList<string> EvidenceIds);

public sealed record EvidenceDto(string Id, string Type, string File, string Status, string Note);

public sealed record AnalysisResponse(
    Guid Id,
    string ProjectId,
    string ProjectTitle,
    string Team,
    string Lifecycle,
    string Recommendation,
    string Summary,
    string Limit,
    double Confidence,
    bool RequiresHumanReview,
    EvidenceCounts Counts,
    IReadOnlyList<CriterionDto> Criteria,
    IReadOnlyList<ClaimDto> Claims,
    IReadOnlyList<ContradictionDto> Contradictions,
    IReadOnlyList<MissingDto> Missing,
    IReadOnlyList<MathCheckDto> MathChecks,
    ReviewDto? Review,
    IReadOnlyList<AuditDto> Audit,
    IReadOnlyList<ActivityDto> Activities,
    IReadOnlyList<EvidenceDto> Evidences);

public sealed record ProjectDetail(
    string Id,
    string Title,
    string Group,
    string Team,
    int DurationWeeks,
    int ActivityCount,
    int EvidenceCount,
    int WarningCount,
    IReadOnlyList<string> Warnings,
    AnalysisResponse? LatestAnalysis);

public sealed record ReviewRequest(string Choice, string? Classification, string Justification, string? AnalystName);

public sealed record ReportSection(string Title, string Body);

public sealed record ReportDocument(string Title, IReadOnlyList<ReportSection> Sections);

public sealed record CalibrationDivergence(string ProjectId, string Expected, string Actual);

public sealed record CriterionScore(string Criterion, int Correct, int Total);

public sealed record CalibrationReport(
    int Projects,
    int CorrectClassifications,
    IReadOnlyList<CriterionScore> Criteria,
    IReadOnlyList<CalibrationDivergence> Divergences);
