namespace LyricsModel
{
    using System.Security.Cryptography;
    using System.Text;

    internal static class Extensions
    {
        public static string CreateHash(string text)
        {
            using (var algorithm = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                var codes = algorithm.ComputeHash(bytes);

                var builder = new StringBuilder();
                foreach (var item in codes)
                {
                    builder.Append(item.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
