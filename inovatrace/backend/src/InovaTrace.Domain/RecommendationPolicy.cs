namespace InovaTrace.Domain;

/// <summary>
/// Combina os cinco estados de critério na classe preliminar.
/// Não lê o gabarito histórico. Ausência essencial não vira ressalva.
/// </summary>
public static class RecommendationPolicy
{
    public static Classification Decide(IReadOnlyDictionary<CriterionType, CriterionStatus> statuses)
    {
        ArgumentNullException.ThrowIfNull(statuses);

        var novelty = Get(statuses, CriterionType.Novelty);
        var creativity = Get(statuses, CriterionType.Creativity);
        var uncertainty = Get(statuses, CriterionType.TechnologicalUncertainty);
        var systematicity = Get(statuses, CriterionType.Systematicity);
        var transferability = Get(statuses, CriterionType.Transferability);

        if (IsEssentialGap(novelty) || IsEssentialGap(creativity) || IsEssentialGap(uncertainty)
            || systematicity == CriterionStatus.Partial
            || transferability == CriterionStatus.InsufficientForCore)
        {
            return Classification.InsufficientEvidence;
        }

        if (novelty == CriterionStatus.NotDemonstrated
            && creativity == CriterionStatus.NotDemonstrated
            && uncertainty == CriterionStatus.NotCharacterized)
        {
            return Classification.NotEligible;
        }

        if (transferability == CriterionStatus.DocumentedWithLimit)
        {
            return Classification.WithReservations;
        }

        if (novelty == CriterionStatus.DemonstratedInScope
            && creativity == CriterionStatus.DemonstratedInScope
            && uncertainty == CriterionStatus.Investigated
            && systematicity == CriterionStatus.Documented
            && transferability == CriterionStatus.DocumentedInScope)
        {
            return Classification.Eligible;
        }

        return Classification.InsufficientEvidence;
    }

    public static bool IsEssentialGap(CriterionStatus status) =>
        status is CriterionStatus.Indeterminate
            or CriterionStatus.AllegedNotVerifiable
            or CriterionStatus.InsufficientForCore
            or CriterionStatus.Partial;

    private static CriterionStatus Get(IReadOnlyDictionary<CriterionType, CriterionStatus> statuses, CriterionType criterion)
    {
        if (!statuses.TryGetValue(criterion, out var status))
        {
            throw new InvalidOperationException($"Estado ausente para o critério {criterion}.");
        }

        return status;
    }
}
