namespace TimesynqServer.Common.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string str, int length)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            return str.Length < length ? str : str[..length];
        }
    }
}
