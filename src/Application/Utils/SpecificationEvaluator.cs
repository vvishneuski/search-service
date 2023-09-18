namespace SearchService.Application.Utils;

using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;

public static class SpecificationEvaluator
{
    public static IQueryable<T> ApplySpecification<T>(this DbSet<T> context, ISpecification<T> spec)
        where T : class => Ardalis.Specification.EntityFrameworkCore.SpecificationEvaluator.Default
        .GetQuery(context.AsQueryable(), spec);
}
