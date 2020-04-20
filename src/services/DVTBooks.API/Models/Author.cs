using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DVTBooks.API.Models
{
    /// <summary>
    /// Represents an author.
    /// </summary>
    public class Author
    {
        /// <summary>
        /// Gets or sets the hypermedia reference of the author, if any.
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// Gets or sets the global unique identifier (GUID) of the author, if any.
        /// </summary>, if any.
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the first name of the author, if any.
        /// </summary>
        [Required]
        [StringLength(255)]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the middle names of the author, if any.
        /// </summary>
        [StringLength(255)]
        public string MiddleNames { get; set; }

        /// <summary>
        /// Gets or sets the last name of the author, if any.
        /// </summary>
        [Required]
        [StringLength(255)]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the name of the author, if any.
        /// </summary>
        [StringLength(255)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a mini description of the author, if any.
        /// </summary>
        public string About { get; set; }

        /// <summary>
        /// Gets or sets the version used for optimistic concurrency checking.
        /// </summary>
        [Timestamp]
        public byte[] Version { get; set; }

        /// <summary>
        /// Gets or sets a collection of book references written by the author, if any.
        /// </summary>
        public ICollection<BookRef> Books { get; set; }
    }
}
