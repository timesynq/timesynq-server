namespace TimesynqServer.Application.DTO
{
    public sealed class RoomJoinedDTO
    {
        public WipDTO Wip { get; }
        public RoomMemberDTO UserWhoJoined { get; }
        public IEnumerable<RoomMemberDTO> AlreadyPresentMembers { get; }

        public RoomJoinedDTO(WipDTO wipDTO, RoomMemberDTO userWhoJoined, IEnumerable<RoomMemberDTO> alreadyPresentMembers)
        {
            Wip = wipDTO;
            UserWhoJoined = userWhoJoined;
            AlreadyPresentMembers = alreadyPresentMembers;
        }
    }
}
