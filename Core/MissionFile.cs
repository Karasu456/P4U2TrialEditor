using System.Diagnostics;
using System.Text;
using P4U2TrialEditor.Util;

namespace P4U2TrialEditor.Core
{
    public class MissionFile
    {
        // Lesson Mode missions
        private List<Mission> m_Lessons = new List<Mission>();
        // Challenge Mode missions
        private Dictionary<CharacterUtil.EChara, List<Mission>> m_Trials =
            new Dictionary<CharacterUtil.EChara, List<Mission>>();

        // Encryption type
        private Encryption m_Encryption = Encryption.NONE;

        public MissionFile()
        {
            // Initialize dictionary
            foreach (CharacterUtil.EChara chara
                in Enum.GetValues(typeof(CharacterUtil.EChara)))
            {
                m_Trials.Add(chara, new List<Mission>());
            }
        }

        #region Enums

        public enum Error
        {
            NONE,
            IO_FAIL,
            DESERIALIZE_FAIL,
        };

        public enum Encryption
        {
            NONE,
            ANG,
            MD5,
            MD5_ANG
        };

        #endregion Enums

        #region Accessors

        public List<Mission> GetLessons()
        {
            return m_Lessons;
        }

        public Dictionary<CharacterUtil.EChara, List<Mission>> GetTrials()
        {
            return m_Trials;
        }

        public List<Mission> GetCharaTrials(CharacterUtil.EChara chara)
        {
            List<Mission>? trials;
            if (!m_Trials.TryGetValue(chara, out trials)
                || trials == null)
            {
                return new List<Mission>();
            }

            return trials;
        }

        public Encryption GetEncryption()
        {
            return m_Encryption;
        }

        #endregion Accessors

        #region ArcSys Format

        /// <summary>
        /// Create mission file from path to ArcSys formatted data.
        /// </summary>
        /// <param name="path">Path to data</param>
        /// <param name="err">Error</param>
        /// <returns>File if successfully opened, otherwise null</returns>
        public static MissionFile? Open(string path, out Error err)
        {
            MissionFile file = new MissionFile();

            try
            {
                // Try interpreting file as plaintext
                using (StreamReaderEx reader = new StreamReaderEx(path))
                {
                    if (file.Deserialize(reader))
                    {
                        file.m_Encryption = Encryption.NONE;
                        err = Error.NONE;
                        return file;
                    }
                }

                // Try interpreting as ANG encrypted text
                using (StreamReaderEx reader =
                    new ANGStream(path, ANGStream.Mode.DECRYPT).StreamReaderEx())
                {
                    if (file.Deserialize(reader))
                    {
                        file.m_Encryption = Encryption.ANG;
                        err = Error.NONE;
                        return file;
                    }
                }

                // Try interpreting as MD5 encrypted text
                using (StreamReaderEx reader = new MD5Stream(path).StreamReaderEx())
                {
                    if (file.Deserialize(reader))
                    {
                        file.m_Encryption = Encryption.MD5;
                        err = Error.NONE;
                        return file;
                    }
                }

                // Try interpreting as MD5 encrypted ANG
                using (StreamReaderEx reader = new ANGStream(
                    new MD5Stream(path).ToArray(),
                    ANGStream.Mode.DECRYPT).StreamReaderEx())
                {
                    if (file.Deserialize(reader))
                    {
                        file.m_Encryption = Encryption.MD5_ANG;
                        err = Error.NONE;
                        return file;
                    }
                }

                // All attempts to read the data have failed
                err = Error.DESERIALIZE_FAIL;
                return file;
            }
            catch (Exception)
            {
                err = Error.IO_FAIL;
                return null;
            }
        }

        /// <summary>
        /// Write mission file contents to filepath.
        /// Data uses ArcSys format.
        /// </summary>
        /// <param name="path">Output filepath</param>
        /// <returns>Success</returns>
        public bool Write(string path)
        {
            try
            {
                // Build text data
                StringBuilder builder = new StringBuilder();

                // Lesson data
                builder.Append("----Lesson----");
                foreach (Mission m in m_Lessons)
                {
                    builder.AppendJoin("\n", m.GetRawText());
                }

                // Trial data
                for (int i = 0; i < (int)CharacterUtil.EChara.COMMON; i++)
                {
                    CharacterUtil.EChara chara = (CharacterUtil.EChara)i;
                    builder.Append(String.Format("----Char---- {0}",
                        CharacterUtil.GetCharaResID(chara)));

                    foreach (Mission m in m_Trials[chara])
                    {
                        builder.AppendJoin("\n", m.GetRawText());
                    }
                }

                // Write plaintext data
                File.WriteAllText(path, builder.ToString());

                // Encrypt data accordingly
                using (FileStream strm = new FileStream(path, FileMode.Create))
                {
                    switch (m_Encryption)
                    {
                        case Encryption.NONE:
                            break;
                        case Encryption.ANG:
                            using (ANGStream astrm = new ANGStream(path, ANGStream.Mode.ENCRYPT))
                            {
                                astrm.WriteTo(strm);
                            }
                            break;
                        case Encryption.MD5:
                            using (MD5Stream mstrm = new MD5Stream("data/trial/trial.ang", path))
                            {
                                mstrm.WriteTo(strm);
                            }
                            break;
                        case Encryption.MD5_ANG:
                            using (ANGStream astrm = new ANGStream(path, ANGStream.Mode.ENCRYPT))
                            {
                                using (MD5Stream mstrm = new MD5Stream("data/trial/trial.ang", astrm.ToArray()))
                                {
                                    mstrm.WriteTo(strm);
                                }
                            }
                            break;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Deserialize mission file from text form.
        /// </summary>
        /// <param name="reader">Stream to script</param>
        /// <returns>Success</returns>
        private bool Deserialize(StreamReaderEx reader)
        {
            if (reader.EndOfStream)
            {
                return false;
            }

            // Find Lesson Mode header
            if (!reader.FindExact("----Lesson----"))
            {
                return false;
            }

            // Parse lessons
            if (!ParseLessonSection(reader))
            {
                return false;
            }

            if (!ParseTrials(reader))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Deserialize lesson section of mission file.
        /// </summary>
        /// <param name="reader">Stream to script</param>
        /// <returns>Success</returns>
        private bool ParseLessonSection(StreamReaderEx reader)
        {
            if (reader.EndOfStream)
            {
                return false;
            }

            // Validate section header
            if (reader.ReadLine()! != "----Lesson----")
            {
                return false;
            }

            // Parse lessons until trial header (or EOF)
            while (reader.PeekLine() != null
                && !reader.PeekLine()!.StartsWith("----Char----")
                && reader.PeekLine()!.Trim() != ("----End----"))
            {
                Mission lesson = new Mission();
                if (!lesson.Deserialize(reader))
                {
                    return false;
                }
                m_Lessons.Add(lesson);

                // Skip post-lesson whitespace
                reader.Trim();
            }

            return true;
        }

        /// <summary>
        /// Deserialize trials section of mission file.
        /// </summary>
        /// <param name="reader">Stream to script</param>
        /// <returns>Success</returns>
        private bool ParseTrials(StreamReaderEx reader)
        {
            if (reader.EndOfStream)
            {
                return false;
            }

            // Validate section header
            if (!reader.PeekLine()!.StartsWith("----Char----"))
            {
                return false;
            }

            // Parse lessons until trial header (or EOF)
            while (reader.PeekLine() != null
                && reader.PeekLine()!.StartsWith("----Char----")
                && reader.PeekLine()!.Trim() != ("----End----"))
            {
                if (!ParseCharSection(reader))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Deserialize char section of mission file.
        /// </summary>
        /// <param name="reader">Stream to script</param>
        /// <returns>Success</returns>
        private bool ParseCharSection(StreamReaderEx reader)
        {
            if (reader.EndOfStream)
            {
                return false;
            }

            // Validate section header
            string[] tokens = reader.ReadLine()!.Split();
            if (tokens.Length != 2
                || tokens[0] != "----Char----")
            {
                return false;
            }

            // Character for trials
            CharacterUtil.EChara chara
                = CharacterUtil.GetCharaEnum(tokens[1]);
            Debug.Assert(chara != CharacterUtil.EChara.COMMON);

            // Parse lessons until trial header (or EOF)
            while (reader.PeekLine() != null
                && !reader.PeekLine()!.StartsWith("----Char----")
                && reader.PeekLine()!.Trim() != ("----End----"))
            {
                Mission trial = new Mission();
                if (!trial.Deserialize(reader))
                {
                    return false;
                }

                trial.SetCharacter(chara);
                m_Trials[chara].Add(trial);

                // Skip post-trial whitespace
                reader.Trim();
            }

            return true;
        }

        #endregion ArcSys Format
    }
}
