using System;
using System.Linq.Expressions;

namespace EFCore.QueryFilterBuilder
{
    internal sealed class QueryFilter<TEntity> where TEntity : class
    {
        public string Name { get; private set; }
        public bool Active { get; private set; }
        public Expression<Func<TEntity, bool>> Expression { get; private set; }

        private QueryFilter() { }

        public static QueryFilter<TEntity> Create(string name, bool active, Expression<Func<TEntity, bool>> expression)
        {
            return new QueryFilter<TEntity>
            {
                Name = name,
                Active = active,
                Expression = expression
            };
        }
    }
}
