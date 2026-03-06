namespace TimesynqServer.Contracts.RequestDTO.Wip
{
    /// <summary>
    /// Represents the required information for an wip unshare request.
    /// </summary>
    public sealed class UnshareFromWipRequestDTO
    {
        /// <summary>
        /// The ID of the user that the wip is to be unshared with.
        /// </summary>
        public required Guid UserId { get; init; }
    }
}
