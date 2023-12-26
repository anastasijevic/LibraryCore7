using Library.Application.Models;
using Library.Domain.Entities;

namespace Library.Api;

public static class BookMappings
{
    public static IEnumerable<BookDto> ToEnumerableBookDto(this IEnumerable<Book> books)
    {
        foreach (var book in books)
            yield return book.ToBookDto();
    }
    
    public static BookDto ToBookDto(this Book book) => 
        new() {
            Id = book.Id,
            Title = book.Title,
            Description = book.Description?? String.Empty,
            AuthorId = book.AuthorId
        };
    
    public static List<Book> ToBookList(this IEnumerable<BookCreateDto> booksDto)
    {
        var books = new List<Book>();
        foreach (var book in booksDto)
            books.Add(book.ToBook());
        return books;
    }
    
    public static Book ToBook(this BookCreateDto book) =>
        new() { 
            Title = book.Title ?? string.Empty, 
            Description = book.Description,
        };
    
    public static Book ToBook(this BookUpdateDto book) =>
        new() { 
            Title = book.Title ?? string.Empty, 
            Description = book.Description,
        };
        
    public static void EntityApplyChangesFrom(this Book book, BookUpdateDto bookUpdateDto)
    {
        var bookEntityForUpdate = bookUpdateDto.ToBook();
        if (bookEntityForUpdate is not null)
        {
            book.Title = bookEntityForUpdate.Title;
            book.Description = bookEntityForUpdate.Description;
        }
    }
    
    public static BookUpdateDto ToBookUpdateDto(this Book book) =>
        new() {
            Title = book.Title,
            Description = book.Description
        };
    
}
