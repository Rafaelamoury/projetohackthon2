using InovaTrace.Application;
using InovaTrace.Domain;
using InovaTrace.Infrastructure;

namespace InovaTrace.Tests;

public class RecommendationPolicyTests
{
    [Fact]
    public void Elegivel_quando_os_cinco_estados_sustentam_pesquisa()
    {
        var decision = RecommendationPolicy.Decide(new Dictionary<CriterionType, CriterionStatus>
        {
            [CriterionType.Novelty] = CriterionStatus.DemonstratedInScope,
            [CriterionType.Creativity] = CriterionStatus.DemonstratedInScope,
            [CriterionType.TechnologicalUncertainty] = CriterionStatus.Investigated,
            [CriterionType.Systematicity] = CriterionStatus.Documented,
            [CriterionType.Transferability] = CriterionStatus.DocumentedInScope
        });

        Assert.Equal(Classification.Eligible, decision);
    }

    [Fact]
    public void Ressalva_nao_esconde_lacuna_essencial()
    {
        var decision = RecommendationPolicy.Decide(new Dictionary<CriterionType, CriterionStatus>
        {
            [CriterionType.Novelty] = CriterionStatus.Indeterminate,
            [CriterionType.Creativity] = CriterionStatus.DemonstratedInScope,
            [CriterionType.TechnologicalUncertainty] = CriterionStatus.Investigated,
            [CriterionType.Systematicity] = CriterionStatus.Documented,
            [CriterionType.Transferability] = CriterionStatus.DocumentedWithLimit
        });

        Assert.Equal(Classification.InsufficientEvidence, decision);
    }

    [Fact]
    public void Rotina_documentada_nao_e_elegivel()
    {
        var decision = RecommendationPolicy.Decide(new Dictionary<CriterionType, CriterionStatus>
        {
            [CriterionType.Novelty] = CriterionStatus.NotDemonstrated,
            [CriterionType.Creativity] = CriterionStatus.NotDemonstrated,
            [CriterionType.TechnologicalUncertainty] = CriterionStatus.NotCharacterized,
            [CriterionType.Systematicity] = CriterionStatus.DocumentedAsAcceptance,
            [CriterionType.Transferability] = CriterionStatus.DocumentedForConfiguration
        });

        Assert.Equal(Classification.NotEligible, decision);
    }
}

public class MeasurementValidatorTests
{
    [Fact]
    public void Contagem_do_prj01_recalcula_oito_sobre_doze()
    {
        var result = new AssayResult
        {
            EssayId = "PRJ01-S01",
            Operation = "contagem",
            Nature = "desempenho",
            Value = 8,
            Base = 12,
            RatePercent = 66.666667m,
            Metric = "duplicatas retidas"
        };
        var rows = new List<Measurement>
        {
            new() { EssayId = "PRJ01-S01", Kind = "contador", Numerator = 8, Denominator = 12 }
        };

        var check = MeasurementValidator.Evaluate(result, rows);

        Assert.True(check.Confirmed);
        Assert.Contains("8", check.Recalculated, StringComparison.Ordinal);
    }

    [Fact]
    public void Percentil_95_usa_o_posto_teto()
    {
        var bins = new List<(decimal, decimal)> { (10, 1), (20, 1), (30, 1), (40, 17) };
        var value = MeasurementValidator.Percentile95(bins);
        Assert.Equal(40, value);
    }
}

public class RubricTests
{
    [Fact]
    public void Metodo_que_nega_mecanismo_novo_nao_e_elegivel()
    {
        var outcome = Evaluate(
            "Configurar deduplicação. Nenhum algoritmo do barramento foi modificado.",
            "Não há hipótese de mecanismo novo, apenas adequação da configuração ao contrato.",
            desempenho: true);

        Assert.Equal(Classification.NotEligible, outcome.Classification);
    }

    [Fact]
    public void Falta_explicita_e_evidencia_insuficiente()
    {
        var outcome = Evaluate(
            "Plano propõe comparar sinais, sem limiares aprovados.",
            "Faltam versão do classificador, causas de referência e decisões por evento.",
            desempenho: false);

        Assert.Equal(Classification.InsufficientEvidence, outcome.Classification);
    }

    [Fact]
    public void Hipotese_com_ponto_nao_ensaiado_gera_ressalva()
    {
        var outcome = Evaluate(
            "A hipótese combina recuperação de gravação parcial e invalidação lógica.",
            "Corte de energia nessa janela não foi ensaiado.",
            desempenho: true);

        Assert.Equal(Classification.WithReservations, outcome.Classification);
    }

    private static RubricOutcome Evaluate(string section2, string section6, bool desempenho)
    {
        var project = new Project
        {
            Id = "PRJXX",
            Results = desempenho
                ? [new AssayResult { Nature = "desempenho", Operation = "contagem", Value = 1, Base = 1 }]
                : []
        };
        var dossier = new ProjectDossier
        {
            Project = project,
            MethodMarkdown = "",
            ReviewMarkdown = "",
            MethodSections = new Dictionary<string, string> { ["2"] = section2, ["6"] = section6 },
            ReviewSections = new Dictionary<string, string>()
        };
        return new FrascatiRubric().Evaluate(project, dossier, []);
    }
}

public class ContradictionTests
{
    [Fact]
    public void Depoimento_1100_diverge_do_registro_1152()
    {
        var results = new List<AssayResult>
        {
            new() { EssayId = "PRJ02-S05", Value = 1152, Base = 1200, RatePercent = 96, Metric = "pares corretos", Version = "restrito-v4", Source = "evidencias/medicoes.csv#PRJ02-S05" }
        };

        var mismatch = ContradictionDetector.FindMemoryMismatch("No fechamento registrei 1.100 pares corretos para o grafo restrito.", results);

        Assert.NotNull(mismatch);
        Assert.Equal("1.100", mismatch.Value.Spoken);
    }

    [Fact]
    public void Depoimento_nomeia_a_versao_e_nao_cola_no_resultado_vizinho()
    {
        var results = new List<AssayResult>
        {
            new() { EssayId = "PRJ02-S03", Value = 1104, Base = 1200, Version = "bipartido-v3", Metric = "pares corretos" },
            new() { EssayId = "PRJ02-S05", Value = 1152, Base = 1200, Version = "restrito-v4", Metric = "pares corretos" }
        };

        var mismatch = ContradictionDetector.FindMemoryMismatch("No fechamento registrei 1.100 pares corretos para o grafo restrito.", results);

        Assert.NotNull(mismatch);
        Assert.Equal("PRJ02-S05", mismatch.Value.Result.EssayId);
    }

    [Fact]
    public void Conclusao_para_quando_a_proxima_pergunta_vem_colada_no_pdf()
    {
        var text = "4. Como ficou a conclusão da rodada? No fechamento registrei 1.100 pares corretos para o grafo restrito.5. Que alternativas ou recursos já existiam? Igualdade textual.";
        var answer = InterviewConclusion.Extract(text);
        Assert.Equal("No fechamento registrei 1.100 pares corretos para o grafo restrito.", answer);
    }
}

public class ImporterTests
{
    [Fact]
    public async Task Prj01_importa_atividades_evidencias_e_documentos_sem_inferir_classe()
    {
        var importer = new HackathonDatasetImporter();
        var project = await importer.ImportAsync(Path.Combine(DatasetRoot.Find(), "01_projetos", "01_historico", "PRJ01"), CancellationToken.None);

        Assert.Equal("PRJ01", project.Id);
        Assert.Equal(8, project.Activities.Count);
        Assert.Equal(14, project.Evidences.Count);
        Assert.Equal(14, project.Documents.Count);
        Assert.Equal(4, project.Results.Count);
        Assert.Contains(project.Activities, a => a.Id == "PRJ01-ATV03" && a.EvidenceIds.Contains("PRJ01-EV06"));
        Assert.DoesNotContain(project.Warnings, w => w.Message.Contains("não está no inventário", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Prj01_nao_recebe_a_classe_historica()
    {
        var importer = new HackathonDatasetImporter();
        var project = await importer.ImportAsync(Path.Combine(DatasetRoot.Find(), "01_projetos", "01_historico", "PRJ01"), CancellationToken.None);
        var engine = new AnalysisEngine(new MeasurementValidator(), new FrascatiRubric(), new ClaimExtractionService(), new ContradictionDetector());
        var dossier = new FileDossierReader().Read(project);
        var run = engine.Run(dossier, DateTimeOffset.Parse("2026-10-07T20:00:00-03:00"));

        Assert.Equal(Classification.NotEligible, run.Recommendation);
        Assert.Equal(4, run.MathChecks.Count);
        Assert.All(run.MathChecks, check => Assert.True(check.Confirmed));
        Assert.Contains(run.Claims, c => c.Code == "PRJ01-CLM-ATV03" && c.EvidenceIds.Contains("PRJ01-EV06"));
    }

    [Fact]
    public async Task Calibracao_mede_os_vinte_historicos_sem_entregar_o_gabarito_ao_motor()
    {
        var root = DatasetRoot.Find();
        var catalog = new HistoricalCatalog().Load(root);
        var importer = new HackathonDatasetImporter();
        var engine = new AnalysisEngine(new MeasurementValidator(), new FrascatiRubric(), new ClaimExtractionService(), new ContradictionDetector());
        var reader = new FileDossierReader();
        var correct = 0;
        var misses = new List<string>();

        foreach (var id in catalog.Keys.OrderBy(k => k, StringComparer.Ordinal))
        {
            var project = await importer.ImportAsync(Path.Combine(root, "01_projetos", "01_historico", id), CancellationToken.None);
            var run = engine.Run(reader.Read(project), DateTimeOffset.UnixEpoch);
            var actual = Labels.Classification(run.Recommendation);
            if (string.Equals(actual, catalog[id].Classification, StringComparison.OrdinalIgnoreCase))
            {
                correct++;
            }
            else
            {
                misses.Add($"{id}: esperado {catalog[id].Classification}, obtido {actual}");
            }
        }

        Assert.True(correct >= 16, $"{correct}/{catalog.Count}\n" + string.Join("\n", misses));
    }
}

public static class DatasetRoot
{
    public static string Find()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "dados-originais");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("dados-originais não encontrado.");
    }
}
