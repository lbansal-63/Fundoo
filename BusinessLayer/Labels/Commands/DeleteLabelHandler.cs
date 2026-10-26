using BusinessLayer.Interfaces;
using BusinessLayer.Notes;
using Microsoft.Extensions.Logging;
using Middleware.Services.Cache;

namespace BusinessLayer.Labels.Commands
{
    public class DeleteLabelHandler
    {
        private readonly ILabelService _service;
        private readonly ICacheService _cache;
        private readonly ILogger<DeleteLabelHandler> _logger;

        public DeleteLabelHandler(ILabelService service, ICacheService cache, ILogger<DeleteLabelHandler> logger)
        {
            _service = service;
            _cache = cache;
            _logger = logger;
        }

        public void Handle(DeleteLabelCommand command)
        {
            _service.Delete(command.UserId, command.LabelId);
            NoteHelper.InvalidateCache(_cache, command.UserId);

            _logger.LogInformation("AUDIT: user {UserId} deleted label {LabelId}", command.UserId, command.LabelId);
        }
    }
}