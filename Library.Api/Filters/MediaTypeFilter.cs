using System.Collections.ObjectModel;

namespace Library.Api.Filters;

public class MediaTypeFilter : IEndpointFilter
{
    private static readonly IList<string> AllowedAcceptMediaTypeNames = new ReadOnlyCollection<string>(new List<string> { 
        "application/json", 
        "application/problem+json",
        "application/vnd.marvin.hateoas+json"
    });
    
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        string title;
        string detail;
        if (!string.IsNullOrWhiteSpace(context.HttpContext.Request.Headers.Accept.ToString()))
        {
            string[]? notAcceptableMediaTypeNames = null;
            if (context.HttpContext.Request.Headers.Accept.ToString().Contains(','))
            {
                string[]? segments = context.HttpContext.Request.Headers.Accept.ToString().Split(',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                notAcceptableMediaTypeNames = segments.Except(AllowedAcceptMediaTypeNames).ToArray().Length > 0 ? segments.Except(AllowedAcceptMediaTypeNames).ToArray() : null;
            }
            else if (!AllowedAcceptMediaTypeNames.Contains(context.HttpContext.Request.Headers.Accept.ToString()))
                notAcceptableMediaTypeNames = new string[] { context.HttpContext.Request.Headers.Accept.ToString() };

            if (notAcceptableMediaTypeNames is null)
                return await next(context);
            else
            {
                title = "Unsuported 'Accept' header";
                detail = $"Unsuported 'Accept' header{(notAcceptableMediaTypeNames.Length > 1 ? "s" : "")} : ({String.Join(", ", notAcceptableMediaTypeNames)})";
            }
        }
        else
        {
            title = "No 'Accept' header provided.";
            detail = "Provide allowed 'Accept' header from documentation.";
        }

        return TypedResults.Problem(statusCode: StatusCodes.Status406NotAcceptable, 
        detail: detail, 
        title: title);
    }
}
