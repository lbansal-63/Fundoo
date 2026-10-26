namespace RepositoryLayer.Entities
{
    // Join table: composite key (NoteId, LabelId), isliye ek note par ek label do baar nahi lag sakta
    public class NoteLabelEntity
    {
        public int NoteId { get; set; }
        public NoteEntity Note { get; set; } = null!;

        public int LabelId { get; set; }
        public LabelEntity Label { get; set; } = null!;
    }
}