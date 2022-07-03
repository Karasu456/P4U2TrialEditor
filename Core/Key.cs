using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P4U2TrialEditor.Core
{
    public class Key
    {
        // Stick input (numpad notation)
        int m_Stick = 5;
        // Button input
        Button m_Buttons = Button.NONE;
        // Input duration
        int m_Duration = 0;

        #region Accessors

        public void SetStick(int stick)
        {
            m_Stick = stick;
        }

        public void SetButton(Button b)
        {
            m_Buttons |= b;
        }

        public bool CheckButton(Button b)
        {
            return (m_Buttons & b) != 0;
        }

        public void SetDuration(int duration)
        {
            m_Duration = duration;
        }

        #endregion Accessors

        public enum Button
        {
            NONE = 0,

            A = (1 << 0),
            B = (1 << 1),
            C = (1 << 2),
            D = (1 << 3)
        };

        public enum Type
        {
            PLAYER,
            ENEMY
        };
    }
}
