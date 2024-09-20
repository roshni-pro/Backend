using System;
using System.Security.Cryptography;
using System.Text;

namespace AngularJSAuthentication.BatchManager.Helpers
{
    public class HashHelper
    {
        public static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // var ticks = DateTime.Now.Ticks;
            input += Guid.NewGuid().ToString();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        public static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GenerateChecksum(string json)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(json));
                return BitConverter.ToString(bytes);
            }
        }
    }
}
