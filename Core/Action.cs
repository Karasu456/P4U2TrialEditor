using P4U2TrialEditor.Util;

namespace P4U2TrialEditor.Core
{
    public class Action
    {
        // Action message ID/"command"
        private string m_MsgID = "";
        // Action message can be overridden with "+"
        private string m_MsgIDOverride = "";

        // Alternative actions (Logical OR)
        private List<Action> m_AltActions = new List<Action>();

        // Action flags
        private Flag m_Flags = Flag.NONE;

        // Damage requirement
        private int m_NumDmg = -1;
        // Hit count requirement
        private int m_NumHits = -1;

        #region Accessors

        public string GetMsgID()
        {
            return m_MsgID;
        }

        public void SetMsgID(string id)
        {
            m_MsgID = id;
        }

        public string GetMsgIDOverride()
        {
            return m_MsgIDOverride;
        }

        public void SetMsgIDOverride(string id)
        {
            m_MsgIDOverride = id;
        }

        public List<Action> GetAltActions()
        {
            return m_AltActions;
        }

        public void AppendAltAction(Action act)
        {
            m_AltActions.Add(act);
        }

        public void SetDamageRequirement(int dmg)
        {
            m_NumDmg = dmg;
        }

        public void SetHitsRequirement(int hits)
        {
            m_NumHits = hits;
        }

        #endregion Accessors

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

        #endregion Flags

        /// <summary>
        /// Get action message based on its ID and the supplied character
        /// </summary>
        /// <param name="chara">Character</param>
        /// <returns>Full action message</returns>
        public string GetMessage(CharacterUtil.EChara chara)
        {
            string id = (string.IsNullOrEmpty(m_MsgIDOverride)) ? m_MsgID : m_MsgIDOverride;
            return ActionUtil.GetCharaActionMsg(id, chara);
        }

        /// <summary>
        /// Get action input message based on its ID and the supplied character
        /// </summary>
        /// <param name="chara">Character</param>
        /// <returns>Full action message</returns>
        public string GetInputMessage(CharacterUtil.EChara chara)
        {
            string id = (string.IsNullOrEmpty(m_MsgIDOverride)) ? m_MsgID : m_MsgIDOverride;
            return ActionUtil.GetCharaActionMsg(id + "i", chara);
        }

        public enum Flag
        {
            NONE = 0,

            // Damage requirement
            DAMAGECOUNT = (1 << 0),

            // Hit count requirement
            HITCOUNT = (1 << 1),

            // Unknown
            FLAG_I = (1 << 2),

            // Unknown
            FLAG_A = (1 << 3),

            // Unknown
            FLAG_P = (1 << 4),

            // Action is common
            COMMONCHAR = (1 << 5),

            // Show input only
            INPUTONLY = (1 << 6),

            // Show name only
            NAMEONLY = (1 << 7),

            // Unknown
            N2MISS = (1 << 8),

            // Unknown
            SPECIAL = (1 << 9),

            // Unknown
            NANDODEMO = (1 << 10),

            // Disallow S Hold
            NOSTYLISH = (1 << 11)
        };
    }
}