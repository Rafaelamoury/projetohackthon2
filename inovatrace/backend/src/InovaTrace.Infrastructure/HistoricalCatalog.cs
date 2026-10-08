using InovaTrace.Application;
using InovaTrace.Domain;

namespace InovaTrace.Infrastructure;

/// <summary>
/// Gabarito só para a calibração. O motor de análise não recebe este catálogo.
/// </summary>
public sealed class HistoricalCatalog : IHistoricalCatalog
{
    public IReadOnlyDictionary<string, HistoricalLabel> Load(string datasetRoot)
    {
        var path = Path.Combine(datasetRoot, "historicos_classificados.csv");
        var rows = HackathonDatasetImporter.ReadCsv(path);
        var map = new Dictionary<string, HistoricalLabel>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            map[row["projeto_id"]] = new HistoricalLabel
            {
                ProjectId = row["projeto_id"],
                Classification = row["classificacao"],
                CriterionStates = new Dictionary<CriterionType, string>
                {
                    [CriterionType.Novelty] = row["estado_1"],
                    [CriterionType.Creativity] = row["estado_2"],
                    [CriterionType.TechnologicalUncertainty] = row["estado_3"],
                    [CriterionType.Systematicity] = row["estado_4"],
                    [CriterionType.Transferability] = row["estado_5"]
                }
            };
        }

        return map;
    }
}
