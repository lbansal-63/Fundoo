namespace BusinessLayer.Notes.Commands
{
    public record AddNoteCommand(int UserId, string Title, string? Description, string? Colour);
}