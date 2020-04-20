using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVTBooks.API.Entities.Configurations
{
    public class AuthorEntityTypeConfiguration : IEntityTypeConfiguration<Author>
    {
        public void Configure(EntityTypeBuilder<Author> builder)
        {
            builder.ToTable("Author").HasKey(x => x.Id).HasName("PK_Author");

            builder.HasAlternateKey(x => x.Guid);

            builder.Property(x => x.Id)
                .IsRequired()
                .UseIdentityColumn();

            builder.Property(x => x.Guid).IsRequired();

            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.MiddleNames)
                .IsRequired(false)
                .HasMaxLength(255);

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.About)
                .IsRequired(false);

            builder.Property(x => x.Version)
                .IsRequired()
                .IsRowVersion();
        }
    }
}
