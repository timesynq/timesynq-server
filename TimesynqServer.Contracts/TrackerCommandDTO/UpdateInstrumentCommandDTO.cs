namespace TimesynqServer.Contracts.TrackerCommandDTO
{
    public class UpdateInstrumentCommandDTO
    {
        public byte Frame { get; init; }
        public byte Channel { get; init; }
        public byte Line { get; init; }
        public byte NoteGroup { get; init; }
        public byte? NewInstrument { get; init; }
    }
}
