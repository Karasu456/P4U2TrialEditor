using System.Diagnostics;
using System.Numerics;

//
// ANG encryption/decryption algorithms written by Geo for GeoArcSysAIOCLITool.
// https://github.com/Geordan9/GeoArcSysAIOCLITool
//
namespace P4U2TrialEditor.Util
{
    public static class ANGUtil
    {
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
        private static void ANGCrypt(byte[] buf, bool encrypt)
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
        /// Encrypt data with ANG algorithm.
        /// </summary>
        /// <param name="buf">Source buffer</param>
        public static void ANGEncrypt(byte[] buf)
        {
            ANGCrypt(buf, true);
        }

        /// <summary>
        /// Decrypt data with ANG algorithm.
        /// </summary>
        /// <param name="buf">Source buffer</param>
        public static void ANGDecrypt(byte[] buf)
        {
            ANGCrypt(buf, false);
        }
    }
}