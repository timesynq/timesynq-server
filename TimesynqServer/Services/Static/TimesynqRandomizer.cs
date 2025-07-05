using System.Text;

namespace TimesynqServer.Services.Static
{
    public static class TimesynqRandomizer
    {
        private static readonly Random _random = new();

        private const int _pixelsLength = 15;
        private const string _pixelVals = "00111";

        private const int _randomCodeLength = 8;
        private const string _characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

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
