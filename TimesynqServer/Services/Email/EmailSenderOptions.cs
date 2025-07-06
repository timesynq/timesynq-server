namespace TimesynqServer.Services.Email
{
    /// <summary>
    /// Configuration options for the email sender service.
    /// </summary>
    public sealed class EmailSenderOptions
    {
        /// <summary>
        /// The name of the configuration section used to bind <see cref="EmailSenderOptions"/>.
        /// </summary>
        public const string ConfigurationSection = nameof(EmailSenderOptions);

        /// <summary>
        /// The email address used as the sender in outgoing emails.
        /// </summary>
        public string FromEmail { get; set; } = string.Empty;
    }
}
