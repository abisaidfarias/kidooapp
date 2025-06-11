using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Request.Account
{
    public class LoginDTO
    {
        [EmailAddress,Required,DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;
        [Required]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@#$%^&+=!]).{8,}$",
            ErrorMessage = "Password must be at least 8 characters long and include at least" +
            " one uppercase letter, one lowercase letter, one number, and one special character")]
        public string Password { get; set; } = string.Empty;
    }
}
