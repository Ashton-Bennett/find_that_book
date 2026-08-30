using Api.Models;

namespace Api.Services;

public interface IGeminiService
{
    Task<BookSearchQuery> GenerateSearchQueryAsync(string query);
}
