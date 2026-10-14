using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ModelLayer.DTOs
{
    public class LoginDto
    {
        [Required, StringLength(254)] public string Email { get; set; } = "";
        [Required, StringLength(128)] public string Password { get; set; } = "";
    }
}
