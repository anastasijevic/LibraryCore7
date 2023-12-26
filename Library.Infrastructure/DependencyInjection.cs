using Library.Application;
using Library.Application.Common.Interfaces.Persistence;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Library.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication(configuration);
        
        services.AddDbContext<LibraryDbContext>(options => 
            options.UseSqlServer(configuration["connectionStrings:libraryDBConnectionString"]));
        
        services.AddScoped<ILibraryRepository, LibraryRepository>();
        
        services.AddTransient<IPropertyMappingService, PropertyMappingService>();
        services.AddTransient<ITypeHelperService, TypeHelperService>();
        
        return services;
    }
}
