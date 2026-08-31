using System.Net;
using Api.Services;
using Api.Tests.TestHelpers;
using Microsoft.Extensions.Configuration;

namespace Api.Tests.Services;

public class GeminiServiceTests
{
    [Fact]
    public async Task GenerateSearchQueryAsync_SendsTheUserQueryAndParsesTheResponse()
    {
        var handler = new RecordingHttpMessageHandler(_ => JsonResponse("""
            {
              "candidates": [{
                "content": {
                  "parts": [{
                    "text": "{\"title\":null,\"author\":\"Charles Dickens\",\"subjects\":[],\"places\":[],\"people\":[],\"publishers\":[],\"languages\":[],\"publishYearFrom\":null,\"publishYearTo\":null}"
                  }]
                }
              }]
            }
            """));
        var service = CreateService(handler, "test-key");

        var searchQuery = await service.GenerateSearchQueryAsync("books by Charles Dickens");

        Assert.Equal("Charles Dickens", searchQuery.Author);
        Assert.Equal("v1beta/models/gemini-3.5-flash:generateContent", handler.RequestUri!.AbsolutePath.TrimStart('/'));
        Assert.Equal("test-key", handler.RequestHeaders!.GetSingleValue("x-goog-api-key"));
        Assert.Contains("books by Charles Dickens", handler.RequestBody);
    }

    [Fact]
    public async Task GenerateSearchQueryAsync_MissingApiKey_ThrowsBeforeSendingARequest()
    {
        var service = CreateService(
            new RecordingHttpMessageHandler(_ => throw new InvalidOperationException("Should not send")),
            null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GenerateSearchQueryAsync("dickens"));

        Assert.Equal("Gemini API key is not configured.", exception.Message);
    }

    [Fact]
    public async Task GenerateSearchQueryAsync_UpstreamFailure_IncludesResponseDetails()
    {
        var service = CreateService(new RecordingHttpMessageHandler(_ => new HttpResponseMessage(
            HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent("rate limit exceeded")
        }), "test-key");

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GenerateSearchQueryAsync("dickens"));

        Assert.Contains("429", exception.Message);
        Assert.Contains("rate limit exceeded", exception.Message);
    }

    private static GeminiService CreateService(HttpMessageHandler handler, string? apiKey)
    {
        var settings = apiKey is null
            ? new Dictionary<string, string?>()
            : new Dictionary<string, string?> { ["Gemini:ApiKey"] = apiKey };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        return new GeminiService(
            new HttpClient(handler) { BaseAddress = new Uri("https://gemini.test/") },
            configuration);
    }

    private static HttpResponseMessage JsonResponse(string content) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
    };
}
