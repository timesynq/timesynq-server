namespace TimesynqServer.Contracts.TrackerCommandDTO
{
    public class UpdateLineCountCommandDTO
    {
        public byte Frame { get; init; }
        public int NewLineCount { get; init; }
    }
}
