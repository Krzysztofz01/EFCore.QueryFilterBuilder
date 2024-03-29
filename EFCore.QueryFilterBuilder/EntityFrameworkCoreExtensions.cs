﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq.Expressions;

namespace EFCore.QueryFilterBuilder
{
    public static class EntityFrameworkCoreExtensions
    {
        /// <summary>
        /// Specifies a LINQ predicate expression that will automatically be applied to any queries targeting this entity type.
        /// </summary>
        /// <returns>QueryFilterBuilder instance which allows to chain multiple query filters.</returns>
        public static IQueryFilterBuilder<TEntity> HasQueryFilters<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder) where TEntity : class
        {
            return QueryFilterBuilder<TEntity>.Create(entityTypeBuilder);
        }

        /// <summary>
        /// Specifies a LINQ predicate expression that will automatically be applied to any queries targeting this entity type.
        /// </summary>
        /// <param name="filter">The LINQ predicate expression.</param>
        /// <returns>QueryFilterBuilder instance which allows to chain multiple query filters.</returns>
        public static IQueryFilterBuilder<TEntity> HasQueryFilter<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, Expression<Func<TEntity, bool>> filter) where TEntity : class
        {
            return QueryFilterBuilder<TEntity>.Create(entityTypeBuilder).AddFilter(filter);
        }

        /// <summary>
        /// Specifies a LINQ predicate expression that will automatically be applied to any queries targeting this entity type.
        /// </summary>
        /// <param name="queryFilterBuilder">QueryFilterBuilder instance which allows to chain multiple query filters.</param>
        /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
        public static EntityTypeBuilder<TEntity> HasQueryFilter<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, IQueryFilterBuilder<TEntity> queryFilterBuilder) where TEntity : class
        {
            return queryFilterBuilder.Build();
        }
    }
}
