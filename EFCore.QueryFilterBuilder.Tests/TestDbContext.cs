using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace EFCore.QueryFilterBuilder.Tests
{
    public class TestDbContext: DbContext
    {
        private readonly bool _status;

        public virtual DbSet<Blog> Blogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>()
                .HasQueryFilter(QueryFilterBuilder<Blog>
                    .Create()
                    .AddFilter(d => d.Name == "Hello World")
                    .AddFilter(d => d.Posts == 20, _status)
                    .Build());
        }

        public TestDbContext(DbContextOptions<TestDbContext> opt, bool status) : base(opt) { _status = status; }
    }

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
}
