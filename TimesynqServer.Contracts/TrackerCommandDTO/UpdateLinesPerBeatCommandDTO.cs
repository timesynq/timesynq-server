namespace TimesynqServer.Contracts.TrackerCommandDTO
{
    public class UpdateLinesPerBeatCommandDTO
    {
        public byte Frame { get; init; }
        public int NewLinesPerBeat { get; init; }
    }
}
