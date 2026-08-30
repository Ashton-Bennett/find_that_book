using System.Net.Http.Json;
using System.Text.Json;

namespace Api.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GeminiService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<JsonDocument> GenerateResponseAsync(string query)
    {
        var apiKey = _configuration["Gemini:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Gemini API key is not configured.");
        }

        var prompt = $$"""
            You are a book search assistant.

            Analyze the user's messy book search request and extract
            information that can be used to search for books.

            Extract:
            - title
            - author
            - keywords

            Only include information that is supported by the user's request.

            If a value cannot be determined, return null for title or author,
            and an empty array for keywords.

            Do not invent information.

            Return only valid JSON using this structure:

            {
                "title": "string or null",
                "author": "string or null",
                "keywords": ["string"]
            }

            User request:
            {{query}}
            """;

        var request = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new
                        {
                            text = prompt
                        }
                    }
                }
            },
            generationConfig = new
            {
                response_mime_type = "application/json"
            }
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "v1beta/models/gemini-3.6-flash:generateContent");

        httpRequest.Headers.Add("x-goog-api-key", apiKey);
        httpRequest.Content = JsonContent.Create(request);

        var response = await _httpClient.SendAsync(httpRequest);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                $"Gemini request failed with {(int)response.StatusCode} " +
                $"({response.StatusCode}). Response: {error}");
        }

        var geminiResponse =
            await response.Content.ReadFromJsonAsync<JsonElement>();

        var responseText = geminiResponse
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(responseText))
        {
            throw new InvalidOperationException(
                "Gemini returned an empty response.");
        }

        return JsonDocument.Parse(responseText);
    }
}
