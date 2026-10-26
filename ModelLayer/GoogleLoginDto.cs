using System.ComponentModel.DataAnnotations;

namespace ModelLayer
{
    public class GoogleLoginDto
    {
        [Required, StringLength(4096)] public string IdToken { get; set; } = "";
    }
}