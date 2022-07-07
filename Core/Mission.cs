using System.Diagnostics;
using P4U2TrialEditor.Util;

namespace P4U2TrialEditor.Core
{
    public class Mission
    {
        // Mission character
        private CharacterUtil.EChara m_Chara = CharacterUtil.EChara.COMMON;
        // Mission ID
        private int m_ID = -1;
        // Mission flags
        private Flag m_Flags = Flag.NONE;

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
        private List<Action> m_ActionList = new List<Action>();

        // Trial demonstration
        private List<Key> m_Keys = new List<Key>();
        // AI inputs
        private List<Key> m_EnemyKeys = new List<Key>();

        // Raw text of mission data
        private List<string> m_RawText = new List<string>();

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

        public List<Action> GetActionList()
        {
            return m_ActionList;
        }

        public List<string> GetRawText()
        {
            return m_RawText;
        }

        public void SetRawText(List<String> text)
        {
            m_RawText = text;
        }

        #endregion Accessors

        #region ArcSys format

        /// <summary>
        /// Deserialize mission from mission file.
        /// </summary>
        /// <param name="reader">Stream to script</param>
        /// <returns>Success</returns>
        public bool Deserialize(StreamReaderEx reader)
        {
            if (reader.EndOfStream)
            {
                return false;
            }

            // Cache raw text for editor
            reader.BeginCache();
            {
                // Find mission header
                if (!reader.Find("-MISSION-"))
                {
                    reader.EndCache();
                    return false;
                }

                // Parse mission section
                if (!ParseMissionSection(reader))
                {
                    reader.EndCache();
                    return false;
                }

                // Parse action list
                if (!ParseListSection(reader))
                {
                    reader.EndCache();
                    return false;
                }

                // Parse optional sections
                string? section = reader.PeekLine();
                if (section != null)
                {
                    switch (section.Trim())
                    {
                        // Trial demonstration
                        case "-KEY-":
                            if (!ParseKeySection(reader))
                            {
                                reader.EndCache();
                                return false;
                            }
                            break;
                        // Enemy input
                        case "-EKEY-":
                            if (!ParseEnemyKeySection(reader))
                            {
                                reader.EndCache();
                                return false;
                            }
                            break;
                        // End of mission/unknown delimiter
                        default:
                            break;
                    }
                }

                // Skip trailing whitespace
                reader.Trim();
            }
            reader.EndCache();

            // Copy script
            m_RawText.Clear();
            m_RawText.AddRange(reader.GetCache());

            return true;
        }

        /// <summary>
        /// Deserialize mission section of mission script.
        /// </summary>
        /// <param name="reader">Stream to script</param>
        /// <returns>Success</returns>
        private bool ParseMissionSection(StreamReaderEx reader)
        {
            if (reader.EndOfStream)
            {
                return false;
            }

            // Parse mission header
            if (!ParseMissionHeader(reader))
            {
                return false;
            }

            // Read until action list header
            while (!reader.EndOfStream
                && reader.PeekLine() != "-LIST-")
            {
                ParseMissionFlag(reader);
                reader.Trim();
            }

            reader.Trim();
            return true;
        }

        /// <summary>
        /// Parse mission header of mission script.
        /// </summary>
        /// <param name="reader">Stream to script</param>
        /// <returns>Success</returns>
        private bool ParseMissionHeader(StreamReaderEx reader)
        {
            if (reader.EndOfStream)
            {
                return false;
            }

            // Validate header
            string header = reader.ReadLine()!;
            if (!header.Trim().StartsWith("-MISSION-"))
            {
                return false;
            }

            // Mission ID
            int id;
            string[] tokens = header.Split();
            if (tokens.Length != 2
                || !int.TryParse(tokens[1], out id))
            {
                return false;
            }

            m_ID = id;
            return true;
        }

        /// <summary>
        /// Parse mission flag/setting
        /// </summary>
        /// <param name="reader">Stream to script</param>
        /// <returns>Success</returns>
        private bool ParseMissionFlag(StreamReaderEx reader)
        {
            if (reader.EndOfStream)
            {
                return false;
            }

            string flag = reader.ReadLine()!;

            // Ignore whitespace and comments
            if (string.IsNullOrWhiteSpace(flag)
                || flag == string.Empty
                || flag.TrimStart().StartsWith("//"))
            {
                return true;
            }

            int val;
            string[] tokens = flag.Split();

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
                default:
                    Console.WriteLine("Invalid mission token: {0}", tokens[0]);
                    return false;
            }

            reader.Trim();
            return true;
        }

        /// <summary>
        /// Parse action list from mission script
        /// </summary>
        /// <param name="reader">Stream to script</param>
        /// <returns>Success</returns>
        private bool ParseListSection(StreamReaderEx reader)
        {
            if (reader.EndOfStream)
            {
                return false;
            }

            // Validate section header
            if (reader.ReadLine()!.Trim() != "-LIST-")
            {
                return false;
            }

            // Parse actions until next section (or end of mission)
            while (!reader.EndOfStream
                && reader.PeekLine()! != string.Empty
                && reader.PeekLine()![0] != '-')
            {
                ParseAction(reader);
            }

            reader.Trim();
            return true;
        }

        /// <summary>
        /// Parse action from mission script
        /// </summary>
        /// <param name="reader">Stream to script</param>
        /// <returns>Success</returns>
        private bool ParseAction(StreamReaderEx reader)
        {
            if (reader.EndOfStream)
            {
                return false;
            }

            string action = reader.ReadLine()!;

            // Ignore whitespace and comments
            if (string.IsNullOrWhiteSpace(action)
                || action == string.Empty
                || action.TrimStart().StartsWith("//"))
            {
                return true;
            }

            string[] tokens = action.Split();

            // Initial action
            Action act = new Action();
            act.SetMsgID(tokens[0]);

            // Extra tokens
            for (int i = 1; i < tokens.Length; i++)
            {
                // Ignore whitespace and comments
                if (string.IsNullOrWhiteSpace(tokens[i])
                    || tokens[i] == string.Empty
                    || tokens[i].TrimStart().StartsWith("//"))
                {
                    continue;
                }

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
                    act.SetFlag(Action.Flag.DAMAGECOUNT);
                    act.SetDamageRequirement(dmg);
                }
                // Hit count requirement
                else if (tokens[i].StartsWith("h"))
                {
                    int hits;
                    if (!int.TryParse(tokens[i].Substring(1), out hits))
                    {
                        return false;
                    }
                    act.SetFlag(Action.Flag.HITCOUNT);
                    act.SetHitsRequirement(hits);
                }
                else
                {
                    switch(tokens[i])
                    {
                        case "i":
                            act.SetFlag(Action.Flag.FLAG_I);
                            break;
                        case "a":
                            act.SetFlag(Action.Flag.FLAG_A);
                            break;
                        case "p":
                            act.SetFlag(Action.Flag.FLAG_P);
                            break;
                        case "c":
                            act.SetFlag(Action.Flag.COMMONCHAR);
                            break;
                        case "iOnly":
                            act.SetFlag(Action.Flag.INPUTONLY);
                            break;
                        case "nOnly":
                            act.SetFlag(Action.Flag.NAMEONLY);
                            break;
                        case "n2miss":
                            act.SetFlag(Action.Flag.N2MISS);
                            break;
                        case "Special":
                            act.SetFlag(Action.Flag.SPECIAL);
                            break;
                        case "Nandodemo":
                            act.SetFlag(Action.Flag.NANDODEMO);
                            break;
                        case "nostylish":
                            act.SetFlag(Action.Flag.NOSTYLISH);
                            break;
                        // Unknown token
                        default:
                            return true;
                    }
                }
            }

            m_ActionList.Add(act);
            return true;
        }

        /// <summary>
        /// Parse player key list from script
        /// </summary>
        /// <param name="reader">Stream to script</param>
        /// <returns>Success</returns>
        private bool ParseKeySection(StreamReaderEx reader)
        {
            if (reader.EndOfStream)
            {
                return false;
            }

            // Validate section header
            if (reader.ReadLine()!.Trim() != "-KEY-")
            {
                return false;
            }

            // Parse actions until next section (or end of mission)
            while (!reader.EndOfStream
                && reader.PeekLine()! != string.Empty
                && reader.PeekLine()![0] != '-')
            {
                ParseKey(reader, Key.Type.PLAYER);
            }

            return true;
        }

        /// <summary>
        /// Parse enemy key list from script
        /// </summary>
        /// <param name="reader">Stream to script</param>
        /// <returns>Success</returns>
        public bool ParseEnemyKeySection(StreamReaderEx reader)
        {
            if (reader.EndOfStream)
            {
                return false;
            }

            // Validate section header
            if (reader.ReadLine()!.Trim() != "-EKEY-")
            {
                return false;
            }

            // Parse actions until next section (or end of mission)
            while (!reader.EndOfStream
                && reader.PeekLine()! != string.Empty
                && reader.PeekLine()![0] != '-')
            {
                ParseKey(reader, Key.Type.ENEMY);
            }

            return true;
        }

        /// <summary>
        /// Parse key input from tokens
        /// </summary>
        /// <param name="reader">Stream to script</param>
        /// <param name="type">Key type (KEY/EKEY)</param>
        /// <returns>Success</returns>
        public bool ParseKey(StreamReaderEx reader, Key.Type type)
        {
            if (reader.EndOfStream)
            {
                return false;
            }

            string key = reader.ReadLine()!;

            // Ignore whitespace and comments
            if (string.IsNullOrWhiteSpace(key)
                || key == string.Empty
                || key.TrimStart().StartsWith("//"))
            {
                return true;
            }

            Key k = new Key();
            string[] tokens = key.Split();

            if (tokens.Length != 3)
            {
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
                    default:
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