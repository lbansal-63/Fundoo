using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer.Entities
{
    public class NoteEntity
    {
        public int NoteId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? Colour { get; set; }
        public bool IsArchived { get; set; }
        public bool IsDeleted { get; set; }

        public int UserId { get; set; }
        public UserEntity User { get; set; } = null!;
    }
}
