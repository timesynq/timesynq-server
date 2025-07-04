namespace TimesynqServer.Models.Cache
{
    public class Room
    {
        public required string RoomCode { get; set; }
        public required Guid OwnerId { get; set; }
    }
}
