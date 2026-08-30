using Api.Models;

namespace Api.Services;

public interface IBookMatcher
{
    IEnumerable<BookCandidate> RankBooks(
        BookSearchQuery searchQuery,
        IEnumerable<BookCandidate> candidates);
}