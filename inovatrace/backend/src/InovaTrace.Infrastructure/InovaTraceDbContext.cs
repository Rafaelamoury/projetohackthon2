using InovaTrace.Domain;
using Microsoft.EntityFrameworkCore;

namespace InovaTrace.Infrastructure;

public sealed class InovaTraceDbContext : DbContext
{
    public InovaTraceDbContext(DbContextOptions<InovaTraceDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<AnalysisRun> Analyses => Set<AnalysisRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasMaxLength(16);
            entity.Property(p => p.Title).HasMaxLength(300);
            entity.Property(p => p.Team).HasMaxLength(200);
            entity.Property(p => p.SourceDirectory).HasMaxLength(500);
            entity.Property(p => p.RawConfigurationJson).HasColumnType("text");
            entity.HasMany(p => p.Documents).WithOne(d => d.Project).HasForeignKey(d => d.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(p => p.Activities).WithOne(a => a.Project).HasForeignKey(a => a.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(p => p.Evidences).WithOne(e => e.Project).HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(p => p.Measurements).WithOne(m => m.Project).HasForeignKey(m => m.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(p => p.Results).WithOne(r => r.Project).HasForeignKey(r => r.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(p => p.Chronology).WithOne(c => c.Project).HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(p => p.Observations).WithOne(o => o.Project).HasForeignKey(o => o.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(p => p.Inputs).WithOne(i => i.Project).HasForeignKey(i => i.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(p => p.Warnings).WithOne(w => w.Project).HasForeignKey(w => w.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(p => p.Audit).WithOne(a => a.Project).HasForeignKey(a => a.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(p => p.Analyses).WithOne(a => a.Project).HasForeignKey(a => a.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentRecord>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Id).HasMaxLength(32);
            entity.Property(d => d.PackagePath).HasMaxLength(400);
        });

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasMaxLength(32);
            entity.Property(a => a.Description).HasColumnType("text");
            entity.Property(a => a.Output).HasColumnType("text");
        });

        modelBuilder.Entity<Evidence>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(32);
        });

        modelBuilder.Entity<Measurement>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Id).HasMaxLength(40);
            entity.Property(m => m.Value).HasPrecision(18, 6);
            entity.Property(m => m.Numerator).HasPrecision(18, 6);
            entity.Property(m => m.Denominator).HasPrecision(18, 6);
            entity.Property(m => m.Weight).HasPrecision(18, 6);
        });

        modelBuilder.Entity<AssayResult>(entity =>
        {
            entity.Property(r => r.Value).HasPrecision(18, 6);
            entity.Property(r => r.Base).HasPrecision(18, 6);
            entity.Property(r => r.RatePercent).HasPrecision(18, 6);
            entity.Property(r => r.BaseDescription).HasColumnType("text");
            entity.HasIndex(r => new { r.ProjectId, r.EssayId });
        });

        modelBuilder.Entity<ChronologyEvent>(entity => entity.HasKey(c => c.Id));
        modelBuilder.Entity<ObservationNote>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Input).HasColumnType("text");
            entity.Property(o => o.OutputOrSituation).HasColumnType("text");
        });
        modelBuilder.Entity<InputRecord>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ContentJson).HasColumnType("text");
        });
        modelBuilder.Entity<ImportWarning>(entity => entity.Property(w => w.Message).HasColumnType("text"));

        modelBuilder.Entity<AnalysisRun>(entity =>
        {
            entity.Property(a => a.Summary).HasColumnType("text");
            entity.Property(a => a.Limit).HasColumnType("text");
            entity.HasMany(a => a.Criteria).WithOne(c => c.AnalysisRun).HasForeignKey(c => c.AnalysisRunId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(a => a.Claims).WithOne(c => c.AnalysisRun).HasForeignKey(c => c.AnalysisRunId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(a => a.Contradictions).WithOne(c => c.AnalysisRun).HasForeignKey(c => c.AnalysisRunId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(a => a.Missing).WithOne(m => m.AnalysisRun).HasForeignKey(m => m.AnalysisRunId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(a => a.MathChecks).WithOne(m => m.AnalysisRun).HasForeignKey(m => m.AnalysisRunId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(a => a.Audit).WithOne(a => a.AnalysisRun).HasForeignKey(a => a.AnalysisRunId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(a => a.Review).WithOne(r => r.AnalysisRun).HasForeignKey<AnalysisReview>(r => r.AnalysisRunId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CriterionEvaluation>(entity =>
        {
            entity.Property(c => c.Reasoning).HasColumnType("text");
        });
        modelBuilder.Entity<Claim>(entity =>
        {
            entity.Property(c => c.Text).HasColumnType("text");
            entity.HasIndex(c => new { c.AnalysisRunId, c.Code }).IsUnique();
        });
        modelBuilder.Entity<Contradiction>(entity => entity.Property(c => c.Explanation).HasColumnType("text"));
        modelBuilder.Entity<MissingEvidence>(entity =>
        {
            entity.Property(m => m.Description).HasColumnType("text");
            entity.Property(m => m.NeededEvidence).HasColumnType("text");
        });
        modelBuilder.Entity<MathCheck>(entity => entity.Property(m => m.Detail).HasColumnType("text"));
        modelBuilder.Entity<AnalysisReview>(entity => entity.Property(r => r.Justification).HasColumnType("text"));
        modelBuilder.Entity<AuditEntry>(entity => entity.Property(a => a.Message).HasColumnType("text"));
    }
}
