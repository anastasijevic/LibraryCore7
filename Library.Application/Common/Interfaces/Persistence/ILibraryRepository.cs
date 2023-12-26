using Library.Application.Helpers;
using Library.Domain.Entities;

namespace Library.Application.Common.Interfaces.Persistence;

public interface ILibraryRepository
{
    bool AuthorExists(Guid authorId);
    Author? GetAuthor(Guid authorId);
    PagedList<Author> GetAuthors(AuthorResourcesParameters authorResourcesParameters);
    IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds);
    void AddAuthor(Author author);
    void DeleteAuthor(Author author);
    
    IEnumerable<Book> GetBooksForAuthor(Guid authorId);
    Book? GetBookForAuthor(Guid authorId, Guid bookId);
    void AddBookForAuthor(Guid authorId, Book book);
    void DeleteBook(Book book);
    void UpdateBook(Book book);
    
    bool Save();
}
