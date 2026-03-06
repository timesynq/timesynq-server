namespace TimesynqServer.Contracts.RequestDTO.Wip
{
    /// <summary>
    /// Represents the required information for an wip name change request.
    /// </summary>
    public sealed class ChangeWipNameRequestDTO
    {
        /// <summary>
        /// Specifies the new name to be applied to the wip.
        /// </summary>
        public required string NewName { get; init; }
    }
}
