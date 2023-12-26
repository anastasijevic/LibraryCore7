namespace Library.Application.Models;

public class BookDto : LinkedResourceBaseDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public Guid AuthorId { get; set; }
    
}
