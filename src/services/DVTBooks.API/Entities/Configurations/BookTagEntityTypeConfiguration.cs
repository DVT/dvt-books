using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVTBooks.API.Entities.Configurations
{
    public class BookTagEntityTypeConfiguration : IEntityTypeConfiguration<BookTag>
    {
        public void Configure(EntityTypeBuilder<BookTag> builder)
        {
            builder.ToTable("BookTag").HasKey(x => new { x.TagId, x.BookId }).HasName("PK_BookTag");

            builder.Property(x => x.BookId).IsRequired();
            builder.Property(x => x.TagId).IsRequired();

            builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId).HasConstraintName("FK_BookTag_Tag");
        }
    }
}
