namespace Api.Models;

public class BookSearchQuery
{
    public string? Title { get; set; }

    public string? Author { get; set; }

    public List<string> Keywords { get; set; } = [];
}