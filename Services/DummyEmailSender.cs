using Microsoft.AspNetCore.Identity.UI.Services;

namespace Nanas_Foundation.Services
{
    public class DummyEmailSender:IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine($"Pretend email sent to {email}: {subject}");
            return Task.CompletedTask;
        }

    }
}
