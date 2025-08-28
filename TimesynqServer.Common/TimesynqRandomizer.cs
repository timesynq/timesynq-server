using System.Text;

namespace TimesynqServer.Common
{
    /// <summary>
    /// Static methods for randomized values used across Timesynq
    /// </summary>
    public static class TimesynqRandomizer
    {
        private static readonly Random _random = new();

        private const int _pixelsLength = 15;
        private const string _pixelVals = "00111";

        private const int _randomCodeLength = 8;
        private const string _characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

        /// <summary>
        /// Generates a unique, 11 character identicon string consisting of a hex color code followed by a hex-encoded series of pixel values.
        /// </summary>
        /// <remarks>
        /// The first 6 characters represent a hex color code.
        /// The 7th character is a period delimiter.
        /// The last 4 characters are a hex code encoding 15 bits of information. The most significant bit is always 0, so the highest possible value for the first hex digit is 7.
        /// These 15 bits are decoded into a 5x5 grid (binary), mirrored:
        /// Columns 1 and 2 are mapped to 5 and 4, respectively.
        /// In the grid, '1' represents the color (from the hex code), and '0' represents no color.
        /// </remarks>
        /// <returns>
        /// A string containing a 6-character hexadecimal color code, followed by a hex-encoded sequence of bits representing the identicon's pixels.
        /// </returns>
        public static string GenerateIdenticon()
        {
            var sb = new StringBuilder();

            string color = string.Format("{0:X6}", _random.Next(0x1000000));
            sb.Append(color);

            sb.Append('.');

            int pixels = 0;
            for(int i = 0; i < _pixelsLength; i++)
            {
                pixels = (pixels << 1) | _random.Next(2);
            }

            sb.Append($"{pixels:X4}");

            return sb.ToString();
        }

        /// <summary>
        /// Generates a random room code string composed of characters from a predefined set.
        /// </summary>
        /// <remarks>
        /// The generated code may not be unique. If needed, uniqueness will need to be verified by the calling method.
        /// </remarks>
        /// <returns>
        /// A string representing the randomly generated room code, with length determined by <c>_randomCodeLength</c>.
        /// </returns>
        public static string GenerateRoomCode()
        {
            var randomCode = new char[_randomCodeLength];
            for (int i = 0; i < _randomCodeLength; i++)
            {
                int randomIndex = _random.Next(_characters.Length);
                randomCode[i] = _characters[randomIndex];
            }
            return new string(randomCode);
        }
    }
}
