namespace Library.Infrastructure.Services;

public interface ITypeHelperService
{
    bool TypeHasProperties<T>(string? fields);
}
