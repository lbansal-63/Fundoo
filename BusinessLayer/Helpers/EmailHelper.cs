using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace BusinessLayer.Helpers
{
    public class EmailHelper
    {
        private readonly IConfiguration _config;

        public EmailHelper(IConfiguration config)
        {
            _config = config;
        }

        public void Send(string to, string subject, string body)
        {
            var host = _config["Email:Host"]!;
            var port = int.Parse(_config["Email:Port"]!);
            var user = _config["Email:User"]!;
            var password = _config["Email:Password"]!;

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, password),
                EnableSsl = true
            };
            client.Send(new MailMessage(user, to, subject, body));
        }
    }
}