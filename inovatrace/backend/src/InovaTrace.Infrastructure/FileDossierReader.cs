using System.Text.RegularExpressions;
using InovaTrace.Application;
using InovaTrace.Domain;
using UglyToad.PdfPig;

namespace InovaTrace.Infrastructure;

public sealed partial class FileDossierReader : IDossierReader
{
    public ProjectDossier Read(Project project)
    {
        var methodPath = Path.Combine(project.SourceDirectory, "evidencias", "metodo.md");
        var reviewPath = Path.Combine(project.SourceDirectory, "evidencias", "revisao_tecnica.md");
        var interviewPath = Path.Combine(project.SourceDirectory, "transcricao_entrevista_tecnica.pdf");
        var method = File.Exists(methodPath) ? File.ReadAllText(methodPath) : "";
        var review = File.Exists(reviewPath) ? File.ReadAllText(reviewPath) : "";
        return new ProjectDossier
        {
            Project = project,
            MethodMarkdown = method,
            ReviewMarkdown = review,
            InterviewText = ReadPdf(interviewPath),
            MethodSections = SplitNumbered(method),
            ReviewSections = SplitHeadings(review)
        };
    }

    private static string ReadPdf(string path)
    {
        if (!File.Exists(path))
        {
            return "";
        }

        using var document = PdfDocument.Open(path);
        return string.Join("\n", document.GetPages().Select(page => page.Text));
    }

    private static Dictionary<string, string> SplitNumbered(string markdown)
    {
        var sections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (title, body) in Headings(markdown))
        {
            var number = NumberedHeading().Match(title);
            if (number.Success)
            {
                sections[number.Groups[1].Value] = body.Trim();
            }
        }

        return sections;
    }

    private static Dictionary<string, string> SplitHeadings(string markdown)
    {
        var sections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (title, body) in Headings(markdown))
        {
            sections[title.Trim()] = body.Trim();
        }

        return sections;
    }

    private static IEnumerable<(string Title, string Body)> Headings(string markdown)
    {
        var matches = Heading().Matches(markdown);
        for (var i = 0; i < matches.Count; i++)
        {
            var start = matches[i].Index + matches[i].Length;
            var end = i + 1 < matches.Count ? matches[i + 1].Index : markdown.Length;
            yield return (matches[i].Groups[1].Value, markdown[start..end]);
        }
    }

    [GeneratedRegex(@"^##\s+(.+)$", RegexOptions.Multiline)]
    private static partial Regex Heading();

    [GeneratedRegex(@"^(\d+)\.")]
    private static partial Regex NumberedHeading();
}
