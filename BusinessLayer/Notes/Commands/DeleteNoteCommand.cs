namespace BusinessLayer.Notes.Commands
{
    public record DeleteNoteCommand(int UserId, int NoteId);
}