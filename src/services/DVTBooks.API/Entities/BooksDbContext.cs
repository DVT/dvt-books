using DVTBooks.API.Entities.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DVTBooks.API.Entities
{
    public class BooksDbContext : DbContext
    {

        public BooksDbContext(DbContextOptions<BooksDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<BookTag> BookTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("books");
            modelBuilder.ApplyConfiguration(new TagEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new BlobEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new AuthorEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new BookEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new BookImageEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new BookTagEntityTypeConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
