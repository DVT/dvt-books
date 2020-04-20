namespace DVTBooks.API.Models
{
    /// <summary>
    /// Represents a book tag.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// Gets or sets the identifier of the tag, if any.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the hypermedia reference of the tag, if any.
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// Gets or sets the description of the tag, if any.
        /// </summary>
        public string Description { get; set; }
    }
}
