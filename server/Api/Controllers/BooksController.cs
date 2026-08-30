using Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly IOpenLibraryService _openLibraryService;
    private readonly IBookMatcher _bookMatcher;

    public BooksController(
        IGeminiService geminiService,
        IOpenLibraryService openLibraryService,
        IBookMatcher bookMatcher)
    {
        _geminiService = geminiService;
        _openLibraryService = openLibraryService;
        _bookMatcher = bookMatcher;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Search query is required.");
        }

        var searchQuery = await _geminiService.GenerateSearchQueryAsync(query);
        
        var candidates =
            await _openLibraryService.SearchBooksAsync(searchQuery);
        Console.WriteLine($"Found {candidates.Count()} candidates from Open Library.");
        var rankedCandidates =
            _bookMatcher.RankBooks(searchQuery, candidates);

        return Ok(rankedCandidates);
    }
}
