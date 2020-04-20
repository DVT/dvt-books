using System;
using System.ComponentModel.DataAnnotations;
using DVTBooks.API.Libs.ComponentModel.DataAnnotations;

namespace DVTBooks.API.Models
{
    /// <summary>
    /// Represents a reference to an author.
    /// </summary>
    public class AuthorRef
    {
        [RelativeOrAbsoluteUrl]
        [Required]
        public string Href { get; set; }

        /// <summary>
        /// Gets or sets the global unique identifier (GUID) of the author, if any.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the author, if any.
        /// </summary>
        [StringLength(255)]
        public string Name { get; set; }
    }
}
