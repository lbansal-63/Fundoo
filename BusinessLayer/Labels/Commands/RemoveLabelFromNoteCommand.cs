namespace BusinessLayer.Labels.Commands
{
    public record RemoveLabelFromNoteCommand(int UserId, int NoteId, int LabelId);
}