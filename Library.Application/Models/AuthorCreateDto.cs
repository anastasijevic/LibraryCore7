namespace Library.Application.Models;

public class AuthorCreateDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTimeOffset DateOfBirth { get; set; }
    public required string Genre { get; set; }
    
    public IEnumerable<BookCreateDto> Books { get; set; } = new List<BookCreateDto>();
    
}
