using P4U2TrialEditor.Core;
using P4U2TrialEditor.Util;

namespace P4U2TrialEditor.UI
{
    public partial class MissionTreeView : TreeView
    {
        // Tree root node
        private TreeNode m_RootNode;
        // Lesson root node
        private TreeNode m_LessonRootNode;
        // Trial root node
        private TreeNode m_TrialRootNode;

        // Mission file associated with tree
        private MissionFile? m_File;

        #region Constructors

        public MissionTreeView()
            : this(null, "Trial Script")
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file">Mission file to visualize</param>
        /// <param name="root">Root node name</param>
        public MissionTreeView(MissionFile? file, string root)
        {
            InitializeComponent();

            m_File = file;

            BeginUpdate();
            {
                // Tree root
                Nodes.Add(root);
                m_RootNode = Nodes[0];

                // Lesson root
                m_RootNode.Nodes.Add("Lessons");
                m_LessonRootNode = m_RootNode.Nodes[0];

                // Trial root
                m_RootNode.Nodes.Add("Trials");
                m_TrialRootNode = m_RootNode.Nodes[1];

                // Trial characters
                for (int i = 0; i < (int)CharacterUtil.EChara.COMMON; i++)
                {
                    CharacterUtil.EChara chara = (CharacterUtil.EChara)i;

                    m_TrialRootNode.Nodes.Add(String.Format("{0} ({1})",
                        CharacterUtil.GetCharaName(chara),
                        CharacterUtil.GetCharaResID(chara)));
                }
            }
            EndUpdate();

            if (m_File != null)
            {
                Rebuild();
            }
        }

        #endregion Constructors

        #region Accessors

        public void SetFile(MissionFile? file)
        {
            m_File = file;
        }

        public void SetRootNode(string name)
        {
            m_RootNode.Text = name;
        }

        #endregion Accessors

        /// <summary>
        /// Build tree view from mission file
        /// </summary>
        /// <param name="file"></param>
        public void OpenFile(MissionFile file)
        {
            SetFile(file);
            Rebuild();
            CollapseAll();
            Enabled = true;
        }

        /// <summary>
        /// Close tree view
        /// </summary>
        public void CloseFile()
        {
            SetFile(null);

            BeginUpdate();
            {
                SetRootNode("Trial Script");
                m_RootNode.Collapse();
                m_LessonRootNode.Nodes.Clear();
                foreach (TreeNode chara in m_TrialRootNode.Nodes)
                {
                    chara.Nodes.Clear();
                }
                CollapseAll();
            }
            EndUpdate();

            Enabled = false;
        }

        /// <summary>
        /// Rebuild the tree contents
        /// </summary>
        public void Rebuild()
        {
            if (m_File == null)
            {
                return;
            }

            BeginUpdate();
            {
                // Build lessons
                m_LessonRootNode.Nodes.Clear();
                foreach (Mission m in m_File.GetLessons())
                {
                    AddLesson(m);
                }

                // Build trials
                for (int i = 0; i < (int)CharacterUtil.EChara.COMMON; i++)
                {
                    m_TrialRootNode.Nodes[i].Nodes.Clear();
                    foreach (Mission m in m_File.GetCharaTrials((CharacterUtil.EChara)i))
                    {
                        AddTrial(m);
                    }
                }
            }
            EndUpdate();
        }

        /// <summary>
        /// Add lesson to tree
        /// </summary>
        /// <param name="m">Mission data</param>
        public void AddLesson(Mission m)
        {
            m_LessonRootNode.Nodes.Add(
                String.Format("Lesson {0}", m.GetID()));
        }

        /// <summary>
        /// Add trial to tree
        /// </summary>
        /// <param name="m">Mission data</param>
        public void AddTrial(Mission m)
        {
            m_TrialRootNode.Nodes[(int)m.GetCharacter()].Nodes.Add(
                String.Format("Trial {0}", m.GetID()));
        }

        /// <summary>
        /// Get mission corresponding to tree node
        /// </summary>
        /// <param name="node">Target node</param>
        /// <returns>Mission data corresponding to node</returns>
        public Mission? GetMission(TreeNode node)
        {
            // No tree view
            if (m_File == null)
            {
                return null;
            }

            // Find which mission was selected
            string[] nodes = node.FullPath.Split(PathSeparator);

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
                    return m_File.GetLessons()[lessonId];
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
                return m_File.GetCharaTrials(chara)[trialId];
            }

            return null;
        }
    }
}
