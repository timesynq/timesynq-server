using System.Text.RegularExpressions;
using TimesynqServer.Common;

namespace TimesynqServer.Domain.Cache.Tracker
{
    public readonly struct CellAddress
    {
        private const byte ColsPerGroup = 2;

        public readonly byte Frame;
        public readonly byte Channel;
        public readonly byte Line;
        public readonly byte Column;
        public readonly string FrameHex;
        public readonly string ChannelHex;
        public readonly string LineHex;
        public readonly string ColumnHex;
        public readonly string Address;

        private CellAddress(byte frame, byte channel, byte line, byte column)
        {
            Frame = frame;
            Channel = channel;
            Line = line;
            Column = column;
            FrameHex = Hex.TwoDigit(frame);
            ChannelHex = Hex.TwoDigit(channel);
            LineHex = Hex.TwoDigit(line);
            ColumnHex = Hex.TwoDigit(column);
            Address = $"{FrameHex}:{ChannelHex}:{LineHex}:{ColumnHex}";
        }

        public static CellAddress CreatePitchAddress(byte frame, byte channel, byte line, byte noteGroup)
            => new (frame, channel, line, (byte)(noteGroup * ColsPerGroup));

        public static CellAddress CreateInstrumentAddress(byte frame, byte channel, byte line, byte noteGroup)
            => new (frame, channel, line, (byte)((noteGroup * ColsPerGroup) + 1));

        public static CellAddress CreateFXSymbolAddress(byte frame, byte channel, byte line, byte fxGroup)
            => new(frame, channel, line, (byte)((fxGroup * ColsPerGroup) + (TrackerConstants.MaxNoteGroups * ColsPerGroup)));

        public static CellAddress CreateFXValueAddress(byte frame, byte channel, byte line, byte fxGroup)
            => new(frame, channel, line, (byte)((fxGroup * ColsPerGroup) + (TrackerConstants.MaxNoteGroups * ColsPerGroup) + 1));

        public static CellAddress? DecodeAddressString(string encodedAddress)
        {
            if (!Regex.IsMatch(encodedAddress, @"^[0-9A-Fa-f]{2}:[0-9A-Fa-f]{2}:[0-9A-Fa-f]{2}:[0-9A-Fa-f]{2}$"))
                return null;

            var segments = encodedAddress.Split(':');

            byte frame = Hex.ByteFromTwoDigits(segments[0]);
            if (frame >= TrackerConstants.MaxFramesPerWip)
                return null;

            byte channel = Hex.ByteFromTwoDigits(segments[1]);
            if (channel >= TrackerConstants.MaxChannels)
                return null;

            byte line = Hex.ByteFromTwoDigits(segments[2]);

            byte column = Hex.ByteFromTwoDigits(segments[3]);
            if (column >= TrackerConstants.MaxNoteGroups + TrackerConstants.MaxFXGroups)
                return null;

            return new CellAddress(frame, channel, line, column);
        }
    }
}
