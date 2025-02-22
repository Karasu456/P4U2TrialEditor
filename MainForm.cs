using System.Diagnostics;
using System.Text;
using P4U2TrialEditor.Core;
using P4U2TrialEditor.Util;
using P4U2TrialEditor.UI;
using ExcelDataReader;
using System.Data;

namespace P4U2TrialEditor
{
    public partial class MainForm : Form
    {
        // Currently open mission file
        private MissionFile? m_OpenFile = null;
        // Path to currently open file
        private string? m_OpenFilePath = null;
        // Name of currently open file
        private string? m_OpenFileName = null;

        // Whether there are unsaved changes to the file
        private bool m_FileDirty = false;

        // Currently selected mission
        private Mission? m_CurrentMission = null;

        public static string excelFilePath = null;

        public static bool excelImport = false;
        public static bool isExcelImport
        {
            get { return excelImport; }
        }

        public static bool challengeMode = false;

        public static bool isChallengeMode
        {
            get { return challengeMode; }
        }

        public static string currentCharacter = null;
        public static int rows;
        public int columns;
        public static string[,] trialDataArray;
        public static string[,] keyDataArray;

        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Update window title.
        /// The title displays the filename and the file's encryption type.
        /// </summary>
        private void UpdateTitle()
        {
            if (m_OpenFile == null)
            {
                Text = "P4U2 Trial Editor";
                return;
            }

            string[] enc2str =
            {
                "TEXT",
                "ANG",
                "MD5",
                "MD5 + ANG",
            };

            // File encryption type
            string enc = (m_OpenFile != null)
                ? String.Format("({0})", enc2str[(int)m_OpenFile.GetEncryption()])
                : String.Empty;

            // Window title
            Text = String.Format("P4U2 Trial Editor - {0} {1}",
                m_OpenFileName, enc);
        }

        /// <summary>
        /// Update mission preview text box
        /// </summary>
        private void UpdatePreview()
        {
            Mission m = new Mission();
            if (m_CurrentMission != null)
            {
                m.SetCharacter(m_CurrentMission.GetCharacter());
            }

            using (MemoryStream ms = new MemoryStream(
                Encoding.ASCII.GetBytes(m_MissionTextBox.Text)))
            {
                using (StreamReaderEx reader = new StreamReaderEx(ms))
                {
                    if (m.Deserialize(reader))
                    {
                        m_MissionPreview.Load(m);
                    }
                    else
                    {
                        m_MissionPreview.Text = "Could not parse mission script";
                    }
                }
            }
        }

        #region Open/Close File

        /// <summary>
        /// Attempt to open mission file from path
        /// </summary>
        /// <param name="path">Path to mission file</param>
        /// <returns></returns>
        private bool OpenFile(string path)
        {
            MissionFile.Error err;
            m_OpenFile = MissionFile.Open(path, out err);

            switch(err)
            {
                case MissionFile.Error.IO_FAIL:
                    m_OpenFilePath = null;
                    m_OpenFileName = null;
                    return false;
                case MissionFile.Error.DESERIALIZE_FAIL:
                    MessageBox.Show("The trial script is not formatted correctly.", "Script Error");
                    m_OpenFilePath = null;
                    m_OpenFileName = null;
                    return false;
            }

            m_OpenFilePath = path;
            m_OpenFileName = Path.GetFileName(m_OpenFilePath);
            OnOpenFile();
            return true;
        }

        /// <summary>
        /// Set excelImport to true and
        /// set the excel file path.
        /// </summary>
        /// <param name="path">Path to mission file</param>
        /// <returns></returns>
        public static void OpenExcelFile(string path)
        {
            excelImport = true;
            excelFilePath= path;

            using (var readExcel = File.Open(MainForm.excelFilePath, FileMode.Open, FileAccess.Read))
            {
                // Specify the encoding as UTF-8
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var reader = ExcelReaderFactory.CreateReader(readExcel, new ExcelReaderConfiguration()
                {
                    FallbackEncoding = Encoding.GetEncoding(65001),
                });

                DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                });

                DataTable trialTable = result.Tables[1];
                DataTable keyTable = result.Tables[2];

                // Get the dimensions of the DataTable
                int rows = trialTable.Rows.Count;
                int columns = trialTable.Columns.Count;
              
                trialDataArray = new string[rows, columns];
                keyDataArray = new string[rows, columns];

                // Populate the array with data from the DataTable
                for (int row = 0; row < rows; row++)
                {
                    for (int column = 0; column < columns; column++)
                    {
                        trialDataArray[row, column] = trialTable.Rows[row][column].ToString();
                        keyDataArray[row, column] = keyTable.Rows[row][column].ToString();
                    }
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Close the currently open file
        /// </summary>
        private void CloseFile()
        {
            if (m_OpenFile == null)
            {
                return;
            }

            // Prompt the user to save if they still have unsaved changes
            if (m_FileDirty)
            {
                switch (ShowFileDirtyMsg())
                {
                    case DialogResult.Yes:
                        m_OpenFile.Write(GetSaveFilePath());
                        m_FileDirty = false;
                        break;
                    case DialogResult.No:
                        m_FileDirty = false;
                        break;
                    case DialogResult.Cancel:
                    default:
                        return;
                }
            }

            m_OpenFile = null;
            m_OpenFilePath = null;
            m_OpenFileName = null;
            OnCloseFile();
        }

        /// <summary>
        /// Open file callback
        /// </summary>
        private void OnOpenFile()
        {
            // Update tree view            
            m_MissionView.SetRootNode(m_OpenFileName!);
            m_MissionView.OpenFile(m_OpenFile!);
    
            // Clear text editors
            m_MissionTextBox.Clear();
            m_MissionPreview.Clear();

            // Update window title
            UpdateTitle();
        }

        /// <summary>
        /// Close file callback
        /// </summary>
        private void OnCloseFile()
        {
            // Update tree view
            m_MissionView.CloseFile();

            // Disable text editors
            m_MissionTextBox.Clear();
            m_MissionTextBox.ReadOnly = true;
            m_MissionTextBox.Enabled = false;
            m_MissionPreview.Clear();
            m_MissionPreview.Enabled = false;

            // Update window title
            UpdateTitle();
        }

        #endregion Open/Close File

        #region Save File

        /// <summary>
        /// Determine where the current file should be saved.
        /// </summary>
        /// <returns>Save file path</returns>
        private string GetSaveFilePath()
        {
            // Newly created files have no filepath set
            if (m_OpenFilePath != null)
            {
                return m_OpenFilePath;
            }

            // Query file to save
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = (m_OpenFileName != null)
                ? m_OpenFileName : "trial.txt";
            dialog.Filter = "Trial script (*.txt/*.ang)|*.txt;*.ang|All files|*.*";
            dialog.ShowDialog();

            // Set new filepath
            m_OpenFilePath = dialog.FileName;
            m_OpenFileName = Path.GetFileName(m_OpenFilePath);

            // Change encryption based on file extension
            switch(Path.GetExtension(m_OpenFilePath).ToLower())
            {
                case ".txt":
                    m_OpenFile.SetEncryption(MissionFile.Encryption.NONE);
                    break;
                case ".ang":
                    m_OpenFile.SetEncryption(MissionFile.Encryption.ANG);
                    break;
                // TO-DO: Fix MD5
                default:
                    m_OpenFile.SetEncryption(MissionFile.Encryption.NONE);
                    break;
            }

            return m_OpenFilePath;
        }

        /// <summary>
        /// Prompt the user if they would like to save changes to the current file.
        /// </summary>
        /// <returns>Dialog box result</returns>
        private DialogResult ShowFileDirtyMsg()
        {
            return MessageBox.Show(
                "Would you like to save your changes to this file?",
                "Unsaved Changes",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button3);
        }

        /// <summary>
        /// Save changes to the mission being edited in the textbox.
        /// </summary>
        private void SaveTextBoxData()
        {
            if (m_CurrentMission != null)
            {
                // Overwrite script contents
                m_CurrentMission.GetRawText().Clear();
                m_CurrentMission.GetRawText().AddRange(
                    m_MissionTextBox.Text.Split('\n'));
            }

        }

        #endregion Save File

        #region Form Events

        /// <summary>
        /// DragEnter event handler.
        /// Allow user to drag and drop file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            // Allow user to drag files onto the window
            e.Effect = (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                ? DragDropEffects.Link : DragDropEffects.None;
        }

        /// <summary>
        /// DragDrop event handler.
        /// Allow user to drag and drop file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            // Open files dropped onto the window
            if (e.Data != null)
            {
                string[] data = (string[])e.Data.GetData(DataFormats.FileDrop);
                OpenFile(data[0]);
            }
        }

        /// <summary>
        /// FormClosing event handler.
        /// Allows user to cancel the close operation,
        /// or save/delete changes to the current file before closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_OpenFile == null)
            {
                return;
            }

            // Prompt the user to save if they still have unsaved changes
            if (m_FileDirty)
            {
                switch (ShowFileDirtyMsg())
                {
                    case DialogResult.Yes:
                        m_OpenFile.Write(GetSaveFilePath());
                        m_FileDirty = false;
                        break;
                    case DialogResult.No:
                        m_FileDirty = false;
                        break;
                    case DialogResult.Cancel:
                        // Stop window from closing
                        e.Cancel = true;
                        break;
                    default:
                        goto case DialogResult.Cancel;
                }
            }
        }

        #endregion Form Events

        #region Menu Tool Strip

        /// <summary>
        /// ToolStripMenu callback for "New" option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Prompt the user to save if they still have unsaved changes
            if (m_OpenFile != null && m_FileDirty)
            {
                switch (ShowFileDirtyMsg())
                {
                    case DialogResult.Yes:
                        m_OpenFile.Write(GetSaveFilePath());
                        m_FileDirty = false;
                        break;
                    case DialogResult.No:
                        m_FileDirty = false;
                        break;
                    case DialogResult.Cancel:
                    default:
                        return;
                }
            }

            // Close existing file and create a new empty file
            CloseFile();
            m_OpenFile = new MissionFile();
            m_OpenFile.MakeEmpty();
            m_OpenFileName = "New Trial Script";
            m_FileDirty = true;
            OnOpenFile();
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
            //dialog.FileName = "Select a trial file";
            // Default filter includes the MD5 hashed version
            dialog.Filter = "Trial script (*.txt/*.ang)" +
                "|*.txt;*.ang;1b1df6db6ea1d4af300ae30c7bbab937*" +
                "|All files|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                OpenFile(dialog.FileName);
            }
        }

        /// <summary>
        /// ToolStripMenu callback for "Open" option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importFromXCLSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Query file to open
            OpenFileDialog dialog = new()
            {
                //FileName = "Select an Excel file",
                Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx",
            };

            if(dialog.ShowDialog() == DialogResult.OK)
            {
                OpenExcelFile(dialog.FileName);
            }

            openToolStripMenuItem_Click(sender, e);
        }

        /// <summary>
        /// ToolStripMenu callback for "Save" option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_OpenFile != null)
            {
                // Save changes currently selected mission
                SaveTextBoxData();
                m_OpenFile.Write(GetSaveFilePath());
                m_FileDirty = false;
                Process.Start("G:\\Steam\\steamapps\\common\\P4U2\\P4PC Modpack\\trial\\P4U2-Trial Encryption.bat");
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_OpenFile != null)
            {
                // Query file to save
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.FileName = (m_OpenFileName != null)
                    ? m_OpenFileName : "trial.txt";
                dialog.Filter = "Trial script (*.txt/*.ang)|*.txt;*.ang|All files|*.*";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Save changes to currently selected mission
                    SaveTextBoxData();

                    // Change to new filename
                    m_OpenFilePath = dialog.FileName;
                    m_OpenFileName = Path.GetFileName(m_OpenFilePath);

                    // Change encryption based on file extension
                    switch (Path.GetExtension(m_OpenFilePath).ToLower())
                    {
                        case ".txt":
                            m_OpenFile.SetEncryption(MissionFile.Encryption.NONE);
                            break;
                        case ".ang":
                            m_OpenFile.SetEncryption(MissionFile.Encryption.ANG);
                            break;
                        // TO-DO: Fix MD5
                        default:
                            m_OpenFile.SetEncryption(MissionFile.Encryption.NONE);
                            break;
                    }

                    // Adjust text referencing filename
                    m_MissionView.SetRootNode(m_OpenFileName);
                    UpdateTitle();

                    // Write file
                    m_OpenFile.Write(dialog.FileName);
                    m_FileDirty = false;
                }
            }
        }

        /// <summary>
        /// ToolStripMenu callback for "Close" option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseFile();
        }

        /// <summary>
        /// ToolStripMenu callback for "Documentation" option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void documentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessStartInfo psInfo = new ProcessStartInfo
            {
                FileName = "https://github.com/kiwi515/P4U2TrialEditor/wiki/Script-Structure",
                UseShellExecute = true
            };

            Process.Start(psInfo);
        }

        #endregion Menu Tool Strip

        #region Mission Text Box

        private void MissionTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (Enabled)
            {
                m_FileDirty = true;
                UpdatePreview();
            }
        }

        #endregion Mission Text Box

        #region Mission Tree View

        /// <summary>
        /// Callback for when the user is selecting a mission from the tree-view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MissionView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            SaveTextBoxData();
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

            // Highlight selected tree node
            e.Node.BackColor = SystemColors.HighlightText;

            // Get selected mission
            Mission? selected = m_MissionView.GetMission(e.Node);
            // If no mission is selected, don't deselect the current one
            m_CurrentMission = (selected == null) ? m_CurrentMission : selected;

            if (m_CurrentMission != null)
            {
                // Load new mission script
                StringBuilder builder = new StringBuilder();
                builder.AppendJoin('\n', m_CurrentMission.GetRawText());
                m_MissionTextBox.Clear();
                m_MissionTextBox.Text = builder.ToString();

                // Allow text box input
                m_MissionTextBox.ReadOnly = false;
                m_MissionTextBox.Enabled = true;

                UpdatePreview();
            }
        }

        #endregion Mission Tree View
        
    }
}