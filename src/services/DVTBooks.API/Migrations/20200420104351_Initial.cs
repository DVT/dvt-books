using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DVTBooks.API.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "books");

            migrationBuilder.CreateTable(
                name: "Author",
                schema: "books",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<Guid>(nullable: false),
                    FirstName = table.Column<string>(maxLength: 255, nullable: false),
                    MiddleNames = table.Column<string>(maxLength: 255, nullable: true),
                    LastName = table.Column<string>(maxLength: 255, nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    About = table.Column<string>(nullable: true),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Author", x => x.Id);
                    table.UniqueConstraint("AK_Author_Guid", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "Blob",
                schema: "books",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<Guid>(nullable: false),
                    ContentType = table.Column<string>(maxLength: 255, nullable: false),
                    Content = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blob", x => x.Id);
                    table.UniqueConstraint("AK_Blob_Guid", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                schema: "books",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
                    table.UniqueConstraint("AK_Tag", x => x.Description);
                });

            migrationBuilder.CreateTable(
                name: "Book",
                schema: "books",
                columns: table => new
                {
                    ISBN13 = table.Column<string>(maxLength: 255, nullable: false),
                    ISBN10 = table.Column<string>(maxLength: 255, nullable: true),
                    Title = table.Column<string>(maxLength: 255, nullable: false),
                    About = table.Column<string>(nullable: true),
                    Abstract = table.Column<string>(maxLength: 255, nullable: true),
                    ImageId = table.Column<int>(nullable: true),
                    AuthorId = table.Column<int>(nullable: false),
                    DatePublished = table.Column<DateTimeOffset>(nullable: true),
                    Publisher = table.Column<string>(maxLength: 255, nullable: true),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Book", x => x.ISBN13);
                    table.ForeignKey(
                        name: "FK_Book_Author",
                        column: x => x.AuthorId,
                        principalSchema: "books",
                        principalTable: "Author",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookImage",
                schema: "books",
                columns: table => new
                {
                    BookId = table.Column<string>(nullable: false),
                    ImageId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookImage", x => new { x.BookId, x.ImageId });
                    table.ForeignKey(
                        name: "FK_Book_BookImage",
                        column: x => x.BookId,
                        principalSchema: "books",
                        principalTable: "Book",
                        principalColumn: "ISBN13",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookImage_Image",
                        column: x => x.ImageId,
                        principalSchema: "books",
                        principalTable: "Blob",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookTag",
                schema: "books",
                columns: table => new
                {
                    BookId = table.Column<string>(nullable: false),
                    TagId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookTag", x => new { x.TagId, x.BookId });
                    table.ForeignKey(
                        name: "FK_Book_BookTag",
                        column: x => x.BookId,
                        principalSchema: "books",
                        principalTable: "Book",
                        principalColumn: "ISBN13",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookTag_Tag",
                        column: x => x.TagId,
                        principalSchema: "books",
                        principalTable: "Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Book_AuthorId",
                schema: "books",
                table: "Book",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ISBN10",
                schema: "books",
                table: "Book",
                column: "ISBN10");

            migrationBuilder.CreateIndex(
                name: "IX_BookImage_BookId",
                schema: "books",
                table: "BookImage",
                column: "BookId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookImage_ImageId",
                schema: "books",
                table: "BookImage",
                column: "ImageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookTag_BookId",
                schema: "books",
                table: "BookTag",
                column: "BookId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookImage",
                schema: "books");

            migrationBuilder.DropTable(
                name: "BookTag",
                schema: "books");

            migrationBuilder.DropTable(
                name: "Blob",
                schema: "books");

            migrationBuilder.DropTable(
                name: "Book",
                schema: "books");

            migrationBuilder.DropTable(
                name: "Tag",
                schema: "books");

            migrationBuilder.DropTable(
                name: "Author",
                schema: "books");
        }
    }
}
