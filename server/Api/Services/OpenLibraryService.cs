using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Api.Models;

namespace Api.Services;

public class OpenLibraryService : IOpenLibraryService
{
    private readonly HttpClient _httpClient;

    public OpenLibraryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<BookCandidate>> SearchBooksAsync(BookSearchQuery searchQuery)
    {
        var query = BuildSearchQuery(searchQuery);

        var response = await _httpClient.GetAsync(
            $"search.json?q={Uri.EscapeDataString(query)}");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                $"Open Library request failed with " +
                $"{(int)response.StatusCode} ({response.StatusCode}). " +
                $"Response: {error}");
        }

        var result =
            await response.Content
                .ReadFromJsonAsync<OpenLibrarySearchResponse>();

        if (result?.Docs == null)
        {
            return [];
        }

        return result.Docs.Select(book => new BookCandidate
        {
            Title = book.Title ?? string.Empty,
            Authors = book.AuthorName ?? [],
            Subjects = book.Subjects ?? [],
            Places = book.Places ?? [],
            People = book.People ?? [],
            Publishers = book.Publishers ?? [],
            Languages = book.Languages ?? [],
            FirstPublishYear = book.FirstPublishYear,
            OpenLibraryKey = book.Key,
            CoverUrl = book.CoverI.HasValue
                ? $"https://covers.openlibrary.org/b/id/{book.CoverI}-L.jpg"
                : null
        });
    }

    private static string BuildSearchQuery(BookSearchQuery searchQuery)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(searchQuery.Title))
            parts.Add($"title:{searchQuery.Title}");

        if (!string.IsNullOrWhiteSpace(searchQuery.Author))
            parts.Add($"author:{searchQuery.Author}");

        foreach (var subject in searchQuery.Subjects)
            parts.Add($"subject:{subject}");

        foreach (var place in searchQuery.Places)
            parts.Add($"place:{place}");

        foreach (var person in searchQuery.People)
            parts.Add($"person:{person}");

        foreach (var publisher in searchQuery.Publishers)
            parts.Add($"publisher:{publisher}");

        foreach (var language in searchQuery.Languages)
            parts.Add($"language:{language}");

        if (searchQuery.PublishYearFrom.HasValue ||
            searchQuery.PublishYearTo.HasValue)
        {
            var from = searchQuery.PublishYearFrom?.ToString() ?? "*";
            var to = searchQuery.PublishYearTo?.ToString() ?? "*";

            parts.Add($"first_publish_year:[{from} TO {to}]");
        }

        if (parts.Count == 0 && !string.IsNullOrWhiteSpace(searchQuery.Query))
            return searchQuery.Query;

        Console.WriteLine($"Built Open Library search query: {string.Join(" ", parts)}");    

        return string.Join(" ", parts);
    }


    private class OpenLibrarySearchResponse
    {
        [JsonPropertyName("docs")]
        public List<OpenLibraryBook> Docs { get; set; } = [];
    }

    private class OpenLibraryBook
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("author_name")]
        public List<string>? AuthorName { get; set; }

        [JsonPropertyName("author_key")]
        public List<string>? AuthorKey { get; set; }

        [JsonPropertyName("first_publish_year")]
        public int? FirstPublishYear { get; set; }

        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("cover_i")]
        public int? CoverI { get; set; }
        
        [JsonPropertyName("subject")]
        public List<string>? Subjects { get; set; }

        [JsonPropertyName("place")]
        public List<string>? Places { get; set; }

        [JsonPropertyName("person")]
        public List<string>? People { get; set; }

        [JsonPropertyName("publisher")]
        public List<string>? Publishers { get; set; }

        [JsonPropertyName("language")]
        public List<string>? Languages { get; set; }
    }
}