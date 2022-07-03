using System.Diagnostics;
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

        public MissionFile()
        {
            // Initialize dictionary
            foreach (CharacterUtil.EChara chara
                in Enum.GetValues(typeof(CharacterUtil.EChara)))
            {
                m_Trials.Add(chara, new List<Mission>());
            }
        }

        public enum Error
        {
            NONE,
            IO_FAIL,
            DESERIALIZE_FAIL,
        };

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

            string[] script;
            try
            {
                script = File.ReadAllLines(path);
            }
            catch (Exception)
            {
                err = Error.IO_FAIL;
                return null;
            }

            if (!file.Deserialize(script))
            {
                err = Error.DESERIALIZE_FAIL;
                return null;
            }

            err = Error.NONE;
            return file;
        }

        /// <summary>
        /// Deserialize mission file from text form.
        /// ArcSys format is expected.
        /// </summary>
        /// <param name="script">Mission file</param>
        /// <returns>Success</returns>
        public bool Deserialize(string[] script)
        {
            int sp = 0;

            try
            {
                // Find Lesson Mode header
                while (script[sp] != "----Lesson----")
                {
                    sp++;
                }

                // Parse lessons
                int lessonSize;
                if (!ParseLessonSection(script, sp, out lessonSize))
                {
                    return false;
                }
                sp += lessonSize;

                int trialsSize;
                if (!ParseTrials(script, sp, out trialsSize))
                {
                    return false;
                }
                sp += trialsSize;

                return true;
            }
            catch (IndexOutOfRangeException)
            {
            }

            return false;
        }

        /// <summary>
        /// Parse lesson section of mission file
        /// </summary>
        /// <param name="script">Misson script</param>
        /// <param name="sp">Script position</param>
        /// <param name="size">Lesson section size</param>
        /// <returns>Success</returns>
        public bool ParseLessonSection(string[] script, int sp, out int size)
        {
            Debug.Assert(sp < script.Length);

            // Start pos
            int start = sp;

            // Validate section header
            if (script[sp++] != "----Lesson----")
            {
                size = -1;
                return false;
            }

            // Parse lessons until trial header (or EOF)
            while (sp < script.Length
                && !script[sp].StartsWith("----Char----")
                && script[sp].Trim() != "----End----")
            {
                Mission lesson = new Mission();
                if (!lesson.Deserialize(script, sp, out size))
                {
                    return false;
                }

                m_Lessons.Add(lesson);
                sp += size;

                if (sp > script.Length)
                {
                    break;
                }

                // Skip post-lesson whitespace
                while (script[sp] == string.Empty
                    && ++sp < script.Length)
                {
                }
            }

            size = sp - start;
            return true;
        }

        /// <summary>
        /// Parse trials from mission file
        /// </summary>
        /// <param name="script">Mission script</param>
        /// <param name="sp">Script position</param>
        /// <param name="size">Trials size</param>
        /// <returns></returns>
        public bool ParseTrials(string[] script, int sp, out int size)
        {
            Debug.Assert(sp < script.Length);

            // Start pos
            int start = sp;

            // Validate section header
            string[] tokens = script[sp].Split();
            if (tokens.Length != 2
                || tokens[0] != "----Char----")
            {
                size = -1;
                return false;
            }

            while (sp < script.Length
                && script[sp].StartsWith("----Char----")
                && script[sp].Trim() != "----End----")
            {
                int charSize;
                if (!ParseCharSection(script, sp, out charSize))
                {
                    size = -1;
                    return false;
                }
                sp += charSize;

                // Skip post-trial whitespace
                while (script[sp] == string.Empty
                    && ++sp < script.Length)
                {
                }
            }

            size = sp - start;
            return true;
        }

        /// <summary>
        /// Parse char section of mission file
        /// </summary>
        /// <param name="script">Mission script</param>
        /// <param name="sp">Script position</param>
        /// <param name="size">Char section size</param>
        /// <returns></returns>
        public bool ParseCharSection(string[] script, int sp, out int size)
        {
            Debug.Assert(sp < script.Length);

            // Start pos
            int start = sp;

            // Validate section header
            string[] tokens = script[sp++].Split();
            if (tokens.Length != 2
                || tokens[0] != "----Char----")
            {
                size = -1;
                return false;
            }

            // Character for trials
            CharacterUtil.EChara chara
                = CharacterUtil.GetCharaEnum(tokens[1]);
            Debug.Assert(chara != CharacterUtil.EChara.COMMON);

            // Parse trials until next char header (or EOF)
            while (sp < script.Length
                && !script[sp].StartsWith("----Char----")
                && script[sp].Trim() != "----End----")
            {
                Mission trial = new Mission();
                if (!trial.Deserialize(script, sp, out size))
                {
                    size = -1;
                    return false;
                }

                trial.SetCharacter(chara);
                m_Trials[chara].Add(trial);
                sp += size;

                if (sp > script.Length)
                {
                    break;
                }
            }

            size = sp - start;
            return true;
        }

        #endregion ArcSys Format
    }
}
