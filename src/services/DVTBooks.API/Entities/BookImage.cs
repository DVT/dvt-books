namespace DVTBooks.API.Entities
{
    public class BookImage
    {
        public string BookId { get; set; }
        public int ImageId { get; set; }

        public virtual Book Book { get; set; }
        public virtual Blob Image { get; set; }
    }
}
