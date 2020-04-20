using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVTBooks.API.Entities.Configurations
{
    public class BookEntityTypeConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.ToTable("Book").HasKey(x => x.ISBN13).HasName("PK_Book");
            builder.HasIndex(x => x.ISBN10).HasName("IX_ISBN10");

            builder.Property(x => x.ISBN13)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.ISBN10)
                .IsRequired(false)
                .HasMaxLength(255);

            builder.Property(x => x.Title)
                .IsRequired(true)
                .HasMaxLength(255);

            builder.Property(x => x.About)
                .IsRequired(false);

            builder.Property(x => x.Abstract)
                .IsRequired(false)
                .HasMaxLength(255);

            builder.Property(x => x.Publisher)
                .IsRequired(false)
                .HasMaxLength(255);
            
            builder.Property(x => x.DatePublished)
                .IsRequired(false);

            builder.Property(x => x.Version)
                .IsRequired(true)
                .IsRowVersion();

            builder.HasOne(x => x.Author).WithMany(x => x.Books).HasConstraintName("FK_Book_Author");
            builder.HasMany(x => x.Tags).WithOne(x => x.Book).HasConstraintName("FK_Book_BookTag");
            builder.HasOne(x => x.Image).WithOne(x => x.Book).HasForeignKey<BookImage>(x => x.BookId).HasConstraintName("FK_Book_BookImage");
        }
    }
}
