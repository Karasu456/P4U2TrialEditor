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

        public Mission()
        {
            m_Chara = CharacterUtil.EChara.COMMON;
            m_ID = -1;
            m_Flags = Flag.NONE;
            m_ActionList = new List<Action>();
        }

        /// <summary>
        /// Deserialize mission from text form.
        /// Format is expected to be ArcSys
        /// </summary>
        /// <param name="script">Mission script</param>
        /// <param name="end">End position of mission data</param>
        /// <returns>Success</returns>
        public bool ArcSysDeserialize(string[] script, out int end)
        {
            int sp = 0;
            string[] tokens;

            try
            {
                for (; sp < script.Length;)
                {
                    // Find mission header
                    while (!script[sp++].StartsWith("-MISSION-"))
                    {
                    }

                    // Parse mission header
                    int id;
                    tokens = script[sp++].Split("\t");
                    if (tokens.Length != 2
                        || !int.TryParse(tokens[1], out id))
                    {
                        goto InvalidScript;
                    }
                    m_ID = id;

                    // Read until action list header
                    while (!script[sp].Equals("-LIST-"))
                    {
                        // Parse mission flags/settings
                        tokens = script[sp++].Split("\t");

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
                                int hp;
                                if (!int.TryParse(tokens[1], out hp))
                                {
                                    goto InvalidScript;
                                }
                                m_HP = hp;
                                break;

                            case "SP":
                                int _sp;
                                if (!int.TryParse(tokens[1], out _sp))
                                {
                                    goto InvalidScript;
                                }
                                m_SP = _sp;
                                break;

                            case "Burst":
                                int burst;
                                if (!int.TryParse(tokens[1], out burst))
                                {
                                    goto InvalidScript;
                                }
                                m_Burst = burst;
                                break;

                            case "ChiesCharge":
                                int powerCharge;
                                if (!int.TryParse(tokens[1], out powerCharge))
                                {
                                    goto InvalidScript;
                                }
                                m_CE_PowerCharge = powerCharge;
                                break;
                            //YosukesSukukaja
                            //YukikosFireBooster
                            //YukikosFireGuardKill
                            //NaotosFate
                            //KumasItem
                            //KumasItemFix
                            //KumasItem2
                            //KumasItem2Fix
                            //KumasSPItemFastRecover
                            //AkihikosThunder
                            //AegissOrgia
                            //AegissBulletsMax
                            //AegissBulletsRecover
                            //LabryssAxInitial
                            //ShadowLabryssProgramFastRecover
                            //JunpeisPoint
                            //JunpeisFullCount
                            //JunpeisOtakebi
                            //RisesMarking
                            //RisesTetraFastRecover
                            //RisesBitFastRecover
                            //KorosHP
                            //AdachisHeat
                            //AdachisYodomi
                            //MariesOtenki

                            // Invalid token
                            default:
                                break;
                        }
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
            }

        // Script is invalid
        InvalidScript:
            end = sp;
            return false;
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