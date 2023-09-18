namespace SearchService.Application.Models;

using Microsoft.EntityFrameworkCore;

public class PaginatedList<T>
{
    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        this.PageIndex = pageIndex;
        this.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        this.TotalCount = count;
        this.Items = items;
    }

    public List<T> Items { get; }
    public int PageIndex { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }

    public bool HasPreviousPage => this.PageIndex > 1;

    public bool HasNextPage => this.PageIndex < this.TotalPages;

#pragma warning disable CA1000 // Do not declare static members on generic types
    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
#pragma warning restore CA1000 // Do not declare static members on generic types
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}
