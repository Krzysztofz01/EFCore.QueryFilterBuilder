using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace EFCore.QueryFilterBuilder
{
    public class QueryFilterBuilder<TEntity> : IQueryFilterBuilder<TEntity> where TEntity : class
    {
        private readonly EntityTypeBuilder<TEntity> _entityTypeBuilder;
        private readonly Dictionary<string, QueryFilter<TEntity>> _queryFilters;

        EntityTypeBuilder<TEntity> IQueryFilterBuilder<TEntity>.Build()
        {
            if (_queryFilters.Count == 0)
                return _entityTypeBuilder;

            var activeQueryFilters = _queryFilters
                .Where(q => q.Value.Active)
                .Select(q => q.Value.Expression)
                .ToArray();

            if (activeQueryFilters.Length == 0)
                return _entityTypeBuilder;

            var expression = activeQueryFilters.First();

            if (activeQueryFilters.Length == 1)
                return _entityTypeBuilder.HasQueryFilter(expression);

            foreach (var exp in activeQueryFilters.Skip(1))
            {
                var leftParam = expression.Parameters.First();
                var rightParam = exp.Parameters.First();

                var visitor = new ReplaceParameterVisitor(rightParam, leftParam);

                var leftBody = expression.Body;
                var rightBody = visitor.Visit(exp.Body);

                expression = Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(leftBody, rightBody), leftParam);
            }

            return _entityTypeBuilder.HasQueryFilter(expression);
        }

        public IQueryFilterBuilder<TEntity> AddFilter(Expression<Func<TEntity, bool>> expression, bool active = true)
        {
            string queryFilterId = Guid.NewGuid().ToString();

            _queryFilters.Add(queryFilterId, QueryFilter<TEntity>.Create(queryFilterId, active, expression));
            return this;
        }

        public IQueryFilterBuilder<TEntity> AddFilter(string filterName, Expression<Func<TEntity, bool>> expression, bool active = true)
        {
            if (string.IsNullOrEmpty(filterName))
                throw new ArgumentNullException(nameof(filterName), "Invalid filter name specified.");

            if (_queryFilters.ContainsKey(filterName))
                throw new InvalidOperationException("The filter name must be unique.");

            _queryFilters.Add(filterName, QueryFilter<TEntity>.Create(filterName, active, expression));
            return this;
        }

        public IQueryFilterBuilder<TEntity> DisableFilter(string filterName)
        {
            if (!_queryFilters.ContainsKey(filterName))
                throw new InvalidOperationException("Filter with given name not found.");

            var queryFilter = _queryFilters[filterName];

            _queryFilters[filterName] = QueryFilter<TEntity>.Create(queryFilter.Name, false, queryFilter.Expression);
            return this;
        }

        public IQueryFilterBuilder<TEntity> EnableFilter(string filterName)
        {
            if (!_queryFilters.ContainsKey(filterName))
                throw new InvalidOperationException("Filter with given name not found.");

            var queryFilter = _queryFilters[filterName];

            _queryFilters[filterName] = QueryFilter<TEntity>.Create(queryFilter.Name, true, queryFilter.Expression);
            return this;
        }


        private QueryFilterBuilder() { }

        private QueryFilterBuilder(EntityTypeBuilder<TEntity> entityTypeBuilder)
        {
            _entityTypeBuilder = entityTypeBuilder ??
                throw new ArgumentNullException(nameof(entityTypeBuilder));

            _queryFilters = new Dictionary<string, QueryFilter<TEntity>>();
        }

        public static QueryFilterBuilder<TEntity> Create(EntityTypeBuilder<TEntity> entityTypeBuilder) => new(entityTypeBuilder);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
    }
}
