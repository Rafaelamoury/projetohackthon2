using System.Globalization;
using System.Text.RegularExpressions;
using InovaTrace.Domain;

namespace InovaTrace.Application;

public sealed class ContradictionDetector
{
    public IReadOnlyList<Contradiction> Detect(Project project, string? conclusion, IReadOnlyList<MathCheck> math)
    {
        var found = new List<Contradiction>();
        foreach (var check in math.Where(m => !m.Confirmed))
        {
            found.Add(new Contradiction
            {
                Id = Guid.NewGuid(),
                SourceA = "resultados.csv",
                SourceB = "medicoes.csv",
                ClaimA = $"{project.Id}-CLM-ATV07",
                ClaimB = check.EssayId,
                AffectedCriterion = CriterionType.Systematicity,
                Severity = ContradictionSeverity.High,
                Explanation = $"O ensaio {check.EssayId} ({check.Metric}) não fecha. {check.Detail} Prevalece o recálculo a partir do registro primário quando a operação do dicionário se aplica."
            });
        }

        if (!string.IsNullOrWhiteSpace(conclusion))
        {
            var memory = FindMemoryMismatch(conclusion, project.Results);
            if (memory is not null)
            {
                found.Add(new Contradiction
                {
                    Id = Guid.NewGuid(),
                    SourceA = "Entrevista",
                    SourceB = $"{memory.Value.Result.Source}",
                    ClaimA = $"{project.Id}-CLM-ENT",
                    ClaimB = memory.Value.Result.EssayId,
                    AffectedCriterion = CriterionType.Systematicity,
                    Severity = ContradictionSeverity.Medium,
                    Explanation = $"A entrevista afirma {memory.Value.Spoken}. O ensaio {memory.Value.Result.EssayId} ({memory.Value.Result.Metric}, versão {memory.Value.Result.Version}) registra {Format(memory.Value.Result.Value)}"
                        + (memory.Value.Result.RatePercent is null ? "." : $" ({Format(memory.Value.Result.RatePercent)}%).")
                        + " Prevalece o registro identificado por versão, recalculável, sobre o depoimento de memória."
                });
            }
        }

        return found;
    }

    public static (string Spoken, AssayResult Result)? FindMemoryMismatch(string conclusion, IReadOnlyList<AssayResult> results)
    {
        if (results.Count == 0)
        {
            return null;
        }

        var pool = PreferResultsNamedInText(conclusion, results);
        var numbers = ExtractNumbers(conclusion);
        foreach (var number in numbers)
        {
            if (pool.Any(r => Close(r.Value, number.Value) || Close(r.Base, number.Value) || (r.RatePercent is not null && Close(r.RatePercent.Value, number.Value))))
            {
                continue;
            }

            var nearest = pool
                .Select(r => new { Result = r, Distance = Relative(r.Value, number.Value) })
                .OrderBy(x => x.Distance)
                .First();
            if (nearest.Distance <= 0.15m && nearest.Distance > 0.005m)
            {
                return (number.Raw, nearest.Result);
            }
        }

        foreach (var percent in ExtractPercents(conclusion))
        {
            var withRate = results.Where(r => r.RatePercent is not null).ToList();
            if (withRate.Count == 0 || withRate.Any(r => decimal.Abs(r.RatePercent!.Value - percent.Value) <= 0.05m))
            {
                continue;
            }

            var nearest = withRate.OrderBy(r => decimal.Abs(r.RatePercent!.Value - percent.Value)).First();
            if (decimal.Abs(nearest.RatePercent!.Value - percent.Value) <= 1.5m)
            {
                return (percent.Raw, nearest);
            }
        }

        return null;
    }

    private static List<AssayResult> PreferResultsNamedInText(string conclusion, IReadOnlyList<AssayResult> results)
    {
        var named = results.Where(result =>
            VersionTokens(result.Version).Any(token => conclusion.Contains(token, StringComparison.OrdinalIgnoreCase))).ToList();
        return named.Count == 0 ? results.ToList() : named;
    }

    private static IEnumerable<string> VersionTokens(string? version) =>
        Regex.Split(version ?? "", @"[^0-9A-Za-zÀ-ÿ]+")
            .Where(token => token.Length >= 6);

    private static bool Close(decimal left, decimal right) => decimal.Abs(left - right) <= 0.05m;

    private static decimal Relative(decimal left, decimal right)
    {
        var scale = Math.Max(Math.Abs(left), 1m);
        return decimal.Abs(left - right) / scale;
    }

    private static List<(string Raw, decimal Value)> ExtractNumbers(string text)
    {
        var found = new List<(string Raw, decimal Value)>();
        var occupied = new List<(int Start, int Length)>();
        foreach (Match match in Regex.Matches(text, @"\b\d{1,3}(?:\.\d{3})+\b"))
        {
            if (decimal.TryParse(match.Value.Replace(".", "", StringComparison.Ordinal), NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            {
                found.Add((match.Value, value));
                occupied.Add((match.Index, match.Length));
            }
        }

        foreach (Match match in Regex.Matches(text, @"\b\d+(?:,\d+)?\s*%"))
        {
            occupied.Add((match.Index, match.Length));
        }

        foreach (Match match in Regex.Matches(text, @"\b\d+\b"))
        {
            if (occupied.Any(span => match.Index >= span.Start && match.Index < span.Start + span.Length))
            {
                continue;
            }

            if (decimal.TryParse(match.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) && value >= 20)
            {
                found.Add((match.Value, value));
            }
        }

        return found;
    }

    private static List<(string Raw, decimal Value)> ExtractPercents(string text)
    {
        var found = new List<(string Raw, decimal Value)>();
        foreach (Match match in Regex.Matches(text, @"\b(\d+(?:,\d+)?)\s*%"))
        {
            var raw = match.Groups[1].Value.Replace(',', '.');
            if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            {
                found.Add((match.Value, value));
            }
        }

        return found;
    }

    private static string Format(decimal? value) =>
        value is null ? "—" : value.Value.ToString("0.######", CultureInfo.InvariantCulture);
}
