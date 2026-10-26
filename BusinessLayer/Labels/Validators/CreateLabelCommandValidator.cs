using BusinessLayer.Labels.Commands;

namespace BusinessLayer.Labels.Validators
{
    public class CreateLabelCommandValidator
    {
        public void Validate(CreateLabelCommand command) => LabelNameRules.Validate(command.Name);
    }
}