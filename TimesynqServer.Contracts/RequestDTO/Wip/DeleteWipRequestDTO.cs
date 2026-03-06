namespace TimesynqServer.Contracts.RequestDTO.Wip
{
    /// <summary>
    /// Represents the required information for an wip delete request.
    /// </summary>
    public sealed class DeleteWipRequestDTO
    {
        /// <summary>
        /// The ID of the wip that is to be deleted.
        /// </summary>
        public required Guid WipID { get; init; }
    }
}
