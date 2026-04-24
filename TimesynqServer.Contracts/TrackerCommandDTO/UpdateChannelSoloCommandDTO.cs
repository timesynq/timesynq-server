namespace TimesynqServer.Contracts.TrackerCommandDTO
{
    public class UpdateChannelSoloCommandDTO
    {
        public byte Frame { get; init; }
        public byte Channel { get; init; }
        public bool IsSolo { get; init; }
    }
}
