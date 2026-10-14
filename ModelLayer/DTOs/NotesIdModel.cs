using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer.DTOs
{
    public class NotesIdModel
    {
        [Range(1, int.MaxValue)] public int NoteId { get; set; }
    }
}
