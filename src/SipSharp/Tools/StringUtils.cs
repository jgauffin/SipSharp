using System.Security.Cryptography;
using System.Text;

namespace SipSharp.Tools
{
    public static class StringUtils
    {
        public static string GetMd5Hash(string value)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(Encoding.ASCII.GetBytes(value));

            var sb = new StringBuilder();
            foreach (byte b in result)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public static string ToMd5Hash(this string value)
        {
            return GetMd5Hash(value);
        }
    }
}