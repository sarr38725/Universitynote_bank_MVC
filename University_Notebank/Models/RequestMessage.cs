using System;

namespace University_Notebank.Models
{
    public class RequestMessage
    {
        public int Id { get; set; }

        public int NoteRequestId { get; set; }
        public NoteRequest NoteRequest { get; set; }

        public int SenderId { get; set; }
        public User Sender { get; set; }

        public string Message { get; set; }

        public bool IsAdmin { get; set; }   // true = admin পাঠিয়েছে

        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}