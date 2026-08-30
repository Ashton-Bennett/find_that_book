using System.Text.Json;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly IOpenLibraryService _openLibraryService;

    public BooksController(
        IGeminiService geminiService,
        IOpenLibraryService openLibraryService)
    {
        _geminiService = geminiService;
        _openLibraryService = openLibraryService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Search query is required.");
        }

        var geminiResponse =
            await _geminiService.GenerateResponseAsync(query);

        var searchQuery =
            JsonSerializer.Deserialize<BookSearchQuery>(
                geminiResponse.RootElement.GetRawText(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (searchQuery == null)
        {
            return BadRequest("Unable to interpret search query.");
        }

        var candidates =
            await _openLibraryService.SearchBooksAsync(searchQuery);

        return Ok(candidates);
    }
}