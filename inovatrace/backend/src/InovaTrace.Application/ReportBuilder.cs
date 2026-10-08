using InovaTrace.Domain;

namespace InovaTrace.Application;

public static class ReportBuilder
{
    public static ReportDocument Build(Project project, AnalysisRun run)
    {
        string Block(CriterionType type)
        {
            var criterion = run.Criteria.FirstOrDefault(c => c.Criterion == type);
            if (criterion is null)
            {
                return "Critério não avaliado nesta análise.";
            }

            var evidence = string.Join(", ", criterion.SupportingEvidenceIds.Concat(criterion.ContraryEvidenceIds).Distinct());
            return $"{Labels.Status(criterion.Status)}\n\n{criterion.Reasoning}\n\nFontes: {(string.IsNullOrWhiteSpace(evidence) ? "não indicadas" : evidence)}.";
        }

        var activities = string.Join("\n\n", project.Activities.OrderBy(a => a.Id).Select(a =>
            $"{a.Id} — {a.Phase}\n{a.Description}\nEvidências: {string.Join(", ", a.EvidenceIds)}"));
        var favorable = string.Join("\n", run.Criteria.SelectMany(c => c.SupportingEvidenceIds).Distinct().Select(id => $"{id} sustenta pelo menos um critério."));
        var contrary = string.Join("\n", run.Criteria.SelectMany(c => c.ContraryEvidenceIds).Distinct().Select(id => $"{id} contraria pelo menos um critério."));
        var contradictions = run.Contradictions.Count == 0
            ? "Nenhuma contradição numérica ou de depoimento foi localizada pelas regras determinísticas."
            : string.Join("\n\n", run.Contradictions.Select(c => $"{c.SourceA} × {c.SourceB} — {Labels.Criterion(c.AffectedCriterion)}\n{c.Explanation}"));
        var gaps = run.Missing.Count == 0
            ? "Nenhuma lacuna essencial foi registrada por esta leitura."
            : string.Join("\n\n", run.Missing.Select(m => $"{Labels.Criterion(m.AffectedCriterion)}: {m.Description}\nEvidência necessária: {m.NeededEvidence}"));
        var review = run.Review is null
            ? "Ainda não há decisão do analista. A recomendação acima é preliminar."
            : $"{run.Review.AnalystName} {(run.Review.Choice == ReviewChoice.Agree ? "concordou" : "alterou")} em {run.Review.DecidedAt:dd/MM/yyyy HH:mm}.\nDecisão: {Labels.Classification(run.Review.FinalDecision)}\n{run.Review.Justification}";
        var decision = run.Review is null ? "Pendente de revisão humana." : Labels.Classification(run.Review.FinalDecision);
        var audit = string.Join("\n", project.Audit.Concat(run.Audit).OrderBy(a => a.At).Select(a => $"{a.At:HH:mm} — {a.Message}"));
        var problem = project.Activities.OrderBy(a => a.Id).FirstOrDefault()?.Description ?? "Não localizado em ATV01.";

        return new ReportDocument(
            $"Parecer preliminar — Lei do Bem — {project.Id}",
            [
                new("1. Identificação", $"{project.Id} — {project.Title}\nEquipe: {project.Team}\nDuração informada: {project.DurationWeeks} semanas\nGrupo: {Labels.Group(project.Group)}"),
                new("2. Problema técnico", problem),
                new("3. Atividades", activities),
                new("4. Novidade", Block(CriterionType.Novelty)),
                new("5. Criatividade", Block(CriterionType.Creativity)),
                new("6. Incerteza tecnológica", Block(CriterionType.TechnologicalUncertainty)),
                new("7. Sistematicidade", Block(CriterionType.Systematicity)),
                new("8. Transferibilidade", Block(CriterionType.Transferability)),
                new("9. Evidências favoráveis", string.IsNullOrWhiteSpace(favorable) ? "Nenhuma evidência foi marcada como favorável." : favorable),
                new("10. Evidências contrárias", string.IsNullOrWhiteSpace(contrary) ? "Nenhuma evidência foi marcada como contrária." : contrary),
                new("11. Contradições", contradictions),
                new("12. Lacunas", gaps),
                new("13. Recomendação do sistema", $"{Labels.Classification(run.Recommendation)}\n\n{run.Summary}\n\nLimite: {run.Limit}"),
                new("14. Revisão do analista", review),
                new("15. Decisão", decision),
                new("16. Trilha de auditoria", string.IsNullOrWhiteSpace(audit) ? "Sem eventos." : audit)
            ]);
    }
}
