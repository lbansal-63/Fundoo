using Middleware.ExceptionHandling;

namespace BusinessLayer.Labels.Validators
{
    internal static class LabelNameRules
    {
        public const int MaxLength = 50;

        public static void Validate(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new RequestValidationException("Label name is required");

            if (name.Trim().Length > MaxLength)
                throw new RequestValidationException($"Label name cannot exceed {MaxLength} characters");
        }
    }
}