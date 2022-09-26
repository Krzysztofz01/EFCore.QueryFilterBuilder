using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace EFCore.QueryFilterBuilder.Tests
{
    public class QueryFilterBuilderTests
    {
        [Fact]
        public void CanQueryAllElementsOnQueryFilterIgnore()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"), new InMemoryDatabaseRoot()).Options;

            var externalServiceSimulatedOutput = true;

            var dbContext = new TestDbContext(options, externalServiceSimulatedOutput);

            ProvideTestData(dbContext);

            int expectedCount = 3;

            int actualCount = dbContext.Blogs.IgnoreQueryFilters().Count();

            Assert.Equal(expectedCount, actualCount);

            dbContext.Dispose();
        }

        [Fact]
        public void CanQueryWithAllFiltersActive()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"), new InMemoryDatabaseRoot()).Options;

            var externalServiceSimulatedOutput = true;

            var dbContext = new TestDbContext(options, externalServiceSimulatedOutput);

            ProvideTestData(dbContext);

            int expectedCount = 1;

            int actualCount = dbContext.Blogs.Count();

            Assert.Equal(expectedCount, actualCount);

            dbContext.Dispose();
        }

        [Fact]
        public void CanQueryWithSomeFiltersActive()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"), new InMemoryDatabaseRoot()).Options;

            var externalServiceSimulatedOutput = false;

            var dbContext = new TestDbContext(options, externalServiceSimulatedOutput);

            ProvideTestData(dbContext);

            int expectedCount = 2;

            int actualCount = dbContext.Blogs.Count();

            Assert.Equal(expectedCount, actualCount);

            dbContext.Dispose();
        }

        // Testing purposes DbContext
        private class TestDbContext : DbContext
        {
            private readonly bool _status;

            public virtual DbSet<Blog> Blogs { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>()
                    .HasQueryFilters()
                    .AddFilter(d => d.Name == "Hello World")
                    .AddFilter(d => d.Posts == 20, _status)
                    .Build();
            }

            public TestDbContext(DbContextOptions<TestDbContext> opt, bool status) : base(opt) { _status = status; }
        }

        // Testing purposes Entity
        public class Blog
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
        private static void ProvideTestData(TestDbContext dbContext)
        {
            dbContext.Blogs.AddRange(new Blog[]
            {
                new Blog { Name = "Hello World", About = "Description", Posts = 20 },
                new Blog { Name = "Hello World", About = "About blog", Posts = 10 },
                new Blog { Name = "EFCore", About = "Description", Posts = 30 }
            });

            dbContext.SaveChanges();
        }
    }
}
