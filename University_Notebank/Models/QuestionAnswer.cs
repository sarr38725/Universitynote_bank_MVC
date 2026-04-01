using System;

namespace University_Notebank.Models
{
    public class QuestionAnswer
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }
        public RelationQuestion Question { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string Body { get; set; }

        public bool IsAccepted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
