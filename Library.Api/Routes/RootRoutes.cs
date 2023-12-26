using Library.Application.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Routes;

public static class RootRoutes
{
    public static IEndpointRouteBuilder MapRootRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", Results<Ok<IEnumerable<LinkDto>>, NoContent> ([FromHeader(Name = "Accept")] string mediaType, HttpContext httpContext, LinkGenerator linkGenerator) => {
            
            if (mediaType.Contains("application/vnd.marvin.hateoas+json"))
            {
                var links = new List<LinkDto>();
                
                links.Add(new LinkDto(linkGenerator.GetUriByName(httpContext, "get_root", values: new { }), "self", "GET"));
                links.Add(new LinkDto(linkGenerator.GetUriByName(httpContext, "get_authors", values: new { }), "authors", "GET"));
                links.Add(new LinkDto(linkGenerator.GetUriByName(httpContext, "create_author", values: new { }), "create_author", "POST"));
                
                return TypedResults.Ok(links as IEnumerable<LinkDto>);
            }
            
            return TypedResults.NoContent();
        }).WithName("get_root").Produces(StatusCodes.Status406NotAcceptable);
        
        return app;
    }
}
