using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq.Expressions;

namespace EFCore.QueryFilterBuilder
{
    public interface IQueryFilterBuilder<TEntity> where TEntity : class
    {
        /// <summary>
        /// Combine all active expressions, apply expression as query filter and return the target EntityTypeBuilder
        /// </summary>
        /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
        EntityTypeBuilder<TEntity> Build();

        /// <summary>
        /// Adding a filter to a given query filter builder.
        /// </summary>
        /// <param name="expression">A LINQ predicate expression.</param>
        /// <param name="active">Indication of whether the filter should be applied, this parameter can be controlled by a service injected into DbContext.</param>
        /// <returns>A QueryFilterBuilder instance to chain methods.</returns>
        IQueryFilterBuilder<TEntity> AddFilter(Expression<Func<TEntity, bool>> expression, bool active = true);

        /// <summary>
        /// Adding a filter to a given query filter builder.
        /// </summary>
        /// <param name="filterName">The unique name of the filter.</param>
        /// <param name="expression">A LINQ predicate expression.</param>
        /// <param name="active">Indication of whether the filter should be applied, this parameter can be controlled by a service injected into DbContext.</param>
        /// <exception cref="ArgumentException">Filter with given name already exists.</exception>
        /// <exception cref="ArgumentNullException">Filter name is null.</exception>
        /// <returns>A QueryFilterBuilder instance to chain methods.</returns>
        IQueryFilterBuilder<TEntity> AddFilter(string filterName, Expression<Func<TEntity, bool>> expression, bool active = true);

        /// <summary>
        /// Searching for a filter based on the given name and disabling it.
        /// </summary>
        /// <param name="filterName">The unique name of the filter.</param>
        /// <exception cref="KeyNotFoundException">Filter with given name does not exist.</exception>
        /// <exception cref="ArgumentNullException">Filter name is null.</exception>
        /// <returns>A QueryFilterBuilder instance to chain methods.</returns>
        IQueryFilterBuilder<TEntity> DisableFilter(string filterName);

        /// <summary>
        /// Searching for a filter based on the given name and enabling it.
        /// </summary>
        /// <param name="filterName">The unique name of the filter.</param>
        /// <exception cref="KeyNotFoundException">Filter with given name does not exist.</exception>
        /// <exception cref="ArgumentNullException">Filter name is null.</exception>
        /// <returnsA QueryFilterBuilder instance to chain methods.></returns>
        IQueryFilterBuilder<TEntity> EnableFilter(string filterName);
    }
}
