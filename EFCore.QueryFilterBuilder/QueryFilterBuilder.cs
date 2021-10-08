using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace EFCore.QueryFilterBuilder
{
    public class QueryFilterBuilder<TEntity> : IQueryFilterBuilder<TEntity> where TEntity : class
    {
        private readonly Dictionary<string, QueryFilter> _queryFilters;

        private QueryFilterBuilder() =>
            _queryFilters = new Dictionary<string, QueryFilter>();

        #region IQueryFilterBuilder interface method implementation

        /// <summary>
        /// Combine all active expressions into one expression. 
        /// </summary>
        /// <exception cref="InvalidOperationException">The builder contains no filters.</exception>
        /// <returns>A LINQ predicate expression.</returns>
        public Expression<Func<TEntity, bool>> Build()
        {
            if (_queryFilters.Count == 0)
                throw new InvalidOperationException("No expressions provided.");

            var activeQueryFilters = _queryFilters
                .Where(q => q.Value.Active)
                .Select(q => q.Value.Expression)
                .ToList();

            if (activeQueryFilters.Count == 0)
                return q => true;

            if (activeQueryFilters.Count == 1)
                return activeQueryFilters.Single();

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
        public IQueryFilterBuilder<TEntity> AddFilter(Expression<Func<TEntity, bool>> expression, bool active = true)
        {
            string key = Guid.NewGuid().ToString();

            _queryFilters.Add(key, QueryFilter.Create(key, active, expression));
            return this;
        }

        /// <summary>
        /// Adding a filter to a given query filter builder.
        /// </summary>
        /// <param name="filterName">The unique name of the filter.</param>
        /// <param name="expression">A LINQ predicate expression.</param>
        /// <param name="active">Indication of whether the filter should be applied, this parameter can be controlled by a service injected into DbContext.</param>
        /// <exception cref="ArgumentException">Filter with given name already exists.</exception>
        /// <exception cref="ArgumentNullException">Filter name is null.</exception>
        /// <returns>A QueryFilterBuilder instance to chain methods.</returns>
        public IQueryFilterBuilder<TEntity> AddFilter(string filterName, Expression<Func<TEntity, bool>> expression, bool active = true)
        {
            _queryFilters.Add(filterName, QueryFilter.Create(filterName, active, expression));
            return this;
        }

        /// <summary>
        /// Searching for a filter based on the given name and disabling it.
        /// </summary>
        /// <param name="filterName">The unique name of the filter.</param>
        /// <exception cref="KeyNotFoundException">Filter with given name does not exist.</exception>
        /// <exception cref="ArgumentNullException">Filter name is null.</exception>
        /// <returns>A QueryFilterBuilder instance to chain methods.</returns>
        public IQueryFilterBuilder<TEntity> DisableFilter(string filterName)
        {
            var queryFilter = _queryFilters[filterName];
            
            _queryFilters[filterName] = QueryFilter.Create(queryFilter.Name, false, queryFilter.Expression);
            return this;
        }

        /// <summary>
        /// Searching for a filter based on the given name and enabling it.
        /// </summary>
        /// <param name="filterName">The unique name of the filter.</param>
        /// <exception cref="KeyNotFoundException">Filter with given name does not exist.</exception>
        /// <exception cref="ArgumentNullException">Filter name is null.</exception>
        /// <returnsA QueryFilterBuilder instance to chain methods.></returns>
        public IQueryFilterBuilder<TEntity> EnableFilter(string filterName)
        {
            var queryFilter = _queryFilters[filterName];

            _queryFilters[filterName] = QueryFilter.Create(queryFilter.Name, true, queryFilter.Expression);
            return this;
        }

        #endregion

        #region QueryFilterBuilder factory

        /// <summary>
        /// QueryFilterBuilder factory.
        /// </summary>
        /// <returns>A new QueryFilterBuilder instance.</returns>
        public static IQueryFilterBuilder<TEntity> Create()
        {
            return new QueryFilterBuilder<TEntity>();
        }

        #endregion

        #region Hide System.Object inherited methods

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        #endregion

        #region QueryFilterBuilder private helper classes

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

        #endregion
    }
}
