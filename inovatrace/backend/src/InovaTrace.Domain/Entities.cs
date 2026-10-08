namespace InovaTrace.Domain;

public sealed class Project
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public ProjectGroup Group { get; set; }
    public string Team { get; set; } = "";
    public int DurationWeeks { get; set; }
    public string SourceDirectory { get; set; } = "";
    public string RawConfigurationJson { get; set; } = "";
    public DateTimeOffset ImportedAt { get; set; }

    public List<DocumentRecord> Documents { get; set; } = [];
    public List<Activity> Activities { get; set; } = [];
    public List<Evidence> Evidences { get; set; } = [];
    public List<Measurement> Measurements { get; set; } = [];
    public List<AssayResult> Results { get; set; } = [];
    public List<ChronologyEvent> Chronology { get; set; } = [];
    public List<ObservationNote> Observations { get; set; } = [];
    public List<InputRecord> Inputs { get; set; } = [];
    public List<ImportWarning> Warnings { get; set; } = [];
    public List<AuditEntry> Audit { get; set; } = [];
    public List<AnalysisRun> Analyses { get; set; } = [];
}

public sealed class DocumentRecord
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string EvidenceId { get; set; } = "";
    public string FileName { get; set; } = "";
    public string PackagePath { get; set; } = "";
    public string DocumentType { get; set; } = "";
    public Project? Project { get; set; }
}

public sealed class Activity
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string Cycle { get; set; } = "";
    public string Phase { get; set; } = "";
    public string DeclaredNature { get; set; } = "";
    public string Description { get; set; } = "";
    public string Output { get; set; } = "";
    public string ResponsibleFunction { get; set; } = "";
    public List<string> EvidenceIds { get; set; } = [];
    public Project? Project { get; set; }
}

public sealed class Evidence
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string EvidenceType { get; set; } = "";
    public string RelativeFile { get; set; } = "";
    public string ExpectedContent { get; set; } = "";
    public string InventoryStatus { get; set; } = "";
    public string Note { get; set; } = "";
    public Project? Project { get; set; }
}

public sealed class Measurement
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string EssayId { get; set; } = "";
    public string Version { get; set; } = "";
    public string Scenario { get; set; } = "";
    public string Kind { get; set; } = "";
    public string Metric { get; set; } = "";
    public decimal? Value { get; set; }
    public decimal? Numerator { get; set; }
    public decimal? Denominator { get; set; }
    public decimal? Weight { get; set; }
    public string Unit { get; set; } = "";
    public Project? Project { get; set; }
}

public sealed class AssayResult
{
    public int Id { get; set; }
    public string ProjectId { get; set; } = "";
    public string EssayId { get; set; } = "";
    public string Version { get; set; } = "";
    public string Metric { get; set; } = "";
    public string Operation { get; set; } = "";
    public decimal Value { get; set; }
    public decimal Base { get; set; }
    public string BaseDescription { get; set; } = "";
    public decimal? RatePercent { get; set; }
    public string Unit { get; set; } = "";
    public string Source { get; set; } = "";
    public string Nature { get; set; } = "";
    public Project? Project { get; set; }
}

public sealed class ChronologyEvent
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string Date { get; set; } = "";
    public string Version { get; set; } = "";
    public string Event { get; set; } = "";
    public string State { get; set; } = "";
    public string Source { get; set; } = "";
    public Project? Project { get; set; }
}

public sealed class ObservationNote
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string Input { get; set; } = "";
    public string Reference { get; set; } = "";
    public string OutputOrSituation { get; set; } = "";
    public string Scope { get; set; } = "";
    public Project? Project { get; set; }
}

public sealed class InputRecord
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string ContentJson { get; set; } = "";
    public Project? Project { get; set; }
}

public sealed class ImportWarning
{
    public int Id { get; set; }
    public string ProjectId { get; set; } = "";
    public string Message { get; set; } = "";
    public Project? Project { get; set; }
}

public sealed class AuditEntry
{
    public Guid Id { get; set; }
    public string? ProjectId { get; set; }
    public Guid? AnalysisRunId { get; set; }
    public DateTimeOffset At { get; set; }
    public string Message { get; set; } = "";
    public Project? Project { get; set; }
    public AnalysisRun? AnalysisRun { get; set; }
}

public sealed class AnalysisRun
{
    public Guid Id { get; set; }
    public string ProjectId { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public Classification Recommendation { get; set; }
    public string Summary { get; set; } = "";
    public string Limit { get; set; } = "";
    public double Confidence { get; set; }
    public AnalysisLifecycle Lifecycle { get; set; } = AnalysisLifecycle.Recommended;
    public Project? Project { get; set; }
    public List<CriterionEvaluation> Criteria { get; set; } = [];
    public List<Claim> Claims { get; set; } = [];
    public List<Contradiction> Contradictions { get; set; } = [];
    public List<MissingEvidence> Missing { get; set; } = [];
    public List<MathCheck> MathChecks { get; set; } = [];
    public List<AuditEntry> Audit { get; set; } = [];
    public AnalysisReview? Review { get; set; }
}

public sealed class CriterionEvaluation
{
    public Guid Id { get; set; }
    public Guid AnalysisRunId { get; set; }
    public CriterionType Criterion { get; set; }
    public CriterionStatus Status { get; set; }
    public double Confidence { get; set; }
    public string Reasoning { get; set; } = "";
    public List<string> ClaimCodes { get; set; } = [];
    public List<string> SupportingEvidenceIds { get; set; } = [];
    public List<string> ContraryEvidenceIds { get; set; } = [];
    public List<string> MissingEvidenceIds { get; set; } = [];
    public AnalysisRun? AnalysisRun { get; set; }
}

public sealed class Claim
{
    public Guid Id { get; set; }
    public Guid AnalysisRunId { get; set; }
    public string Code { get; set; } = "";
    public string? ActivityId { get; set; }
    public string Text { get; set; } = "";
    public string Source { get; set; } = "";
    public double Confidence { get; set; }
    public List<string> EvidenceIds { get; set; } = [];
    public AnalysisRun? AnalysisRun { get; set; }
}

public sealed class Contradiction
{
    public Guid Id { get; set; }
    public Guid AnalysisRunId { get; set; }
    public string SourceA { get; set; } = "";
    public string SourceB { get; set; } = "";
    public string? ClaimA { get; set; }
    public string? ClaimB { get; set; }
    public CriterionType AffectedCriterion { get; set; }
    public ContradictionSeverity Severity { get; set; }
    public string Explanation { get; set; } = "";
    public AnalysisRun? AnalysisRun { get; set; }
}

public sealed class MissingEvidence
{
    public Guid Id { get; set; }
    public Guid AnalysisRunId { get; set; }
    public CriterionType AffectedCriterion { get; set; }
    public string Description { get; set; } = "";
    public string NeededEvidence { get; set; } = "";
    public AnalysisRun? AnalysisRun { get; set; }
}

public sealed class MathCheck
{
    public Guid Id { get; set; }
    public Guid AnalysisRunId { get; set; }
    public string EssayId { get; set; } = "";
    public string Metric { get; set; } = "";
    public string Operation { get; set; } = "";
    public string Declared { get; set; } = "";
    public string Recalculated { get; set; } = "";
    public bool Confirmed { get; set; }
    public string Detail { get; set; } = "";
    public AnalysisRun? AnalysisRun { get; set; }
}

public sealed class AnalysisReview
{
    public Guid Id { get; set; }
    public Guid AnalysisRunId { get; set; }
    public ReviewChoice Choice { get; set; }
    public Classification FinalDecision { get; set; }
    public string Justification { get; set; } = "";
    public string AnalystName { get; set; } = "";
    public DateTimeOffset DecidedAt { get; set; }
    public AnalysisRun? AnalysisRun { get; set; }
}
