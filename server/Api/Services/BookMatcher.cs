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
            .Select(result =>
            {
                result.Candidate.Explanation = result.Explanation;
                return result.Candidate;
            });
    }

    private static MatchResult ScoreCandidate(
        BookSearchQuery searchQuery,
        BookCandidate candidate)
    {
        var scores = new List<double>();
        var matchedCriteria = new List<string>();

        if (!string.IsNullOrWhiteSpace(searchQuery.Title))
        {
            var score = CalculateStringScore(
                searchQuery.Title,
                candidate.Title);

            scores.Add(score);

            if (score > 0)
                matchedCriteria.Add("title");
        }

        if (!string.IsNullOrWhiteSpace(searchQuery.Author))
        {
            var score = CalculateListScore(
                searchQuery.Author,
                candidate.Authors);

            scores.Add(score);

            if (score > 0)
                matchedCriteria.Add("author");
        }

        if (searchQuery.Subjects.Count > 0)
        {
            var score = CalculateListScore(
                searchQuery.Subjects,
                candidate.Subjects);

            scores.Add(score);

            if (score > 0)
                matchedCriteria.Add("subject");
        }

        if (searchQuery.Places.Count > 0)
        {
            var score = CalculateListScore(
                searchQuery.Places,
                candidate.Places);

            scores.Add(score);

            if (score > 0)
                matchedCriteria.Add("place");
        }

        if (searchQuery.People.Count > 0)
        {
            var score = CalculateListScore(
                searchQuery.People,
                candidate.People);

            scores.Add(score);

            if (score > 0)
                matchedCriteria.Add("person");
        }

        if (searchQuery.Publishers.Count > 0)
        {
            var score = CalculateListScore(
                searchQuery.Publishers,
                candidate.Publishers);

            scores.Add(score);

            if (score > 0)
                matchedCriteria.Add("publisher");
        }

        if (searchQuery.Languages.Count > 0)
        {
            var score = CalculateListScore(
                searchQuery.Languages,
                candidate.Languages);

            scores.Add(score);

            if (score > 0)
                matchedCriteria.Add("language");
        }

        if (searchQuery.PublishYearFrom.HasValue ||
            searchQuery.PublishYearTo.HasValue)
        {
            var score = CalculateYearScore(
                searchQuery.PublishYearFrom,
                searchQuery.PublishYearTo,
                candidate.FirstPublishYear);

            scores.Add(score);

            if (score > 0)
                matchedCriteria.Add("publication year");
        }

        var finalScore = scores.Count > 0
            ? scores.Average()
            : 0;

        var explanation = GenerateExplanation(
            finalScore,
            matchedCriteria,
            scores.Count);

        return new MatchResult(
            candidate,
            finalScore,
            explanation);
    }

    private static double CalculateStringScore(
        string? searchValue,
        string candidateValue)
    {
        if (string.IsNullOrWhiteSpace(searchValue) ||
            string.IsNullOrWhiteSpace(candidateValue))
        {
            return 0;
        }

        var normalizedSearch = Normalize(searchValue);
        var normalizedCandidate = Normalize(candidateValue);

        if (normalizedSearch == normalizedCandidate)
            return 1.0;

        if (normalizedCandidate.Contains(normalizedSearch) ||
            normalizedSearch.Contains(normalizedCandidate))
        {
            return 0.8;
        }

        return 0;
    }

    private static double CalculateListScore(
        string searchValue,
        List<string> candidateValues)
    {
        return CalculateListScore(
            [searchValue],
            candidateValues);
    }

    private static double CalculateListScore(
        IEnumerable<string> searchValues,
        List<string> candidateValues)
    {
        if (!candidateValues.Any())
            return 0;

        var normalizedCandidates = candidateValues
            .Select(Normalize)
            .ToList();

        var scores = searchValues
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(searchValue =>
            {
                var normalizedSearch = Normalize(searchValue);

                if (normalizedCandidates.Any(
                    candidate => candidate == normalizedSearch))
                {
                    return 1.0;
                }

                if (normalizedCandidates.Any(
                    candidate =>
                        candidate.Contains(normalizedSearch) ||
                        normalizedSearch.Contains(candidate)))
                {
                    return 0.8;
                }

                return 0;
            })
            .ToList();

        return scores.Count > 0
            ? scores.Average()
            : 0;
    }

    private static double CalculateYearScore(
        int? searchYearFrom,
        int? searchYearTo,
        int? candidateYear)
    {
        if (!candidateYear.HasValue)
            return 0;

        if (searchYearFrom.HasValue &&
            candidateYear < searchYearFrom.Value)
        {
            return 0;
        }

        if (searchYearTo.HasValue &&
            candidateYear > searchYearTo.Value)
        {
            return 0;
        }

        return 1.0;
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
        double score,
        List<string> matchedCriteria,
        int criteriaCount)
    {
        if (criteriaCount == 0)
            return "No specific search criteria were provided.";

        if (score >= 0.9)
        {
            return $"Strong match on {FormatCriteria(matchedCriteria)}.";
        }

        if (score >= 0.7)
        {
            return $"Good match on {FormatCriteria(matchedCriteria)}.";
        }

        if (score > 0)
        {
            return $"Partial match on {FormatCriteria(matchedCriteria)}.";
        }

        return "No strong matches found for the requested criteria.";
    }

    private static string FormatCriteria(List<string> criteria)
    {
        if (criteria.Count == 0)
            return "the requested criteria";

        if (criteria.Count == 1)
            return criteria[0];

        if (criteria.Count == 2)
            return $"{criteria[0]} and {criteria[1]}";

        return $"{string.Join(", ", criteria.Take(criteria.Count - 1))}, " +
               $"and {criteria[^1]}";
    }

    private record MatchResult(
        BookCandidate Candidate,
        double Score,
        string Explanation);
}