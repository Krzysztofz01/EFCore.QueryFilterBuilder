using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.QueryFilterBuilder
{
    public static class EntityFrameworkCoreExtensions
    {
        /// <summary>
        /// Specifies a LINQ predicate expression that will automatically be applied to any queries targeting this entity type.
        /// </summary>
        /// <param name="queryFilterBuilder">The QueryFilterBuilder instance whose expression is to be applied to the given entity.</param>
        /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
        public static EntityTypeBuilder<TEntity> HasQueryFilter<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, QueryFilterBuilder<TEntity> queryFilterBuilder) where TEntity : class
        {
            return entityTypeBuilder.HasQueryFilter(queryFilterBuilder.Build());
        }
    }
}
