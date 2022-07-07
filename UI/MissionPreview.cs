using P4U2TrialEditor.Core;
using P4U2TrialEditor.Util;
using Action = P4U2TrialEditor.Core.Action;

namespace P4U2TrialEditor.UI
{
    public partial class MissionPreview : RichTextBox
    {
        private Mission? m_Mission = null;

        public MissionPreview()
        {
            InitializeComponent();
        }

        public void Load(Mission m)
        {
            m_Mission = m;

            Clear();

            // Mission flags/vars
            Text = "Mission Preview\n";
            CheckFlag(Mission.Flag.SPAWN_FAR, "Spawn far apart\n");
            CheckFlag(Mission.Flag.SPAWN_SIDE, "Spawn near corner\n");
            CheckFlag(Mission.Flag.SPAWN_CORNER, "Spawn at corner\n");
            CheckFlag(Mission.Flag.PLAYER_AWAKENING, "Player awakening\n");
            CheckFlag(Mission.Flag.PLAYER_IK_AVAIL, "IK available\n");
            CheckFlag(Mission.Flag.ENEMY_JUMP, "Enemy will jump\n");
            CheckFlag(Mission.Flag.ENEMY_ATTACK, "Enemy will attack\n");
            CheckFlag(Mission.Flag.ENEMY_CROUCH, "Enemy will crouch\n");
            CheckFlag(Mission.Flag.ENEMY_FEAR, "Enemy has Fear ailment\n");
            CheckFlag(Mission.Flag.ENEMY_HP_RECOVER, "Enemy recovers HP after combo\n");
            CheckFlag(Mission.Flag.AILMENT_PANIC, "Player has Panic ailment\n");
            CheckFlag(Mission.Flag.AILMENT_SHOCK, "Player has Shock ailment\n");
            CheckFlag(Mission.Flag.AILMENT_BREAK, "Everyone has Persona Break\n");
            CheckFlag(Mission.Flag.GLOBAL_SP_MAX, "SPMax\n");
            CheckFlag(Mission.Flag.GLOBAL_HP_RECOVER, "All players recover HP after combo\n");
            CheckFlag(Mission.Flag.GLOBAL_ALWAYS_FRENZY, "Shadow Frenzy meter does not auto decrease\n");
            CheckFlag(Mission.Flag.GLOBAL_NO_MISS, "NoMiss\n");
            CheckFlag(Mission.Flag.GLOBAL_NO_DMG_MISS, "ForceNoDamageMiss\n");
            CheckFlag(Mission.Flag.GLOBAL_CH_START, "Start with Counter Hit\n");
            CheckFlag(Mission.Flag.GLOBAL_CH_START_ND, "CounterND\n");
            CheckFlag(Mission.Flag.GLOBAL_ALL_MOVES, "Use every move\n");
            CheckFlag(Mission.Flag.GLOBAL_DEBUG, "Debug\n");

            // Action list
            Text += "\n";
            foreach (Action a in m.GetActionList())
            {
                // Ignore mission character for common actions
                CharacterUtil.EChara chara =
                    (a.HasFlag(Core.Action.Flag.COMMONCHAR))
                    ? CharacterUtil.EChara.COMMON
                    : m.GetCharacter();

                if (a.HasFlag(Action.Flag.INPUTONLY))
                {
                    Text += a.GetInputMessage(chara) + "\n";
                }
                else if (a.HasFlag(Action.Flag.NAMEONLY))
                {
                    Text += a.GetMessage(chara) + "\n";
                }
                else
                {
                    Text += a.GetMessage(chara)
                        + " - "
                        + a.GetInputMessage(chara)
                        + "\n";
                }
            }
        }

        private void CheckFlag(Mission.Flag f, string text)
        {
            if (m_Mission!.HasFlag(f))
            {
                Text += text;
            }
        }
    }
}
