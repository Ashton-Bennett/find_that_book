using Api.Models;
using Api.Services;

namespace Api.Tests.Services;

public class BookMatcherTests
{
    private readonly BookMatcher _matcher = new();

    [Fact]
    public void RankBooks_ExactTitleMatch_IsRankedFirstWithStrongExplanation()
    {
        var searchQuery = new BookSearchQuery { Title = "A Tale of Two Cities" };
        var exactMatch = new BookCandidate { Title = "A Tale of Two Cities" };
        var nonMatch = new BookCandidate { Title = "Great Expectations" };

        var results = _matcher.RankBooks(searchQuery, [nonMatch, exactMatch]).ToList();

        Assert.Equal(exactMatch, results[0]);
        Assert.Equal(1.0, exactMatch.ConfidenceScore);
        Assert.Equal("Strong match on title.", exactMatch.Explanation);
        Assert.Equal(0, nonMatch.ConfidenceScore);
    }

    [Fact]
    public void RankBooks_NormalizesPunctuationAndCaseBeforeMatching()
    {
        var searchQuery = new BookSearchQuery { Author = "O'CONNOR" };
        var candidate = new BookCandidate { Authors = ["Flannery O'Connor"] };

        var result = _matcher.RankBooks(searchQuery, [candidate]).Single();

        Assert.Equal(0.8, result.ConfidenceScore);
        Assert.Equal("Good match on author.", result.Explanation);
    }

    [Fact]
    public void RankBooks_YearRange_ExcludesCandidatesOutsideTheRange()
    {
        var searchQuery = new BookSearchQuery
        {
            PublishYearFrom = 1980,
            PublishYearTo = 2000
        };
        var inRange = new BookCandidate { Title = "In range", FirstPublishYear = 1980 };
        var outOfRange = new BookCandidate { Title = "Out of range", FirstPublishYear = 2001 };

        _matcher.RankBooks(searchQuery, [inRange, outOfRange]).ToList();

        Assert.Equal(1.0, inRange.ConfidenceScore);
        Assert.Equal(0, outOfRange.ConfidenceScore);
    }
}
