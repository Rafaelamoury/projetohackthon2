using InovaTrace.Application;
using InovaTrace.Domain;
using Microsoft.EntityFrameworkCore;

namespace InovaTrace.Infrastructure;

public sealed class ProjectRepository : IProjectRepository
{
    private readonly InovaTraceDbContext _db;

    public ProjectRepository(InovaTraceDbContext db)
    {
        _db = db;
    }

    public Task<bool> AnyAsync(CancellationToken cancellationToken) => _db.Projects.AnyAsync(cancellationToken);

    public async Task AddRangeAsync(IEnumerable<Project> projects, CancellationToken cancellationToken)
    {
        await _db.Projects.AddRangeAsync(projects, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> ListSummariesAsync(CancellationToken cancellationToken) =>
        await _db.Projects
            .AsNoTracking()
            .Include(p => p.Warnings)
            .Include(p => p.Analyses).ThenInclude(a => a.Review)
            .AsSplitQuery()
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

    public Task<Project?> GetAsync(string id, CancellationToken cancellationToken) =>
        Query().AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Project?> GetTrackedAsync(string id, CancellationToken cancellationToken) =>
        Query().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Project?> FindByAnalysisAsync(Guid analysisId, CancellationToken cancellationToken)
    {
        var projectId = await _db.Analyses
            .Where(a => a.Id == analysisId)
            .Select(a => a.ProjectId)
            .FirstOrDefaultAsync(cancellationToken);
        return projectId is null ? null : await GetTrackedAsync(projectId, cancellationToken);
    }

    public void StageNewAnalysis(AnalysisRun run) => _db.Analyses.Add(run);

    public void StageNewReview(AnalysisReview review, IReadOnlyList<AuditEntry> entries)
    {
        MarkAdded(review);
        foreach (var entry in entries)
        {
            MarkAdded(entry);
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _db.SaveChangesAsync(cancellationToken);

    private void MarkAdded(object entity) => _db.Entry(entity).State = EntityState.Added;

    private IQueryable<Project> Query() =>
        _db.Projects
            .Include(p => p.Activities)
            .Include(p => p.Evidences)
            .Include(p => p.Documents)
            .Include(p => p.Measurements)
            .Include(p => p.Results)
            .Include(p => p.Chronology)
            .Include(p => p.Observations)
            .Include(p => p.Inputs)
            .Include(p => p.Warnings)
            .Include(p => p.Audit)
            .Include(p => p.Analyses).ThenInclude(a => a.Criteria)
            .Include(p => p.Analyses).ThenInclude(a => a.Claims)
            .Include(p => p.Analyses).ThenInclude(a => a.Contradictions)
            .Include(p => p.Analyses).ThenInclude(a => a.Missing)
            .Include(p => p.Analyses).ThenInclude(a => a.MathChecks)
            .Include(p => p.Analyses).ThenInclude(a => a.Audit)
            .Include(p => p.Analyses).ThenInclude(a => a.Review)
            .AsSplitQuery();
}
