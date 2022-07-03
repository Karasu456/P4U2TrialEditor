using System.Text;
using P4U2TrialEditor.Util;

namespace P4U2TrialEditor.Core
{
    public class ANGStream : MemoryStream
    {
        public enum Mode
        {
            ENCRYPT,
            DECRYPT
        }

        #region Constructors

        public ANGStream(string path, Mode mode)
            : this(File.ReadAllBytes(path), mode)
        {
        }

        public ANGStream(byte[] buffer, Mode mode)
        {
            switch (mode)
            {
                case Mode.ENCRYPT:
                    CryptUtil.EncryptANG(buffer);
                    break;
                case Mode.DECRYPT:
                    CryptUtil.DecryptANG(buffer);
                    break;
            }

            Write(buffer);
            Seek(0, SeekOrigin.Begin);
        }

        #endregion Constructors

        public StreamReader StreamReader()
        {
            return new StreamReader(this, Encoding.ASCII);
        }
    }
}
