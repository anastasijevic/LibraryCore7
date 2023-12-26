
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api;

public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is not null)
        {
            var entity = context.Arguments.OfType<T>().FirstOrDefault(t => t?.GetType() == typeof(T));
            if (entity is not null)
            {
                var validation = await validator.ValidateAsync(entity);
                if (validation.IsValid)
                    return await next(context);
                else
                    return TypedResults.Problem(new HttpValidationProblemDetails(validation.ToDictionary()) { Status = StatusCodes.Status422UnprocessableEntity });
            }
            else
                return TypedResults.Problem(new ProblemDetails() 
                { 
                    Detail = $"Empty request body. Please provide valid object of {typeof(T).Name}.",
                    Title = "Empty request body.",
                    Status = StatusCodes.Status400BadRequest
                });
        }
        return await next(context);
    }
}
    
