using System.Text.RegularExpressions;
using InovaTrace.Domain;

namespace InovaTrace.Application;

/// <summary>
/// Transcreve atividades, a seção 2 do método e a conclusão da entrevista.
/// Não classifica e não inventa evidência: o vínculo é o que o arquivo já declara.
/// </summary>
public sealed class ClaimExtractionService : IClaimExtractionService
{
    public IReadOnlyList<Claim> Extract(ProjectDossier dossier)
    {
        var project = dossier.Project;
        var claims = new List<Claim>();
        foreach (var activity in project.Activities.OrderBy(a => a.Id))
        {
            var suffix = activity.Id.Split('-').Last();
            claims.Add(new Claim
            {
                Id = Guid.NewGuid(),
                Code = $"{project.Id}-CLM-{suffix}",
                ActivityId = activity.Id,
                Text = activity.Description,
                Source = activity.Id,
                Confidence = 1,
                EvidenceIds = activity.EvidenceIds.ToList()
            });
        }

        var mechanism = dossier.MethodSections.TryGetValue("2", out var section) ? section.Trim() : "";
        claims.Add(new Claim
        {
            Id = Guid.NewGuid(),
            Code = $"{project.Id}-CLM-MEC",
            Text = string.IsNullOrWhiteSpace(mechanism) ? "A seção 2 do método não foi localizada." : mechanism,
            Source = "evidencias/metodo.md#2",
            Confidence = string.IsNullOrWhiteSpace(mechanism) ? 0.2 : 1,
            EvidenceIds = [$"{project.Id}-EV05", $"{project.Id}-EV06"]
        });

        var conclusion = InterviewConclusion.Extract(dossier.InterviewText);
        claims.Add(new Claim
        {
            Id = Guid.NewGuid(),
            Code = $"{project.Id}-CLM-ENT",
            Text = string.IsNullOrWhiteSpace(conclusion) ? "A entrevista não trouxe uma conclusão localizável." : conclusion,
            Source = "transcricao_entrevista_tecnica.pdf",
            Confidence = 0.45,
            EvidenceIds = [$"{project.Id}-EV10"]
        });

        return claims;
    }
}

public static partial class InterviewConclusion
{
    public static string Extract(string? interview)
    {
        if (string.IsNullOrWhiteSpace(interview))
        {
            return "";
        }

        var marker = "Como ficou a conclusão da rodada?";
        var start = interview.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
        {
            return "";
        }

        var rest = interview[(start + marker.Length)..].Trim();
        var end = NextQuestion().Match(rest);
        var answer = end.Success ? rest[..end.Index] : rest;
        var condition = answer.IndexOf("Condição do registro", StringComparison.OrdinalIgnoreCase);
        if (condition >= 0)
        {
            answer = answer[..condition];
        }

        return string.Join(" ", answer.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    [GeneratedRegex(@"(?:\n\s*|(?<=[.!?]))\d{1,2}\.\s+\p{Lu}")]
    private static partial Regex NextQuestion();
}
