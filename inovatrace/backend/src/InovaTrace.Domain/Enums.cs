namespace InovaTrace.Domain;

public enum ProjectGroup
{
    Historical = 1,
    Analysis = 2
}

public enum CriterionType
{
    Novelty = 1,
    Creativity = 2,
    TechnologicalUncertainty = 3,
    Systematicity = 4,
    Transferability = 5
}

public enum Classification
{
    Eligible = 1,
    WithReservations = 2,
    NotEligible = 3,
    InsufficientEvidence = 4
}

public enum CriterionStatus
{
    DemonstratedInScope = 1,
    NotDemonstrated = 2,
    Indeterminate = 3,
    Investigated = 4,
    NotCharacterized = 5,
    AllegedNotVerifiable = 6,
    Documented = 7,
    DocumentedAsAcceptance = 8,
    Partial = 9,
    DocumentedInScope = 10,
    DocumentedWithLimit = 11,
    DocumentedForConfiguration = 12,
    InsufficientForCore = 13
}

public enum AnalysisLifecycle
{
    Recommended = 1,
    Concluded = 2
}

public enum ReviewChoice
{
    Agree = 1,
    Change = 2
}

public enum ContradictionSeverity
{
    Low = 1,
    Medium = 2,
    High = 3
}
