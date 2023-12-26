using Library.Application.Helpers;
using Library.Api.Mappings;
using Library.Application.Models;
using Library.Application.Common.Interfaces.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using System.Security.AccessControl;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Text.Json;
using Library.Infrastructure.Services;
using Library.Domain.Entities;
using System.Dynamic;
using Asp.Versioning.Builder;

namespace Library.Api.Routes;

public static class AuthorRoutes
{
    public static IEndpointRouteBuilder MapAuthorRoutes(this IEndpointRouteBuilder app, ApiVersionSet apiVersionSet)
    {
        app.MapGet("/authors", Results<Ok<object>, BadRequest> 
        (AuthorResourcesParameters authorResourcesParameters,
         [FromHeader(Name = "Accept")] string mediaType,
         IPropertyMappingService propertyMappingService,
         ITypeHelperService typeHelperService,
         HttpContext httpContext,
         LinkGenerator linkGenerator,
         ILibraryRepository libraryRepository) => {
            
            if (!propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(authorResourcesParameters.OrderBy))
                return TypedResults.BadRequest();
            
            if (!typeHelperService.TypeHasProperties<AuthorDto>(authorResourcesParameters.Fields))
                return TypedResults.BadRequest();
            
            var authorsFromRepo = libraryRepository.GetAuthors(authorResourcesParameters);
            
            if (mediaType.Contains("application/vnd.marvin.hateoas+json"))
            {
                var paginationMetadata = new {
                    pageNumber = authorsFromRepo.PageNumber,
                    pageSize = authorsFromRepo.PageSize,
                    totalPages = authorsFromRepo.TotalPages,
                    totalCount = authorsFromRepo.TotalCount
                };
            
                httpContext.Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
            
                var links = CreateLinksForAuthors(authorResourcesParameters, authorsFromRepo.HasNext, authorsFromRepo.HasPrevious, httpContext, linkGenerator);
                
                var shapedAuthors = authorsFromRepo.ToEnumerableAuthorDto().ShapeData(authorResourcesParameters.Fields);
                
                var shapedAuthorsWithLinks = shapedAuthors.Select(author => {
                    var authorAsDictionary = author as IDictionary<string, object>;
                    var authorLinks = CreateLinksForAuthor((Guid)authorAsDictionary["Id"], authorResourcesParameters.Fields, httpContext, linkGenerator);
                    authorAsDictionary.Add("links", authorLinks);
                    return authorAsDictionary;
                });
                
                var linkedCollectionResource = new {
                    value = shapedAuthorsWithLinks,
                    links
                };
                
                return TypedResults.Ok(linkedCollectionResource as object);
            }
            else
            {
                var previousPageLink = authorsFromRepo.HasPrevious ? CreateAuthorsResourceUri(authorResourcesParameters, ResourceUriType.PreviousPage, httpContext, linkGenerator) : null;
                
                var nextPageLink = authorsFromRepo.HasNext ? CreateAuthorsResourceUri(authorResourcesParameters, ResourceUriType.NextPage, httpContext, linkGenerator) : null;
                
                var paginationMetadata = new {
                    pageNumber = authorsFromRepo.PageNumber,
                    pageSize = authorsFromRepo.PageSize,
                    totalPages = authorsFromRepo.TotalPages,
                    totalCount = authorsFromRepo.TotalCount,
                    previousPageLink,
                    nextPageLink
                };
                
                httpContext.Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
                
                return TypedResults.Ok(authorsFromRepo.ToEnumerableAuthorDto().ShapeData(authorResourcesParameters.Fields) as object);
            }
            
        }).WithName("get_authors").Produces(StatusCodes.Status406NotAcceptable);//.Produces<IEnumerable<AuthorDto>>(StatusCodes.Status200OK, "application/json");
        
        app.MapGet("/authors/{id}", Results<Ok<ExpandoObject>, NotFound, BadRequest> 
        ([FromRoute] Guid id,
         [FromQuery] string? fields,
         ITypeHelperService typeHelperService,
         ILibraryRepository libraryRepository,
         HttpContext httpContext,
         LinkGenerator linkGenerator) => {
            
            if (!typeHelperService.TypeHasProperties<AuthorDto>(fields))
                return TypedResults.BadRequest();
            
            var authorFromRepo = libraryRepository.GetAuthor(id);
            
            if (authorFromRepo is null)
                 return TypedResults.NotFound();
            
            var links = CreateLinksForAuthor(id, fields, httpContext, linkGenerator);
            
            var linkedResourceToReturn = authorFromRepo.ToAuthorDto().ShapeData(fields) as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);
            
            return TypedResults.Ok(linkedResourceToReturn as ExpandoObject);
        }).WithName("get_author").Produces(StatusCodes.Status406NotAcceptable);//.Produces<AuthorDto>(StatusCodes.Status200OK, "application/json");
        
        app.MapPost("/authors", Results<CreatedAtRoute<IDictionary<string, object>>, BadRequest, StatusCodeHttpResult> 
        ([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] AuthorCreateDto authorDto,
         ILibraryRepository libraryRepository,
         HttpContext httpContext,
         LinkGenerator linkGenerator) => {
            
            if (authorDto is null)
                return TypedResults.BadRequest();
            
            var authorEntity = authorDto.ToAuthor();
            
            libraryRepository.AddAuthor(authorEntity);
            
            if (!libraryRepository.Save())
                return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
            
            var authorToReturn = authorEntity.ToAuthorDto();
            
            var links = CreateLinksForAuthor(authorToReturn.Id, null, httpContext,  linkGenerator);
            
            var linkedResourceToReturn = authorToReturn.ShapeData(null) as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);
            
            return TypedResults.CreatedAtRoute(linkedResourceToReturn, routeName: "get_author", routeValues: new { id = linkedResourceToReturn["Id"] });
        }).WithApiVersionSet(apiVersionSet).MapToApiVersion(1.0).WithName("create_author").AddEndpointFilter<ValidationFilter<AuthorCreateDto>>().Produces(StatusCodes.Status406NotAcceptable);
        
        // app.MapPost("/authors", Results<CreatedAtRoute<IDictionary<string, object>>, BadRequest, StatusCodeHttpResult> 
        // ([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] AuthorCreateWithDateOfDeathDto authorDto,
        //  ILibraryRepository libraryRepository,
        //  HttpContext httpContext,
        //  LinkGenerator linkGenerator) => {
            
        //     if (authorDto is null)
        //         return TypedResults.BadRequest();
            
        //     var authorEntity = authorDto.ToAuthor();
            
        //     libraryRepository.AddAuthor(authorEntity);
            
        //     if (!libraryRepository.Save())
        //         return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
            
        //     var authorToReturn = authorEntity.ToAuthorDto();
            
        //     var links = CreateLinksForAuthor(authorToReturn.Id, null, httpContext,  linkGenerator);
            
        //     var linkedResourceToReturn = authorToReturn.ShapeData(null) as IDictionary<string, object>;
        //     linkedResourceToReturn.Add("links", links);
            
        //     return TypedResults.CreatedAtRoute(linkedResourceToReturn, routeName: "get_author", routeValues: new { id = linkedResourceToReturn["Id"] });
        // }).WithApiVersionSet(apiVersionSet).MapToApiVersion(1.1).WithName("create_author").AddEndpointFilter<ValidationFilter<AuthorCreateWithDateOfDeathDto>>().Produces(StatusCodes.Status406NotAcceptable);
        
        app.MapPost("/authors/{id}", Results<NotFound, Conflict> (Guid id, ILibraryRepository libraryRepository) => {
            if (libraryRepository.AuthorExists(id))
                return TypedResults.Conflict();
            return TypedResults.NotFound();
        }).Produces(StatusCodes.Status406NotAcceptable);
        
        app.MapDelete("/authors/{id}", Results<NotFound, NoContent, StatusCodeHttpResult> (Guid id, ILibraryRepository libraryRepository) => {
            var authorfromRepo = libraryRepository.GetAuthor(id);
            if (authorfromRepo is null)
                return TypedResults.NotFound();
            
            libraryRepository.DeleteAuthor(authorfromRepo);
            
            if (!libraryRepository.Save())
                return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
                
            return TypedResults.NoContent();
        }).WithName("delete_author").Produces(StatusCodes.Status406NotAcceptable);
        
        return app;
    }
    
    private static string? CreateAuthorsResourceUri(AuthorResourcesParameters authorResourcesParameters, ResourceUriType type, HttpContext httpContext, LinkGenerator linkGenerator)
    {
        return type switch
        {
            ResourceUriType.PreviousPage => linkGenerator.GetUriByName(httpContext, "get_authors", values: new
            {
                fields = authorResourcesParameters.Fields,
                orderBy = authorResourcesParameters.OrderBy,
                searchQuery = authorResourcesParameters.SearchQuery,
                genre = authorResourcesParameters.Genre,
                pageNumber = authorResourcesParameters.PageNumber - 1,
                pageSize = authorResourcesParameters.PageSize
            }),
            ResourceUriType.NextPage => linkGenerator.GetUriByName(httpContext, "get_authors", values: new
            {
                fields = authorResourcesParameters.Fields,
                orderBy = authorResourcesParameters.OrderBy,
                searchQuery = authorResourcesParameters.SearchQuery,
                genre = authorResourcesParameters.Genre,
                pageNumber = authorResourcesParameters.PageNumber + 1,
                pageSize = authorResourcesParameters.PageSize
            }),
            ResourceUriType.Current or
            _ => linkGenerator.GetUriByName(httpContext, "get_authors", values: new
            {
                fields = authorResourcesParameters.Fields,
                orderBy = authorResourcesParameters.OrderBy,
                searchQuery = authorResourcesParameters.SearchQuery,
                genre = authorResourcesParameters.Genre,
                pageNumber = authorResourcesParameters.PageNumber,
                pageSize = authorResourcesParameters.PageSize
            }),
        };
    }
        
    private static IEnumerable<LinkDto> CreateLinksForAuthor(Guid id, string? fields, HttpContext httpContext, LinkGenerator linkGenerator)
    {
        var links = new List<LinkDto>();
        
        if (string.IsNullOrWhiteSpace(fields))
            links.Add(new LinkDto(linkGenerator.GetUriByName(httpContext, "get_author", values: new { id }), "self", "GET"));
        else
            links.Add(new LinkDto(linkGenerator.GetUriByName(httpContext, "get_author", values: new { id, fields }), "self", "GET"));
        
        links.Add(new LinkDto(linkGenerator.GetUriByName(httpContext, "delete_author", values: new { id }), "delete_author", "DELETE"));
        
        links.Add(new LinkDto(linkGenerator.GetUriByName(httpContext, "create_book_for_author", values: new { authorId = id }), "create_book_for_author", "POST"));
        
        links.Add(new LinkDto(linkGenerator.GetUriByName(httpContext, "get_books_for_author", values: new { authorId = id }), "books", "GET"));
        
        return links;
    }
    
    private static IEnumerable<LinkDto> CreateLinksForAuthors(AuthorResourcesParameters authorResourcesParameters, bool hasNext, bool hasPrevious, HttpContext httpContext, LinkGenerator linkGenerator)
    {
        var links = new List<LinkDto>();
        
        links.Add(new LinkDto(CreateAuthorsResourceUri(authorResourcesParameters, ResourceUriType.Current, httpContext, linkGenerator), "self", "GET"));
        
        if (hasNext)
            links.Add(new LinkDto(CreateAuthorsResourceUri(authorResourcesParameters, ResourceUriType.Current, httpContext, linkGenerator), "nextPage", "GET"));
        if (hasPrevious)
            links.Add(new LinkDto(CreateAuthorsResourceUri(authorResourcesParameters, ResourceUriType.Current, httpContext, linkGenerator), "previousPage", "GET"));
        
        return links;
    }
    
}
