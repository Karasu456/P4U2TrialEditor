using System.Text;
using System.Diagnostics;

namespace P4U2TrialEditor.Core
{
    public class StreamReaderEx : StreamReader
    {
        // Cached next line
        private string? m_PeekCache = null;

        // Cache status
        private bool m_DoCache = false;
        // Cached data
        private List<string> m_Cache = new List<string>();

        #region Constructors

        public StreamReaderEx(Stream strm)
            : base(strm)
        {
        }

        public StreamReaderEx(Stream strm, Encoding enc)
            : base(strm, enc)
        {
        }

        public StreamReaderEx(string path)
            : base(path)
        {
        }

        #endregion Constructors

        #region Accessors

        public List<string> GetCache()
        {
            return m_Cache;
        }

        #endregion Accessors

        /// <summary>
        /// Start caching all stream reads
        /// </summary>
        /// <returns>Success</returns>
        public bool BeginCache()
        {
            if (m_DoCache)
            {
                Debug.Assert(false, "BeginCache not allowed");
                return false;
            }

            // Clear old cache
            m_Cache.Clear();
            m_DoCache = true;
            return true;
        }

        /// <summary>
        /// End caching all stream reads
        /// </summary>
        public void EndCache()
        {
            Debug.Assert(m_DoCache, "EndCache not allowed");
            m_DoCache = false;
        }

        /// <summary>
        /// Reads and returns the next available line.
        /// </summary>
        /// <returns>Next line if it exists, otherwise null.</returns>
        public override string? ReadLine()
        {
            // Return cached line if it exists from a previous peek operation
            if (m_PeekCache != null)
            {
                string? temp = m_PeekCache;
                m_PeekCache = null;

                if (m_DoCache && temp != null)
                {
                    m_Cache.Add(temp);
                }

                return temp;
            }

            // If the stream will advance, cache the data if applicable
            string? line = base.ReadLine();
            if (m_DoCache && line != null)
            {
                m_Cache.Add(line);
            }

            return line;
        }

        /// <summary>
        /// Returns the next available line but does not consume it.
        /// </summary>
        /// <returns>Next line if it exists, otherwise null.</returns>
        public string? PeekLine()
        {
            // Line is retrieved through read, but is cached such that
            // the next ReadLine call does not need to advance the stream position.
            if (m_PeekCache == null)
            {
                m_PeekCache = base.ReadLine();
            }

            return m_PeekCache;
        }

        /// <summary>
        /// Attempts to find the specified token in the stream data.
        /// If the token is found, the stream position will point to it.
        /// </summary>
        /// <param name="token">Search token</param>
        /// <returns>true if token was found, otherwise false</returns>
        public bool Find(string token)
        {
            while (!EndOfStream)
            {
                if (PeekLine()!.Contains(token))
                {
                    return true;
                }

                ReadLine();
            }

            return false;
        }

        /// <summary>
        /// Attempts to find a line in the stream data consisting of only the provided token.
        /// If the token is found, the stream position will point to it.
        /// </summary>
        /// <param name="token">Search token</param>
        /// <returns>true if token was found, otherwise false</returns>
        public bool FindExact(string token)
        {
            while (!EndOfStream)
            {
                if (PeekLine()! == token)
                {
                    return true;
                }

                ReadLine();
            }

            return false;
        }

        /// <summary>
        /// Advance stream to the first non-whitespace line
        /// </summary>
        /// <returns>true if the line was found, false if the end of the stream was hit</returns>
        public bool Trim()
        {
            // Read until non-whitespace
            while (string.IsNullOrWhiteSpace(PeekLine()))
            {
                // Check ONLY for EOF; advance stream
                if (ReadLine() == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
