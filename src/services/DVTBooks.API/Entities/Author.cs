using System;
using System.Collections.Generic;

namespace DVTBooks.API.Entities
{
    public class Author
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string FirstName { get; set; }
        public string MiddleNames { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public byte[] Version { get; set; }
        public ICollection<Book> Books { get; set; }
    }
}
