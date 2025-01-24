using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Data;

public class SpecificationEvaluator<T> where T : BaseEntity
{
    public static IQueryable<T> GetQuery(IQueryable<T> query, ISpecification<T> spec)
    {
        query = AddCriteria(query, spec);
        query = AddOrderBy(query, spec);
        query = AddOrderByDescending(query, spec);

        if (spec.IsDistinct)
        {
            query = query.Distinct();
        }

        return query;
    }

    public static IQueryable<TResult> GetQuery<TSpec, TResult>(IQueryable<T> query, ISpecification<T, TResult> spec)
    {
        query = AddCriteria(query, spec);
        query = AddOrderBy(query, spec);
        query = AddOrderByDescending(query, spec);

        var selectQuery = query as IQueryable<TResult>;

        if (spec.Select != null)
        {
            selectQuery = query.Select(spec.Select);
        }

        if (spec.IsDistinct)
        {
            selectQuery = selectQuery?.Distinct();
        }

        return selectQuery ?? query.Cast<TResult>();
    }

    private static IQueryable<T> AddCriteria(IQueryable<T> query, ISpecification<T> spec)
    {
        if (spec.Criteria != null)
        {
            query = query.Where(spec.Criteria);
        }

        return query;
    }

    private static IQueryable<T> AddOrderBy(IQueryable<T> query, ISpecification<T> spec)
    {
        if (spec.OrderBy != null)
        {
            query = query.OrderBy(spec.OrderBy);
        }

        return query;
    }

    private static IQueryable<T> AddOrderByDescending(IQueryable<T> query, ISpecification<T> spec)
    {
        if (spec.OrderByDescending != null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        return query;
    }
}
