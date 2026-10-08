using System.Globalization;
using InovaTrace.Domain;

namespace InovaTrace.Application;

public sealed class MeasurementValidator
{
    private const decimal Tolerance = 0.000001m;

    public IReadOnlyList<MathCheck> Validate(Project project)
    {
        var checks = new List<MathCheck>();
        foreach (var result in project.Results)
        {
            var rows = project.Measurements.Where(m => m.EssayId == result.EssayId).ToList();
            var check = Evaluate(result, rows);
            checks.Add(check);
        }

        return checks;
    }

    public static MathCheck Evaluate(AssayResult result, IReadOnlyList<Measurement> rows)
    {
        decimal? value = null;
        decimal? basis = null;
        decimal? rate = null;
        string detail;

        switch (result.Operation)
        {
            case "contagem":
                var counters = rows.Where(r => r.Kind == "contador").ToList();
                if (counters.Count == 0 || counters.Any(r => r.Numerator is null || r.Denominator is null))
                {
                    detail = "Não há contadores completos deste ensaio em medicoes.csv.";
                    break;
                }

                value = counters.Sum(r => r.Numerator!.Value);
                basis = counters.Sum(r => r.Denominator!.Value);
                if (result.Nature == "desempenho" && basis != 0)
                {
                    rate = decimal.Round(100m * value.Value / basis.Value, 6, MidpointRounding.AwayFromZero);
                }

                detail = $"{Format(value)} / {Format(basis)}";
                break;

            case "media":
                var meanValues = Values(rows, "medicao");
                if (meanValues.Count == 0)
                {
                    detail = "Não há medições deste ensaio para calcular a média.";
                    break;
                }

                value = meanValues.Average();
                basis = meanValues.Count;
                detail = $"média de {meanValues.Count} valores";
                break;

            case "mediana":
                var medianValues = Values(rows, "medicao");
                if (medianValues.Count == 0)
                {
                    detail = "Não há medições deste ensaio para calcular a mediana.";
                    break;
                }

                value = Median(medianValues);
                basis = medianValues.Count;
                detail = $"mediana de {medianValues.Count} valores";
                break;

            case "diferenca_maior_menor":
                var span = Values(rows, "medicao");
                if (span.Count == 0)
                {
                    detail = "Não há medições deste ensaio para a diferença.";
                    break;
                }

                value = span.Max() - span.Min();
                basis = span.Count;
                detail = "maior menos menor; a base é a quantidade de valores, não um divisor";
                break;

            case "percentil_95":
                var bins = rows.Where(r => r.Kind == "histograma" && r.Value is not null && r.Weight is not null)
                    .Select(r => (r.Value!.Value, r.Weight!.Value))
                    .ToList();
                if (bins.Count == 0)
                {
                    detail = "Não há histograma deste ensaio.";
                    break;
                }

                value = Percentile95(bins);
                basis = bins.Sum(b => b.Item2);
                detail = "posto teto(0,95 × soma dos pesos)";
                break;

            case "valor_observado":
                var observed = Values(rows, "medicao");
                if (observed.Count != 1)
                {
                    detail = observed.Count == 0
                        ? "Não há valor observado deste ensaio."
                        : "valor_observado exige uma única medição; há mais de uma linha.";
                    break;
                }

                value = observed[0];
                basis = 1;
                detail = "transcrição de uma medição";
                break;

            case "indicador_precalculado":
                var transcribed = rows.Where(r => r.Value is not null).Select(r => r.Value!.Value).ToList();
                if (transcribed.Count != 1)
                {
                    detail = "indicador_precalculado confere a transcrição e não refaz o cálculo original.";
                    break;
                }

                value = transcribed[0];
                basis = 1;
                detail = "conferência de transcrição";
                break;

            default:
                detail = $"Operação '{result.Operation}' não está no dicionário do pacote.";
                break;
        }

        var valueOk = value is not null && decimal.Abs(value.Value - result.Value) <= Tolerance;
        var baseOk = basis is null || decimal.Abs(basis.Value - result.Base) <= Tolerance;
        var rateOk = result.RatePercent is null
            ? rate is null
            : rate is not null && decimal.Abs(decimal.Round(rate.Value, 6) - result.RatePercent.Value) <= Tolerance;
        var confirmed = valueOk && baseOk && rateOk;

        if (!confirmed && value is not null)
        {
            detail = $"Declarado {Format(result.Value)} (base {Format(result.Base)}). Recalculado {Format(value)} (base {Format(basis)}). {detail}";
        }
        else if (confirmed)
        {
            detail = $"Confirmado: {detail}.";
        }

        return new MathCheck
        {
            Id = Guid.NewGuid(),
            EssayId = result.EssayId,
            Metric = result.Metric,
            Operation = result.Operation,
            Declared = result.RatePercent is null ? Format(result.Value) : $"{Format(result.Value)} ({Format(result.RatePercent)}%)",
            Recalculated = value is null ? "não recalculado" : rate is null ? Format(value) : $"{Format(value)} ({Format(rate)}%)",
            Confirmed = confirmed,
            Detail = detail
        };
    }

    public static decimal Percentile95(IReadOnlyList<(decimal Value, decimal Weight)> bins)
    {
        var ordered = bins.OrderBy(b => b.Value).ToList();
        var total = ordered.Sum(b => b.Weight);
        var rank = decimal.Ceiling(0.95m * total);
        decimal accumulated = 0;
        foreach (var bin in ordered)
        {
            accumulated += bin.Weight;
            if (accumulated >= rank)
            {
                return bin.Value;
            }
        }

        return ordered[^1].Value;
    }

    public static decimal Median(IReadOnlyList<decimal> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        var n = sorted.Count;
        if (n % 2 == 1)
        {
            return sorted[n / 2];
        }

        return (sorted[n / 2 - 1] + sorted[n / 2]) / 2m;
    }

    private static List<decimal> Values(IReadOnlyList<Measurement> rows, string kind) =>
        rows.Where(r => r.Kind == kind && r.Value is not null).Select(r => r.Value!.Value).ToList();

    private static string Format(decimal? value) =>
        value is null ? "—" : value.Value.ToString("0.######", CultureInfo.InvariantCulture);
}
