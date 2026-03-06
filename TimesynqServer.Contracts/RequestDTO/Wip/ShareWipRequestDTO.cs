namespace TimesynqServer.Contracts.RequestDTO.Wip
{
    /// <summary>
    /// Represents the required information for an wip share request.
    /// </summary>
    public sealed class ShareWipRequestDTO
    {
        /// <summary>
        /// The ID of the user that the wip is to be shared with.
        /// </summary>
        public required Guid UserId { get; init; }
    }
}
