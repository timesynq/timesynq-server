namespace TimesynqServer.Contracts.TrackerCommandDTO
{
    public class UpdateChannelMuteCommandDTO
    {
        public byte Frame { get; init; }
        public byte Channel { get; init; }
        public bool IsOn { get; init; }
    }
}
