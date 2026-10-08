using InovaTrace.Domain;

namespace InovaTrace.Application;

public sealed class InovaTraceService
{
    private readonly IProjectRepository _projects;
    private readonly IDatasetImporter _importer;
    private readonly IDossierReader _dossiers;
    private readonly IAnalysisEngine _engine;
    private readonly IHistoricalCatalog _historical;
    private readonly string _datasetRoot;

    public InovaTraceService(
        IProjectRepository projects,
        IDatasetImporter importer,
        IDossierReader dossiers,
        IAnalysisEngine engine,
        IHistoricalCatalog historical,
        string datasetRoot)
    {
        _projects = projects;
        _importer = importer;
        _dossiers = dossiers;
        _engine = engine;
        _historical = historical;
        _datasetRoot = datasetRoot;
    }

    public async Task ImportCatalogIfEmptyAsync(CancellationToken cancellationToken)
    {
        if (await _projects.AnyAsync(cancellationToken))
        {
            return;
        }

        var imported = await _importer.ImportCatalogAsync(_datasetRoot, cancellationToken);
        await _projects.AddRangeAsync(imported, cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectListItem>> ListAsync(CancellationToken cancellationToken)
    {
        var projects = await _projects.ListSummariesAsync(cancellationToken);
        return projects.Select(ToListItem).ToList();
    }

    public async Task<ProjectDetail?> GetAsync(string id, CancellationToken cancellationToken)
    {
        var project = await _projects.GetAsync(id, cancellationToken);
        if (project is null)
        {
            return null;
        }

        var latest = project.Analyses.OrderByDescending(a => a.CreatedAt).FirstOrDefault();
        return new ProjectDetail(
            project.Id,
            project.Title,
            Labels.Group(project.Group),
            project.Team,
            project.DurationWeeks,
            project.Activities.Count,
            project.Evidences.Count,
            project.Warnings.Count,
            project.Warnings.Select(w => w.Message).ToList(),
            latest is null ? null : ToResponse(project, latest));
    }

    public async Task<AnalysisResponse?> AnalyzeAsync(string id, CancellationToken cancellationToken)
    {
        var project = await _projects.GetTrackedAsync(id, cancellationToken);
        if (project is null)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var dossier = _dossiers.Read(project);
        var run = _engine.Run(dossier, now);
        _projects.StageNewAnalysis(run);
        await _projects.SaveChangesAsync(cancellationToken);
        return ToResponse(project, run);
    }

    public async Task<AnalysisResponse?> ReviewAsync(Guid analysisId, ReviewRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Justification))
        {
            throw new InvalidOperationException("A justificativa do analista é obrigatória.");
        }

        var owner = await _projects.FindByAnalysisAsync(analysisId, cancellationToken);
        var run = owner?.Analyses.FirstOrDefault(a => a.Id == analysisId);
        if (owner is null || run is null)
        {
            return null;
        }

        if (run.Review is not null)
        {
            throw new InvalidOperationException("Esta análise já tem decisão do analista. A recomendação da IA não é reaberta por cima.");
        }

        var agree = request.Choice.Equals("Concordar", StringComparison.OrdinalIgnoreCase)
            || request.Choice.Equals("Agree", StringComparison.OrdinalIgnoreCase);
        Classification final;
        if (agree)
        {
            final = run.Recommendation;
        }
        else
        {
            var parsed = Labels.ParseClassification(request.Classification);
            if (parsed is null)
            {
                throw new InvalidOperationException("Informe a classificação alterada.");
            }

            final = parsed.Value;
        }

        var now = DateTimeOffset.UtcNow;
        var review = new AnalysisReview
        {
            Id = Guid.NewGuid(),
            AnalysisRunId = run.Id,
            Choice = agree ? ReviewChoice.Agree : ReviewChoice.Change,
            FinalDecision = final,
            Justification = request.Justification.Trim(),
            AnalystName = string.IsNullOrWhiteSpace(request.AnalystName) ? "Analista" : request.AnalystName.Trim(),
            DecidedAt = now
        };
        var reviewed = new AuditEntry
        {
            Id = Guid.NewGuid(),
            AnalysisRunId = run.Id,
            At = now,
            Message = $"Analista revisou. Decisão: {Labels.Classification(final)}. Justificativa: {review.Justification}"
        };
        var concluded = new AuditEntry
        {
            Id = Guid.NewGuid(),
            AnalysisRunId = run.Id,
            At = now.AddSeconds(1),
            Message = "Análise concluída. A recomendação da IA permanece registrada."
        };
        run.Review = review;
        run.Lifecycle = AnalysisLifecycle.Concluded;
        run.Audit.Add(reviewed);
        run.Audit.Add(concluded);
        _projects.StageNewReview(review, [reviewed, concluded]);
        await _projects.SaveChangesAsync(cancellationToken);
        return ToResponse(owner, run);
    }

    public async Task<ReportDocument?> ReportAsync(string projectId, CancellationToken cancellationToken)
    {
        var project = await _projects.GetAsync(projectId, cancellationToken);
        var run = project?.Analyses.OrderByDescending(a => a.CreatedAt).FirstOrDefault();
        if (project is null || run is null)
        {
            return null;
        }

        return ReportBuilder.Build(project, run);
    }

    public async Task<CalibrationReport> CalibrateAsync(CancellationToken cancellationToken)
    {
        var key = _historical.Load(_datasetRoot);
        var projects = await _projects.ListSummariesAsync(cancellationToken);
        var divergences = new List<CalibrationDivergence>();
        var criterionHits = Enum.GetValues<CriterionType>().ToDictionary(c => c, _ => (Correct: 0, Total: 0));
        var correct = 0;
        var total = 0;

        foreach (var summary in projects.Where(p => p.Group == ProjectGroup.Historical).OrderBy(p => p.Id))
        {
            if (!key.TryGetValue(summary.Id, out var expected))
            {
                continue;
            }

            var project = await _projects.GetAsync(summary.Id, cancellationToken);
            if (project is null)
            {
                continue;
            }

            var dossier = _dossiers.Read(project);
            var run = _engine.Run(dossier, DateTimeOffset.UtcNow);
            total++;
            var actual = Labels.Classification(run.Recommendation);
            if (string.Equals(actual, expected.Classification, StringComparison.OrdinalIgnoreCase))
            {
                correct++;
            }
            else
            {
                divergences.Add(new CalibrationDivergence(project.Id, expected.Classification, actual));
            }

            foreach (var criterion in run.Criteria)
            {
                var hit = criterionHits[criterion.Criterion];
                hit.Total++;
                var predicted = Labels.HistoricalCriterionState(criterion.Criterion, criterion.Status);
                if (expected.CriterionStates.TryGetValue(criterion.Criterion, out var state)
                    && string.Equals(predicted, state, StringComparison.OrdinalIgnoreCase))
                {
                    hit.Correct++;
                }

                criterionHits[criterion.Criterion] = hit;
            }
        }

        return new CalibrationReport(
            total,
            correct,
            criterionHits.Select(pair => new CriterionScore(Labels.Criterion(pair.Key), pair.Value.Correct, pair.Value.Total)).ToList(),
            divergences);
    }

    private static ProjectListItem ToListItem(Project project)
    {
        var latest = project.Analyses.OrderByDescending(a => a.CreatedAt).FirstOrDefault();
        string status;
        string? recommendation = null;
        Guid? analysisId = null;
        if (latest is null)
        {
            status = "Aguardando análise";
        }
        else if (latest.Review is null)
        {
            status = "Requer revisão";
            recommendation = Labels.Classification(latest.Recommendation);
            analysisId = latest.Id;
        }
        else
        {
            status = "Concluído";
            recommendation = Labels.Classification(latest.Review.FinalDecision);
            analysisId = latest.Id;
        }

        return new ProjectListItem(project.Id, project.Title, Labels.Group(project.Group), project.Team, project.DurationWeeks, status, recommendation, analysisId, project.Warnings.Count);
    }

    public static AnalysisResponse ToResponse(Project project, AnalysisRun run)
    {
        var favorable = run.Criteria.SelectMany(c => c.SupportingEvidenceIds).Distinct().Count();
        var contrary = run.Criteria.SelectMany(c => c.ContraryEvidenceIds).Distinct().Count();
        var contradictory = run.Contradictions.Count;
        var audit = project.Audit.Concat(run.Audit).OrderBy(a => a.At)
            .Select(a => new AuditDto(a.At, a.Message))
            .ToList();

        return new AnalysisResponse(
            run.Id,
            project.Id,
            project.Title,
            project.Team,
            run.Lifecycle == AnalysisLifecycle.Concluded ? "Concluída" : "Recomendada",
            Labels.Classification(run.Recommendation),
            run.Summary,
            run.Limit,
            run.Confidence,
            run.Review is null,
            new EvidenceCounts(favorable, contrary, contradictory, run.Missing.Count),
            run.Criteria.OrderBy(c => c.Criterion).Select(c => new CriterionDto(
                c.Criterion.ToString(),
                Labels.Criterion(c.Criterion),
                c.Status.ToString(),
                Labels.Status(c.Status),
                c.Confidence,
                c.Reasoning,
                c.ClaimCodes,
                c.SupportingEvidenceIds,
                c.ContraryEvidenceIds,
                c.MissingEvidenceIds)).ToList(),
            run.Claims.OrderBy(c => c.Code).Select(c => new ClaimDto(c.Code, c.ActivityId, c.Text, c.Source, c.Confidence, c.EvidenceIds)).ToList(),
            run.Contradictions.Select(c => new ContradictionDto(c.SourceA, c.SourceB, c.ClaimA, c.ClaimB, Labels.Criterion(c.AffectedCriterion), c.Severity.ToString(), c.Explanation)).ToList(),
            run.Missing.Select(m => new MissingDto(Labels.Criterion(m.AffectedCriterion), m.Description, m.NeededEvidence)).ToList(),
            run.MathChecks.Select(m => new MathCheckDto(m.EssayId, m.Metric, m.Operation, m.Declared, m.Recalculated, m.Confirmed, m.Detail)).ToList(),
            run.Review is null ? null : new ReviewDto(run.Review.Choice == ReviewChoice.Agree ? "Concordar" : "Alterar", Labels.Classification(run.Review.FinalDecision), run.Review.Justification, run.Review.AnalystName, run.Review.DecidedAt),
            audit,
            project.Activities.OrderBy(a => a.Id).Select(a => new ActivityDto(a.Id, a.Cycle, a.Phase, a.DeclaredNature, a.Description, a.Output, a.EvidenceIds)).ToList(),
            project.Evidences.OrderBy(e => e.Id).Select(e => new EvidenceDto(e.Id, e.EvidenceType, e.RelativeFile, e.InventoryStatus, e.Note)).ToList());
    }
}
