using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Library.Api;

public static class ErrorRoutes
{
    public static IEndpointRouteBuilder MapErrorRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/error", ProblemHttpResult () => {
            // Exception? exception = accessor.HttpContext!.Features.Get<IExceptionHandlerFeature>()?.Error;
            // var (statusCode, message) = exception switch
            // {
            //     DuplicateEmailException => (StatusCodes.Status409Conflict, "Email already exists."),
            //     _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            // };
            //log exception
            return TypedResults.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.");
        }).ExcludeFromDescription();
        
        app.MapPost("/error", ProblemHttpResult () => {
            // Exception? exception = accessor.HttpContext!.Features.Get<IExceptionHandlerFeature>()?.Error;
            // var (statusCode, message) = exception switch
            // {
            //     DuplicateEmailException => (StatusCodes.Status409Conflict, "Email already exists."),
            //     _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            // };
            //log exception
            return TypedResults.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.");
        }).ExcludeFromDescription();
        
        return app;
    }
}