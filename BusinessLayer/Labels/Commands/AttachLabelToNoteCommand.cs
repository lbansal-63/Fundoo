namespace BusinessLayer.Labels.Commands
{
    public record AttachLabelToNoteCommand(int UserId, int NoteId, int LabelId);
}