﻿using P4U2TrialEditor.Util;

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
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// Deserialize mission file from text form.
        /// ArcSys format is expected.
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public bool ArcSysDeserialize(string[] script)
        {
            int sp = 0;
            int size;

            try
            {
                // Find Lesson Mode header
                while (!script[sp].Equals("----Lesson----"))
                {
                    sp++;
                }

                // Skip header
                sp++;

                // Parse lessons
                while (!script[sp].StartsWith("----Char----"))
                {
                    Mission lesson = new Mission();

                    // Mission script
                    int missionSize = script.Length - sp;
                    string[] missionScript = new string[missionSize];
                    Array.Copy(script, sp, missionScript, 0, missionSize);

                    if (!lesson.ArcSysDeserialize(missionScript, out size))
                    {
                        return false;
                    }

                    m_Lessons.Add(lesson);

                    // Point script index to end of mission data
                    sp = sp + size;
                }

                // TO-DO: Parse trials
                return true;
            }
            catch (IndexOutOfRangeException e)
            {
            }

            return false;
        }
    }
}
