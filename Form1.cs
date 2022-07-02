using P4U2TrialEditor.Core;
using P4U2TrialEditor.Util;

namespace P4U2TrialEditor
{
    public partial class Form1 : Form
    {
        // Currently open mission file
        private MissionFile? m_OpenFile = null;
        // Path to currently open file
        private string? m_OpenFilePath = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Query file to open
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.FileName = "Select a trial file";
            dialog.Filter = "Trial script (*.txt/*.ang)|*.txt;*.ang|All files|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Open(dialog.FileName);
            }
        }

        /// <summary>
        /// Attempt to open mission file from path
        /// </summary>
        /// <param name="path">Path to mission file</param>
        /// <returns></returns>
        private bool Open(string path)
        {
            MissionFile.Error err;
            MissionFile? file = MissionFile.Open(path, out err);

            switch(err)
            {
                case MissionFile.Error.IO_FAIL:
                    MessageBox.Show("The file could not be opened.", "File Error");
                    m_OpenFile = null;
                    m_OpenFilePath = null;
                    return false;
                case MissionFile.Error.DESERIALIZE_FAIL:
                    MessageBox.Show("The trial script is not formatted correctly.", "Script Error");
                    m_OpenFile = null;
                    m_OpenFilePath = null;
                    return false;
            }

            m_OpenFile = file;
            m_OpenFilePath = path;
            UpdateTreeView();
            return true;
        }

        /// <summary>
        /// Update tree view of mission file
        /// </summary>
        private void UpdateTreeView()
        {
            if (m_OpenFile == null)
            {
                return;
            }

            m_MissionView.BeginUpdate();
            {
                // Remove existing nodes
                m_MissionView.Nodes.Clear();

                // Root node displays filename
                string rootName = (m_OpenFilePath != null)
                    ? m_OpenFilePath : "Trial Script";
                m_MissionView.Nodes.Add(rootName);
                TreeNode rootNode = m_MissionView.Nodes[0];

                // Lesson Mode
                rootNode.Nodes.Add("Lessons");
                TreeNode lessonRootNode = rootNode.Nodes[0];
                foreach (Mission m in m_OpenFile.GetLessons())
                {
                    lessonRootNode.Nodes.Add(String.Format("Lesson {0}", m.GetID()));
                }

                // Challenge Mode (trials)
                rootNode.Nodes.Add("Trials");
                TreeNode trialRootNode = rootNode.Nodes[1];
                for (int i = 0; i < (int)CharacterUtil.EChara.COMMON; i++)
                {
                    // Character root node
                    trialRootNode.Nodes.Add(String.Format("{0} ({1})",
                        CharacterUtil.GetCharaName((CharacterUtil.EChara)i),
                        CharacterUtil.GetCharaResID((CharacterUtil.EChara)i)));
                    TreeNode charaRootNode = trialRootNode.Nodes[i];

                    // Character trials
                    foreach (Mission m
                        in m_OpenFile.GetCharaTrials((CharacterUtil.EChara)i))
                    {
                        charaRootNode.Nodes.Add(String.Format("Trial {0}", m.GetID()));
                    }
                }
            }
            m_MissionView.EndUpdate();
        }
    }
}