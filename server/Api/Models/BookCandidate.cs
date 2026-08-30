namespace Api.Models;

public class BookCandidate
{
    public string Title { get; set; } = string.Empty;

    public List<string> Authors { get; set; } = [];

    public int? FirstPublishYear { get; set; }

    public string? OpenLibraryKey { get; set; }

    public string? CoverUrl { get; set; }

    public string Explanation { get; set; } = string.Empty;
}