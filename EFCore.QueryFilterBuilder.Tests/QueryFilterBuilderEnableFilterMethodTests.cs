using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace EFCore.QueryFilterBuilder.Tests
{
    public class QueryFilterBuilderEnableFilterMethodTests
    {
        [Fact]
        public void QueryFilterIsApplying()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"), new InMemoryDatabaseRoot()).Options;

            using var dbContext = new TestDbContext(options, false);
            InitializeData(dbContext);

            int expectedCount = 3;

            int actualCount = dbContext.Blogs.Count();

            Assert.Equal(expectedCount, actualCount);

            dbContext.Dispose();
        }

        [Fact]
        public void QueryFilterCanBeEnabled()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"), new InMemoryDatabaseRoot()).Options;

            using var dbContext = new TestDbContext(options, true);
            InitializeData(dbContext);

            int expectedCount = 2;

            int actualCount = dbContext.Blogs.Count();

            Assert.Equal(expectedCount, actualCount);

            dbContext.Dispose();
        }

        // Testing purposes DbContext
        private class TestDbContext : DbContext
        {
            private readonly bool _enableFilter;

            public DbSet<Blog> Blogs { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                string filterName = "NameFilter";

                if (_enableFilter)
                {
                    modelBuilder.Entity<Blog>()
                        .HasQueryFilters()
                        .AddFilter(filterName, b => b.Name == "Hello World", false)
                        .EnableFilter(filterName)
                        .Build();
                }
                else
                {
                    modelBuilder.Entity<Blog>()
                        .HasQueryFilters()
                        .AddFilter(filterName, b => b.Name == "Hello World", false)
                        .Build();
                }
            }

            public TestDbContext(DbContextOptions<TestDbContext> opt, bool enableFilter) : base(opt) { _enableFilter = enableFilter; }
        }

        // Testing purposes Entity
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

        // Testing purposes mock-up data
        private static void InitializeData(TestDbContext context)
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
