using System;
namespace University_Notebank.Models
{
    public class Note
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int MajorId { get; set; }
        public Major Major { get; set; }
        public int TermId { get; set; }
        public Term Term { get; set; }
        public string? Level { get; set; }
        public int? Batch { get; set; }
        public string? CoverImagePath { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public long FileSize { get; set; }
        public int UploaderId { get; set; }
        public User Uploader { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ✅ নতুন field
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    }
}