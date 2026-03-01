namespace University_Notebank.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "Student";

        // ✅ Extra profile fields
        public string? Department { get; set; }
        public string? StudentId { get; set; }
        public string? Phone { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}