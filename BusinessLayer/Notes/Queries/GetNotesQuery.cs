using ModelLayer;

namespace BusinessLayer.Notes.Queries
{
    public record GetNotesQuery(int UserId, NoteView View);
}