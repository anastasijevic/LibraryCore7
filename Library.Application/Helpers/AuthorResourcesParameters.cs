using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Library.Application.Helpers;

public class AuthorResourcesParameters
{
    const int MaxPageSize = 20;
    private int _pageSize = 10;
    
    public int PageNumber { get; set; } = 1;
    public int PageSize { get { return _pageSize; } set { _pageSize = (value > MaxPageSize) ? MaxPageSize : value; } }
    public string Genre { get; set; } = string.Empty;
    public string SearchQuery { get; set; } = string.Empty;
    public string OrderBy { get; set; } = "Name";
    public string Fields { get; set; } = string.Empty;
    
    public static ValueTask<AuthorResourcesParameters?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var result = new AuthorResourcesParameters();
        
        if (!StringValues.IsNullOrEmpty(context.Request.Query["pageNumber"]))
            if (int.TryParse(context.Request.Query["pageNumber"], out int pageNumber))
                result.PageNumber = pageNumber;
        
        if (!StringValues.IsNullOrEmpty(context.Request.Query["pageSize"]))
            if (int.TryParse(context.Request.Query["pageSize"], out int pageSize))
                result.PageSize = pageSize;
                
        if (!StringValues.IsNullOrEmpty(context.Request.Query["genre"]))
            if (!string.IsNullOrWhiteSpace(context.Request.Query["genre"]))
                result.Genre = context.Request.Query["genre"]!;
        
        if (!StringValues.IsNullOrEmpty(context.Request.Query["searchQuery"]))
            if (!string.IsNullOrWhiteSpace(context.Request.Query["searchQuery"]))
                result.SearchQuery = context.Request.Query["searchQuery"]!;
        
        if (!StringValues.IsNullOrEmpty(context.Request.Query["orderBy"]))
            if (!string.IsNullOrWhiteSpace(context.Request.Query["orderBy"]))
                result.OrderBy = context.Request.Query["orderBy"]!;
        
        if (!StringValues.IsNullOrEmpty(context.Request.Query["fields"]))
            if (!string.IsNullOrWhiteSpace(context.Request.Query["fields"]))
                result.Fields = context.Request.Query["fields"]!;
        
        context.Response.Headers.Add("custom", "customValue");
        return ValueTask.FromResult<AuthorResourcesParameters?>(result);
    }
}
