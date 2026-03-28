using System.Text.RegularExpressions;

namespace TimesynqServer.Common
{
    public static class Hex
    {
        public static string TwoDigit(int n)
            => $"{n:X2}";
        public static string TwoDigit(byte n)
            => $"{n:X2}";

        public static byte ByteFromTwoDigits(string twoDigits)
        {
            if (!Regex.IsMatch(twoDigits, @"^[0-9A-Fa-f]{2}$"))
                throw new ArgumentException("Input was not a valid two digit hexadecimal value");
            return Convert.ToByte(twoDigits, 16);
        }
    }
}
