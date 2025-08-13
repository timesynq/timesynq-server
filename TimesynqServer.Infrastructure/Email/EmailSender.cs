using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace TimesynqServer.Infrastructure.Email
{
    public class EmailSender<TUser> : IEmailSender<TUser> where TUser : class
    {

        private readonly IAmazonSimpleEmailService _emailService;
        private readonly IOptions<EmailSenderOptions> _options;

        public EmailSender(IAmazonSimpleEmailService emailService, IOptions<EmailSenderOptions> options)
        {
            _emailService = emailService;
            _options = options;
        }

        public async Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink)
        {
            await Execute(email, "Confirm email for Timesynq", "Click the link below to confirm your email.", confirmationLink);
        }

        public async Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
        {
            await Execute(email, "Reset password for Timesynq", "Your password reset code", resetCode);
        }

        public async Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink)
        {
            await Execute(email, "Password reset code for Timesynq", "Click the link below to reset your password", resetLink);
        }

        private async Task Execute(string email, string subject, string body, string link)
        {
            var request = new SendEmailRequest
            {
                Source = _options.Value.FromEmail,
                Destination = new Destination
                {
                    ToAddresses = [email],
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = new Content($"<h1>{body}</h1><p>{link}</p>")
                    }
                }
            };

            await _emailService.SendEmailAsync(request);

        }
    }
}
