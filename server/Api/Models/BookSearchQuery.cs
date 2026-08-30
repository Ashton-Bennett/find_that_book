namespace Api.Models;

public class BookSearchQuery
{
    public string? Query { get; set; }

    public string? Title { get; set; }

    public string? Author { get; set; }

    public List<string> Subjects { get; set; } = [];

    public List<string> Places { get; set; } = [];

    public List<string> People { get; set; } = [];

    public List<string> Publishers { get; set; } = [];

    public List<string> Languages { get; set; } = [];

    public int? PublishYearFrom { get; set; }

    public int? PublishYearTo { get; set; }
}