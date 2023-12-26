using Library.Api.Mappings;
using Library.Application.Models;
using Library.Api.ParameterBindings;
using Library.Application.Common.Interfaces.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Library.Api.Routes;

public static class AuthorCollectionRoutes
{
    public static IEndpointRouteBuilder MapAuthorCollectionRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/authorcollections/{ids}", Results<Ok<IEnumerable<AuthorDto>>, BadRequest, NotFound> ([FromRoute] GuidListBinding ids, ILibraryRepository libraryRepository) => {
            
            if (ids is null)
                return TypedResults.BadRequest();
            
            var authorsFromRepo = libraryRepository.GetAuthors(ids.Guids);
            
            if (ids.Guids.Count != authorsFromRepo.Count())
                return TypedResults.NotFound();
            
            return TypedResults.Ok(authorsFromRepo.ToEnumerableAuthorDto());
        }).WithName("get_author_collection").Produces(StatusCodes.Status406NotAcceptable).Produces<IEnumerable<AuthorDto>>(StatusCodes.Status200OK, "application/json");
        
        app.MapPost("/authorcollections", Results<CreatedAtRoute<IEnumerable<AuthorDto>>, BadRequest, StatusCodeHttpResult> 
        ([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] IEnumerable<AuthorCreateDto> authorCollection,
         ILibraryRepository libraryRepository) => {
            
            if (authorCollection is null)
                return TypedResults.BadRequest();
            
            var authorEnties = authorCollection.ToAuthorList();
            
            foreach (var authorEntity in authorEnties)
                libraryRepository.AddAuthor(authorEntity);
            
            if (!libraryRepository.Save())
                return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
            
            var authorCollectionToReturn = authorEnties.ToEnumerableAuthorDto();
            
            return TypedResults.CreatedAtRoute(authorCollectionToReturn, routeName: "get_author_collection", routeValues: new { ids = string.Join(",", "(" + authorCollectionToReturn.Select(a => a.Id) + ")") });
        }).AddEndpointFilter<ValidationFilter<List<AuthorCreateDto>>>().Produces(StatusCodes.Status406NotAcceptable);
        
        return app; 
    }
}
