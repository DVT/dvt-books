using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVTBooks.API.Entities.Configurations
{
    public class BlobEntityTypeConfiguration : IEntityTypeConfiguration<Blob>
    {
        public void Configure(EntityTypeBuilder<Blob> builder)
        {
            builder.ToTable("Blob").HasKey(x => x.Id).HasName("PK_Blob");

            builder.HasAlternateKey(x => x.Guid);

            builder.Property(x => x.Id).UseIdentityColumn();

            builder.Property(x => x.Guid).IsRequired();

            builder.Property(x => x.ContentType)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Content)
                .IsRequired();
        }
    }
}
