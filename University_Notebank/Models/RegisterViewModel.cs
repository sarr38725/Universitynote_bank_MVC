using System.ComponentModel.DataAnnotations;

namespace University_Notebank.Models
{
    public class RegisterViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; } = "Student"; // Student, Teacher, Others
    }
}