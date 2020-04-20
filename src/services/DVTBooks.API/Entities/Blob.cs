using System;

namespace DVTBooks.API.Entities
{
    public class Blob
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
    }
}
