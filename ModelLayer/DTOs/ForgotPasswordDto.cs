using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ModelLayer.DTOs
{
    public class ForgotPasswordDto
    {
        [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";

    }
}
