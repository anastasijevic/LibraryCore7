using Library.Api;
using Library.Api.Filters;
using Library.Api.Routes;
using Library.Infrastructure.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;
using Library.Application.Validators;
using Library.Application.Models;
using System.Net;
using System.Text.Json;
using Serilog;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Host.UseSerilog((context, configuration) => 
        configuration.ReadFrom.Configuration(context.Configuration));
    
    builder.Services.AddScoped<IValidator<List<AuthorCreateDto>>, AuthorCollectionValidator>();
    builder.Services.AddScoped<IValidator<AuthorCreateDto>, AuthorValidator>();
    builder.Services.AddScoped<IValidator<AuthorCreateWithDateOfDeathDto>, AuthorWithDateOfDeathValidator>();
    builder.Services.AddScoped<IValidator<BookCreateDto>, BookCreateValidator>();
    builder.Services.AddScoped<IValidator<BookUpdateDto>, BookUpdateValidator>();
    
    // builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
    
    builder.Services.AddProblemDetails();
    builder.Services.AddPresentation(builder.Configuration);
    builder.Services.AddHttpContextAccessor();
    builder.Services.ConfigureHttpJsonOptions(options => {
        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });
    builder.Services.AddApiVersioning(options => {
        var builder = new MediaTypeApiVersionReaderBuilder();
        options.ApiVersionReader = builder.Include("application/vnd.marvin.hateoas+json").Include("application/vnd.marvin.hateoas+json").Build();
        //options.DefaultApiVersion = new ApiVersion(1.0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
        
    });
    // builder.Services.Configure<RouteHandlerOptions>(o => {
    //     o.ThrowOnBadRequest = true;
    // });
}

var app = builder.Build();
{
    var version1 = new ApiVersion(1.0);
    var version11 = new ApiVersion(1.1);
    var versionSet = app.NewApiVersionSet().HasApiVersion(version1).HasApiVersion(version11).Build();
    
    app.Services.ApplyMigration();
    
    app.UseSerilogRequestLogging();
    
    if (app.Environment.IsDevelopment())
    {}
        app.UseSwagger()
           .UseSwaggerUI();
    
    app.UseStatusCodePages();
    app.UseHttpsRedirection();
    app.UseExceptionHandler("/error");
    app.UseExceptionHandler(app => 
    {
        app.Run(async context => {
            Exception? exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            var (statusCode, message) = exception switch
            {
                BadHttpRequestException => (StatusCodes.Status400BadRequest, exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };
            var problem = new ProblemDetails { Title = ((HttpStatusCode)statusCode).ToString() , Detail = message, Status = statusCode };
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = Application.Json;
            await context.Response.WriteAsJsonAsync(problem);
        });
    });
    
    app.MapGroup("/")
       .AddEndpointFilter<MediaTypeFilter>()
       .MapAuthorRoutes(versionSet)
       .MapAuthorCollectionRoutes()
       .MapBookRoutes()
       .MapRootRoutes()
       .MapErrorRoutes();
    
    app.Run();
}