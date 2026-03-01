using System.ComponentModel.DataAnnotations;

namespace University_Notebank.Models
{
    public class ProfileViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? CurrentPassword { get; set; }

        [MinLength(6)]
        public string? NewPassword { get; set; }

        public string? Role { get; set; }
    }
}