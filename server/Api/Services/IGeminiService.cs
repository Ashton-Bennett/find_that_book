using System.Text.Json;

namespace Api.Services;

public interface IGeminiService
{
    Task<JsonDocument> GenerateResponseAsync(string query);
}