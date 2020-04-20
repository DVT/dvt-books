using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVTBooks.API.Entities.Configurations
{
    public class BookImageEntityTypeConfiguration : IEntityTypeConfiguration<BookImage>
    {
        public void Configure(EntityTypeBuilder<BookImage> builder)
        {
            builder.ToTable("BookImage").HasKey(x => new { x.BookId, x.ImageId }).HasName("PK_BookImage");

            builder.Property(x => x.BookId).IsRequired();
            builder.Property(x => x.ImageId).IsRequired();  


            builder.HasOne(x => x.Image).WithOne().HasForeignKey<BookImage>(x => x.ImageId).HasConstraintName("FK_BookImage_Image");
        }
    }
}
