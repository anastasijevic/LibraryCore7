namespace Library.Domain.Entities;

public class Author
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTimeOffset DateOfBirth { get; set; }
    public DateTimeOffset? DateOfDeath { get; set; }
    public required string Genre { get; set; }
    
    public List<Book> Books { get; set; } = new List<Book>();
    
}
