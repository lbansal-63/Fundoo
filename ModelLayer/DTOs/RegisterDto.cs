using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer.DTOs
{
    public class RegisterDto
    {
        [Required, StringLength(50)] public string FirstName { get; set; } = "";
        [Required, StringLength(50)] public string LastName { get; set; } = "";
        [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
        [Required, StringLength(72, MinimumLength = 8)] public string Password { get; set; } = "";
    }
}
