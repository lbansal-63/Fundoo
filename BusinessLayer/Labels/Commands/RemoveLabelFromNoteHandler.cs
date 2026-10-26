using BusinessLayer.Interfaces;
using BusinessLayer.Notes;
using Microsoft.Extensions.Logging;
using Middleware.Services.Cache;

namespace BusinessLayer.Labels.Commands
{
    public class RemoveLabelFromNoteHandler
    {
        private readonly ILabelService _service;
        private readonly ICacheService _cache;
        private readonly ILogger<RemoveLabelFromNoteHandler> _logger;

        public RemoveLabelFromNoteHandler(ILabelService service, ICacheService cache, ILogger<RemoveLabelFromNoteHandler> logger)
        {
            _service = service;
            _cache = cache;
            _logger = logger;
        }

        public void Handle(RemoveLabelFromNoteCommand command)
        {
            _service.RemoveFromNote(command.UserId, command.NoteId, command.LabelId);
            NoteHelper.InvalidateCache(_cache, command.UserId);

            _logger.LogInformation("AUDIT: user {UserId} removed label {LabelId} from note {NoteId}",
                command.UserId, command.LabelId, command.NoteId);
        }
    }
}