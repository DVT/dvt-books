using System;
using System.Collections.Generic;

namespace DVTBooks.API.Entities
{
    public class Book
    {
        public string ISBN10 { get; set; }
        public string ISBN13 { get; set; }
        public string Title { get; set; }
        public string About { get; set; }
        public string Abstract { get; set; }
        public int? ImageId { get; set; }
        public int AuthorId { get; set; }
        public DateTimeOffset? DatePublished { get; set; }
        public string Publisher { get; set; }
        public byte[] Version { get; set; }
        public virtual Author Author { get; set; }
        public virtual BookImage Image { get; set; }
        public virtual ICollection<BookTag> Tags { get; set; }
    }
}
