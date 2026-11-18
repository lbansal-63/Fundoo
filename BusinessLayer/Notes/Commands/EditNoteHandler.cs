using Microsoft.Extensions.Logging;
using Middleware.Services.Cache;
using ModelLayer;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.Notes.Commands
{
    public class EditNoteHandler
    {
        private readonly INoteRepository _repo;
        private readonly ICacheService _cache;
        private readonly ILogger<EditNoteHandler> _logger;

        public EditNoteHandler(INoteRepository repo, ICacheService cache, ILogger<EditNoteHandler> logger)
        {
            _repo = repo;
            _cache = cache;
            _logger = logger;
        }

        public NoteResponseModel Handle(EditNoteCommand command)
        {
            var note = NoteHelper.GetOwned(_repo, command.UserId, command.NoteId);

            note.Title = command.Title.Trim();
            note.Description = command.Description;
            note.Colour = command.Colour;

            _repo.Update(note);
            NoteHelper.InvalidateCache(_cache, command.UserId);

            _logger.LogInformation("AUDIT: user {UserId} edited note {NoteId}", command.UserId, note.NoteId);
            return NoteHelper.ToResponse(note);
        }
    }
}