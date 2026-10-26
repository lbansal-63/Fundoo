namespace BusinessLayer.User.Commands
{
    public record ResetPasswordCommand(string Token, string NewPassword);
}