namespace TimesynqServer.Services.Email
{
    public sealed class EmailSenderOptions
    {
        public const string ConfigurationSection = nameof(EmailSenderOptions);
        public string FromEmail { get; set; } = string.Empty;
    }
}
