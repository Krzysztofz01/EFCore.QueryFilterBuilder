using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
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

        private void ProvideTestData(TestDbContext dbContext)
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
