using InovaTrace.Domain;

namespace InovaTrace.Application;

public sealed class RubricOutcome
{
    public required Classification Classification { get; init; }
    public required string Summary { get; init; }
    public required string Limit { get; init; }
    public required double Confidence { get; init; }
    public required IReadOnlyList<CriterionEvaluation> Criteria { get; init; }
    public required IReadOnlyList<MissingEvidence> Missing { get; init; }
}

/// <summary>
/// Lê o que o próprio método e a revisão afirmam. Não consulta historicos_classificados.
/// Frases literais estão documentadas em docs/BUSINESS_RULES.md.
/// </summary>
public sealed class FrascatiRubric
{
    private static readonly string[] GapPhrases =
    [
        "Faltam ",
        "Não foram recuperados",
        "não há saída executada",
        "Sem regras e saídas",
        "Sem saídas ",
        "não há como determinar",
        "não completam a cadeia",
        "demonstram preparação, não execução",
        "não demonstra a técnica",
        "não demonstram a técnica"
    ];

    private static readonly string[] KnownApplicationPhrases =
    [
        "Não há hipótese de mecanismo novo",
        "já admitida pelo produto",
        "Nenhum algoritmo",
        "Não se investigou técnica nova",
        "sem modificar motor",
        "Aplicar receita do fornecedor",
        "Não treinar modelo",
        "não houve alteração do comparador",
        "Não alterar lógica",
        "Não modificar protocolo",
        "Não se propôs alterar o mecanismo",
        "Não se propôs mecanismo novo",
        "Nenhum novo método",
        "não corresponde a novo método",
        "aplicação do conversor existente",
        "Aceite do comportamento contratado",
        "funções listadas no manual",
        "não transforma a configuração documentada em pesquisa",
        "Não há inferência ou técnica nova",
        "verificação funcional de acesso"
    ];

    private static readonly string[] ReservationPhrases =
    [
        "não foi ensaiado",
        "permanece em aberto",
        "ainda não foi validada",
        "segue aberto",
        "segue em teste",
        "não satisfaz a exigência",
        "Falta regressão",
        "não há ensaio",
        "não foi validada",
        "integra a pretensão",
        "incluído na pretensão",
        "objetivo original incluía",
        "o estresse não",
        "não constitui validação completa"
    ];

    private static readonly string[] ExcludedUpFrontPhrases =
    [
        "excluídos antes",
        "não foram reivindicadas",
        "Não se reivindica",
        "não são condições incluídas na conclusão"
    ];

    public RubricOutcome Evaluate(Project project, ProjectDossier dossier, IReadOnlyList<MathCheck> math)
    {
        var mechanism = Section(dossier.MethodSections, "2");
        var limit = Section(dossier.MethodSections, "6");
        var reviewLimit = Section(dossier.ReviewSections, "Limites e pendências técnicas");
        var corpus = string.Join("\n", new[] { mechanism, limit, reviewLimit }.Where(s => !string.IsNullOrWhiteSpace(s)));
        var excerpt = Excerpt(mechanism);
        var limitText = string.IsNullOrWhiteSpace(limit) ? Excerpt(reviewLimit) : Excerpt(limit);
        var hasPerformance = project.Results.Any(r => r.Nature == "desempenho");
        var mathFailed = math.Any(m => !m.Confirmed);
        var ev = (string suffix) => $"{project.Id}-{suffix}";

        var gap = ContainsAny(corpus, GapPhrases);
        var known = ContainsAny(corpus, KnownApplicationPhrases);
        var reservation = ContainsAny(corpus, ReservationPhrases) && !ContainsAny(corpus, ExcludedUpFrontPhrases);
        var hypothesis = corpus.Contains("hipótese", StringComparison.OrdinalIgnoreCase)
            || corpus.Contains("hipoteses", StringComparison.OrdinalIgnoreCase);

        List<CriterionEvaluation> criteria;
        List<MissingEvidence> missing;
        double confidence;

        if (gap)
        {
            criteria = GapCriteria(project, excerpt, limitText, ev);
            missing =
            [
                Missing(CriterionType.TechnologicalUncertainty, limitText, "Versão executada, regra de decisão e saídas que liguem o mecanismo alegado aos registros.")
            ];
            confidence = 0.84;
        }
        else if (known)
        {
            criteria = KnownCriteria(project, excerpt, hasPerformance, ev);
            missing = [];
            confidence = 0.86;
        }
        else if (!hasPerformance)
        {
            criteria = UnverifiedCriteria(project, excerpt, ev);
            missing =
            [
                Missing(CriterionType.Systematicity, limitText, "Registros de desempenho do núcleo alegado, com versão e denominador.")
            ];
            confidence = 0.7;
        }
        else
        {
            criteria = InvestigatedCriteria(project, excerpt, reservation, mathFailed, ev);
            missing = reservation
                ? [Missing(CriterionType.Transferability, limitText, "Ensaio do ponto que a própria revisão deixa em aberto, dentro da pretensão original.")]
                : [];
            confidence = hypothesis ? 0.8 : 0.64;
            if (mathFailed)
            {
                confidence = Math.Max(0.45, confidence - 0.15);
            }
        }

        var map = criteria.ToDictionary(c => c.Criterion, c => c.Status);
        var classification = RecommendationPolicy.Decide(map);
        var summary = classification switch
        {
            Classification.Eligible => "As evidências do próprio projeto sustentam P&D no recorte registrado. A recomendação preliminar é Elegível.",
            Classification.WithReservations => "Há investigação documentada, com limitação concreta que restringe parte da conclusão. A recomendação preliminar é Com ressalvas.",
            Classification.NotEligible => "O mecanismo registrado descreve aplicação de técnica já prevista, sem investigação tecnológica demonstrada. A recomendação preliminar é Não elegível.",
            _ => "Falta informação essencial para distinguir P&D de rotina. A recomendação preliminar é Evidência insuficiente."
        };

        return new RubricOutcome
        {
            Classification = classification,
            Summary = summary + " A decisão final permanece com o analista.",
            Limit = string.IsNullOrWhiteSpace(limitText) ? "O método não registra um limite explícito na seção 6." : limitText,
            Confidence = confidence,
            Criteria = criteria,
            Missing = missing
        };
    }

    private static List<CriterionEvaluation> GapCriteria(Project project, string excerpt, string limit, Func<string, string> ev) =>
    [
        Criterion(project, CriterionType.Novelty, CriterionStatus.Indeterminate, 0.8,
            $"O método não fecha a novidade. {excerpt}", [ev("CLM-MEC")], [], [ev("EV06")], [ev("EV06")]),
        Criterion(project, CriterionType.Creativity, CriterionStatus.Indeterminate, 0.8,
            "A criatividade técnica fica indeterminada porque o mecanismo executado não está verificável.", [ev("CLM-MEC")], [], [ev("EV06")], [ev("EV06")]),
        Criterion(project, CriterionType.TechnologicalUncertainty, CriterionStatus.AllegedNotVerifiable, 0.86,
            $"A incerteza está alegada e não verificável. {limit}", [ev("CLM-MEC")], [], [ev("EV06"), ev("EV13")], [ev("EV08")]),
        Criterion(project, CriterionType.Systematicity, CriterionStatus.Partial, 0.86,
            "Os registros não ligam versão, causa e saída do núcleo alegado.", [ev("CLM-ATV05")], [ev("EV08")], [], [ev("EV09")]),
        Criterion(project, CriterionType.Transferability, CriterionStatus.InsufficientForCore, 0.86,
            "Sem o núcleo verificável, o conhecimento não é transferível no ponto que a equipe alega.", [ev("CLM-ATV08")], [], [ev("EV13")], [ev("EV13")])
    ];

    private static List<CriterionEvaluation> KnownCriteria(Project project, string excerpt, bool hasPerformance, Func<string, string> ev)
    {
        return
        [
            Criterion(project, CriterionType.Novelty, CriterionStatus.NotDemonstrated, 0.88,
                $"O próprio método descreve aplicação de recurso já previsto. {excerpt}", [ev("CLM-MEC"), ev("CLM-ATV02")], [], [ev("EV06")], []),
            Criterion(project, CriterionType.Creativity, CriterionStatus.NotDemonstrated, 0.88,
                "O trabalho registrado é configuração, mapeamento ou receita existente, não um mecanismo novo.", [ev("CLM-MEC"), ev("CLM-ATV03")], [], [ev("EV05"), ev("EV06")], []),
            Criterion(project, CriterionType.TechnologicalUncertainty, CriterionStatus.NotCharacterized, 0.88,
                "Os desvios registrados se resolvem por configuração ou receita já descrita. Não há hipótese técnica desconhecida verificada.", [ev("CLM-MEC")], [], [ev("EV06")], []),
            Criterion(project, CriterionType.Systematicity, CriterionStatus.DocumentedAsAcceptance, 0.8,
                hasPerformance
                    ? "Há registros de aceite. A existência de roteiros e resultados não transforma rotina em P&D."
                    : "O método caracteriza o trabalho como rotina. Não há indicador de desempenho além dessa declaração.",
                [ev("CLM-ATV05"), ev("CLM-ATV07")], [ev("EV08"), ev("EV09")], [], []),
            Criterion(project, CriterionType.Transferability, CriterionStatus.DocumentedForConfiguration, 0.84,
                "Receita, parâmetros e resultados estão localizados. A documentação descreve a configuração, não um conhecimento novo de P&D.", [ev("CLM-ATV08")], [ev("EV07"), ev("EV13")], [], [])
        ];
    }

    private static List<CriterionEvaluation> UnverifiedCriteria(Project project, string excerpt, Func<string, string> ev) =>
    [
        Criterion(project, CriterionType.Novelty, CriterionStatus.Indeterminate, 0.6, $"O mecanismo está descrito, mas sem desempenho que o verifique. {excerpt}", [ev("CLM-MEC")], [ev("EV06")], [], [ev("EV08")]),
        Criterion(project, CriterionType.Creativity, CriterionStatus.Indeterminate, 0.6, "A especificação não chega a uma execução verificável.", [ev("CLM-MEC")], [ev("EV06")], [], [ev("EV09")]),
        Criterion(project, CriterionType.TechnologicalUncertainty, CriterionStatus.AllegedNotVerifiable, 0.72, "A incerteza pode estar alegada, mas não há resultado de desempenho para investigá-la.", [ev("CLM-MEC")], [], [ev("EV06")], [ev("EV08")]),
        Criterion(project, CriterionType.Systematicity, CriterionStatus.Partial, 0.8, "Não há linha de resultados com natureza desempenho.", [ev("CLM-ATV07")], [], [], [ev("EV09")]),
        Criterion(project, CriterionType.Transferability, CriterionStatus.InsufficientForCore, 0.8, "O núcleo alegado não tem registro suficiente para outra pessoa reproduzir a conclusão.", [ev("CLM-ATV08")], [], [ev("EV13")], [ev("EV13")])
    ];

    private static List<CriterionEvaluation> InvestigatedCriteria(Project project, string excerpt, bool reservation, bool mathFailed, Func<string, string> ev)
    {
        var transfer = reservation ? CriterionStatus.DocumentedWithLimit : CriterionStatus.DocumentedInScope;
        var mathNote = mathFailed
            ? " Há divergência entre o valor declarado e o recálculo das medições; o registro primário prevalece onde a conta fecha, e a divergência fica explícita."
            : " Os indicadores de desempenho conferem com as medições do ensaio.";
        return
        [
            Criterion(project, CriterionType.Novelty, CriterionStatus.DemonstratedInScope, 0.78,
                $"O método distingue o mecanismo do recurso anterior. {excerpt}", [ev("CLM-MEC"), ev("CLM-ATV02")], [ev("EV06"), ev("EV01")], [], []),
            Criterion(project, CriterionType.Creativity, CriterionStatus.DemonstratedInScope, 0.78,
                "A seção 2 do método registra uma regra operacional própria, não só o nome de um produto.", [ev("CLM-MEC"), ev("CLM-ATV03")], [ev("EV05"), ev("EV06")], [], []),
            Criterion(project, CriterionType.TechnologicalUncertainty, CriterionStatus.Investigated, 0.78,
                "A incerteza foi colocada em método, versões e comparação, não apenas anunciada.", [ev("CLM-MEC")], [ev("EV06"), ev("EV08")], [], []),
            Criterion(project, CriterionType.Systematicity, CriterionStatus.Documented, mathFailed ? 0.62 : 0.84,
                "Há resultados de desempenho ligados a ensaio e versão." + mathNote, [ev("CLM-ATV05"), ev("CLM-ATV07")], [ev("EV08"), ev("EV09")], mathFailed ? [ev("EV09")] : [], []),
            Criterion(project, CriterionType.Transferability, transfer, 0.76,
                reservation
                    ? "O próprio limite do método restringe parte da conclusão pretendida. Isso sustenta ressalva, não ausência total de prova."
                    : "O limite registrado é fronteira do recorte, não um ponto da pretensão que tenha ficado sem ensaio.",
                [ev("CLM-ATV08")], [ev("EV13"), ev("EV07")], [], reservation ? [ev("EV13")] : [])
        ];
    }

    private static CriterionEvaluation Criterion(
        Project project,
        CriterionType type,
        CriterionStatus status,
        double confidence,
        string reasoning,
        IEnumerable<string> claims,
        IEnumerable<string> supporting,
        IEnumerable<string> contrary,
        IEnumerable<string> missing) => new()
    {
        Id = Guid.NewGuid(),
        Criterion = type,
        Status = status,
        Confidence = confidence,
        Reasoning = reasoning,
        ClaimCodes = claims.ToList(),
        SupportingEvidenceIds = supporting.ToList(),
        ContraryEvidenceIds = contrary.ToList(),
        MissingEvidenceIds = missing.ToList()
    };

    private static MissingEvidence Missing(CriterionType criterion, string description, string needed) => new()
    {
        Id = Guid.NewGuid(),
        AffectedCriterion = criterion,
        Description = string.IsNullOrWhiteSpace(description) ? "O material não fecha o núcleo alegado." : description,
        NeededEvidence = needed
    };

    private static string Section(IReadOnlyDictionary<string, string> sections, string key) =>
        sections.TryGetValue(key, out var value) ? value : "";

    private static bool ContainsAny(string text, IEnumerable<string> phrases) =>
        phrases.Any(phrase => text.Contains(phrase, StringComparison.OrdinalIgnoreCase));

    private static string Excerpt(string text)
    {
        var flat = string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (flat.Length <= 420)
        {
            return flat;
        }

        return flat[..420].TrimEnd() + "…";
    }
}
