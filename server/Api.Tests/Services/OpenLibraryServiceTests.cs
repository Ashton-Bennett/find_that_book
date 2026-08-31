using System.Net;
using Api.Models;
using Api.Services;
using Api.Tests.TestHelpers;

namespace Api.Tests.Services;

public class OpenLibraryServiceTests
{
    [Fact]
    public async Task SearchBooksAsync_BuildsSearchRequestAndMapsBookFields()
    {
        var handler = new RecordingHttpMessageHandler(_ => JsonResponse("""
            { "docs": [{
              "title": "Dune",
              "author_name": ["Frank Herbert"],
              "subject": ["Science fiction"],
              "place": ["Arrakis"],
              "person": ["Paul Atreides"],
              "publisher": ["Chilton"],
              "language": ["eng"],
              "first_publish_year": 1965,
              "key": "/works/OL123W",
              "cover_i": 12345
            }] }
            """));
        var service = CreateService(handler);

        var books = await service.SearchBooksAsync(new BookSearchQuery
        {
            Title = "Dune",
            Author = "Frank Herbert",
            Subjects = ["Science fiction"],
            PublishYearFrom = 1960,
            PublishYearTo = 1970
        });

        var book = Assert.Single(books);
        Assert.Equal("Dune", book.Title);
        Assert.Equal(["Frank Herbert"], book.Authors);
        Assert.Equal(["Science fiction"], book.Subjects);
        Assert.Equal(["Arrakis"], book.Places);
        Assert.Equal(["Paul Atreides"], book.People);
        Assert.Equal(["Chilton"], book.Publishers);
        Assert.Equal(["eng"], book.Languages);
        Assert.Equal(1965, book.FirstPublishYear);
        Assert.Equal("/works/OL123W", book.OpenLibraryKey);
        Assert.Equal("https://covers.openlibrary.org/b/id/12345-L.jpg", book.CoverUrl);
        Assert.Contains("title%3ADune", handler.RequestUri!.Query);
        Assert.Contains("author%3AFrank", handler.RequestUri.Query);
        Assert.Contains("first_publish_year%3A%5B1960%20TO%201970%5D", handler.RequestUri.Query);
    }

    [Fact]
    public async Task SearchBooksAsync_MissingDocs_ReturnsAnEmptyCollection()
    {
        var service = CreateService(new RecordingHttpMessageHandler(
            _ => JsonResponse("{}")));

        var books = await service.SearchBooksAsync(new BookSearchQuery { Query = "dickens" });

        Assert.Empty(books);
    }

    [Fact]
    public async Task SearchBooksAsync_UpstreamFailure_ThrowsWithStatusCode()
    {
        var service = CreateService(new RecordingHttpMessageHandler(_ => new HttpResponseMessage(
            HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent("temporarily unavailable")
        }));

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.SearchBooksAsync(new BookSearchQuery { Query = "dickens" }));

        Assert.Contains("503", exception.Message);
        Assert.Contains("temporarily unavailable", exception.Message);
    }

    private static OpenLibraryService CreateService(HttpMessageHandler handler) => new(
        new HttpClient(handler) { BaseAddress = new Uri("https://openlibrary.test/") });

    private static HttpResponseMessage JsonResponse(string content) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
    };
}
