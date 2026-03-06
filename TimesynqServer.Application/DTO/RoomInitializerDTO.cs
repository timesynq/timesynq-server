namespace TimesynqServer.Application.DTO
{
    public sealed class RoomInitializerDTO
    {
        public WipDTO Wip { get; }
        public IEnumerable<RoomMemberDTO> Members { get; }

        public RoomInitializerDTO(WipDTO wip, IEnumerable<RoomMemberDTO> members) 
        {
            Wip = wip;
            Members = members;
        }
    }
}
