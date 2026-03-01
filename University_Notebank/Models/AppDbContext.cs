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
    }
}