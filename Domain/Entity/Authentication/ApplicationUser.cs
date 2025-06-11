using Microsoft.AspNetCore.Identity;

namespace Domain.Entity.Authentication
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? SchoolName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Position { get; set; } = string.Empty;
        public string? ClassroomAssigned { get; set; }
    }
}
