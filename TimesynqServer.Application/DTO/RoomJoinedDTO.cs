namespace TimesynqServer.Application.DTO
{
    public sealed class RoomJoinedDTO
    {
        public UserDTO UserDTO { get; }
        public WipDTO WipDTO { get; }

        public RoomJoinedDTO(UserDTO userDTO, WipDTO wipDTO)
        {
            UserDTO = userDTO;
            WipDTO = wipDTO;
        }
    }
}
