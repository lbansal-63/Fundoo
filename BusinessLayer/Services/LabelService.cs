using BusinessLayer.Interfaces;
using BusinessLayer.Labels;
using Middleware.ExceptionHandling;
using ModelLayer;
using RepositoryLayer.Entities;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.Services
{
    public class LabelService : ILabelService
    {
        private readonly ILabelRepository _labels;
        private readonly INoteRepository _notes;

        public LabelService(ILabelRepository labels, INoteRepository notes)
        {
            _labels = labels;
            _notes = notes;
        }

        // Work / work / WORK teeno ka normalized naam WORK
        private static string Normalize(string name) => name.Trim().ToUpperInvariant();

        // Label na mile ya kisi aur user ka ho, dono me same NotFound
        private LabelEntity GetOwnedLabel(int userId, int labelId) =>
            _labels.GetById(userId, labelId) ?? throw new NotFoundException("Label not found");

        // Note ke liye bhi wahi rule
        private NoteEntity GetOwnedNote(int userId, int noteId) =>
            _notes.GetById(userId, noteId) ?? throw new NotFoundException("Note not found");

        public LabelResponseModel Create(int userId, string name)
        {
            var trimmed = name.Trim();
            var normalized = Normalize(trimmed);

            if (_labels.ExistsByNormalizedName(userId, normalized))
                throw new ConflictException("A label with this name already exists");

            var label = new LabelEntity
            {
                UserId = userId,
                Name = trimmed,
                NormalizedName = normalized,
                CreatedAt = DateTime.UtcNow
            };

            // Upar ka check sirf friendly check hai, asli guard DB ka unique index hai
            if (!_labels.Add(label))
                throw new ConflictException("A label with this name already exists");

            return LabelMapper.ToResponse(label);
        }

        public List<LabelResponseModel> GetAll(int userId) =>
            _labels.GetByUser(userId).Select(LabelMapper.ToResponse).ToList();

        public LabelResponseModel Rename(int userId, int labelId, string name)
        {
            var label = GetOwnedLabel(userId, labelId);

            var trimmed = name.Trim();
            var normalized = Normalize(trimmed);

            // Apne aap ko exclude karte hain, taaki "work" -> "Work" (sirf case badalna) allowed rahe
            if (_labels.ExistsByNormalizedName(userId, normalized, excludeLabelId: labelId))
                throw new ConflictException("A label with this name already exists");

            label.Name = trimmed;
            label.NormalizedName = normalized;

            if (!_labels.Update(label))
                throw new ConflictException("A label with this name already exists");

            return LabelMapper.ToResponse(label);
        }

        public void Delete(int userId, int labelId)
        {
            var label = GetOwnedLabel(userId, labelId);
            _labels.Delete(label);
        }

        public void AttachToNote(int userId, int noteId, int labelId)
        {
            // Note aur label dono isi user ke hone chahiye
            var note = GetOwnedNote(userId, noteId);
            GetOwnedLabel(userId, labelId);

            if (note.IsDeleted)
                throw new ConflictException("Labels cannot be attached to a trashed note");

            _labels.AttachToNote(noteId, labelId);
        }

        public void RemoveFromNote(int userId, int noteId, int labelId)
        {
            GetOwnedNote(userId, noteId);
            GetOwnedLabel(userId, labelId);

            _labels.RemoveFromNote(noteId, labelId);
        }
    }
}