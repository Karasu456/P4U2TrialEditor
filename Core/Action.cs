namespace P4U2TrialEditor.Core
{
    internal class Action
    {
        // Action message ID/"command"
        private string m_MsgID { get; set; }

        // Alternative actions (Logical OR)
        private List<Action> m_AltActions;

        // Action flags
        private Flag m_Flags;

        #region Flags

        /// <summary>
        /// Check if a mission flag is set
        /// </summary>
        /// <param name="f">Flag</param>
        /// <returns>Whether the flag is set</returns>
        public bool HasFlag(MissionFlag f)
        {
            return (m_Flags & f) != 0;
        }

        /// <summary>
        /// Set a mission flag
        /// </summary>
        /// <param name="f">Flag</param>
        public void SetFlag(MissionFlag f)
        {
            m_Flags |= f;
        }

        #endregion Flags

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

            // Action is common
            COMMONCHAR = (1 << 4),

            // Show input only
            INPUTONLY = (1 << 5),

            // Show name only
            NAMEONLY = (1 << 6),

            // Unknown
            N2MISS = (1 << 7),

            // Unknown
            SPECIAL = (1 << 8),

            // Unknown
            NANDODEMO = (1 << 9),

            // Disallow S Hold
            NOSTYLISH = (1 << 10)
        };
    }
}