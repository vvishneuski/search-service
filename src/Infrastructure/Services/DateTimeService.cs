namespace SearchService.Infrastructure.Services;

using Application.Interfaces;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.UtcNow;
}
