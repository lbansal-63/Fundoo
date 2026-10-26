using BusinessLayer.Interfaces;
using BusinessLayer.Labels.Validators;
using Microsoft.Extensions.Logging;
using ModelLayer;

namespace BusinessLayer.Labels.Commands
{
    public class CreateLabelHandler
    {
        private readonly CreateLabelCommandValidator _validator;
        private readonly ILabelService _service;
        private readonly ILogger<CreateLabelHandler> _logger;

        public CreateLabelHandler(
            CreateLabelCommandValidator validator,
            ILabelService service,
            ILogger<CreateLabelHandler> logger)
        {
            _validator = validator;
            _service = service;
            _logger = logger;
        }

        public LabelResponseModel Handle(CreateLabelCommand command)
        {
            _validator.Validate(command);

            var label = _service.Create(command.UserId, command.Name);

            _logger.LogInformation("AUDIT: user {UserId} created label {LabelId}", command.UserId, label.LabelId);
            return label;
        }
    }
}