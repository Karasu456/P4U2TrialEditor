using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P4U2TrialEditor.Core
{
    internal class Key
    {
        // Stick input
        int m_Stick;
        // Button input
        Button m_Buttons;
        // Input duration
        int m_Duration;

        public Key()
        {
            m_Stick = 5;
            m_Buttons = Button.BTN_NONE;
            m_Duration = 1;
        }

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
            BTN_NONE = 0,

            BTN_A = (1 << 0),
            BTN_B = (1 << 1),
            BTN_C = (1 << 2),
            BTN_D = (1 << 3)
        };
    }
}
