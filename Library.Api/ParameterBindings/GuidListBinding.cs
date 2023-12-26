namespace Library.Api.ParameterBindings;

public class GuidListBinding
{
    public List<Guid> Guids { get; set; } = new List<Guid>();
    
    public static bool TryParse(string? value, out GuidListBinding? guidList)
    {
        if (value is not null && value.StartsWith('(') && value.EndsWith(')'))
        {
            var trimmedValue = value.TrimStart('(').TrimEnd(')');
            if (trimmedValue is not null && !string.IsNullOrWhiteSpace(trimmedValue))
            {
                var segments = trimmedValue.Split(',',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (segments is not null && segments.Length > 0)
                {
                    guidList = new GuidListBinding();
                    foreach (var segment in segments)
                    {
                        if (Guid.TryParse(segment, out Guid guidResult))
                            guidList.Guids.Add(guidResult);
                    }
                    return true;
                }
            }
        }
        
        guidList = null;
        return false;
    }
}
