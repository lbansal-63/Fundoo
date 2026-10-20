using BusinessLayer.Interfaces;
using Microsoft.Extensions.Logging;
using Middleware.ExceptionHandling;
using Middleware.Services.Cache;
using ModelLayer;
using RepositoryLayer.Entities;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.Services
{
    public class NoteBusiness : INoteBusiness
    {
        private static readonly TimeSpan NotesTtl = TimeSpan.FromMinutes(10);

        private readonly INoteRepository _repo;
        private readonly ICacheService _cache;
        private readonly ILogger<NoteBusiness> _logger;

        public NoteBusiness(INoteRepository repo, ICacheService cache, ILogger<NoteBusiness> logger)
        {
            _repo = repo;
            _cache = cache;
            _logger = logger;
        }

        private static NoteResponseModel ToResponse(NoteEntity n) => new()
        {
            NoteId = n.NoteId,
            Title = n.Title,
            Description = n.Description,
            Colour = n.Colour,
            IsArchived = n.IsArchived,
            IsDeleted = n.IsDeleted
        };

        // Kisi bhi write ke baad is user ki teeno lists (Active/Archived/Trash) clear
        private void InvalidateNotes(int userId)
        {
            foreach (var v in Enum.GetValues<NoteView>())
                _cache.Remove(CacheKeys.Notes(userId, v.ToString()));
        }

        private NoteEntity GetOwnedNote(int userId, int noteId)
        {
            return _repo.GetById(userId, noteId)
                ?? throw new NotFoundException("Note not found");
        }

        public NoteResponseModel CreateNote(int userId, NoteCreationModel model)
        {
            var note = new NoteEntity
            {
                Title = model.Title.Trim(),
                Description = model.Description,
                Colour = model.Colour,
                UserId = userId
            };

            _repo.Add(note);
            InvalidateNotes(userId);
            _logger.LogInformation("AUDIT: user {UserId} created note {NoteId}", userId, note.NoteId);
            return ToResponse(note);
        }

        public List<NoteResponseModel> ViewNotes(int userId, NoteView view)
        {
            var key = CacheKeys.Notes(userId, view.ToString());

            var cached = _cache.Get<List<NoteResponseModel>>(key);
            if (cached != null) return cached;

            var notes = _repo.GetByUser(userId, view).Select(ToResponse).ToList();
            _cache.Set(key, notes, NotesTtl);
            return notes;
        }

        public NoteResponseModel ViewNotesById(int userId, int noteId)
        {
            return ToResponse(GetOwnedNote(userId, noteId));
        }

        public NoteResponseModel EditNotes(int userId, EditNotesModel model)
        {
            var note = GetOwnedNote(userId, model.NoteId);

            note.Title = model.Title.Trim();
            note.Description = model.Description;
            note.Colour = model.Colour;

            _repo.Update(note);
            InvalidateNotes(userId);
            _logger.LogInformation("AUDIT: user {UserId} edited note {NoteId}", userId, note.NoteId);
            return ToResponse(note);
        }

        public void DeleteNotes(int userId, int noteId)
        {
            var note = GetOwnedNote(userId, noteId);

            _repo.Delete(note);
            InvalidateNotes(userId);
            _logger.LogInformation("AUDIT: user {UserId} permanently deleted note {NoteId}", userId, noteId);
        }

        public NoteResponseModel ArchiUnarchived(int userId, int noteId)
        {
            var note = GetOwnedNote(userId, noteId);

            note.IsArchived = !note.IsArchived;
            _repo.Update(note);
            InvalidateNotes(userId);
            return ToResponse(note);
        }

        public NoteResponseModel TrashUntrash(int userId, int noteId)
        {
            var note = GetOwnedNote(userId, noteId);

            note.IsDeleted = !note.IsDeleted;
            _repo.Update(note);
            InvalidateNotes(userId);
            return ToResponse(note);
        }
    }
}