using Api.Controllers;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Tests.Controllers;

public class BooksControllerTests
{
    [Fact]
    public async Task Search_BlankQuery_ReturnsBadRequestWithoutCallingServices()
    {
        var gemini = new StubGeminiService();
        var controller = new BooksController(gemini, new StubOpenLibraryService(), new StubBookMatcher());

        var result = await controller.Search(" ");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Search query is required.", badRequest.Value);
        Assert.False(gemini.WasCalled);
    }

    [Fact]
    public async Task Search_ValidQuery_UsesServicesAndReturnsRankedBooks()
    {
        var searchQuery = new BookSearchQuery { Author = "Charles Dickens" };
        var candidate = new BookCandidate { Title = "Oliver Twist" };
        var rankedCandidate = new BookCandidate { Title = "Oliver Twist", ConfidenceScore = 1.0 };
        var gemini = new StubGeminiService(searchQuery);
        var openLibrary = new StubOpenLibraryService([candidate]);
        var matcher = new StubBookMatcher([rankedCandidate]);
        var controller = new BooksController(gemini, openLibrary, matcher);

        var result = await controller.Search("books by Dickens");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal([rankedCandidate], Assert.IsAssignableFrom<IEnumerable<BookCandidate>>(okResult.Value));
        Assert.Same(searchQuery, openLibrary.ReceivedSearchQuery);
        Assert.Same(searchQuery, matcher.ReceivedSearchQuery);
        Assert.Equal([candidate], matcher.ReceivedCandidates);
    }

    private sealed class StubGeminiService(BookSearchQuery? result = null) : IGeminiService
    {
        public bool WasCalled { get; private set; }

        public Task<BookSearchQuery> GenerateSearchQueryAsync(string query)
        {
            WasCalled = true;
            return Task.FromResult(result ?? new BookSearchQuery());
        }
    }

    private sealed class StubOpenLibraryService(
        IEnumerable<BookCandidate>? results = null) : IOpenLibraryService
    {
        public BookSearchQuery? ReceivedSearchQuery { get; private set; }

        public Task<IEnumerable<BookCandidate>> SearchBooksAsync(BookSearchQuery searchQuery)
        {
            ReceivedSearchQuery = searchQuery;
            return Task.FromResult(results ?? Enumerable.Empty<BookCandidate>());
        }
    }

    private sealed class StubBookMatcher(
        IEnumerable<BookCandidate>? results = null) : IBookMatcher
    {
        public BookSearchQuery? ReceivedSearchQuery { get; private set; }

        public IEnumerable<BookCandidate>? ReceivedCandidates { get; private set; }

        public IEnumerable<BookCandidate> RankBooks(
            BookSearchQuery searchQuery,
            IEnumerable<BookCandidate> candidates)
        {
            ReceivedSearchQuery = searchQuery;
            ReceivedCandidates = candidates;
            return results ?? Enumerable.Empty<BookCandidate>();
        }
    }
}
