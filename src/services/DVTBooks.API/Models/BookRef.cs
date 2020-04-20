using System.ComponentModel.DataAnnotations;
using DVTBooks.API.Libs.ComponentModel.DataAnnotations;

namespace DVTBooks.API.Models
{
    /// <summary>
    /// Represents a reference to a book.
    /// </summary>
    public class BookRef
    {
        /// <summary>
        /// Gets or sets the hypermedia reference to the book.
        /// </summary>
        [Required]
        [RelativeOrAbsoluteUrl]
        public string Href { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the book, if any.
        /// </summary>
        [Key]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the ISBN-10 number of the book, if any.
        /// </summary>
        [Isbn10]
        public string ISBN10 { get; set; }

        /// <summary>
        /// Gets or sets the ISBN-13 number of the book, if any.
        /// </summary>
        [Isbn13]
        public string ISBN13 { get; set; }
    }
}
