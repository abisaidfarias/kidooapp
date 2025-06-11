using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Request.Account
{
    public class CreateAccountDTO
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required] 
        public string Role { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? SchoolName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Position { get; set; } = string.Empty;
        public string? ClassroomAssigned { get; set; }
    }
}
