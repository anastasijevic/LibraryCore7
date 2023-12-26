namespace Library.Application.Models;

public class LinkDto
{
    public string Href { get; private set; } = string.Empty;
    public string Rel { get; private set; } = string.Empty;
    public string Method { get; private set; } = string.Empty;
    
    public LinkDto(string? href, string rel, string method)
    {
        Href = href ?? string.Empty;
        Rel = rel;
        Method = method;
        
    }
}
