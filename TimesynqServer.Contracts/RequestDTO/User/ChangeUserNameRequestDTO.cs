namespace TimesynqServer.Contracts.RequestDTO.User
{
    public sealed class ChangeUserNameRequestDTO
    {
        public required string NewUserName { get; init; }
    }
}
