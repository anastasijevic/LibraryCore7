namespace Library.Application.Models;

public class AuthorCreateWithDateOfDeathDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTimeOffset DateOfBirth { get; set; }
    public DateTimeOffset? DateOfDeath { get; set; }
    public required string Genre { get; set; }
    
}
