using Api.Models;

namespace Api.Services;

public interface IOpenLibraryService
{
    Task<IEnumerable<BookCandidate>> SearchBooksAsync(
        BookSearchQuery searchQuery);
}