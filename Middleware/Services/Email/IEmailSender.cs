namespace Middleware.Services.Email
{
    public interface IEmailSender
    {
        void Send(string to, string subject, string body);
    }
}
