namespace BusinessLayer.User.Commands
{
    public record RegisterUserCommand(
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        string Password);
}