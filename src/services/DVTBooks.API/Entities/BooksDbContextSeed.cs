using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DVTBooks.API.Entities
{
    public class BooksDbContextSeed
    {
        public async Task SeedAsync(BooksDbContext context)
        {
            await SeedTagsAsync(context);
        }

        private async Task SeedTagsAsync(BooksDbContext context)
        {
            if (!await context.Tags.AnyAsync())
            {
                await context.Tags.AddAsync(new Tag { Description = "C#" });
                await context.Tags.AddAsync(new Tag { Description = "Java" });
                await context.Tags.AddAsync(new Tag { Description = "HTML" });
                await context.Tags.AddAsync(new Tag { Description = "Kotlin" });
                await context.Tags.AddAsync(new Tag { Description = "iOS" });
                await context.Tags.AddAsync(new Tag { Description = "UI/UX" });
                await context.Tags.AddAsync(new Tag { Description = "Design" });
                await context.Tags.AddAsync(new Tag { Description = "Apple" });
                await context.Tags.AddAsync(new Tag { Description = "Linux" });
                await context.Tags.AddAsync(new Tag { Description = "React" });
                await context.Tags.AddAsync(new Tag { Description = "Angular" });
                await context.Tags.AddAsync(new Tag { Description = "Redux" });
                await context.Tags.AddAsync(new Tag { Description = "Microsoft" });

                await context.SaveChangesAsync();
            }
        }
    }
}
