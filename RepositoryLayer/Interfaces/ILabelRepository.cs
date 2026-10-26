using RepositoryLayer.Entities;

namespace RepositoryLayer.Interfaces
{
    public interface ILabelRepository
    {
        // false = is user ka same normalized naam pehle se hai (DB unique index ne roka)
        bool Add(LabelEntity label);
        bool Update(LabelEntity label);

        List<LabelEntity> GetByUser(int userId);

        // Hamesha LabelId + UserId dono se dhundte hain, sirf LabelId se kabhi nahi
        LabelEntity? GetById(int userId, int labelId);

        bool ExistsByNormalizedName(int userId, string normalizedName, int? excludeLabelId = null);

        // Label delete hone par NoteLabel rows DB cascade se hat jaati hain, notes safe rehte hain
        void Delete(LabelEntity label);

        void AttachToNote(int noteId, int labelId);
        void RemoveFromNote(int noteId, int labelId);
    }
}