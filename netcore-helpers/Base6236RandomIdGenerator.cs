using System;
using System.Text;

namespace netcore.helpers
{
    /// <summary>
    /// Base 32 or Base 36 Id generator
    /// </summary>
    /// <remarks>
    /// Base 62 uniqueness metrics
    /// 5 chars in base 62 will give you 62^5 unique IDs = 916,132,832 (~1 billion) At 10k IDs per day you will be ok for 91k+ days
    /// 6 chars in base 62 will give you 62^6 unique IDs = 56,800,235,584 (56+ billion) At 10k IDs per day you will be ok for 5+ million days
    /// Base 36 uniqueness metrics
    /// 6 chars will give you 36^6 unique IDs = 2,176,782,336 (2+ billion)
    /// 7 chars will give you 36^7 unique IDs = 78,364,164,096 (78+ billion)
    /// </remarks>
    public static class Base6236RandomIdGenerator
    {
        private static readonly char[] _base62chars =
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
                .ToCharArray();

        private static readonly Random _random = new Random();

        /// <summary>
        /// Generate a random Base 62 code
        /// </summary>
        /// <example>
        /// 
        /// for lenght = 5
        /// z5KyMg
        /// wd4SUp
        /// uSzQtH
        /// 
        /// </example>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetBase62(int length)
        {
            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
                sb.Append(_base62chars[_random.Next(62)]);

            return sb.ToString();
        }

        /// <summary>
        /// Generate a random Base 36 code
        /// </summary>
        /// <example>
        /// 
        /// for lenght = 8
        /// QCF9GNM5
        /// 0UV3TFSS
        /// 3MG91VKP
        /// 
        /// </example>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetBase36(int length)
        {
            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
                sb.Append(_base62chars[_random.Next(36)]);

            return sb.ToString();
        }
    }
}
