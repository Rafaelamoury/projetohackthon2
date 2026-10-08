using InovaTrace.Domain;

namespace InovaTrace.Application;

public static class Labels
{
    public static string Classification(Classification value) => value switch
    {
        Domain.Classification.Eligible => "Elegível",
        Domain.Classification.WithReservations => "Com ressalvas",
        Domain.Classification.NotEligible => "Não elegível",
        Domain.Classification.InsufficientEvidence => "Evidência insuficiente",
        _ => value.ToString()
    };

    public static Classification? ParseClassification(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var normalized = text.Trim().ToLowerInvariant();
        return normalized switch
        {
            "elegível" or "elegivel" or "eligible" => Domain.Classification.Eligible,
            "com ressalvas" or "withreservations" => Domain.Classification.WithReservations,
            "não elegível" or "nao elegivel" or "noteligible" => Domain.Classification.NotEligible,
            "evidência insuficiente" or "evidencia insuficiente" or "insufficientevidence" => Domain.Classification.InsufficientEvidence,
            _ => null
        };
    }

    public static string Criterion(CriterionType value) => value switch
    {
        CriterionType.Novelty => "Novidade",
        CriterionType.Creativity => "Criatividade",
        CriterionType.TechnologicalUncertainty => "Incerteza tecnológica",
        CriterionType.Systematicity => "Sistematicidade",
        CriterionType.Transferability => "Transferibilidade",
        _ => value.ToString()
    };

    public static string Status(CriterionStatus value) => value switch
    {
        CriterionStatus.DemonstratedInScope => "Demonstrada no recorte",
        CriterionStatus.NotDemonstrated => "Não demonstrada",
        CriterionStatus.Indeterminate => "Indeterminada",
        CriterionStatus.Investigated => "Investigada",
        CriterionStatus.NotCharacterized => "Não caracterizada",
        CriterionStatus.AllegedNotVerifiable => "Alegada, não verificável",
        CriterionStatus.Documented => "Documentada",
        CriterionStatus.DocumentedAsAcceptance => "Documentada como aceite",
        CriterionStatus.Partial => "Parcial",
        CriterionStatus.DocumentedInScope => "Documentada no escopo",
        CriterionStatus.DocumentedWithLimit => "Documentada com limite",
        CriterionStatus.DocumentedForConfiguration => "Documentada para a configuração",
        CriterionStatus.InsufficientForCore => "Insuficiente para o núcleo alegado",
        _ => value.ToString()
    };

    public static string HistoricalCriterionState(CriterionType criterion, CriterionStatus status) => (criterion, status) switch
    {
        (CriterionType.Novelty or CriterionType.Creativity, CriterionStatus.NotDemonstrated) => "NÃO DEMONSTRADA",
        (CriterionType.Novelty or CriterionType.Creativity, CriterionStatus.DemonstratedInScope) => "DEMONSTRADA NO RECORTE",
        (CriterionType.Novelty or CriterionType.Creativity, CriterionStatus.Indeterminate) => "INDETERMINADA",
        (CriterionType.TechnologicalUncertainty, CriterionStatus.NotCharacterized) => "NÃO CARACTERIZADA",
        (CriterionType.TechnologicalUncertainty, CriterionStatus.Investigated) => "INVESTIGADA",
        (CriterionType.TechnologicalUncertainty, CriterionStatus.AllegedNotVerifiable) => "ALEGADA, NÃO VERIFICÁVEL",
        (CriterionType.Systematicity, CriterionStatus.DocumentedAsAcceptance) => "DOCUMENTADA COMO ACEITE",
        (CriterionType.Systematicity, CriterionStatus.Documented) => "DOCUMENTADA",
        (CriterionType.Systematicity, CriterionStatus.Partial) => "PARCIAL",
        (CriterionType.Transferability, CriterionStatus.DocumentedForConfiguration) => "DOCUMENTADA PARA A CONFIGURAÇÃO",
        (CriterionType.Transferability, CriterionStatus.DocumentedInScope) => "DOCUMENTADA NO ESCOPO",
        (CriterionType.Transferability, CriterionStatus.DocumentedWithLimit) => "DOCUMENTADA COM LIMITE",
        (CriterionType.Transferability, CriterionStatus.InsufficientForCore) => "INSUFICIENTE PARA O NÚCLEO ALEGADO",
        _ => status.ToString()
    };

    public static string Group(ProjectGroup group) => group == ProjectGroup.Historical ? "Histórico" : "Análise";
}
