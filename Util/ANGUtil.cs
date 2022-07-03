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
        /// ANG cryptography algorithm.
        /// (Reverse engineering done by Geo)
        /// </summary>
        /// <param name="src">Source buffer</param>
        /// <param name="dst">Destination buffer</param>
        /// <param name="encrypt">Whether to encrypt/decrypt</param>
        private static void ANGCrypt(in byte[] src, ref byte[] dst, bool encrypt)
        {
            Debug.Assert(src.Length == dst.Length);

            byte key = 0x7B;
            for (int i = 0; i < src.Length; i++)
            {
                dst[i] = (byte)(BitOperations.RotateLeft(key, 1) ^ src[i] ^ i);
                key = encrypt ? dst[i] : src[i];
            }
        }

        /// <summary>
        /// Encrypt data with ANG algorithm.
        /// </summary>
        /// <param name="src">Source buffer</param>
        /// <param name="dst">Destination buffer</param>
        public static void ANGEncrypt(in byte[] src, ref byte[] dst)
        {
            ANGCrypt(src, ref dst, true);
        }

        /// <summary>
        /// Decrypt data with ANG algorithm.
        /// </summary>
        /// <param name="src">Source buffer</param>
        /// <param name="dst">Destination buffer</param>
        public static void ANGDecrypt(in byte[] src, ref byte[] dst)
        {
            ANGCrypt(src, ref dst, false);
        }
    }
}