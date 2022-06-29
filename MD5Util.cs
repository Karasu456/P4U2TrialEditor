using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace P4U2TrialEditor
{
    internal static class MD5Util
    {
        public const string TRIAL_ANG_HASH = "1b1df6db6ea1d4af300ae30c7bbab937";

        public static string MD5Hash(string name)
        {
            return Convert.ToHexString(MD5.Create().ComputeHash(
                Encoding.ASCII.GetBytes(name)));
        }
    }
}
