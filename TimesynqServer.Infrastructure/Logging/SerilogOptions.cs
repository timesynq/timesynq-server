namespace TimesynqServer.Infrastructure.Logging
{
    public sealed class SerilogOptions
    {
        /// <summary>
        /// The name of the configuration section used to bind <see cref="SerilogOptions"/>.
        /// </summary>
        public const string ConfigurationSection = nameof(SerilogOptions);

        /// <summary>
        /// 
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;
    }
}
