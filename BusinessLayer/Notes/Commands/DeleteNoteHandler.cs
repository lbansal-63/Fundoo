using Microsoft.Extensions.Logging;
using Middleware.Services.Cache;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.Notes.Commands
{
    public class DeleteNoteHandler
    {
        private readonly INoteRepository _repo;
        private readonly ICacheService _cache;
        private readonly ILogger<DeleteNoteHandler> _logger;

        public DeleteNoteHandler(INoteRepository repo, ICacheService cache, ILogger<DeleteNoteHandler> logger)
        {
            _repo = repo;
            _cache = cache;
            _logger = logger;
        }

        public void Handle(DeleteNoteCommand command)
        {
            var note = NoteHelper.GetOwned(_repo, command.UserId, command.NoteId);

            _repo.Delete(note);
            NoteHelper.InvalidateCache(_cache, command.UserId);

            _logger.LogInformation("AUDIT: user {UserId} permanently deleted note {NoteId}",
                command.UserId, command.NoteId);
        }
    }
}