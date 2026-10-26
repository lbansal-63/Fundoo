namespace BusinessLayer.Labels.Commands
{
    public record RenameLabelCommand(int UserId, int LabelId, string Name);
}