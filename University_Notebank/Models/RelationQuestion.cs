using System;
using System.Collections.Generic;

namespace University_Notebank.Models
{
    public class RelationQuestion
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string Title { get; set; }
        public string Body { get; set; }

        public int? NoteId { get; set; }
        public Note? Note { get; set; }

        public int? MajorId { get; set; }
        public Major? Major { get; set; }

        public int? TermId { get; set; }
        public Term? Term { get; set; }

        // Open → Answered → Closed
        public string Status { get; set; } = "Open";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<QuestionAnswer> Answers { get; set; } = new List<QuestionAnswer>();
    }
}
