using BusinessLayer.Interfaces;
using BusinessLayer.Labels.Validators;
using BusinessLayer.Notes;
using Microsoft.Extensions.Logging;
using Middleware.Services.Cache;
using ModelLayer;

namespace BusinessLayer.Labels.Commands
{
    public class RenameLabelHandler
    {
        private readonly RenameLabelCommandValidator _validator;
        private readonly ILabelService _service;
        private readonly ICacheService _cache;
        private readonly ILogger<RenameLabelHandler> _logger;

        public RenameLabelHandler(
            RenameLabelCommandValidator validator,
            ILabelService service,
            ICacheService cache,
            ILogger<RenameLabelHandler> logger)
        {
            _validator = validator;
            _service = service;
            _cache = cache;
            _logger = logger;
        }

        public LabelResponseModel Handle(RenameLabelCommand command)
        {
            _validator.Validate(command);

            var label = _service.Rename(command.UserId, command.LabelId, command.Name);

            // Notes ki cached list me label ka naam bhi hota hai, isliye cache clear
            NoteHelper.InvalidateCache(_cache, command.UserId);

            _logger.LogInformation("AUDIT: user {UserId} renamed label {LabelId}", command.UserId, command.LabelId);
            return label;
        }
    }
}