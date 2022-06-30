using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

//
// MD5 encryption/decryption algorithms written by Geo for ArcSysLib.
// https://github.com/Geordan9/ArcSysLib
//
namespace P4U2TrialEditor.Util
{
    internal static class MD5Util
    {
        private static readonly byte[] P4U2_KEY = {
            0x71, 0x59, 0x7A, 0xBA, 0xC5, 0x04, 0x08, 0x9D, 0x73, 0x90, 0xB7,
            0xFE, 0x29, 0x95, 0xFF, 0xE0, 0x6A, 0x01, 0x3F, 0xFB, 0xB9, 0x3A,
            0x2C, 0x6E, 0xEC, 0x13, 0x96, 0xAF, 0xFF, 0xEB, 0xA4, 0x73, 0xD3,
            0x4E, 0x52, 0x19, 0xE8, 0xAD, 0x27
        };

        /// <summary>
        /// Generates a MD5 hash of a given string.
        /// </summary>
        /// <param name="name">String to hash</param>
        /// <returns>MD5 hash (hex string)</returns>
        public static string MD5Hash(string name)
        {
            return Convert.ToHexString(MD5.Create().ComputeHash(
                Encoding.ASCII.GetBytes(name)));
        }

        /// <summary>
        /// MD5 cryptography algorithm.
        /// (Reverse engineering done by Geo)
        /// </summary>
        /// <param name="res">Resource name (for hashing)</param>
        /// <param name="src">Source buffer</param>
        /// <param name="dst">Destination buffer</param>
        private static void MD5Crypt(string res, in byte[] src, ref byte[] dst)
        {
            Debug.Assert(src.Length == dst.Length);

            int keyIdx = Encoding.ASCII.GetBytes(MD5Hash(res))[7] % P4U2_KEY.Length;
            for (int i = 0; i < src.Length; i++, keyIdx++)
            {
                dst[i] = (byte)(P4U2_KEY[keyIdx % P4U2_KEY.Length] ^ src[i]);
            }
        }
    }
}