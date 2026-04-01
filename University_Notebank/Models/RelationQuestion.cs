using System;
using System.Collections.Generic;

namespace University_Notebank.Models
{
    public class RelationQuestion
    {
        public int Id { get; set; }

        // The teacher who posted this question
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

        // Teacher can save as draft before publishing to students
        public bool IsPublished { get; set; } = false;

        // Optional deadline for student answers
        public DateTime? Deadline { get; set; }

        // Open → Active (students answering) → Closed
        public string Status { get; set; } = "Open";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<QuestionAnswer> Answers { get; set; } = new List<QuestionAnswer>();
    }
}
