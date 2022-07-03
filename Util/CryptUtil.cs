using System.Text;
using System.Security.Cryptography;

namespace P4U2TrialEditor.Util
{
    //
    // MD5 encryption/decryption algorithms written by Geo for ArcSysLib.
    // https://github.com/Geordan9/ArcSysLib
    // 
    // ANG encryption/decryption algorithms written by Geo for GeoArcSysAIOCLITool.
    // https://github.com/Geordan9/GeoArcSysAIOCLITool
    //

    public class CryptUtil
    {
        #region MD5

        private static readonly byte[] P4U2_MD5_KEY = {
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
        public static string HashMD5(string name)
        {
            return Convert.ToHexString(MD5.Create().ComputeHash(
                Encoding.ASCII.GetBytes(name)));
        }

        /// <summary>
        /// MD5 cryptography algorithm.
        /// (Reverse engineering done by Geo)
        /// </summary>
        /// <param name="res">Resource name (for hashing)</param>
        /// <param name="buf">Source buffer</param>
        public static void CryptMD5(string res, byte[] buf)
        {
            int keyIdx = Encoding.ASCII.GetBytes(HashMD5(res))[7] % P4U2_MD5_KEY.Length;
            for (int i = 0; i < buf.Length; i++, keyIdx++)
            {
                buf[i] = (byte)(P4U2_MD5_KEY[keyIdx % P4U2_MD5_KEY.Length] ^ buf[i]);
            }
        }

        /// <summary>
        /// MD5 cryptography algorithm.
        /// (Reverse engineering done by Geo)
        /// </summary>
        /// <param name="res">Resource name (for hashing)</param>
        /// <param name="buf">Source buffer</param>
        public static byte[] CryptMD5(string res, in byte[] buf)
        {
            byte[] result = new byte[buf.Length];
            buf.CopyTo(result, 0);
            CryptMD5(res, result);
            return result;
        }

        #endregion MD5

        #region ANG

        /// <summary>
        /// Rotate left byte
        /// </summary>
        /// <param name="x">Byte value</param>
        /// <param name="n">Bit count to rotate</param>
        /// <returns></returns>
        private static byte __rol(byte x, byte n)
        {
            return (byte)((x << n) | (x >> (8 - n)));
        }

        /// <summary>
        /// ANG cryptography algorithm.
        /// (Reverse engineering done by Geo)
        /// </summary>
        /// <param name="buf">Source buffer</param>
        /// <param name="encrypt">Whether to encrypt/decrypt</param>
        private static void CryptANG(byte[] buf, bool encrypt)
        {
            byte key = 0x7B;
            for (int i = 0; i < buf.Length; i++)
            {
                byte dec = (byte)(__rol(key, 1) ^ buf[i] ^ i);
                key = encrypt ? dec : buf[i];
                buf[i] = dec;
            }
        }

        /// <summary>
        /// Encrypt data using the ANG algorithm
        /// </summary>
        /// <param name="buf">Input buffer</param>
        public static void EncryptANG(byte[] buf)
        {
            CryptANG(buf, true);
        }

        /// <summary>
        /// Decrypt data using the ANG algorithm
        /// </summary>
        /// <param name="buf">Input buffer</param>
        public static void DecryptANG(byte[] buf)
        {
            CryptANG(buf, false);
        }

        #endregion ANG

        #region MD5 and ANG

        /// <summary>
        /// Encrypt data first with ANG, then MD5
        /// </summary>
        /// <param name="res">Resource/file name for MD5 hashing</param>
        /// <param name="buf">Input buffer</param>
        /// <returns>Encrypted data</returns>
        public static byte[] EncryptMD5Ang(string res, in byte[] buf)
        {
            byte[] result = new byte[buf.Length];
            buf.CopyTo(result, 0);
            CryptANG(result, true);
            CryptMD5(res, result);
            return result;
        }

        /// <summary>
        /// Decrypt data first with MD5, then ANG
        /// </summary>
        /// <param name="res">Resource/file name for MD5 hashing</param>
        /// <param name="buf">Input buffer</param>
        /// <returns>Decrypted data</returns>
        public static byte[] DecryptMD5Ang(string res, in byte[] buf)
        {
            byte[] result = new byte[buf.Length];
            buf.CopyTo(result, 0);
            CryptMD5(res, result);
            CryptANG(result, false);
            return result;
        }

        #endregion MD5 and ANG
    }
}
