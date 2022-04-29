using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Be.Windows.Forms;

namespace ZRB_Tool_WV
{
    public partial class frmMain : Form
    {
        public string basePath = "";
        public bool flatView = true;
        public List<ZRBFile> zrbList = new List<ZRBFile>();
        public ZRBFile currentZRB = null;
        public frmMain()
        {
            InitializeComponent();
            Log.box = richTextBox1;
            if (File.Exists("lastpath.txt"))
                basePath = File.ReadAllText("lastpath.txt").Trim();
            toolStripComboBox1.SelectedIndex = 0;
        }

        private void loadFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = basePath;
            if(fbd.ShowDialog() == DialogResult.OK)
            {
                basePath = fbd.SelectedPath;
                if (!basePath.EndsWith("\\"))
                    basePath += "\\";
                string[] files = Directory.GetFiles(basePath, "*.zrb", SearchOption.AllDirectories);
                LoadFiles(files);
                RefreshFiles();
            }
        }

        private void loadFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.zrb|*.zrb";
            if (d.ShowDialog() == DialogResult.OK)
            {
                basePath = Path.GetDirectoryName(d.FileName);
                if (!basePath.EndsWith("\\"))
                    basePath += "\\";
                string[] files = new string[] { d.FileName };
                LoadFiles(files);
                RefreshFiles();
            }
        }

        private void LoadFiles(string[] files)
        {
            zrbList = new List<ZRBFile>();
            Log.Clear();
            StringBuilder sb = new StringBuilder();
            int count = 0;
            foreach(string file in files)
            {
                sb.Append("Loading " + file.Substring(basePath.Length) + " ...");
                ZRBFile zrb = new ZRBFile(file);
                if(zrb.valid)
                {
                    sb.AppendLine("ok");
                    zrbList.Add(zrb);
                }
                else
                    sb.AppendLine("fail");
                if(++count == 100)
                {
                    Log.Write(sb.ToString());
                    Application.DoEvents();
                    count = 0;
                }
            }
            Log.Write(sb.ToString());
        }

        private void RefreshFiles()
        {
            if(flatView)
            {
                listBox1.Items.Clear();
                listBox1.BringToFront();
            }
            else
            {
                tvZrbs.Nodes.Clear();
                tvZrbs.Nodes.Add(basePath);
                tvZrbs.BringToFront();
            }
            foreach (ZRBFile zrb in zrbList)
                if (flatView)
                    listBox1.Items.Add(zrb.myPath.Substring(basePath.Length));
                else
                    AddPathToNode(tvZrbs.Nodes[0], zrb.myPath.Substring(basePath.Length));            
        }

        private void AddPathToNode(TreeNode root, string path)
        {
            string[] parts = path.Split('\\');
            if (parts.Length == 1)
                root.Nodes.Add(path);
            else
            {
                TreeNode subNode = null;
                foreach(TreeNode t in root.Nodes)
                    if(t.Text == parts[0])
                    {
                        subNode = t;
                        break;
                    }    
                if(subNode == null)
                {
                    subNode = new TreeNode(parts[0]);
                    root.Nodes.Add(subNode);
                }
                AddPathToNode(subNode, path.Substring(parts[0].Length + 1));
            }
        }

        private void RefreshFileDetails()
        {
            lstEntries.Items.Clear();
            if (currentZRB == null)
                return;
            int count = 0;
            foreach(ZRBResourceEntry e in currentZRB.header.entries)
            {
                string s = (count++).ToString("D4") + " : ";
                switch(e.resourceType)
                {
                    case ZRBResourceEntry.ResType.LocalizedText:
                        s += "Localized text";
                        break;
                    case ZRBResourceEntry.ResType.Texture:
                        ZRBResTexture tex = (ZRBResTexture)e.resource;
                        s += "Texture (" + tex.sizeX + "x" + tex.sizeY + ")";
                        break;
                    case ZRBResourceEntry.ResType.Script:
                        ZRBResScript scr = (ZRBResScript)e.resource;
                        s += "Script (\"" + scr.FileName + "\")";
                        break;
                    default:
                        s += "Unknown (0x" + e.resourceType.ToString("X") + ")";
                        break;
                }
                lstEntries.Items.Add(s);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            string file = basePath + listBox1.Items[n].ToString();
            currentZRB = null;
            foreach(ZRBFile zrb in zrbList)
                if(zrb.myPath == file)
                {
                    currentZRB = zrb;
                    RefreshFileDetails();
                    break;
                }
        }

        private void tvZrbs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode sel = tvZrbs.SelectedNode;
            if (sel == null)
                return;
            string path = sel.Text;
            while (sel.Parent != null && sel.Parent.Parent != null)
            {
                sel = sel.Parent;
                path = sel.Text + "\\" + path;
            }
            path = basePath + path;
            currentZRB = null;
            foreach (ZRBFile zrb in zrbList)
                if (zrb.myPath == path)
                {
                    currentZRB = zrb;
                    RefreshFileDetails();
                    break;
                }
        }

        private void lstEntries_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentZRB == null)
                return;
            int selectedIndex = lstEntries.SelectedIndex;
            if (selectedIndex == -1)
                return;
            rtb1.Text = richTextBox1.Text = "";
            hb1.ByteProvider = new DynamicByteProvider(new byte[0]);
            pb1.Image = null;
            FileStream fs = new FileStream(currentZRB.myPath, FileMode.Open, FileAccess.Read);
            ZRBResourceEntry en = currentZRB.header.entries[selectedIndex];
            if (en.resource != null)
                en.resource.LoadData(fs);
            switch (en.resourceType)
            {
                case ZRBResourceEntry.ResType.LocalizedText:
                    ZRBResLocalizedText ltex = (ZRBResLocalizedText)en.resource;
                    rtb1.Text = ltex.Text;
                    break;
                case ZRBResourceEntry.ResType.Texture:
                    ZRBResTexture tex = (ZRBResTexture)en.resource;
                    hb1.ByteProvider = new DynamicByteProvider(tex.Data);
                    if (File.Exists("dump.dds"))
                        File.Delete("dump.dds");
                    if (File.Exists("dump.png"))
                        File.Delete("dump.png");
                    if (tex.textureType == 1)
                    {
                        tex.DumpToPNG();
                        if (File.Exists("dump.png"))
                        {
                            byte[] data = File.ReadAllBytes("dump.png");
                            pb1.Image = new Bitmap(new MemoryStream(data));
                            File.Delete("dump.png");
                        }
                    }
                    break;
                case ZRBResourceEntry.ResType.Script:
                    ZRBResScript scr = (ZRBResScript)en.resource;
                    hb1.ByteProvider = new DynamicByteProvider(scr.Data);
                    break;
                default:
                    break;
            }
            Log.Write(en.GetDetails());
            fs.Close();
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            flatView = toolStripComboBox1.SelectedIndex == 0;
            RefreshFiles();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (basePath != "")
                File.WriteAllText("lastpath.txt", basePath);
        }

        private void extractGameDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var s_FolderBrowserDialog = new FolderBrowserDialog
            {
                Description = "Select MAG directory (containing USRDIR, TROPDIR, LICDIR)",
                ShowNewFolderButton = false
            };

            if (s_FolderBrowserDialog.ShowDialog() != DialogResult.OK)
                return;
        }

        private void ExtractText(ZRBResLocalizedText p_LocalizedText)
        {
            var s_Contents = p_LocalizedText.Text;

            var s_SaveFileDialog = new SaveFileDialog
            {
                Title = "Extract text",
                Filter = "Text Files (*.txt)|*.txt",
                FileName = "text"
            };

            if (s_SaveFileDialog.ShowDialog() != DialogResult.OK)
                return;

            File.WriteAllText(s_SaveFileDialog.FileName, s_Contents);
        }

        private void ExtractTexture(ZRBResTexture p_Texture)
        {
            var s_SaveFileDialog = new SaveFileDialog
            {
                Title = "Extract texture",
                Filter = "Texture Data (*.bin)|*.bin|Direct Draw Surface (*.dds)|*.dds",
                FileName = $"texture_{p_Texture.sizeX}_{p_Texture.sizeY}"
            };

            if (s_SaveFileDialog.ShowDialog() != DialogResult.OK)
                return;

            File.WriteAllBytes(s_SaveFileDialog.FileName, p_Texture.Data);
        }

        private void ExtractBinary(ZRBRes p_Resource)
        {
            var s_SaveFileDialog = new SaveFileDialog
            {
                Title = "Extract data",
                Filter = "Binary File (*.bin)|*.bin",
                FileName = "data"
            };

            if (s_SaveFileDialog.ShowDialog() != DialogResult.OK)
                return;

            File.WriteAllBytes(s_SaveFileDialog.FileName, p_Resource.Data);
        }

        private void cmuExtract_Click(object sender, EventArgs e)
        {
            if (currentZRB == null)
                return;
            int selectedIndex = lstEntries.SelectedIndex;
            if (selectedIndex == -1)
                return;
            rtb1.Text = richTextBox1.Text = "";
            hb1.ByteProvider = new DynamicByteProvider(new byte[0]);
            pb1.Image = null;
            FileStream fs = new FileStream(currentZRB.myPath, FileMode.Open, FileAccess.Read);
            ZRBResourceEntry en = currentZRB.header.entries[selectedIndex];
            
            if (en.resource != null)
                en.resource.LoadData(fs);
            
            switch (en.resourceType)
            {
                case ZRBResourceEntry.ResType.LocalizedText:
                    ZRBResLocalizedText s_LocalizedText = (ZRBResLocalizedText)en.resource;
                    ExtractText(s_LocalizedText);
                    break;
                case ZRBResourceEntry.ResType.Texture:
                    ZRBResTexture s_Texture = (ZRBResTexture)en.resource;
                    ExtractTexture(s_Texture);
                    break;
                //case ZRBResourceEntry.ResType.Script:
                //    ZRBResScript s_Script = (ZRBResScript)en.resource;
                //    Extrac
                //    break;
                default:
                    ExtractBinary(en.resource);
                    break;
            }
            Log.Write(en.GetDetails());
            fs.Close();
        }
    }
}
