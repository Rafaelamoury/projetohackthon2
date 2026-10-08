using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using InovaTrace.Application;

namespace InovaTrace.Infrastructure;

public sealed class LanguageModelOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "";
}

/// <summary>
/// Cliente de um endpoint compatível com o formato de chat da OpenAI.
/// O banco troca BaseUrl, chave e modelo sem espalhar um SDK pelo domínio.
/// </summary>
public sealed class CorporateLanguageModel : ILanguageModel
{
    private readonly HttpClient _http;
    private readonly LanguageModelOptions _options;

    public CorporateLanguageModel(HttpClient http, LanguageModelOptions options)
    {
        _http = http;
        _options = options;
    }

    public bool IsConfigured =>
        _options.Enabled
        && !string.IsNullOrWhiteSpace(_options.BaseUrl)
        && !string.IsNullOrWhiteSpace(_options.ApiKey)
        && !string.IsNullOrWhiteSpace(_options.Model);

    public async Task<string> CompleteJsonAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Nenhum endpoint corporativo de IA está configurado. O motor determinístico permanece em uso.");
        }

        var baseUrl = _options.BaseUrl.TrimEnd('/');
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = JsonContent.Create(new
        {
            model = _options.Model,
            temperature = 0,
            response_format = new { type = "json_object" },
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        });

        using var response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return document.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()
            ?? throw new InvalidOperationException("O endpoint de IA devolveu conteúdo vazio.");
    }
}
