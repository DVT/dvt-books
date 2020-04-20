using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DVTBooks.API.Libs.ComponentModel.DataAnnotations;

namespace DVTBooks.API.Models
{
    /// <summary>
    /// Represents a book.
    /// </summary>
    public class Book
    {
        /// <summary>
        /// Gets or sets the ISBN-10 number of the book, if any.
        /// </summary>
        [Isbn10]
        [StringLength(255)]
        public string ISBN10 { get; set; }

        /// <summary>
        /// Gets or sets the ISBN-13 number of the book.
        /// </summary>
        [Required]
        [Isbn13]
        [StringLength(255)]
        public string ISBN13 { get; set; }

        /// <summary>
        /// Gets or sets the title of the book, if any.
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of what the book is about, if any.
        /// </summary>
        public string About { get; set; }

        /// <summary>
        /// Gets or sets a hypermedia reference to the author, if any.
        /// </summary>
        public AuthorRef Author { get; set; }

        /// <summary>
        /// Gets or sets the image of the book, if any.
        /// </summary>
        [RelativeOrAbsoluteUrl]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets a collection of tags associated with the book.
        /// </summary>
        public ICollection<Tag> Tags { get; set; }

    }
}
