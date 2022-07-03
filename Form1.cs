using System.Diagnostics;
using System.Text;
using System.IO;
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
        // Name of currently open file
        private string? m_OpenFileName = null;

        // Currently selected mission
        private Mission? m_CurrentMission = null;

        public Form1()
        {
            InitializeComponent();
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
            m_OpenFileName = Path.GetFileName(m_OpenFilePath);
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
                string rootName = (m_OpenFileName != null)
                    ? m_OpenFileName : "Trial Script";
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

        /// <summary>
        /// Attempt to find mission data given a node from the tree-view
        /// </summary>
        /// <param name="node">Tree-view node</param>
        /// <returns>Mission data from node</returns>
        public Mission? GetMissionFromTreeNode(TreeNode node)
        {
            // No file is open
            if (m_OpenFile == null)
            {
                return null;
            }

            // Find which mission was selected
            string[] nodes = node.FullPath.Split(
                m_MissionView.PathSeparator);

            // Check for at least path to Lesson/Trial root node
            if (nodes.Length < 3)
            {
                return null;
            }

            // Path < 4 means it can't be a trial mission
            if (nodes.Length < 4)
            {
                if (nodes[1] == "Lessons")
                {
                    int lessonId = int.Parse(nodes[2].Split().Last()) - 1;
                    return m_OpenFile.GetLessons()[lessonId];
                }
            }
            else if (nodes[1] == "Trials")
            {
                // Character resource ID
                string resId = nodes[2].Split().Last();
                resId = resId.Replace("(", "").Replace(")", "");
                // Get enum value from res ID
                CharacterUtil.EChara chara = CharacterUtil.GetCharaEnum(resId);
                int trialId = int.Parse(nodes[3].Split().Last()) - 1;
                return m_OpenFile.GetCharaTrials(chara)[trialId];
            }

            return null;
        }

        /// <summary>
        /// ToolStripMenu callback for "Open" option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// Callback for when the user is selecting a mission from the tree-view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MissionView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            // Preserve unsaved changes to the current mission
            if (m_CurrentMission != null)
            {
                // Allocate room for new script
                string[] newScript = m_MissionTextBox.Text.Split('\n');
                string[] oldScript = m_CurrentMission.GetRawText();
                if (newScript.Length > oldScript.Length)
                {
                    m_CurrentMission.SetRawText(new string[newScript.Length]);
                }

                // Copy script contents
                Array.Copy(newScript, m_CurrentMission.GetRawText(), newScript.Length);
            }
        }

        /// <summary>
        /// Callback for when the user selects a mission from the tree-view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MissionView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // No selection was made or no file is open
            if (e.Node == null || m_OpenFile == null)
            {
                return;
            }

            // Get selected mission
            m_CurrentMission = GetMissionFromTreeNode(e.Node);

            // Sanity check
            Debug.Assert(m_CurrentMission != null);
            if (m_CurrentMission == null)
            {
                return;
            }

            // Load new mission script
            StringBuilder builder = new StringBuilder();
            builder.AppendJoin('\n', m_CurrentMission.GetRawText());
            m_MissionTextBox.Clear();
            m_MissionTextBox.Text = builder.ToString();

            // Allow text box input
            m_MissionTextBox.ReadOnly = false;
            m_MissionTextBox.Enabled = true;
        }

        private void MissionTextBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}