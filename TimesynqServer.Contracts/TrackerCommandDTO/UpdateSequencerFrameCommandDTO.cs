namespace TimesynqServer.Contracts.TrackerCommandDTO
{
    public class UpdateSequencerFrameCommandDTO
    {
        public byte Line { get; init; }
        public byte NewFrame { get; init; }
    }
}
