using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DVTBooks.API.Models;
using DVTBooks.API.Serialization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DVTBooks.API.Controllers
{
    /// <summary>
    /// Represents book authors.
    /// </summary>
    [Route("[controller]")]
    public class AuthorsController : Controller
    {
        private readonly Entities.BooksDbContext _db;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes the controller
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="configuration">The application configuration.</param>
        public AuthorsController(Entities.BooksDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        /// <summary>
        /// Returns a specific author.
        /// </summary>
        /// <param name="id">The global unique identifier (GUID) of the author.</param>
        /// <returns>An action result.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Author), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> Get(Guid id)
        {
            Author model = await Query().FirstOrDefaultAsync(x => x.Id == id);

            if (model == null)
                return NotFound();

            return Ok(model);
        }

        /// <summary>
        /// Gets a collection of authors.
        /// </summary>
        /// <param name="query">The text to match in the name of the author.</param>
        /// <param name="skip">The number of authors to skip for paging.</param>
        /// <param name="top">The number of artist to skip for paging.</param>
        /// <returns>A collection of authors.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ICollection<Author>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<ICollection<Author>> Get([FromQuery] string query, [FromQuery] int? skip = null, [FromQuery]int? top = null)
        {
            var model = Query(query);

            if (skip != null)
            {
                model = model.Skip(skip.Value);
            }

            if (top != null)
            {
                model = model.Take(top.Value);
            }

            return await model.ToListAsync();
        }

        /// <summary>
        /// Creates a new author.
        /// </summary>
        /// <param name="model">The author.</param>
        /// <returns>An action result.</returns>
        [ProducesResponseType(typeof(IDictionary<string, string[]>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(Author), (int)HttpStatusCode.Created)]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<IActionResult> Post([FromBody]Author model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
            }

            var entity = new Entities.Author();

            _db.Authors.Add(entity);

             MapAuthor(model, entity);

            await _db.SaveChangesAsync();

            string booksApiUri = _configuration["BooksApiUri"];

            var result = new AuthorRef
            {
                Href = $"{_configuration["BooksApiUri"]}/Authors/{model.Id}",
                Id = model.Id,
                Name = model.Name
            };

            return CreatedAtAction(nameof(Get), result);
        }

        /// <summary>
        /// Updates an existing author or creates a new author.
        /// </summary>
        /// <param name="id">The global unique identifier (GUID) of the person.</param>
        /// <param name="model">The person.</param>
        /// <returns>An action result.</returns>
        [ProducesResponseType(typeof(IDictionary<string, string[]>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(Author), (int)HttpStatusCode.Created)]
        [ProducesResponseType(((int)HttpStatusCode.NoContent))]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [Produces("application/json")]
        [Consumes("application/json")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody]Author model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Id == Guid.Empty)
            {
                model.Id = id;
            }
            else if (model.Id != id)
            {
                ModelState.AddModelError("id", "The valei invalid");
                return BadRequest(ModelState);
            }

            Entities.Author author = await _db.Authors
                    .FirstOrDefaultAsync(x => x.Guid == id);

            if (author == null)
                return await Post(model);

            MapAuthor(model, author);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode((int)HttpStatusCode.Conflict);
            }

            return NoContent();

        }

        /// <summary>
        /// Partially updates and existing author or creates a new author.
        /// </summary>
        /// <param name="id">The global unique identifier (GUID) of the author.</param>
        /// <param name="patch">The RFC 6902 JSON patch document.</param>
        /// <returns>An action result.</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(AuthorRef), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [Consumes("application/json-patch+json")]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(Guid id, [FromBody]JsonPatchDocument<Author> patch)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Author model = await Query().FirstOrDefaultAsync(x => x.Id == id);

            if (model == null)
            {
                model = new Author
                {
                    Id = id
                };

                patch.ContractResolver = new JsonLowerCaseUnderscoreContractResolver();
                patch.ApplyTo(model, ModelState);

                return await Post(model);
            }

            patch.ContractResolver = new JsonLowerCaseUnderscoreContractResolver();
            patch.ApplyTo(model, ModelState);

            return await Put(id, model);
        }

        private void MapAuthor(Author model, Entities.Author entity)
        {
            string fullName = $"{model.FirstName ?? " "} {model.MiddleNames ?? string.Empty} {model.LastName ?? string.Empty}";

            entity.Guid = model.Id;
            entity.FirstName = model.FirstName;
            entity.MiddleNames = model.MiddleNames;
            entity.LastName = model.LastName;
            entity.Name = fullName.Trim();
            entity.About = model.About;

            if (model.Version != null)
            {
                _db.Entry(entity).Property(x => x.Version).OriginalValue = model.Version;
                _db.Entry(entity).State = EntityState.Modified;
            }
            
        }

        private IQueryable<Author> Query(string query = null)
        {
            string startsWithPattern, containsPattern;

            if (query != null)
            {
                startsWithPattern = query.Trim();
                containsPattern = string.Concat(' ', startsWithPattern);
            }
            else
            {
                startsWithPattern =
                    containsPattern = null;
            }

            return from author in _db.Authors
                   let originalFullName = (author.FirstName + " ") + (author.LastName)
                   where startsWithPattern == null
                       || author.Name.StartsWith(startsWithPattern) || originalFullName.StartsWith(startsWithPattern)
                       || author.Name.Contains(containsPattern) || author.Name.StartsWith(containsPattern)
                   orderby
                       author.Name.IndexOf(containsPattern) < originalFullName.IndexOf(containsPattern)
                       ? author.Name.IndexOf(containsPattern)
                       : originalFullName.IndexOf(containsPattern),
                       author.Name
                   select new Author
                   {
                       Id = author.Guid,
                       Href = $"{_configuration["BooksApiUri"]}/Authors/{author.Guid}",
                       FirstName = author.FirstName,
                       MiddleNames = author.MiddleNames,
                       LastName = author.LastName,
                       Name = author.Name,
                       About = author.About,
                       Version = author.Version,
                       Books = (from book in author.Books
                                select new BookRef
                                {
                                    Href = $"{_configuration["BooksApiUri"]}/Books/{book.ISBN13}",
                                    Id = book.ISBN13,
                                    ISBN10 = book.ISBN10,
                                    ISBN13 = book.ISBN13
                                }).ToList()
                   };
        }
    }
}
