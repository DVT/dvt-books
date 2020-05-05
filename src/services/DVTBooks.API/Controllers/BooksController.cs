using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dvt.Drawing;
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

            string isbn13Digits = model.ISBN13 != null ? Regex.Replace(model.ISBN13, @"[^\d]", string.Empty, RegexOptions.None) : null;
            string isbn10Digits = model.ISBN10 != null ? Regex.Replace(model.ISBN10, @"[^\d]", string.Empty, RegexOptions.None) : null;

            var entity = new Entities.Book
            {
                ISBN13 = isbn13Digits,
                ISBN10 = isbn10Digits
            };

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
                Id = isbn13Digits,
                Title = model.Title
            };

            return CreatedAtAction(nameof(Get), result);
        }

        /// <summary>
        /// Creates or updates an existing book.
        /// </summary>
        /// <param name="isbn13">The International Standard Book Number (ISBN).</param>
        /// <param name="model">The book.</param>
        /// <returns>An action result.</returns>
        [HttpPut("{isbn13}")]
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
        /// <param name="isbn">The International Standard Book Number (ISBN).</param>
        /// <param name="patch">The RFC 6902 JSON patch document.</param>
        /// <returns>An action result.</returns>
        [HttpPatch("{isbn}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(AuthorRef), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [Consumes("application/json-patch+json")]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string isbn, [FromBody]JsonPatchDocument<Book> patch)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Book model = await Query().FirstOrDefaultAsync(x => x.ISBN13 == isbn);

            if (model == null)
            {
                model = new Book
                {
                    ISBN13 = isbn
                };

                patch.ContractResolver = new JsonLowerCaseUnderscoreContractResolver();
                patch.ApplyTo(model, ModelState);

                return await Post(model);
            }

            patch.ContractResolver = new JsonLowerCaseUnderscoreContractResolver();
            patch.ApplyTo(model, ModelState);

            return await Put(isbn, model);
        }

        /// <summary>
        /// Returns the pictur of a book, if any.
        /// </summary>
        /// <param name="isbn">The International Standard Book Number (ISBN).</param>
        /// <returns>An action result.</returns>
        [HttpGet("{isbn}/picture")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [Produces("image/bmp", "image/gif", "image/jpeg", "image/png")]
        public async Task<IActionResult> GetPicture(string isbn)
        {
            var image = await _db.Books
                .Include(x => x.BookImage)
                .ThenInclude(x => x.Image)
                .Where(x => x.ISBN10 == isbn || x.ISBN13 == isbn)
                .Select(x => x.BookImage.Image)
                .FirstOrDefaultAsync();

            if (image == null)
                return NotFound();

            var stream = new MemoryStream(image.Content);
            return new FileStreamResult(stream, image.ContentType);
        }

        /// <summary>
        /// Returns the picture of the book, if any.
        /// </summary>
        /// <param name="isbn">The Internation Standard Book Number.</param>
        /// <param name="_"></param>
        /// <returns>An action result.</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("{isbn}/{_}.picture")]
        [ResponseCache(CacheProfileName = "Static")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [Produces("image/bmp","image/gif", "image/jpeg", "image/png")]
        public async Task<IActionResult> GetPictureCached(string isbn, string _ = null)
        {
            return await GetPicture(isbn);
        }

        /// <summary>
        /// Updates the book picture.
        /// </summary>
        /// <param name="isbn">The International Standard Book Number.</param>
        /// <returns>An action result.</returns>
        [HttpPut("{isbn}/picture")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.UnsupportedMediaType)]
        [Consumes("image/bmp", "image/gif", "image/jpeg", "image/png")]
        public async Task<IActionResult> PutPicture(string isbn)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!new[] { "image/bmp", "image/gif", "image/jpeg", "image/png" }.Contains(Request.ContentType))
                return new UnsupportedMediaTypeResult();

            Entities.Book book = await _db.Books
                 .Include(x => x.BookImage)
                 .ThenInclude(x => x.Image)
                 .FirstOrDefaultAsync(x => x.ISBN10 == isbn || x.ISBN13 == isbn);

            if (book == null)
                return NotFound();

            byte[] content;

            using (var resized = new MemoryStream())
            {
                ImageHelper.Resize(Request.Body, resized, 196, 196, 75);
                content = resized.ToArray();
            }

            var entity = new Entities.Blob
            {
                Guid = Guid.NewGuid(),
                ContentType = Request.ContentType,
                Content = content
            };

            if (book.BookImage == null)
            {
                book.BookImage = new Entities.BookImage
                {
                    Image = entity
                };
            }
            else
            {
                book.BookImage.Image = entity;
            }

            await _db.SaveChangesAsync();

            return NoContent();
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

                    var tagId = (await _db.Tags.FirstOrDefaultAsync(x => x.Description == description))?.Id;

                    if (tagId != null)
                    {
                        if (!bookTags.Any(x => x.TagId == tagId))
                        {
                            bookTags.Add(new Entities.BookTag
                            {
                                TagId = tagId.Value
                            });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("tags", "The value is invalid");
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
                    var tag = _db.Tags.First(x => x.Id == bookTag.TagId);

                    if (model.Tags != null && !model.Tags.Any(x => string.Equals(tag.Description, x.Href.Substring(x.Href.LastIndexOf("/") + 1), StringComparison.CurrentCultureIgnoreCase)))
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

            entity.Title = model.Title;
            entity.About = model.About;
            entity.Abstract = model.Abstract;
            entity.Publisher = model.Publisher;
            entity.DatePublished = model.DatePublished;
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
                       About = book.About,
                       Abstract = book.Abstract,
                       Author = new AuthorRef
                       {
                           Id = book.Author.Guid,
                           Href = $"{_configuration["BooksApiUri"]}/Authors/{book.Author.Guid}",
                           Name = book.Author.Name
                       },
                       Publisher = book.Publisher,
                       DatePublished = book.DatePublished,
                       Image = book.BookImage != null ? $"{_configuration["BooksApiUri"]}/Books/{book.ISBN13}/{BitConverter.ToUInt64(book.BookImage.Image.Guid.ToByteArray(), 0)}.picture" : null,
                       Tags = (from bookTag in book.Tags
                               select new Tag
                               {
                                   Id = bookTag.Tag.Description,
                                   Href = $"{_configuration["BooksApiUri"]}/Tags/{bookTag.Tag.Description}",
                                   Description = bookTag.Tag.Description
                               }).ToList()
                   };
        }
    }
}
