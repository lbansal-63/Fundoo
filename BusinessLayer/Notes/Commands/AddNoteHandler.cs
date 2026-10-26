using Microsoft.Extensions.Logging;
using Middleware.Services.Cache;
using ModelLayer;
using RepositoryLayer.Entities;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.Notes.Commands
{
    public class AddNoteHandler
    {
        private readonly INoteRepository _repo;
        private readonly ICacheService _cache;
        private readonly ILogger<AddNoteHandler> _logger;

        public AddNoteHandler(INoteRepository repo, ICacheService cache, ILogger<AddNoteHandler> logger)
        {
            _repo = repo;
            _cache = cache;
            _logger = logger;
        }

        public NoteResponseModel Handle(AddNoteCommand command)
        {
            var note = new NoteEntity
            {
                Title = command.Title.Trim(),
                Description = command.Description,
                Colour = command.Colour,
                UserId = command.UserId
            };

            _repo.Add(note);
            NoteHelper.InvalidateCache(_cache, command.UserId);

            _logger.LogInformation("AUDIT: user {UserId} created note {NoteId}", command.UserId, note.NoteId);
            return NoteHelper.ToResponse(note);
        }
    }
}