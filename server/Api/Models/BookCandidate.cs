namespace Api.Models;

public class BookCandidate
{
    public string Title { get; set; } = string.Empty;

    public List<string> Authors { get; set; } = [];

    public List<string> Subjects { get; set; } = [];

    public List<string> Places { get; set; } = [];

    public List<string> People { get; set; } = [];

    public List<string> Publishers { get; set; } = [];

    public List<string> Languages { get; set; } = [];

    public int? FirstPublishYear { get; set; }

    public string? OpenLibraryKey { get; set; }

    public string? CoverUrl { get; set; }

    public string Explanation { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
}