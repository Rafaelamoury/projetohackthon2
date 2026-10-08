using System.Globalization;
using System.Text;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using InovaTrace.Application;
using InovaTrace.Domain;

namespace InovaTrace.Infrastructure;

public sealed class HackathonDatasetImporter : IDatasetImporter
{
    public Task<Project> ImportAsync(string projectDirectory, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var directory = new DirectoryInfo(projectDirectory);
        if (!directory.Exists)
        {
            throw new DirectoryNotFoundException($"Pasta do projeto não encontrada: {projectDirectory}");
        }

        var datasetRoot = FindDatasetRoot(directory) ?? directory.FullName;
        var index = ReadIndex(datasetRoot);
        index.TryGetValue(directory.Name, out var meta);

        var project = new Project
        {
            Id = directory.Name,
            Title = meta?.Title ?? directory.Name,
            Group = meta?.Group ?? (directory.FullName.Contains("historico", StringComparison.OrdinalIgnoreCase) ? ProjectGroup.Historical : ProjectGroup.Analysis),
            Team = meta?.Team ?? "",
            DurationWeeks = meta?.DurationWeeks ?? 0,
            SourceDirectory = directory.FullName,
            ImportedAt = DateTimeOffset.UtcNow
        };

        if (meta is null)
        {
            Warn(project, "projetos.csv não tem linha para esta pasta. Título e grupo vieram só do caminho.");
        }

        var inventory = ReadCsv(Path.Combine(directory.FullName, "inventario_evidencias.csv"));
        var documents = ReadDocuments(datasetRoot);
        foreach (var row in inventory)
        {
            var evidenceId = Field(row, "id_evidencia");
            var relative = Field(row, "arquivo").Replace('\\', '/');
            var absolute = Path.Combine(directory.FullName, relative.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(absolute))
            {
                Warn(project, $"Inventário cita {evidenceId} em '{relative}', mas o arquivo não está na pasta.");
            }

            project.Evidences.Add(new Evidence
            {
                Id = evidenceId,
                ProjectId = project.Id,
                EvidenceType = Field(row, "tipo"),
                RelativeFile = relative,
                ExpectedContent = Field(row, "conteudo_esperado"),
                InventoryStatus = Field(row, "status"),
                Note = Field(row, "observacao")
            });

            var doc = documents.FirstOrDefault(d => Field(d, "id_evidencia") == evidenceId);
            project.Documents.Add(new DocumentRecord
            {
                Id = doc is null ? evidenceId.Replace("-EV", "-DOC", StringComparison.Ordinal) : Field(doc, "documento_id"),
                ProjectId = project.Id,
                EvidenceId = evidenceId,
                FileName = relative,
                PackagePath = doc is null ? relative : Field(doc, "caminho_no_pacote"),
                DocumentType = Field(row, "tipo")
            });
        }

        if (documents.Count > 0 && !documents.Any(d => Field(d, "projeto_id") == project.Id))
        {
            Warn(project, "documentos.csv não tem linhas deste projeto. Os documentos foram montados só pelo inventário.");
        }

        var evidenceIds = project.Evidences.Select(e => e.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var row in ReadCsv(Path.Combine(directory.FullName, "atividades.csv")))
        {
            var refs = Field(row, "evidencias_relacionadas")
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            foreach (var reference in refs.Where(id => !evidenceIds.Contains(id)))
            {
                Warn(project, $"Atividade {Field(row, "id_atividade")} cita {reference}, que não está no inventário.");
            }

            project.Activities.Add(new Activity
            {
                Id = Field(row, "id_atividade"),
                ProjectId = project.Id,
                Cycle = Field(row, "ciclo"),
                Phase = Field(row, "fase"),
                DeclaredNature = Field(row, "natureza_informada_pela_equipe"),
                Description = Field(row, "descricao"),
                Output = Field(row, "resultado_ou_saida"),
                ResponsibleFunction = Field(row, "responsavel_por_funcao"),
                EvidenceIds = refs
            });
        }

        foreach (var row in ReadCsv(Path.Combine(directory.FullName, "evidencias", "medicoes.csv")))
        {
            project.Measurements.Add(new Measurement
            {
                Id = Field(row, "registro_id"),
                ProjectId = project.Id,
                EssayId = Field(row, "ensaio_id"),
                Version = Field(row, "versao"),
                Scenario = Field(row, "cenario"),
                Kind = Field(row, "tipo"),
                Metric = Field(row, "metrica"),
                Value = DecimalOrNull(row, "valor"),
                Numerator = DecimalOrNull(row, "numerador"),
                Denominator = DecimalOrNull(row, "denominador"),
                Weight = DecimalOrNull(row, "peso"),
                Unit = Field(row, "unidade")
            });
        }

        foreach (var row in ReadCsv(Path.Combine(directory.FullName, "evidencias", "resultados.csv")))
        {
            project.Results.Add(new AssayResult
            {
                ProjectId = project.Id,
                EssayId = Field(row, "ensaio_id"),
                Version = Field(row, "versao"),
                Metric = Field(row, "metrica"),
                Operation = Field(row, "operacao"),
                Value = DecimalOrNull(row, "valor") ?? 0,
                Base = DecimalOrNull(row, "base_de_calculo") ?? 0,
                BaseDescription = Field(row, "descricao_base"),
                RatePercent = DecimalOrNull(row, "taxa_percentual"),
                Unit = Field(row, "unidade"),
                Source = Field(row, "fonte"),
                Nature = Field(row, "natureza")
            });
        }

        var measured = project.Measurements.Select(m => m.EssayId).ToHashSet(StringComparer.Ordinal);
        var resulted = project.Results.Select(r => r.EssayId).ToHashSet(StringComparer.Ordinal);
        foreach (var essay in resulted.Where(id => !measured.Contains(id)))
        {
            Warn(project, $"Resultado {essay} não tem linhas em medicoes.csv.");
        }

        foreach (var essay in measured.Where(id => !resulted.Contains(id)))
        {
            Warn(project, $"Medição do ensaio {essay} não tem linha em resultados.csv.");
        }

        foreach (var row in ReadCsv(Path.Combine(directory.FullName, "evidencias", "cronologia.csv")))
        {
            project.Chronology.Add(new ChronologyEvent
            {
                Id = Field(row, "evento_id"),
                ProjectId = project.Id,
                Date = Field(row, "data"),
                Version = Field(row, "versao"),
                Event = Field(row, "evento"),
                State = Field(row, "estado"),
                Source = Field(row, "fonte")
            });
        }

        foreach (var row in ReadCsv(Path.Combine(directory.FullName, "evidencias", "observacoes.csv")))
        {
            project.Observations.Add(new ObservationNote
            {
                Id = Field(row, "observacao_id"),
                ProjectId = project.Id,
                Input = Field(row, "entrada"),
                Reference = Field(row, "referencia"),
                OutputOrSituation = Field(row, "saida_ou_situacao"),
                Scope = Field(row, "escopo")
            });
        }

        foreach (var row in ReadCsv(Path.Combine(directory.FullName, "evidencias", "entradas.csv")))
        {
            var json = Field(row, "conteudo_json");
            try
            {
                using var _ = JsonDocument.Parse(json);
            }
            catch (JsonException)
            {
                Warn(project, $"Entrada {Field(row, "entrada_id")} não tem JSON válido. O texto foi preservado sem interpretação.");
            }

            project.Inputs.Add(new InputRecord
            {
                Id = Field(row, "entrada_id"),
                ProjectId = project.Id,
                ContentJson = json
            });
        }

        var configPath = Path.Combine(directory.FullName, "evidencias", "configuracao.json");
        if (File.Exists(configPath))
        {
            project.RawConfigurationJson = File.ReadAllText(configPath);
            NoteExtraJsonKeys(project);
            NoteEssayMismatch(project);
        }
        else
        {
            Warn(project, "evidencias/configuracao.json não foi encontrado.");
        }

        project.Audit.Add(new AuditEntry
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            At = project.ImportedAt,
            Message = $"{project.Id} importado."
        });
        project.Audit.Add(new AuditEntry
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            At = project.ImportedAt.AddSeconds(1),
            Message = $"{project.Evidences.Count} evidências identificadas."
        });

        return Task.FromResult(project);
    }

    public async Task<IReadOnlyList<Project>> ImportCatalogAsync(string datasetRoot, CancellationToken cancellationToken)
    {
        var indexPath = Path.Combine(datasetRoot, "00_dados_apoio", "projetos.csv");
        var rows = ReadCsv(indexPath);
        var projects = new List<Project>();
        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relative = Field(row, "pasta").Replace('/', Path.DirectorySeparatorChar);
            var directory = Path.Combine(datasetRoot, relative);
            projects.Add(await ImportAsync(directory, cancellationToken));
        }

        return projects;
    }

    private static void NoteExtraJsonKeys(Project project)
    {
        try
        {
            using var document = JsonDocument.Parse(project.RawConfigurationJson);
            var expected = new HashSet<string>(StringComparer.Ordinal)
            {
                "projeto_id", "natureza", "versao_documental", "escopo", "parametros", "versoes_registradas",
                "dicionario_medicoes", "ensaios", "dicionario_resultados", "versao_esquema_resultados", "operacoes_resultados"
            };
            var extra = document.RootElement.EnumerateObject().Select(p => p.Name).Where(name => !expected.Contains(name)).ToList();
            if (extra.Count > 0)
            {
                Warn(project, "configuracao.json tem chaves fora do miolo comum, preservadas no JSON bruto: " + string.Join(", ", extra) + ".");
            }
        }
        catch (JsonException)
        {
            Warn(project, "configuracao.json não pôde ser lido como objeto JSON.");
        }
    }

    private static void NoteEssayMismatch(Project project)
    {
        try
        {
            using var document = JsonDocument.Parse(project.RawConfigurationJson);
            if (!document.RootElement.TryGetProperty("ensaios", out var essays) || essays.ValueKind != JsonValueKind.Array)
            {
                Warn(project, "configuracao.json não tem a lista ensaios.");
                return;
            }

            var jsonIds = essays.EnumerateArray()
                .Select(e => e.TryGetProperty("ensaio_id", out var id) ? id.GetString() : null)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet(StringComparer.Ordinal);
            var resultIds = project.Results.Select(r => r.EssayId).ToHashSet(StringComparer.Ordinal);
            if (!jsonIds.SetEquals(resultIds))
            {
                Warn(project, "Os ensaio_id de configuracao.json não coincidem com resultados.csv.");
            }
        }
        catch (JsonException)
        {
            // Já avisado na leitura das chaves.
        }
    }

    private static Dictionary<string, ProjectMeta> ReadIndex(string datasetRoot)
    {
        var path = Path.Combine(datasetRoot, "00_dados_apoio", "projetos.csv");
        if (!File.Exists(path))
        {
            return [];
        }

        var map = new Dictionary<string, ProjectMeta>(StringComparer.Ordinal);
        foreach (var row in ReadCsv(path))
        {
            map[Field(row, "projeto_id")] = new ProjectMeta(
                Field(row, "titulo"),
                Field(row, "grupo").Contains("Hist", StringComparison.OrdinalIgnoreCase) ? ProjectGroup.Historical : ProjectGroup.Analysis,
                Field(row, "equipe"),
                int.TryParse(Field(row, "duracao_semanas"), out var weeks) ? weeks : 0);
        }

        return map;
    }

    private static List<Dictionary<string, string>> ReadDocuments(string datasetRoot)
    {
        var path = Path.Combine(datasetRoot, "00_dados_apoio", "documentos.csv");
        return File.Exists(path) ? ReadCsv(path) : [];
    }

    private static string? FindDatasetRoot(DirectoryInfo start)
    {
        var current = start;
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "00_dados_apoio", "projetos.csv")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }

    public static List<Dictionary<string, string>> ReadCsv(string path)
    {
        using var reader = new StreamReader(path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            BadDataFound = null,
            MissingFieldFound = null,
            HeaderValidated = null
        });
        if (!csv.Read() || !csv.ReadHeader() || csv.HeaderRecord is null)
        {
            return [];
        }

        var rows = new List<Dictionary<string, string>>();
        while (csv.Read())
        {
            var row = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var header in csv.HeaderRecord)
            {
                row[header] = csv.GetField(header) ?? "";
            }

            rows.Add(row);
        }

        return rows;
    }

    private static string Field(Dictionary<string, string> row, string name) =>
        row.TryGetValue(name, out var value) ? value.Trim() : "";

    private static decimal? DecimalOrNull(Dictionary<string, string> row, string name)
    {
        var raw = Field(row, name);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return decimal.Parse(raw, CultureInfo.InvariantCulture);
    }

    private static void Warn(Project project, string message) =>
        project.Warnings.Add(new ImportWarning { ProjectId = project.Id, Message = message });

    private sealed record ProjectMeta(string Title, ProjectGroup Group, string Team, int DurationWeeks);
}
