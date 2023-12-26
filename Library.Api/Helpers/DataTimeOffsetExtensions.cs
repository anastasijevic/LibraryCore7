namespace Library.Api.Helpers;

public static class DataTimeOffsetExtensions
{
    public static int GetCurrentAge(this DateTimeOffset dateTimeOffset, DateTimeOffset? dateOfDeath)
    {
        var currentDate = DateTime.UtcNow;
        
        if (dateOfDeath is not null)
            currentDate = dateOfDeath.Value.UtcDateTime;
        
        int age = currentDate.Year - dateTimeOffset.Year;
        
        if (currentDate < dateTimeOffset.AddYears(age))
            age--;
        
        return age;
    }
}
