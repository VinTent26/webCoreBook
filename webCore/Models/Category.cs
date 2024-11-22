using System;

namespace webCore.Models
{
    public class Category
    {
        public string _id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ParentId { get; set; }
        public string ParentTitle { get; set; }
        public string Status { get; set; }
        public int Position { get; set; }
        public bool Deleted { get; set; }
        public string Slug { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
