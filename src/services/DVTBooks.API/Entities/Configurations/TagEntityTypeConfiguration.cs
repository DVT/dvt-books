using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVTBooks.API.Entities.Configurations
{
    public class TagEntityTypeConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("Tag").HasKey(x => x.Id).HasName("PK_Tag");

            builder.HasAlternateKey(x => x.Description).HasName("AK_Tag");

            builder.Property(x => x.Id)
                .IsRequired()
                .UseIdentityColumn();

            builder.Property(x => x.Description)
                .HasMaxLength(255)
                .IsRequired();
        }
    }
}
