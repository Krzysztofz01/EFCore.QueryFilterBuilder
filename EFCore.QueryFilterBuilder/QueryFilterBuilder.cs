using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EFCore.QueryFilterBuilder
{
    public class QueryFilterBuilder<TEntity> where TEntity : class
    {
        private readonly Dictionary<string, QueryFilter> _queryFilters;

        private QueryFilterBuilder() =>
            _queryFilters = new Dictionary<string, QueryFilter>();

        /// <summary>
        /// Combine all active expressions into one expression. 
        /// </summary>
        /// <returns>A LINQ predicate expression.</returns>
        public Expression<Func<TEntity, bool>> Build()
        {
            if (_queryFilters.Count == 0)
                throw new InvalidOperationException("No expressions provided.");

            if (_queryFilters.Count == 1)
                return _queryFilters.Single().Value.Expression;

            var activeQueryFilters = _queryFilters
                .Where(q => q.Value.Active)
                .Select(q => q.Value.Expression);

            var exp = activeQueryFilters.First();

            foreach (var e in activeQueryFilters.Skip(1))
            {
                var leftParam = exp.Parameters[0];
                var rightParam = e.Parameters[0];

                var visitor = new ReplaceParameterVisitor(rightParam, leftParam);

                var leftBody = exp.Body;
                var rightBody = visitor.Visit(e.Body);

                exp = Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(leftBody, rightBody), leftParam);
            }

            return exp;
        }

        /// <summary>
        /// Adding a filter to a given query filter builder.
        /// </summary>
        /// <param name="expression">A LINQ predicate expression.</param>
        /// <param name="active">Indication of whether the filter should be applied, this parameter can be controlled by a service injected into DbContext.</param>
        /// <returns>A QueryFilterBuilder instance to chain methods.</returns>
        public QueryFilterBuilder<TEntity> AddFilter(Expression<Func<TEntity, bool>> expression, bool active = true)
        {
            string key = Guid.NewGuid().ToString();

            _queryFilters.Add(key, QueryFilter.Create(key, active, expression));
            return this;
        }

        /// <summary>
        /// Adding a filter to a given query filter builder.
        /// </summary>
        /// <param name="filterName">The name of the filter.</param>
        /// <param name="expression">A LINQ predicate expression.</param>
        /// <param name="active">Indication of whether the filter should be applied, this parameter can be controlled by a service injected into DbContext.</param>
        /// <returns>A QueryFilterBuilder instance to chain methods.</returns>
        public QueryFilterBuilder<TEntity> AddFilter(string filterName, Expression<Func<TEntity, bool>> expression, bool active = true)
        {
            _queryFilters.Add(filterName, QueryFilter.Create(filterName, active, expression));
            return this;
        }

        /// <summary>
        /// QueryFilterBuilder factory.
        /// </summary>
        /// <returns>A new QueryFilterBuilder instance.</returns>
        public static QueryFilterBuilder<TEntity> Create()
        {
            return new QueryFilterBuilder<TEntity>();
        }

        private class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;

            public ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return ReferenceEquals(node, _oldParameter) ? _newParameter : base.VisitParameter(node);
            }
        }

        private class QueryFilter
        {
            public string Name { get; private set; }
            public bool Active { get; private set; }
            public Expression<Func<TEntity, bool>> Expression { get; private set; }

            private QueryFilter() { }

            public static QueryFilter Create(string name, bool active, Expression<Func<TEntity, bool>> expression)
            {
                return new QueryFilter
                {
                    Name = name,
                    Active = active,
                    Expression = expression
                };
            }
        }
    }
}
