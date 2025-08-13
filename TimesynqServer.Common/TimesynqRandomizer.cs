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
        /// Generates a unique, 21 character identicon string consisting of a hex color code followed by a series of pixel values.
        /// </summary>
        /// <remarks>
        /// First 6 chars = hex color;
        /// Next 15 = 5x5 grid (binary), mirrored:
        /// Cols 1=5, 2=4. '1' = color, '0' = white.
        /// </remarks>
        /// <returns>
        /// A string containing a 6-character hexadecimal color code, followed by a sequence of characters representing the identicon's pixels.
        /// </returns>
        public static string GenerateIdenticon()
        {
            var sb = new StringBuilder();

            string color = string.Format("{0:X6}", _random.Next(0x1000000));
            sb.Append(color);

            var identiconPixels = new char[_pixelsLength];
            for (int i = 0; i < _pixelsLength; i++)
            {
                int randomIndex = _random.Next(_pixelVals.Length);
                identiconPixels[i] = _pixelVals[randomIndex];
            }
            sb.Append(new string(identiconPixels));

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
