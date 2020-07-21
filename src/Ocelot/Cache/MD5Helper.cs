using System.Security.Cryptography;
using System.Text;

namespace Ocelot.Cache
{
    public static class MD5Helper
    {
        public static string GenerateMd5(byte[] contentBytes)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(contentBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }

        public static string GenerateMd5(string contentString)
        {
            byte[] contentBytes = Encoding.Unicode.GetBytes(contentString);
            return GenerateMd5(contentBytes);
        }
    }
}
