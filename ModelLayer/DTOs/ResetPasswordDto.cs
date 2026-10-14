using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ModelLayer.DTOs
{
    public class ResetPasswordDto
    {
        [Required] public string Token { get; set; } = "";
        [Required, StringLength(72, MinimumLength = 8)] public string NewPassword { get; set; } = "";
    }
}
