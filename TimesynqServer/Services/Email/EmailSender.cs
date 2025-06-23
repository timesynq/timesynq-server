using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TimesynqServer.Database.Entities;

namespace TimesynqServer.Services.Email
{
    public class EmailSender<TUser> : IEmailSender<TUser> where TUser : class
    {

        private IAmazonSimpleEmailService _emailService;
        private IOptions<EmailSenderOptions> _options;

        public EmailSender(IAmazonSimpleEmailService emailService, IOptions<EmailSenderOptions> options)
        {
            _emailService = emailService;
            _options = options;
        }

        public async Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink)
        {
            await Execute(email, "Confirm email for Timesynq", confirmationLink);
        }

        public async Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
        {
            await Execute(email, "Reset password for Timesynq", resetCode);
        }

        public async Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink)
        {
            await Execute(email, "Password reset code for Timesynq", resetLink);
        }

        private async Task Execute(string email, string subject, string body)
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
                        Html = new Content($"<h1>Click the link below to confirm your email.</h1><p>{body}</p>")
                    }
                }
            };

            await _emailService.SendEmailAsync(request);

        }
    }
}
