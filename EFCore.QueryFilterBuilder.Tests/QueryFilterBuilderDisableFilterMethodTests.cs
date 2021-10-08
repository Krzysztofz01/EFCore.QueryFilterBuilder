using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace EFCore.QueryFilterBuilder.Tests
{
    public class QueryFilterBuilderDisableFilterMethodTests
    {
        [Fact]
        public void QueryFilterIsApplying()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"), new InMemoryDatabaseRoot()).Options;

            using var dbContext = new TestDbContext(options, false);
            InitializeData(dbContext);

            int expectedCount = 2;

            int actualCount = dbContext.Blogs.Count();

            Assert.Equal(expectedCount, actualCount);

            dbContext.Dispose();
        }

        [Fact]
        public void QueryFilterCanBeDisabled()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"), new InMemoryDatabaseRoot()).Options;

            using var dbContext = new TestDbContext(options, true);
            InitializeData(dbContext);

            int expectedCount = 3;

            int actualCount = dbContext.Blogs.Count();

            Assert.Equal(expectedCount, actualCount);

            dbContext.Dispose();
        }

        //Testing DbContext
        private class TestDbContext : DbContext
        {
            private readonly bool _disableFilter;

            public DbSet<Blog> Blogs { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                string filterName = "NameFilter";

                if (_disableFilter)
                {
                    modelBuilder.Entity<Blog>()
                    .HasQueryFilter(QueryFilterBuilder<Blog>
                        .Create()
                        .AddFilter(filterName, b => b.Name == "Hello World")
                        .DisableFilter(filterName)
                        .Build());
                }
                else
                {
                    modelBuilder.Entity<Blog>()
                    .HasQueryFilter(QueryFilterBuilder<Blog>
                        .Create()
                        .AddFilter(filterName, b => b.Name == "Hello World")
                        .Build());
                }
            }

            public TestDbContext(DbContextOptions<TestDbContext> opt, bool disableFilter) : base(opt) { _disableFilter = disableFilter; }
        }

        //Testing entity
        private class Blog
        {
            public Blog() =>
                Id = Guid.NewGuid();

            [Key]
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int Posts { get; set; }
            public string About { get; set; }
        }

        private void InitializeData(TestDbContext context)
        {
            context.AddRange(new Blog[]
            {
                new Blog { Name = "Hello World", About = "Description", Posts = 20 },
                new Blog { Name = "Hello World", About = "About blog", Posts = 10 },
                new Blog { Name = "EFCore", About = "Description", Posts = 30 }
            });

            context.SaveChanges();
        }
    }
}
