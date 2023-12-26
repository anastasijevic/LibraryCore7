namespace Library.Application.Models;

public abstract class LinkedResourceBaseDto
{
    public List<LinkDto> Links { get; set; } = new List<LinkDto>();
    
}
