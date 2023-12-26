using System.Text.Json;
using Azure;
using FluentValidation;
using FluentValidation.Results;
using Library.Application.Models;
using Library.Application.Common.Interfaces.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Library.Domain.Entities;

namespace Library.Api.Routes;

public static class BookRoutes
{
    public static IEndpointRouteBuilder MapBookRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/authors/{authorId}/books", Results<Ok<LinkedCollectionResourceWrapperDto<BookDto>>, NotFound> (Guid authorId, HttpContext httpContext, LinkGenerator linkGenerator, ILibraryRepository libraryRepository) => {
            
            if (!libraryRepository.AuthorExists(authorId))
                return TypedResults.NotFound();
            
            var booksForAuthorFromRepo = libraryRepository.GetBooksForAuthor(authorId);
            
            var bookForAuthor = booksForAuthorFromRepo.ToEnumerableBookDto().Select(book => {
                book = CreateLinksForBook(book, httpContext, linkGenerator);
                return book;
            });
            
            var wrapper = new LinkedCollectionResourceWrapperDto<BookDto>(bookForAuthor);
            
            return TypedResults.Ok(CreateLinksForBooks(wrapper, authorId, httpContext, linkGenerator));
        }).WithName("get_books_for_author").Produces(StatusCodes.Status406NotAcceptable);//.Produces<IEnumerable<BookDto>>(StatusCodes.Status200OK, "application/json");
        
        app.MapGet("/authors/{authorId}/books/{bookId}", Results<Ok<BookDto>, NotFound> (Guid authorId, Guid bookId, HttpContext httpContext, LinkGenerator linkGenerator, ILibraryRepository libraryRepository) => {
            
            if (!libraryRepository.AuthorExists(authorId))
                return TypedResults.NotFound();
            
            var bookForAuthorFromRepo = libraryRepository.GetBookForAuthor(authorId, bookId);
            
            if (bookForAuthorFromRepo is null)
                return TypedResults.NotFound();
            
            return TypedResults.Ok(CreateLinksForBook(bookForAuthorFromRepo.ToBookDto(), httpContext, linkGenerator));
        }).WithName("get_book_for_author").Produces(StatusCodes.Status406NotAcceptable);//.Produces<BookDto>(StatusCodes.Status200OK, "application/json");
        
        app.MapPost("/authors/{authorId}/books", Results<CreatedAtRoute<BookDto>, BadRequest, NotFound, StatusCodeHttpResult> 
        ([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] BookCreateDto bookDto,
         Guid authorId,
         HttpContext httpContext, 
         LinkGenerator linkGenerator,
         ILibraryRepository libraryRepository) => {
            
            if (bookDto is null)
                return TypedResults.BadRequest();
            
            if (!libraryRepository.AuthorExists(authorId))
                return TypedResults.NotFound();
            
            var bookEntity = bookDto.ToBook();
            
            libraryRepository.AddBookForAuthor(authorId, bookEntity);
            
            if (!libraryRepository.Save())
                return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
            
            var bookToReturn = bookEntity.ToBookDto();
            
            return TypedResults.CreatedAtRoute(CreateLinksForBook(bookToReturn, httpContext, linkGenerator), routeName: "get_book_for_author", routeValues: new { authorId, bookId = bookToReturn.Id });
        }).WithName("create_book_for_author").AddEndpointFilter<ValidationFilter<BookCreateDto>>().Produces(StatusCodes.Status406NotAcceptable);
        
        app.MapDelete("/authors/{authorId}/books/{bookId}", Results<NotFound, NoContent, StatusCodeHttpResult> 
        (Guid authorId,
         Guid bookId,
         ILibraryRepository libraryRepository,
         ILoggerFactory loggerFactory) => {
            if (!libraryRepository.AuthorExists(authorId))
                return TypedResults.NotFound();
            
            var bookForAuthorFromRepo = libraryRepository.GetBookForAuthor(authorId, bookId);
            if (bookForAuthorFromRepo is null)
                return TypedResults.NotFound();
            
            libraryRepository.DeleteBook(bookForAuthorFromRepo);
            
            if (!libraryRepository.Save())
                return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
            
            var logger = loggerFactory.CreateLogger(nameof(BookRoutes));
            logger.LogInformation($"Book {bookId} for author {authorId} was deleted.");
            
            return TypedResults.NoContent();
        }).WithName("delete_book_for_author").Produces(StatusCodes.Status406NotAcceptable);
        
        app.MapPut("/authors/{authorId}/books/{bookId}", Results<CreatedAtRoute<BookDto>, NotFound, BadRequest, NoContent, StatusCodeHttpResult> 
        ([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] BookUpdateDto book,
         Guid authorId,
         Guid bookId,
         ILibraryRepository libraryRepository) => {
            if (book is null)
                return TypedResults.BadRequest();
            
            if (!libraryRepository.AuthorExists(authorId))
                return TypedResults.NotFound();
            
            var bookForAuthorFromRepo = libraryRepository.GetBookForAuthor(authorId, bookId);
            if (bookForAuthorFromRepo is null)
            {
                var bookToAdd = book.ToBook();
                bookToAdd.Id = bookId;
                
                libraryRepository.AddBookForAuthor(authorId, bookToAdd);
                
                if (!libraryRepository.Save())
                return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
                
                var bookToReturn = bookToAdd.ToBookDto();
                
                return TypedResults.CreatedAtRoute(bookToReturn, routeName: "get_book_for_author", routeValues: new { authorId, bookId = bookToReturn.Id });
            }
            
            bookForAuthorFromRepo.EntityApplyChangesFrom(book);
            
            libraryRepository.UpdateBook(bookForAuthorFromRepo);
            
            if (!libraryRepository.Save())
                return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
                
            return TypedResults.NoContent();
        }).WithName("update_book_for_author").AddEndpointFilter<ValidationFilter<BookUpdateDto>>().Produces(StatusCodes.Status406NotAcceptable);
        
        app.MapPatch("/authors/{authorId}/books/{bookId}", Results<CreatedAtRoute<BookDto>, NotFound, BadRequest, NoContent, StatusCodeHttpResult, ProblemHttpResult>
        ([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonElement jsonElement,
         Guid authorId,
         Guid bookId,
         ILibraryRepository libraryRepository,
         IValidator<BookUpdateDto> validator) => {
            if (jsonElement.ValueKind == JsonValueKind.Undefined)
                return TypedResults.BadRequest();
                
            var json = jsonElement.GetRawText();
            if (string.IsNullOrWhiteSpace(json))
                return TypedResults.BadRequest();
            
            Microsoft.AspNetCore.JsonPatch.JsonPatchDocument? patchDoc = null;
            try
            {
                patchDoc = JsonConvert.DeserializeObject<Microsoft.AspNetCore.JsonPatch.JsonPatchDocument>(json);
            }
            catch (Exception)
            {
                //log
            }
            
            if (patchDoc is null)
                return TypedResults.BadRequest();
            
            if (!libraryRepository.AuthorExists(authorId))
                return TypedResults.NotFound();
            
            Dictionary<string, string[]>? validationErrors = null;
            ValidationResult? validationResult;
            
            var bookForAuthorFromRepo = libraryRepository.GetBookForAuthor(authorId, bookId);
            if (bookForAuthorFromRepo is null)
            {
                var bookDto = new BookUpdateDto();
                patchDoc.ApplyTo(bookDto, jsonPatchError => {
                    validationErrors ??= new Dictionary<string, string[]>();
                    
                    validationErrors.TryGetValue(jsonPatchError.AffectedObject.GetType().Name, out string[]? validationError);
                    
                    if (validationError is null)
                        validationErrors.Add(jsonPatchError.AffectedObject.GetType().Name, new string[] { jsonPatchError.ErrorMessage });
                    else
                    {
                        validationError = validationError.Append(jsonPatchError.ErrorMessage).ToArray();
                        validationErrors[jsonPatchError.AffectedObject.GetType().Name] = validationError;
                    }
                });
                
                validationResult = validator.Validate(bookDto);
                if (!validationResult.IsValid)
                {
                    validationErrors ??= new Dictionary<string, string[]>();
                    validationErrors = validationErrors.Concat(validationResult.ToDictionary()).ToDictionary(x => x.Key, x => x.Value);
                }
                
                if (validationErrors is not null)
                    return TypedResults.Problem(new HttpValidationProblemDetails(validationErrors) { Status = StatusCodes.Status422UnprocessableEntity });
                
                var bookToAdd = bookDto.ToBook();
                bookToAdd.Id = bookId;
                
                libraryRepository.AddBookForAuthor(authorId, bookToAdd);
                
                if (!libraryRepository.Save())
                    return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
                
                var bookToReturn = bookToAdd.ToBookDto();
                
                return TypedResults.CreatedAtRoute(bookToReturn, routeName: "get_book_for_author", routeValues: new { authorId, bookId = bookToReturn.Id });
            }
            
            var bookUpdateDtoToPatch = bookForAuthorFromRepo.ToBookUpdateDto();
            
            patchDoc.ApplyTo(bookUpdateDtoToPatch, jsonPatchError => {
                validationErrors ??= new Dictionary<string, string[]>();
                
                validationErrors.TryGetValue(jsonPatchError.AffectedObject.GetType().Name, out string[]? validationError);
                
                if (validationError is null)
                    validationErrors.Add(jsonPatchError.AffectedObject.GetType().Name, new string[] { jsonPatchError.ErrorMessage });
                else
                {
                    validationError = validationError.Append(jsonPatchError.ErrorMessage).ToArray();
                    validationErrors[jsonPatchError.AffectedObject.GetType().Name] = validationError;
                }
            });
            
            if (validationErrors is not null)
                return TypedResults.Problem(new HttpValidationProblemDetails(validationErrors) { Status = StatusCodes.Status422UnprocessableEntity });
            
            validationResult = validator.Validate(bookUpdateDtoToPatch);
            if (!validationResult.IsValid)
                return TypedResults.Problem(new HttpValidationProblemDetails(validationResult.ToDictionary()) { Status = StatusCodes.Status422UnprocessableEntity });
                
            bookForAuthorFromRepo.EntityApplyChangesFrom(bookUpdateDtoToPatch);
            
            libraryRepository.UpdateBook(bookForAuthorFromRepo);
            
            if (!libraryRepository.Save())
                return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
            
            return TypedResults.NoContent();
        }).WithName("partially_update_book_for_author").Produces(StatusCodes.Status406NotAcceptable);
        
        return app;
    }
    
    private static BookDto CreateLinksForBook(BookDto book, HttpContext httpContext, LinkGenerator linkGenerator)
    {
        book.Links.Add(new LinkDto(linkGenerator.GetUriByName(httpContext, "get_book_for_author", values: new { authorId = book.AuthorId, bookId = book.Id }), "self", "GET"));
        book.Links.Add(new LinkDto(linkGenerator.GetUriByName(httpContext, "delete_book_for_author", values: new { authorId = book.AuthorId, bookId = book.Id }), "delete_book", "DELETE"));
        book.Links.Add(new LinkDto(linkGenerator.GetUriByName(httpContext, "update_book_for_author", values: new { authorId = book.AuthorId, bookId = book.Id }), "update_book", "PUT"));
        book.Links.Add(new LinkDto(linkGenerator.GetUriByName(httpContext, "partially_update_book_for_author", values: new { authorId = book.AuthorId, bookId = book.Id }), "partially_update_book", "PATCH"));
        return book;
    }
    
    private static LinkedCollectionResourceWrapperDto<BookDto> CreateLinksForBooks(LinkedCollectionResourceWrapperDto<BookDto> bookWrapper, Guid authorId, HttpContext httpContext, LinkGenerator linkGenerator)
    {
        bookWrapper.Links.Add(new LinkDto(linkGenerator.GetUriByName(httpContext, "get_books_for_author", values: new { authorId }), "self", "GET"));
        
        return bookWrapper;
    }
    
}
 