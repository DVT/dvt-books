using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DVTBooks.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DVTBooks.API.Controllers
{
    /// <summary>
    /// Represents book tags.
    /// </summary>
    [Route("[controller]")]
    public class TagsController : Controller
    {
        private readonly Entities.BooksDbContext _db;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes the controller.
        /// </summary>
        public TagsController(Entities.BooksDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        /// <summary>
        /// Returns book tags
        /// </summary>
        /// <returns>A collection of book tags.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ICollection<Tag>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<ICollection<Tag>> Get()
        {
            return await Query().OrderBy(x => x.Description).ToListAsync();
        }

        /// <summary>
        /// Returns a specific book tag.
        /// </summary>
        /// <param name="name">The name of the tag.</param>
        /// <returns>An action result</returns>
        [HttpGet("{name}")]
        [ProducesResponseType(typeof(Tag), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> Get(string name)
        {
            Tag model = await Query(name).FirstOrDefaultAsync();

            if (model == null)
                return NotFound();

            return Ok(model);
        }

        private IQueryable<Tag> Query(string name = null)
        {
            string startsWithPattern, containsPattern;

            if (name != null)
            {
                startsWithPattern = name.Trim();
                containsPattern = string.Concat(' ', startsWithPattern);
            }
            else
            {
                startsWithPattern =
                    containsPattern = null;
            }

            return from tag in _db.Tags
                   where startsWithPattern == null
                        || tag.Description.StartsWith(startsWithPattern) || tag.Description.Contains(containsPattern)
                    orderby tag.Description, tag.Description.IndexOf(containsPattern)
                   select new Tag
                   {
                       Id = tag.Description,
                       Description = tag.Description,
                       Href = $"{_configuration["BookApiUri"]}/Tags/{tag.Description}"
                   };
        }
    }
}
