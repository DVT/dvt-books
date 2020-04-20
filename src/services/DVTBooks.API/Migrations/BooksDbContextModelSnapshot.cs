﻿// <auto-generated />
using System;
using DVTBooks.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DVTBooks.API.Migrations
{
    [DbContext(typeof(BooksDbContext))]
    partial class BooksDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("books")
                .HasAnnotation("ProductVersion", "3.1.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DVTBooks.API.Entities.Author", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:IdentityIncrement", 1)
                        .HasAnnotation("SqlServer:IdentitySeed", 1)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("About")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<Guid>("Guid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<string>("MiddleNames")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<byte[]>("Version")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id")
                        .HasName("PK_Author");

                    b.HasAlternateKey("Guid");

                    b.ToTable("Author");
                });

            modelBuilder.Entity("DVTBooks.API.Entities.Blob", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:IdentityIncrement", 1)
                        .HasAnnotation("SqlServer:IdentitySeed", 1)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<byte[]>("Content")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<Guid>("Guid")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id")
                        .HasName("PK_Blob");

                    b.HasAlternateKey("Guid");

                    b.ToTable("Blob");
                });

            modelBuilder.Entity("DVTBooks.API.Entities.Book", b =>
                {
                    b.Property<string>("ISBN13")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<string>("About")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Abstract")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<int>("AuthorId")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("DatePublished")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("ISBN10")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<int?>("ImageId")
                        .HasColumnType("int");

                    b.Property<string>("Publisher")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<byte[]>("Version")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("ISBN13")
                        .HasName("PK_Book");

                    b.HasIndex("AuthorId");

                    b.HasIndex("ISBN10")
                        .HasName("IX_ISBN10");

                    b.ToTable("Book");
                });

            modelBuilder.Entity("DVTBooks.API.Entities.BookImage", b =>
                {
                    b.Property<string>("BookId")
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("ImageId")
                        .HasColumnType("int");

                    b.HasKey("BookId", "ImageId")
                        .HasName("PK_BookImage");

                    b.HasIndex("BookId")
                        .IsUnique();

                    b.HasIndex("ImageId")
                        .IsUnique();

                    b.ToTable("BookImage");
                });

            modelBuilder.Entity("DVTBooks.API.Entities.BookTag", b =>
                {
                    b.Property<int>("TagId")
                        .HasColumnType("int");

                    b.Property<string>("BookId")
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("TagId", "BookId")
                        .HasName("PK_BookTag");

                    b.HasIndex("BookId");

                    b.ToTable("BookTag");
                });

            modelBuilder.Entity("DVTBooks.API.Entities.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:IdentityIncrement", 1)
                        .HasAnnotation("SqlServer:IdentitySeed", 1)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.HasKey("Id")
                        .HasName("PK_Tag");

                    b.HasAlternateKey("Description")
                        .HasName("AK_Tag");

                    b.ToTable("Tag");
                });

            modelBuilder.Entity("DVTBooks.API.Entities.Book", b =>
                {
                    b.HasOne("DVTBooks.API.Entities.Author", "Author")
                        .WithMany("Books")
                        .HasForeignKey("AuthorId")
                        .HasConstraintName("FK_Book_Author")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DVTBooks.API.Entities.BookImage", b =>
                {
                    b.HasOne("DVTBooks.API.Entities.Book", "Book")
                        .WithOne("Image")
                        .HasForeignKey("DVTBooks.API.Entities.BookImage", "BookId")
                        .HasConstraintName("FK_Book_BookImage")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DVTBooks.API.Entities.Blob", "Image")
                        .WithOne()
                        .HasForeignKey("DVTBooks.API.Entities.BookImage", "ImageId")
                        .HasConstraintName("FK_BookImage_Image")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DVTBooks.API.Entities.BookTag", b =>
                {
                    b.HasOne("DVTBooks.API.Entities.Book", "Book")
                        .WithMany("Tags")
                        .HasForeignKey("BookId")
                        .HasConstraintName("FK_Book_BookTag")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DVTBooks.API.Entities.Tag", "Tag")
                        .WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("FK_BookTag_Tag")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
