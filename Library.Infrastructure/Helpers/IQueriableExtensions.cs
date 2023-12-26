using Library.Infrastructure.Services;
using System.Linq.Dynamic.Core;
using System.Text;

namespace Library.Infrastructure.Helpers;

public static class IQueriableExtensions
{
    public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy, Dictionary<string, PropertyMappingValue> mappingDictionary)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (mappingDictionary is null)
            throw new ArgumentNullException(nameof(mappingDictionary));
        if (string.IsNullOrWhiteSpace(orderBy))
            return source;
        
        StringBuilder orderByQuery = new();
        var orderByAfterSplit = orderBy.Split(',');
        
        foreach (var orderByClause in orderByAfterSplit)
        {
            var trimmedOrderByClause = orderByClause.Trim();
            
            var orderDescending = trimmedOrderByClause.EndsWith(" desc");
            
            var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
            
            var propertyName = indexOfFirstSpace == -1 ?
                trimmedOrderByClause : trimmedOrderByClause.Remove(indexOfFirstSpace);
            
            if (!mappingDictionary.ContainsKey(propertyName))
                throw new ArgumentException($"Key mapping of {propertyName} is missing.");
            
            var propertyMappingValue = mappingDictionary[propertyName] ?? throw new ArgumentNullException("propertyMappingValue");
            
            foreach (var destinationProperty in propertyMappingValue.DestinationProperties)
            {
                if (propertyMappingValue.Revert)
                    orderDescending = !orderDescending;
                orderByQuery.Append(destinationProperty + (orderDescending ? " descending," : " ascending,"));
            }
        }
        
        source = source.OrderBy(orderByQuery.ToString().TrimEnd(','));
        
        return source;
    }
}
