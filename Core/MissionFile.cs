using System.Diagnostics;
using P4U2TrialEditor.Util;

namespace P4U2TrialEditor.Core
{
    internal class MissionFile
    {
        // Lesson Mode missions
        private List<Mission> m_Lessons;

        // Challenge Mode missions
        private Dictionary<CharacterUtil.EChara, List<Mission>> m_Trials;

        public MissionFile()
        {
            m_Lessons = new List<Mission>();
            m_Trials = new Dictionary<CharacterUtil.EChara, List<Mission>>();

            // Initialize dictionary
            foreach (CharacterUtil.EChara chara
                in Enum.GetValues(typeof(CharacterUtil.EChara)))
            {
                m_Trials.Add(chara, new List<Mission>());
            }
        }

        /// <summary>
        /// Create mission file from path to ArcSys formatted data.
        /// </summary>
        /// <param name="path">Path to data</param>
        /// <returns>File if successfully opened, otherwise null</returns>
        public static MissionFile? ArcSysOpen(string path)
        {
            try
            {
                MissionFile file = new MissionFile();
                if (!file.ArcSysDeserialize(File.ReadAllLines(path)))
                {
                    return null;
                }

                return file;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Deserialize mission file from text form.
        /// ArcSys format is expected.
        /// </summary>
        /// <param name="script">Mission file</param>
        /// <returns>Success</returns>
        public bool ArcSysDeserialize(string[] script)
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
                if (!ArcSysParseLessonSection(script, sp, out lessonSize))
                {
                    return false;
                }
                sp += lessonSize;

                int trialsSize;
                if (!ArcSysParseTrials(script, sp, out trialsSize))
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
        public bool ArcSysParseLessonSection(string[] script, int sp, out int size)
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
            while (!script[sp].StartsWith("----Char----"))
            {
                Mission lesson = new Mission();
                if (!lesson.ArcSysDeserialize(script, sp, out size))
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
        public bool ArcSysParseTrials(string[] script, int sp, out int size)
        {
            Debug.Assert(sp < script.Length);

            // Start pos
            int start = sp;

            // Validate section header
            string[] tokens = script[sp].Split("\t");
            if (tokens.Length != 2
                || tokens[0] != "----Char----")
            {
                size = -1;
                return false;
            }

            while (sp < script.Length
                && script[sp].StartsWith("----Char----"))
            {
                int charSize;
                if (!ArcSysParseCharSection(script, sp, out charSize))
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
        public bool ArcSysParseCharSection(string[] script, int sp, out int size)
        {
            Debug.Assert(sp < script.Length);

            // Start pos
            int start = sp;

            // Validate section header
            string[] tokens = script[sp++].Split("\t");
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
            while (!script[sp].StartsWith("----Char----"))
            {
                Mission trial = new Mission();
                if (!trial.ArcSysDeserialize(script, sp, out size))
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
    }
}
