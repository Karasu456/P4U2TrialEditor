using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P4U2TrialEditor.Util;

namespace P4U2TrialEditor.Core
{
    internal class Mission
    {
        // Mission character
        private CharacterUtil.EChara m_Chara { get; set; }
        // Mission ID
        private int m_ID { get; set; }
        // Mission flags
        private Flag m_Flags;
        // Mission variables set
        private Setting m_Settings;
        // Mission actions
        private List<Action> m_ActionList;

        public Mission()
        {
            m_Chara = CharacterUtil.EChara.COMMON;
            m_ID = -1;
            m_Flags = Flag.NONE;
            m_Settings = Setting.NONE;
            m_ActionList = new List<Action>();
        }

        public int ANGDeserialize(string[] script)
        {
            // Opcode
            int sp = 0;

            // Trim leading whitespace
            while (string.IsNullOrWhiteSpace(script[sp++])) {}

            // Mission header
            string[] missionTokens = script[sp++].Split("\t");
            if (missionTokens[0] != "-MISSION-")
            {
                return -1;
            }

            return sp;
        }

        #region Flags

        /// <summary>
        /// Check if a mission flag is set
        /// </summary>
        /// <param name="f">Flag</param>
        /// <returns>Whether the flag is set</returns>
        public bool HasFlag(Flag f)
        {
            return (m_Flags & f) != 0;
        }

        /// <summary>
        /// Set a mission flag
        /// </summary>
        /// <param name="f">Flag</param>
        public void SetFlag(Flag f)
        {
            m_Flags |= f;
        }

        #endregion

        #region Settings

        /// <summary>
        /// Check if a mission variable was given a value
        /// </summary>
        /// <param name="s">Setting</param>
        /// <returns>Whether the setting was given a value</returns>
        public bool HasSetting(Setting s)
        {
            return (m_Settings & s) != 0;
        }

        /// <summary>
        /// Record that a mission variable has been given a value
        /// </summary>
        /// <param name="s">Setting</param>
        public void SetSetting(Setting s)
        {
            m_Settings |= s;
        }

        #endregion

        public enum Flag
        {
            NONE = 0,

            // Players are spaced far apart
            SPAWN_FAR = (1 << 0),
            // Players spawn near corner
            SPAWN_SIDE = (1 << 1),
            // Players spawn at corner
            SPAWN_CORNER = (1 << 2),

            // Player begins in Awakening state
            PLAYER_AWAKENING = (1 << 3),
            // Player begins with IK available
            PLAYER_IK_AVAIL = (1 << 4),

            // AI jumps
            DUMMY_JUMP = (1 << 5),
            // AI attacks
            DUMMY_ATTACK = (1 << 6),
            // AI crouches
            DUMMY_CROUCH = (1 << 7),
            // AI has fear ailment
            DUMMY_FEAR = (1 << 8),
            // AI recovers HP when combo ends
            DUMMY_HP_RECOVER = (1 << 9),

            // Player has Panic ailment
            AILMENT_PANIC = (1 << 10),
            // Player has Shock ailment
            AILMENT_SHOCK = (1 << 11),
            // ALL PLAYERS have Persona Break
            AILMENT_BREAK = (1 << 12),

            // TO-DO
            GLOBAL_SP_MAX = (1 << 13),
            // All players recover HP when combo ends
            GLOBAL_HP_RECOVER = (1 << 14),
            // Meter does not auto decrease during Shadow Frenzy
            GLOBAL_ALWAYS_FRENZY = (1 << 15),
            // TO-DO (Attack must connect?)
            GLOBAL_NO_MISS = (1 << 16),
            // TO-DO
            GLOBAL_NO_DMG_MISS = (1 << 17),
            // Start with Counter Hit
            GLOBAL_CH_START = (1 << 18),
            // TO-DO
            GLOBAL_CH_START_ND = (1 << 19),
            // Use every move
            GLOBAL_ALL_MOVES = (1 << 20),

            // Doesn't seem to have an effect
            GLOBAL_DEBUG = (1 << 21)
        }
        public enum Setting
        {
            NONE = 0
        };
    }
}
