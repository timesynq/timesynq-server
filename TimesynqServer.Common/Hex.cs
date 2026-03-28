namespace TimesynqServer.Common
{
    public static class Hex
    {
        public static string TwoDigit(int n)
            => $"{n:X2}";
        public static string TwoDigit(byte n)
            => $"{n:X2}";
    }
}
