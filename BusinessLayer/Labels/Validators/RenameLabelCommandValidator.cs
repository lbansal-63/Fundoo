using BusinessLayer.Labels.Commands;

namespace BusinessLayer.Labels.Validators
{
    public class RenameLabelCommandValidator
    {
        public void Validate(RenameLabelCommand command) => LabelNameRules.Validate(command.Name);
    }
}