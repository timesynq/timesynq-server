namespace TimesynqServer.Contracts.TrackerCommandDTO
{
    public class UpdateChannelTypeCommandDTO
    {
        public byte Frame { get; init; }
        public byte Channel { get; init; }
        public bool IsSend { get; init; }
    }
}
