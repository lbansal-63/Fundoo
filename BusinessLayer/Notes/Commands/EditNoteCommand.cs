namespace BusinessLayer.Notes.Commands
{
    public record EditNoteCommand(int UserId, int NoteId, string Title, string? Description, string? Colour);
}