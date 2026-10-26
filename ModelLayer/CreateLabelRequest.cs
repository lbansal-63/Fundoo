using System.ComponentModel.DataAnnotations;

namespace ModelLayer
{
    public class CreateLabelRequest
    {
        // UserId yahan nahi hai: owner hamesha JWT claims se aata hai
        [Required, StringLength(50)] public string Name { get; set; } = "";
    }
}