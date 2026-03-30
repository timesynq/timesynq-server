namespace TimesynqServer.Contracts.TrackerCommandDTO
{
    public class UpdateFXSymbolCommandDTO
    {
        public byte Frame { get; init; }
        public byte Channel { get; init; }
        public byte Line { get; init; }
        public byte FXGroup { get; init; }
        public byte? NewFXSymbol { get; init; }
    }
}
