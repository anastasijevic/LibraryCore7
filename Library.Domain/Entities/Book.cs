namespace Library.Domain.Entities;

public class Book
{
    private Author? _author;
    
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public Guid AuthorId { get; set; }
    public Author Author {
        set => _author = value;
        get => _author ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Author));
    }
    
}
