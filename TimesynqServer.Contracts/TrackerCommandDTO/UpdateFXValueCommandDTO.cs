namespace TimesynqServer.Contracts.TrackerCommandDTO
{
    public class UpdateFXValueCommandDTO
    {
        public byte Frame { get; init; }
        public byte Channel { get; init; }
        public byte Line { get; init; }
        public byte FXGroup { get; init; }
        public byte? NewFXValue { get; init; }
    }
}
