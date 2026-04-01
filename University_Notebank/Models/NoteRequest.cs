using System;
using System.Collections.Generic;

namespace University_Notebank.Models
{
    public class NoteRequest
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string Subject { get; set; }        // কোন note চাই
        public string? Description { get; set; }   // বিস্তারিত

        public int? MajorId { get; set; }
        public Major? Major { get; set; }

        public int? TermId { get; set; }
        public Term? Term { get; set; }

        public int? Batch { get; set; }

        // Pending → admin দেখেনি | Reviewed → admin reply করেছে | Fulfilled → note upload হয়েছে | Rejected
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<RequestMessage> Messages { get; set; } = new List<RequestMessage>();
    }
}