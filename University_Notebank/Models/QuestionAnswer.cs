using System;

namespace University_Notebank.Models
{
    public class QuestionAnswer
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }
        public RelationQuestion Question { get; set; }

        // The student who answered
        public int UserId { get; set; }
        public User User { get; set; }

        public string Body { get; set; }

        // Teacher marks this answer as the correct/best one
        public bool IsAccepted { get; set; } = false;

        // Teacher can give marks and feedback
        public int? Marks { get; set; }
        public string? TeacherFeedback { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
