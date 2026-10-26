using BusinessLayer.Interfaces;
using BusinessLayer.Notes;
using Microsoft.Extensions.Logging;
using Middleware.Services.Cache;

namespace BusinessLayer.Labels.Commands
{
    public class AttachLabelToNoteHandler
    {
        private readonly ILabelService _service;
        private readonly ICacheService _cache;
        private readonly ILogger<AttachLabelToNoteHandler> _logger;

        public AttachLabelToNoteHandler(ILabelService service, ICacheService cache, ILogger<AttachLabelToNoteHandler> logger)
        {
            _service = service;
            _cache = cache;
            _logger = logger;
        }

        public void Handle(AttachLabelToNoteCommand command)
        {
            _service.AttachToNote(command.UserId, command.NoteId, command.LabelId);
            NoteHelper.InvalidateCache(_cache, command.UserId);

            _logger.LogInformation("AUDIT: user {UserId} attached label {LabelId} to note {NoteId}",
                command.UserId, command.LabelId, command.NoteId);
        }
    }
}