namespace TimesynqServer.Contracts.TrackerCommandDTO
{
    public class UpdateSequencerChannelCommandDTO
    {
        public byte Line { get; init; }
        public byte Channel { get; init; }
        public bool IsMuted { get; init; }
    }
}
