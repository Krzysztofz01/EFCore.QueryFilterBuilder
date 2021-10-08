using System;
using System.Linq.Expressions;

namespace EFCore.QueryFilterBuilder
{
    public interface IQueryFilterBuilder<TEntity> where TEntity : class
    {
        Expression<Func<TEntity, bool>> Build();
        IQueryFilterBuilder<TEntity> AddFilter(Expression<Func<TEntity, bool>> expression, bool active = true);
        IQueryFilterBuilder<TEntity> AddFilter(string filterName, Expression<Func<TEntity, bool>> expression, bool active = true);
    }
}
