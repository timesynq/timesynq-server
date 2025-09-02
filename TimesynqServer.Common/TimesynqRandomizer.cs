using System.Text;

namespace TimesynqServer.Common
{
    /// <summary>
    /// Static methods for randomized values used across Timesynq
    /// </summary>
    public static class TimesynqRandomizer
    {
        private static readonly Random _random = new();

        private const int _randomCodeLength = 8;
        private const string _characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

        /// <summary>
        /// Generates a random unsigned 32 bit integer, which encodes an identicon.
        /// </summary>
        /// <remarks>
        /// Bit 0 is ignored.
        /// Bit 1-15 = 5x5 grid (binary), mirroed:
        /// Bits 16 - 31 encodes an RGB565 color
        /// The bits for the color are read in reverse since it's more convenient that way and the order is not relevant
        /// </remarks>
        /// <returns>
        /// An unsigned 32 bit integer in the range 1,000,000,000 and 4,294,967,295 
        /// </returns>
        public static uint GenerateIdenticon()
        {
            return (uint)_random.NextInt64(1000000000, uint.MaxValue);
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
