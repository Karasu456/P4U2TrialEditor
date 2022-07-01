using System.Diagnostics;
using P4U2TrialEditor.Util;

namespace P4U2TrialEditor.Core
{
    internal class Mission
    {
        // Mission character
        private CharacterUtil.EChara m_Chara;
        // Mission ID
        private int m_ID;
        // Mission flags
        private Flag m_Flags;

        #region Mission Vars

        // Player HP
        private int m_HP = -1;

        // Player SP
        private int m_SP = -1;

        // Player Burst
        private int m_Burst = -1;

        // Chie's Power Charge
        private int m_CE_PowerCharge = -1;

        // Yosuke's Sukukaja
        private int m_YO_Sukukaja = -1;

        // Yukiko's Fire Counter
        private int m_YU_FireCounter = -1;

        // Yukiko's Fire Break
        private int m_YU_FireBreak = -1;

        // Naoto's Fate
        private int m_NA_Fate = -1;

        // Teddie's Item
        private int m_KU_Item = -1;

        // Teddie's ItemFix
        private int m_KU_ItemFix = -1;

        // Teddie's Item2
        private int m_KU_Item2 = -1;

        // Teddie's Item2Fix
        private int m_KU_Item2Fix = -1;

        // Teddie's SPItemFastRecover
        private int m_KU_SPItemFastRecover = -1;

        // Akihiko's Thunder
        private int m_AK_Thunder = -1;

        // Aegis's Orgia
        private int m_AG_Orgia = -1;

        // Aegis's BulletsMax
        private int m_AG_BulletsMax = -1;

        // Aegis's BulletsRecover
        private int m_AG_BulletsRecover = -1;

        // Labrys's AxInitial
        private int m_LA_AxInitial = -1;

        // Shadow Labrys's ProgramFastRecover
        private int m_LS_ProgramFastRecover = -1;

        // Junpei's Point
        private int m_JU_Point = -1;

        // Junpei's Full Count
        private int m_JU_FullCount = -1;

        // Junpei's Otakebi (Victory Cry?)
        private int m_JU_Otakebi = -1;

        // Rise's Marking
        private int m_RI_Marking = -1;

        //Rise's TetraFastRecover
        private int m_RI_TetraFastRecover = -1;

        // Rise's BitFastRecover
        private int m_RI_BitFastRecover = -1;

        // Koro's HP
        private int m_AM_KoroHP = -1;

        // Adachi's Heat Riser
        private int m_AD_Heat = -1;

        // Adachi's Magatsu Mandala
        private int m_AD_Yodomi = -1;

        // Marie's Weather
        private int m_MR_Weather = -1;

        #endregion Mission Vars

        // Mission actions
        private List<Action> m_ActionList;

        // Trial demonstration
        private List<Key> m_Keys;
        // AI inputs
        private List<Key> m_EnemyKeys;

        public Mission()
        {
            m_Chara = CharacterUtil.EChara.COMMON;
            m_ID = -1;
            m_Flags = Flag.NONE;
            m_ActionList = new List<Action>();
            m_Keys = new List<Key>();
            m_EnemyKeys = new List<Key>();
        }

        #region Accessors

        public CharacterUtil.EChara GetCharacter()
        {
            return m_Chara;
        }

        public void SetCharacter(CharacterUtil.EChara chara)
        {
            m_Chara = chara;
        }

        public int GetID()
        {
            return m_ID;
        }

        public void SetID(int id)
        {
            m_ID = id;
        }

        #endregion Accessors

        #region ArcSys format

        /// <summary>
        /// Deserialize mission from text form.
        /// ArcSys format is expected.
        /// </summary>
        /// <param name="script">Mission script</param>
        /// <param name="sp">Script position</param>
        /// <param name="size">Size of mission data</param>
        /// <returns>Success</returns>
        public bool ArcSysDeserialize(string[] script, int sp, out int size)
        {
            // Start pos
            int start = sp;

            try
            {
                // Find mission header
                while (!script[sp].StartsWith("-MISSION-"))
                {
                    sp++;
                }

                // Parse mission section
                int missionSize;
                if (!ArcSysParseMissionSection(script, sp, out missionSize))
                {
                    size = -1;
                    return false;
                }
                sp += missionSize;

                // Parse action list
                int listSize;
                if (!ArcSysParseListSection(script, sp, out listSize))
                {
                    size = -1;
                    return false;
                }
                sp += listSize;

                switch(script[sp])
                {
                    // Trial demonstration
                    case "-KEY-":
                        int keySize;
                        if (!ArcSysParseKeySection(script, sp, out keySize))
                        {
                            size = -1;
                            return false;
                        }
                        sp += keySize;
                        break;
                    // Enemy input
                    case "-EKEY-":
                        int enemyKeySize;
                        if (!ArcSysParseEnemyKeySection(script, sp, out enemyKeySize))
                        {
                            size = -1;
                            return false;
                        }
                        sp += enemyKeySize;
                        break;
                    // End of mission/unknown delimiter
                    // Sometimes the devs forgot to separate missions with newlines :)
                    case "":
                    default:
                        break;
                }

                size = sp - start;
                return true;
            }
            catch (IndexOutOfRangeException)
            {
            }

            size = -1;
            return false;
        }

        /// <summary>
        /// Parse mission section from script.
        /// ArcSys format is expected.
        /// </summary>
        /// <param name="script">Mission script</param>
        /// <param name="sp">Script position</param>
        /// <param name="size">Mission section size</param>
        /// <returns></returns>
        public bool ArcSysParseMissionSection(string[] script, int sp, out int size)
        {
            Debug.Assert(sp < script.Length);

            // Start pos
            int start = sp;

            // Parse mission header
            if (!ArcSysParseMissionHeader(script[sp++]))
            {
                size = -1;
                return false;
            }

            // Read until action list header
            while (script[sp] != "-LIST-")
            {
                // Parse mission flags/settings
                if (!ArcSysParseMissionFlag(script[sp++])
                    || sp >= script.Length)
                {
                    size = -1;
                    return false;
                }
            }

            size = sp - start;
            return true;
        }

        /// <summary>
        /// Parse mission header.
        /// ArcSys format is expected.
        /// </summary>
        /// <param name="header">Mission header</param>
        /// <returns>Success</returns>
        public bool ArcSysParseMissionHeader(string header)
        {
            // Validate section header
            if (!header.StartsWith("-MISSION-"))
            {
                return false;
            }

            int id;
            string[] tokens = header.Split("\t");

            if (tokens.Length != 2
                || !int.TryParse(tokens[1], out id))
            {
                return false;
            }

            m_ID = id;
            return true;
        }

        /// <summary>
        /// Parse mission flag/setting from tokens.
        /// ArcSys format is expected.
        /// </summary>
        /// <param name="script">Mission script line</param>
        /// <returns>Success</returns>
        public bool ArcSysParseMissionFlag(string script)
        {
            // Ignore whitespace and comments
            if (string.IsNullOrWhiteSpace(script)
                || script == string.Empty
                || script.TrimStart().StartsWith("//"))
            {
                return true;
            }

            int val;
            string[] tokens = script.Split("\t");

            switch (tokens[0])
            {
                // Spawn
                case "Tooi":
                    m_Flags |= Flag.SPAWN_FAR;
                    break;

                case "Hashi":
                    m_Flags |= Flag.SPAWN_SIDE;
                    break;

                case "HashiEx":
                    m_Flags |= Flag.SPAWN_CORNER;
                    break;

                // Player
                case "Kakusei":
                    m_Flags |= Flag.PLAYER_AWAKENING;
                    break;

                case "Ichigeki":
                    m_Flags |= Flag.PLAYER_IK_AVAIL;
                    break;

                // Enemy
                case "Jump":
                    m_Flags |= Flag.ENEMY_JUMP;
                    break;

                case "Attack":
                    m_Flags |= Flag.ENEMY_ATTACK;
                    break;

                case "Crouch":
                    m_Flags |= Flag.ENEMY_CROUCH;
                    break;

                case "EnemyKyofu":
                    m_Flags |= Flag.ENEMY_FEAR;
                    break;

                case "HPRecoverEnemyOnly":
                    m_Flags |= Flag.ENEMY_HP_RECOVER;
                    break;

                // Ailment
                case "Konran":
                    m_Flags |= Flag.AILMENT_PANIC;
                    break;

                case "Kanden":
                    m_Flags |= Flag.AILMENT_SHOCK;
                    break;

                case "PersonaBreak":
                    m_Flags |= Flag.AILMENT_BREAK;
                    break;

                // Global
                case "SPMax":
                    m_Flags |= Flag.GLOBAL_SP_MAX;
                    break;

                case "HPRecover":
                    m_Flags |= Flag.GLOBAL_HP_RECOVER;
                    break;

                case "HeatUpForever":
                    m_Flags |= Flag.GLOBAL_ALWAYS_FRENZY;
                    break;

                case "NoMiss":
                    m_Flags |= Flag.GLOBAL_NO_MISS;
                    break;

                case "ForceNoDamageMiss":
                    m_Flags |= Flag.GLOBAL_NO_DMG_MISS;
                    break;

                case "Counter":
                    m_Flags |= Flag.GLOBAL_CH_START;
                    break;

                case "CounterND":
                    m_Flags |= Flag.GLOBAL_CH_START_ND;
                    break;

                case "Junhudo":
                    m_Flags |= Flag.GLOBAL_ALL_MOVES;
                    break;

                // Mission variables
                case "HP":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_HP = Math.Max(val, 0);
                    break;

                case "SP":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_SP = Math.Max(val, 0);
                    break;

                case "Burst":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_Burst = Math.Max(val, 0);
                    break;

                case "ChiesCharge":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_CE_PowerCharge = Math.Max(val, 0);
                    break;

                case "YosukesSukukaja":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_YO_Sukukaja = Math.Max(val, 0);
                    break;

                case "YukikosFireBooster":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_YU_FireCounter = Math.Max(val, 0);
                    break;

                case "YukikosFireGuardKill":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_YU_FireBreak = Math.Max(val, 0);
                    break;

                case "NaotosFate":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_NA_Fate = Math.Max(val, 0);
                    break;

                case "KumasItem":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_KU_Item = Math.Max(val, 0);
                    break;

                case "KumasItemFix":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_KU_ItemFix = Math.Max(val, 0);
                    break;

                case "KumasItem2":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_KU_Item2 = Math.Max(val, 0);
                    break;

                case "KumasItem2Fix":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_KU_Item2Fix = Math.Max(val, 0);
                    break;

                case "KumasSPItemFastRecover":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_KU_SPItemFastRecover = Math.Max(val, 0);
                    break;

                case "AkihikosThunder":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_AK_Thunder = Math.Max(val, 0);
                    break;

                case "AegissOrgia":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_AG_Orgia = Math.Max(val, 0);
                    break;

                case "AegissBulletsMax":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_AG_BulletsMax = Math.Max(val, 0);
                    break;

                case "AegissBulletsRecover":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_AG_BulletsRecover = Math.Max(val, 0);
                    break;

                case "LabryssAxInitial":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_LA_AxInitial = Math.Max(val, 0);
                    break;

                case "ShadowLabryssProgramFastRecover":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_LS_ProgramFastRecover = Math.Max(val, 0);
                    break;

                case "JunpeisPoint":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_JU_Point = Math.Max(val, 0);
                    break;

                case "JunpeisFullCount":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_JU_FullCount = Math.Max(val, 0);
                    break;

                case "JunpeisOtakebi":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_JU_Otakebi = Math.Max(val, 0);
                    break;

                case "RisesMarking":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_RI_Marking = Math.Max(val, 0);
                    break;

                case "RisesTetraFastRecover":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_RI_TetraFastRecover = Math.Max(val, 0);
                    break;

                case "RisesBitFastRecover":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_RI_BitFastRecover = Math.Max(val, 0);
                    break;

                case "KorosHP":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_AM_KoroHP = Math.Max(val, 0);
                    break;

                case "AdachisHeat":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_AD_Heat = Math.Max(val, 0);
                    break;

                case "AdachisYodomi":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_AD_Yodomi = Math.Max(val, 0);
                    break;

                case "MariesOtenki":
                    if (!int.TryParse(tokens[1], out val))
                    {
                        return false;
                    }
                    m_MR_Weather = Math.Max(val, 0);
                    break;

                // Invalid token
                default:
                    Debug.Assert(false, "Invalid token");
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Parse action list from script.
        /// ArcSys format is expected.
        /// </summary>
        /// <param name="script">Mission script</param>
        /// <param name="sp">Script position</param>
        /// <param name="size">List section size</param>
        /// <returns>Success</returns>
        public bool ArcSysParseListSection(string[] script, int sp, out int size)
        {
            Debug.Assert(sp < script.Length);

            // Start pos
            int start = sp;

            // Validate section header
            if (script[sp++] != "-LIST-")
            {
                size = -1;
                return false;
            }

            // Parse actions until next section (or end of mission)
            while (script[sp] != string.Empty
                && script[sp][0] != '-')
            {
                if (!ArcSysParseAction(script[sp])
                    || ++sp >= script.Length)
                {
                    size = -1;
                    return false;
                }
            }

            size = sp - start;
            return true;
        }

        /// <summary>
        /// Parse action from tokens.
        /// ArcSys format is expected.
        /// </summary>
        /// <param name="action">Action</param>
        /// <returns>Success</returns>
        public bool ArcSysParseAction(string action)
        {
            // Ignore whitespace and comments
            if (string.IsNullOrWhiteSpace(action)
                || action == string.Empty
                || action.TrimStart().StartsWith("//"))
            {
                return true;
            }

            Action act = new Action();
            string[] tokens = action.Split("\t");

            // Initial action
            act.SetMsgID(tokens[0]);

            // Extra tokens
            for (int i = 1; i < tokens.Length; i++)
            {
                // Flags apply to the last action
                Action actForFlags = act.GetAltActions().Count != 0
                    ? act.GetAltActions().Last() : act;

                // Alternative action
                if (tokens[i].StartsWith("|"))
                {
                    Action alt = new Action();
                    alt.SetMsgID(tokens[i].Substring(1));
                    act.AppendAltAction(alt);
                }
                // Action name override
                else if (tokens[i].StartsWith("+"))
                {
                    act.SetMsgIDOverride(tokens[i].Substring(1));
                }
                // Damage requirement
                else if (tokens[i].StartsWith("d"))
                {
                    int dmg;
                    if (!int.TryParse(tokens[i].Substring(1), out dmg))
                    {
                        return false;
                    }
                    actForFlags.SetFlag(Action.Flag.DAMAGECOUNT);
                    actForFlags.SetDamageRequirement(dmg);
                }
                // Hit count requirement
                else if (tokens[i].StartsWith("h"))
                {
                    int hits;
                    if (!int.TryParse(tokens[i].Substring(1), out hits))
                    {
                        return false;
                    }
                    actForFlags.SetFlag(Action.Flag.HITCOUNT);
                    actForFlags.SetHitsRequirement(hits);
                }
                else
                {
                    switch(tokens[i])
                    {
                        case "i":
                            actForFlags.SetFlag(Action.Flag.FLAG_I);
                            break;
                        case "a":
                            actForFlags.SetFlag(Action.Flag.FLAG_A);
                            break;
                        case "p":
                            actForFlags.SetFlag(Action.Flag.FLAG_P);
                            break;
                        case "c":
                            actForFlags.SetFlag(Action.Flag.COMMONCHAR);
                            break;
                        case "iOnly":
                            actForFlags.SetFlag(Action.Flag.INPUTONLY);
                            break;
                        case "nOnly":
                            actForFlags.SetFlag(Action.Flag.NAMEONLY);
                            break;
                        case "n2miss":
                            actForFlags.SetFlag(Action.Flag.N2MISS);
                            break;
                        case "Special":
                            actForFlags.SetFlag(Action.Flag.SPECIAL);
                            break;
                        case "Nandodemo":
                            actForFlags.SetFlag(Action.Flag.NANDODEMO);
                            break;
                        case "nostylish":
                            actForFlags.SetFlag(Action.Flag.NOSTYLISH);
                            break;
                        // Unknown token
                        default:
                            return false;
                    }
                }
            }

            m_ActionList.Add(act);
            return true;
        }

        /// <summary>
        /// Parse player key list from script
        /// </summary>
        /// <param name="script">Mission script</param>
        /// <param name="sp">Script position</param>
        /// <param name="size">Key section size</param>
        /// <returns>Success</returns>
        public bool ArcSysParseKeySection(string[] script, int sp, out int size)
        {
            Debug.Assert(sp < script.Length);

            // Start pos
            int start = sp;

            if (script[sp++] != "-KEY-")
            {
                size = -1;
                return false;
            }

            // Parse actions until next section (or end of mission)
            while (script[sp] != string.Empty
                && script[sp][0] != '-')
            {
                if (!ArcSysParseKey(script[sp], Key.Type.PLAYER)
                    || ++sp >= script.Length)
                {
                    size = -1;
                    return false;
                }
            }

            size = sp - start;
            return true;
        }

        /// <summary>
        /// Parse enemy key list from script
        /// </summary>
        /// <param name="script">Mission script</param>
        /// <param name="sp">Script position</param>
        /// <param name="size">Key section size</param>
        /// <returns>Success</returns>
        public bool ArcSysParseEnemyKeySection(string[] script, int sp, out int size)
        {
            Debug.Assert(sp < script.Length);

            // Start pos
            int start = sp;

            // Validate section header
            if (script[sp++] != "-EKEY-")
            {
                size = -1;
                return false;
            }

            // Parse actions until next section (or end of mission)
            while (script[sp] != string.Empty
                && script[sp][0] != '-')
            {
                if (!ArcSysParseKey(script[sp], Key.Type.ENEMY)
                    || ++sp >= script.Length)
                {
                    size = -1;
                    return false;
                }
            }

            size = sp - start;
            return true;
        }

        /// <summary>
        /// Parse key input from tokens
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Key type (KEY/EKEY)</param>
        /// <returns>Success</returns>
        public bool ArcSysParseKey(string key, Key.Type type)
        {
            // Ignore whitespace and comments
            if (string.IsNullOrWhiteSpace(key)
                || key == string.Empty
                || key.TrimStart().StartsWith("//"))
            {
                return true;
            }

            Key k = new Key();
            string[] tokens = key.Split("\t");

            if (tokens.Length != 3)
            {
                Debug.Assert(false, "Invalid key input");
                return false;
            }

            // Stick input + key duration
            int stick, duration;
            if (!int.TryParse(tokens[0], out stick)
                || !int.TryParse(tokens[2], out duration))
            {
                return false;
            }
            k.SetStick(stick);
            k.SetDuration(duration);

            // Key buttons
            foreach (char btn in tokens[1])
            {
                switch(btn)
                {
                    case 'A':
                        k.SetButton(Key.Button.A);
                        break;
                    case 'B':
                        k.SetButton(Key.Button.B);
                        break;
                    case 'C':
                        k.SetButton(Key.Button.C);
                        break;
                    case 'D':
                        k.SetButton(Key.Button.D);
                        break;
                    case '-':
                        break;
                    default:
                        Debug.Assert(false, "Invalid key");
                        break;
                }
            }

            switch(type)
            {
                case Key.Type.PLAYER:
                    m_Keys.Add(k);
                    break;
                case Key.Type.ENEMY:
                    m_EnemyKeys.Add(k);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            return true;
        }

        #endregion ArcSys format

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
            ENEMY_JUMP = (1 << 5),

            // AI attacks
            ENEMY_ATTACK = (1 << 6),

            // AI crouches
            ENEMY_CROUCH = (1 << 7),

            // AI has fear ailment
            ENEMY_FEAR = (1 << 8),

            // AI recovers HP when combo ends
            ENEMY_HP_RECOVER = (1 << 9),

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
    }
}