using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer
{
    public class NoteResponseModel
    {
        public int NoteId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? Colour { get; set; }
        public bool IsArchived { get; set; }
        public bool IsDeleted { get; set; }
        public List<LabelResponseModel> Labels { get; set; } = new();
    }
}
