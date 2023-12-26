namespace Library.Application.Helpers;

public class PagedList<T> : List<T>
{
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }
    public int TotalPages { get; private set; }
    
    public bool HasPrevious { get { return PageNumber > 1; } }
    public bool HasNext { get { return PageNumber < TotalPages; } }
    
    public PagedList(List<T> items, int pageNumber, int pageSize, int totalCount)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        AddRange(items);
    }
    
    public static PagedList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var totalCount = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return new PagedList<T>(items, pageNumber, pageSize, totalCount);
    }
}
