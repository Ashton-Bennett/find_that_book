using Api.Models;

namespace Api.Services;

public class BookMatcher : IBookMatcher
{
    public IEnumerable<BookCandidate> RankBooks(
        BookSearchQuery searchQuery,
        IEnumerable<BookCandidate> candidates)
    {
        return candidates
            .Select(candidate => ScoreCandidate(searchQuery, candidate))
            .OrderByDescending(result => result.Score)
            .Select(result => result.Candidate);
    }

    private static MatchResult ScoreCandidate(
        BookSearchQuery searchQuery,
        BookCandidate candidate)
    {
        var titleScore = CalculateTitleScore(
            searchQuery.Title,
            candidate.Title);

        var authorScore = CalculateAuthorScore(
            searchQuery.Author,
            candidate.Authors);

        var score =
            (titleScore * 0.6) +
            (authorScore * 0.4);

        candidate.Explanation =
            GenerateExplanation(
                titleScore,
                authorScore,
                searchQuery);

        return new MatchResult(candidate, score);
    }

    private static double CalculateTitleScore(
        string? searchTitle,
        string candidateTitle)
    {
        if (string.IsNullOrWhiteSpace(searchTitle))
        {
            return 0;
        }

        var normalizedSearchTitle = Normalize(searchTitle);
        var normalizedCandidateTitle = Normalize(candidateTitle);

        if (normalizedSearchTitle == normalizedCandidateTitle)
        {
            return 1.0;
        }

        if (normalizedCandidateTitle.Contains(normalizedSearchTitle) ||
            normalizedSearchTitle.Contains(normalizedCandidateTitle))
        {
            return 0.8;
        }

        return 0;
    }

    private static double CalculateAuthorScore(
        string? searchAuthor,
        List<string> candidateAuthors)
    {
        if (string.IsNullOrWhiteSpace(searchAuthor) ||
            candidateAuthors.Count == 0)
        {
            return 0;
        }

        var normalizedSearchAuthor = Normalize(searchAuthor);

        foreach (var author in candidateAuthors)
        {
            var normalizedAuthor = Normalize(author);

            if (normalizedSearchAuthor == normalizedAuthor)
            {
                return 1.0;
            }

            if (normalizedAuthor.Contains(normalizedSearchAuthor) ||
                normalizedSearchAuthor.Contains(normalizedAuthor))
            {
                return 0.8;
            }
        }

        return 0;
    }

    private static string Normalize(string value)
    {
        return value
            .ToLowerInvariant()
            .Replace(":", "")
            .Replace(",", "")
            .Replace(".", "")
            .Replace("'", "")
            .Trim();
    }

    private static string GenerateExplanation(
        double titleScore,
        double authorScore,
        BookSearchQuery searchQuery)
    {
        if (titleScore >= 1.0 && authorScore >= 1.0)
        {
            return "Strong title and author match.";
        }

        if (titleScore >= 1.0)
        {
            return "Strong title match.";
        }

        if (authorScore >= 1.0)
        {
            return "Strong author match.";
        }

        if (titleScore > 0 && authorScore > 0)
        {
            return "Title and author partially match.";
        }

        if (titleScore > 0)
        {
            return "Partial title match.";
        }

        if (authorScore > 0)
        {
            return "Partial author match.";
        }

        return "Weak match based on the available information.";
    }

    private record MatchResult(
        BookCandidate Candidate,
        double Score);
}