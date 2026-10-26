using Middleware.Services.Cache;
using ModelLayer;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.Notes.Commands
{
    public class TrashNoteHandler
    {
        private readonly INoteRepository _repo;
        private readonly ICacheService _cache;

        public TrashNoteHandler(INoteRepository repo, ICacheService cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public NoteResponseModel Handle(TrashNoteCommand command)
        {
            var note = NoteHelper.GetOwned(_repo, command.UserId, command.NoteId);

            note.IsDeleted = !note.IsDeleted;
            _repo.Update(note);
            NoteHelper.InvalidateCache(_cache, command.UserId);

            return NoteHelper.ToResponse(note);
        }
    }
}