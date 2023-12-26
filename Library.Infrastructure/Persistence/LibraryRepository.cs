using Library.Application.Common.Interfaces.Persistence;
using Library.Application.Helpers;
using Library.Application.Models;
using Library.Domain.Entities;
using Library.Infrastructure.Helpers;
using Library.Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Library.Infrastructure.Persistence;

public class LibraryRepository : ILibraryRepository
{
    private readonly LibraryDbContext _context;
    private readonly IPropertyMappingService _propertyMappingService;
    private readonly ILogger<LibraryRepository> _logger;

    public LibraryRepository(LibraryDbContext context, IPropertyMappingService propertyMappingService, ILogger<LibraryRepository> logger)
    {
        _context = context;
        _propertyMappingService = propertyMappingService;
        _logger = logger;
    }
    
    public bool AuthorExists(Guid authorId)
    {
        return _context.Author.Any(a => a.Id == authorId);
    }
    
    public Author? GetAuthor(Guid authorId)
    {
        return _context.Author.FirstOrDefault(a => a.Id == authorId);
    } 

    public PagedList<Author> GetAuthors(AuthorResourcesParameters authorResourcesParameters)
    {
        // var collectionBeforePaging = _context.Author
        // .OrderBy(a => a.FirstName)
        // .ThenBy(a => a.LastName).AsQueryable();
        
        var collectionBeforePaging = _context.Author
            .ApplySort(authorResourcesParameters.OrderBy, _propertyMappingService.GetPropertyMapping<AuthorDto, Author>());
        
        if (!string.IsNullOrWhiteSpace(authorResourcesParameters.Genre))
        {
            var genreForWhereClause = authorResourcesParameters.Genre.Trim();
            collectionBeforePaging = collectionBeforePaging.Where(a => a.Genre == genreForWhereClause);
        }
        
        if (!string.IsNullOrWhiteSpace(authorResourcesParameters.SearchQuery))
        {
            var searchQueryForWhereClause = authorResourcesParameters.SearchQuery.Trim();
            collectionBeforePaging = collectionBeforePaging.Where(a => a.Genre.Contains(searchQueryForWhereClause) 
                || a.FirstName.Contains(searchQueryForWhereClause)
                || a.LastName.Contains(searchQueryForWhereClause));
        }
        
        return PagedList<Author>.Create(collectionBeforePaging, authorResourcesParameters.PageNumber, authorResourcesParameters.PageSize);
    }

    public IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds)
    {
        return _context.Author.Where(a => authorIds.Contains(a.Id))
            .OrderBy(a => a.FirstName)
            .OrderBy(a => a.LastName)
            .ToList();
    }
    
    public void AddAuthor(Author author) 
    {
        author.Id = Guid.NewGuid();
               
        if (author.Books.Any())
            foreach (var book in author.Books)
                book.Id = Guid.NewGuid();
        
        _context.Add(author);
        
    }
    
    public void DeleteAuthor(Author author)
    {
        _context.Author.Remove(author);
    }
    
    public Book? GetBookForAuthor(Guid authorId, Guid bookId)
    {
        return _context.Book.FirstOrDefault(b => b.AuthorId == authorId && b.Id == bookId);
    }

    public IEnumerable<Book> GetBooksForAuthor(Guid authorId)
    {
        return _context.Book.Where(b => b.AuthorId == authorId);
    }
    
    public void AddBookForAuthor(Guid authorId, Book book) {
        if (book.Id == Guid.Empty)
            book.Id = Guid.NewGuid();
        book.AuthorId = authorId;
        _context.Book.Add(book);
    }
    
    public void DeleteBook(Book book)
    {
        _context.Book.Remove(book);
    }
    
    public void UpdateBook(Book book)
    {
        //EF Core update automaticaly
    }
    
    public bool Save()
    {
        try
        {
            return _context.SaveChanges() >= 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An unexpected fault happened. Message: {ex.Message}");
            return false;
        }
    }
}
