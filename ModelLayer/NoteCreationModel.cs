using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer
{
    public class NoteCreationModel
    {
        [Required, StringLength(200)] public string Title { get; set; } = "";
        [StringLength(10000)] public string? Description { get; set; }
        [StringLength(50)] public string? Colour { get; set; }
    }
}
