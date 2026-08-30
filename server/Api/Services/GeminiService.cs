using System.Net.Http.Json;
using System.Text.Json;
using Api.Models;

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

    public async Task<BookSearchQuery> GenerateSearchQueryAsync(string query)
    {
        var apiKey = _configuration["Gemini:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Gemini API key is not configured.");
        }

        var prompt = $$"""
            You are a book search assistant.

            Analyze the user's messy or natural-language book search request and extract
            all information that can be used to search for books using the Open Library
            Search API.

            Extract:
            - title
            - author
            - subjects
            - places
            - people
            - publishers
            - languages
            - publishYearFrom
            - publishYearTo

            A request may contain multiple search criteria. Extract all applicable
            criteria rather than choosing only one.

            Field definitions:
            - title: A specific book title the user is looking for.
            - author: An author the user wants to find books by.
            - subjects: Topics, themes, or subjects the user wants books about.
            - places: Locations that the books should be about or associated with.
            - people: Specific people that the books should be about.
            - publishers: Publishers the user wants books from.
            - languages: Languages the user wants the books to be written in.
            - publishYearFrom: The earliest publication year requested.
            - publishYearTo: The latest publication year requested.

            Only include information that is supported by the user's request.

            Do not invent information or infer criteria that the user did not specify.

            If a title or author cannot be determined, return null.

            If a list-based field cannot be determined, return an empty array.

            If a publication year range is not specified, return null for the
            corresponding year field.

            If the user asks for a specific book, extract the title if one is provided.

            If the user asks for books similar to another book, author, or subject,
            do not treat the referenced book or author as an exact title or author
            search unless the user's request explicitly asks for that.

            A request can contain zero, one, or multiple search criteria.

            For example, if the user asks:
            "Find books about travel in Istanbul"

            return:
            {
                "title": null,
                "author": null,
                "subjects": ["travel"],
                "places": ["Istanbul"],
                "people": [],
                "publishers": [],
                "languages": [],
                "publishYearFrom": null,
                "publishYearTo": null
            }

            If the user asks:
            "Find books by Stephen King about horror published between 1980 and 2000"

            return:
            {
                "title": null,
                "author": "Stephen King",
                "subjects": ["horror"],
                "places": [],
                "people": [],
                "publishers": [],
                "languages": [],
                "publishYearFrom": 1980,
                "publishYearTo": 2000
            }

            Return only valid JSON using this structure:

            {
                "title": "string or null",
                "author": "string or null",
                "subjects": ["string"],
                "places": ["string"],
                "people": ["string"],
                "publishers": ["string"],
                "languages": ["string"],
                "publishYearFrom": "number or null",
                "publishYearTo": "number or null"
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

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        Console.WriteLine($"____> ResponseText = {responseText}");

        return JsonSerializer.Deserialize<BookSearchQuery>(responseText, options)
            ?? throw new InvalidOperationException(
                "Gemini returned an invalid search query.");
    }
}
