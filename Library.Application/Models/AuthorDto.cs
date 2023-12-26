namespace Library.Application.Models;

public class AuthorDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public int Age { get; set; }
    public required string Genre { get; set; }
    
}
