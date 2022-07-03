using System.Text;
using P4U2TrialEditor.Util;

namespace P4U2TrialEditor.Core
{
    public class MD5Stream : MemoryStream
    {
        #region Constructors

        public MD5Stream(string path)
            : this(path, File.ReadAllBytes(path))
        {
        }

        public MD5Stream(string res, string path)
            : this(res, File.ReadAllBytes(path))
        {
        }

        public MD5Stream(string res, byte[] buffer)
        {
            CryptUtil.CryptMD5(res, buffer);
            Write(buffer);
            Seek(0, SeekOrigin.Begin);
        }

        #endregion Constructors

        public StreamReaderEx StreamReaderEx()
        {
            return new StreamReaderEx(this, Encoding.ASCII);
        }
    }
}
