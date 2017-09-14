using System;
using System.Security.Cryptography;
using System.Text;

namespace netcore.helpers
{
    /// <summary>
    /// Wrapper on MD5 Hash
    /// </summary>
    public static class MD5Helpers
    {
        /// <summary>
        /// Generate a MD 5 Hash base on input text
        /// </summary>
        /// <param name="input">String to hash</param>
        /// <returns>The Hash of input</returns>
        public static String GetMd5Hash(string input)
        {
            using (var md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                var sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                foreach (var t in data)
                {
                    sBuilder.Append(t.ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

        /// <summary>
        /// Verify a hash against a string.
        /// </summary>
        /// <param name="md5Hash">Hash to verify</param>
        /// <param name="input">Text to compare to hash</param>
        /// <param name="hash">Hash type</param>
        /// <returns>True if Hash equal to Input Hash</returns>
        public static Boolean VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            var hashOfInput = GetMd5Hash(input);

            // Create a StringComparer an compare the hashes.
            var comparer = StringComparer.OrdinalIgnoreCase;

            return 0 == comparer.Compare(hashOfInput, hash);
        }
    }
}
