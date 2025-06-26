namespace TimesynqServer.Models.DTO
{
    public sealed class UserDTO
    {
        public required Guid Id { get; set; }
        public required string UserName { get; set; }
        public required string ProfilePicture { get; set; }
        public required DateTime CreatedOnUTC { get; set; }
    }
}
