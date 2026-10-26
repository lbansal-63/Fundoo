using System.ComponentModel.DataAnnotations;

namespace ModelLayer
{
    public class RenameLabelRequest
    {
        [Required, StringLength(50)] public string Name { get; set; } = "";
    }
}