using Microsoft.EntityFrameworkCore;

namespace University_Notebank.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Major> Majors { get; set; }
        public DbSet<Term> Terms { get; set; }
        public DbSet<NoteAttachment> NoteAttachments { get; set; }
        public DbSet<NoteRequest> NoteRequests { get; set; }
        public DbSet<RequestMessage> RequestMessages { get; set; }
        public DbSet<RelationQuestion> RelationQuestions { get; set; }
        public DbSet<QuestionAnswer> QuestionAnswers { get; set; }
    }
}