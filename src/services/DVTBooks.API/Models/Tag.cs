namespace DVTBooks.API.Models
{
    /// <summary>
    /// Represents a book tag.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// Gets or sets the identifier of the tag.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the description of the tag.
        /// </summary>
        public string Description { get; set; }
    }
}
