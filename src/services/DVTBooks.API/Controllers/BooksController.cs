using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DVTBooks.API.Models;
using DVTBooks.API.Serialization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DVTBooks.API.Controllers
{
    /// <summary>
    /// Represents books.
    /// </summary>
    [Route("[controller]")]
    public class BooksController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly Entities.BooksDbContext _db;

        /// <summary>
        /// Initializes the controller.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="configuration">The application configuration.</param>
        public BooksController(Entities.BooksDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        /// <summary>
        /// Gets a specific book.
        /// </summary>
        /// <param name="isbn">The International Standard Book Number (ISBN).</param>
        /// <returns>An action result.</returns>
        [HttpGet("{isbn}")]
        [ProducesResponseType(typeof(Book), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> Get(string isbn)
        {
            var isbnNumber = Regex.Replace(isbn, @"[^\d]", string.Empty, RegexOptions.None);
            Book model = await Query().FirstOrDefaultAsync(x => x.ISBN10 == isbnNumber || x.ISBN13 == isbnNumber);

            if (model == null)
                return NotFound();

            return Ok(model);
        }

        /// <summary>
        /// Gets a collection of books.
        /// </summary>
        /// <param name="query">The text to match in the title of the book.</param>
        /// <param name="skip">The number of books to skip for paging.</param>
        /// <param name="top">The number of books to return for paging.</param>
        /// <returns>A collection of books.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ICollection<Author>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<ICollection<Book>> Get([FromQuery] string query, [FromQuery] int? skip = null, [FromQuery] int? top = null)
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
        /// Creates a new book
        /// </summary>
        /// <param name="model">The book.</param>
        /// <returns>An action result.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(BookRef), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), (int)HttpStatusCode.BadRequest)]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody] Book model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = new Entities.Book();

            await _db.Books.AddAsync(entity);

            await MapBookAsync(model, entity);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _db.SaveChangesAsync();

            }
            catch (DbUpdateException exception)
                when ((exception.InnerException as SqlException)?.Number == 2601/*Unique index violation*/ &&
                (exception.InnerException as SqlException).Message.IndexOf("duplicate", StringComparison.OrdinalIgnoreCase) != -1)
            {
                if ((exception.InnerException as SqlException).Message.IndexOf("isbn13", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    ModelState.AddModelError("isbn13", "The value must be unique.");
                    return BadRequest(ModelState);
                }

                if ((exception.InnerException as SqlException).Message.IndexOf("isbn10", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    ModelState.AddModelError("isbn10", "The value must be unique.");
                    return BadRequest(ModelState);
                }
            }

            var result = new BookRef
            {
                Href = $"{_configuration["BooksApiUri"]}/Books/{model.ISBN13}",
                Id = model.ISBN13,
                Title = model.Title
            };

            return CreatedAtAction(nameof(Get), result);
        }

        /// <summary>
        /// Creates or updates an existing book.
        /// </summary>
        /// <param name="id">The global unique identifier (GUID) of the book.</param>
        /// <param name="model">The book.</param>
        /// <returns>An action result.</returns>
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(BookRef), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> Put(string isbn13, [FromBody] Book model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.ISBN13 == null)
            {
                model.ISBN13 = isbn13; 
            }
            else if (model.ISBN13 != isbn13)
            {
                ModelState.AddModelError("isbn13", "The value is invalid");
                return BadRequest(ModelState);
            }

            string isbnDigits = model.ISBN13 != null ? Regex.Replace(model.ISBN13, @"[^\d]", string.Empty, RegexOptions.None) : null;

            Entities.Book book = await _db.Books
                .Include(x => x.Author)
                .Include(x => x.BookImage)
                .Include(x => x.BookImage.Image)
                .Include(x => x.Tags)
                .ThenInclude(x => x.Tag)
                .FirstOrDefaultAsync(x => x.ISBN13 == isbnDigits);

            if (book == null)
                return await Post(model);

            await MapBookAsync(model, book);

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
        /// Partially updates and existing author or creates a new book.
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
        public async Task<IActionResult> Patch(string isbn13, [FromBody]JsonPatchDocument<Book> patch)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Book model = await Query().FirstOrDefaultAsync(x => x.ISBN13 == isbn13);

            if (model == null)
            {
                model = new Book
                {
                    ISBN13 = isbn13
                };

                patch.ContractResolver = new JsonLowerCaseUnderscoreContractResolver();
                patch.ApplyTo(model, ModelState);

                return await Post(model);
            }

            patch.ContractResolver = new JsonLowerCaseUnderscoreContractResolver();
            patch.ApplyTo(model, ModelState);

            return await Post(model);
        }

        private async Task MapBookAsync(Book model, Entities.Book entity)
        {
            if (entity.Author == null)
            {
                entity.Author = new Entities.Author();
            }

            if (entity.ISBN13 == null)
            {
                ModelState.AddModelError("ISBN13", "The value is invalid.");
            }

            if (entity.Tags == null)
            {
                entity.Tags = new List<Entities.BookTag>();
            }

            int? authorId = null;

            if (!string.IsNullOrEmpty(model.Author?.Href))
            {
                string authorGuidString = WebUtility.UrlDecode(model.Author.Href.Substring(model.Author.Href.LastIndexOf("/") + 1));

                if (Guid.TryParse(authorGuidString, out Guid authorGuid))
                {
                    authorId = (await _db.Authors.FirstOrDefaultAsync(x => x.Guid == authorGuid))?.Id;

                    if (authorId == null)
                    {
                        ModelState.AddModelError("author.href", "The value is invalid");
                    }
                }
                else
                {
                    ModelState.AddModelError("author.href", "The value is invalid");
                }
            }

            if (authorId.HasValue)
            {
                entity.AuthorId = authorId.Value;
            }

            ICollection<Entities.BookTag> bookTags = entity.Tags;

            if (model.Tags != null)
            {
                foreach(var tag in model.Tags)
                {
                    string description = tag.Href.Substring(tag.Href.LastIndexOf("/") + 1);

                    if(!bookTags.Any(x => string.Equals(x.Tag.Description, description, StringComparison.OrdinalIgnoreCase)))
                    {
                        bookTags.Add(new Entities.BookTag
                        {
                            Tag = new Entities.Tag
                            {
                                Description = description
                            }
                        });
                    }
                }
            }

            if (model.Tags == null)
            {
                bookTags = null;
            }
            else
            {
                foreach (var bookTag in bookTags)
                {
                    if (model.Tags != null && !model.Tags.Any(x => string.Equals(bookTag.Tag.Description, x.Href.Substring(x.Href.LastIndexOf("/") + 1), StringComparison.CurrentCultureIgnoreCase)))
                    {
                        bookTags.Remove(bookTag);
                    }
                }
            }

            if (model.Version != null)
            {
                _db.Entry(entity).Property(x => x.Version).OriginalValue = model.Version;
                _db.Entry(entity).State = EntityState.Modified;
            }

            string isbn10Digits = model.ISBN10 != null ? Regex.Replace(model.ISBN10, @"[^\d]", string.Empty, RegexOptions.None) : null;
            string isbn13Digits = model.ISBN13 != null ? Regex.Replace(model.ISBN13, @"[^\d]", string.Empty, RegexOptions.None) : null;

            entity.ISBN10 = isbn10Digits;
            entity.ISBN13 = isbn13Digits;
            entity.Title = model.Title;
            entity.About = model.About;
            entity.Publisher = model.Publisher;
            entity.DatePublished = model.PublishedDate;
            entity.Tags = bookTags;
        }

        private IQueryable<Book> Query(string query = null)
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

            return from book in _db.Books
                   where startsWithPattern == null
                        || book.Title.StartsWith(startsWithPattern) || book.Title.Contains(containsPattern)
                   orderby
                        book.Title.IndexOf(containsPattern), book.Title
                   select new Book
                   {
                       ISBN10 = book.ISBN10,
                       ISBN13 = book.ISBN13,
                       Title = book.Title,
                       About = book.Title,
                       Author = new AuthorRef
                       {
                           Id = book.Author.Guid,
                           Href = $"{_configuration["BooksApiUri"]}/Authors/{book.Author.Guid}",
                           Name = book.Author.Name
                       },
                       Image = book.ImageId != null
                            ? $"{_configuration["BooksApiUri"]}/Books/{book.ISBN13}/${BitConverter.ToUInt64(book.BookImage.Image.Guid.ToByteArray(), 0)}.picture"
                            : null,
                       Tags = (from tag in _db.Tags
                               select new Tag
                               {
                                   Id = tag.Description,
                                   Href = $"{_configuration["BooksApiUri"]}/Tags/{tag.Description}",
                                   Description = tag.Description
                               }).ToList()
                   };
        }
    }
}
