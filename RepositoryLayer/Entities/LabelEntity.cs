namespace RepositoryLayer.Entities
{
    public class LabelEntity
    {
        public int LabelId { get; set; }

        // Jo naam user ko dikhta hai (Work)
        public string Name { get; set; } = "";

        // Duplicate check ke liye: Name.Trim().ToUpperInvariant() (WORK)
        public string NormalizedName { get; set; } = "";

        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }
        public UserEntity User { get; set; } = null!;

        public ICollection<NoteLabelEntity> NoteLabels { get; set; } = new List<NoteLabelEntity>();
    }
}