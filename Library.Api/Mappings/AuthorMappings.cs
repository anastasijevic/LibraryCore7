using Library.Api.Helpers;
using Library.Application.Models;
using Library.Domain.Entities;

namespace Library.Api.Mappings;

public static class AuthorMappings
{
    public static IEnumerable<AuthorDto> ToEnumerableAuthorDto(this IEnumerable<Author> authors)
    {
        foreach (var author in authors)
            yield return author.ToAuthorDto();
    }
    
    public static AuthorDto ToAuthorDto(this Author author) => 
        new() {
            Id = author.Id,
            Genre = author.Genre,
            Name = $"{author.FirstName} {author.LastName}",
            Age = author.DateOfBirth.GetCurrentAge(author.DateOfDeath)
        };
    
    public static Author ToAuthor(this AuthorCreateDto author) =>
        new() { 
            FirstName = author.FirstName, 
            LastName = author.LastName, 
            DateOfBirth = author.DateOfBirth, 
            Genre = author.Genre,
            Books = author.Books.ToBookList()
            
        };
    
    public static Author ToAuthor(this AuthorCreateWithDateOfDeathDto author) =>
        new() { 
            FirstName = author.FirstName, 
            LastName = author.LastName, 
            DateOfBirth = author.DateOfBirth,
            DateOfDeath = author.DateOfDeath,
            Genre = author.Genre,
            
        };
    
    public static IEnumerable<Author> ToAuthorList(this IEnumerable<AuthorCreateDto> authorCollections)
    {
        var authors = new List<Author>();
        foreach (var authorCollection in authorCollections)
            authors.Add(authorCollection.ToAuthor());
        return authors;
    }
    
}
