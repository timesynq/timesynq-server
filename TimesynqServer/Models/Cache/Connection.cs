namespace TimesynqServer.Models.Cache
{
    public class Connection
    {
        public required Guid UserId { get; set; }
        public required string RoomCode { get; set; }
        public required string ConnectionId { get; set; }
    }
}
