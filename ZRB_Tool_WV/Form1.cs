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
    public partial class Form1 : Form
    {
        public string basePath = "";
        public bool flatView = true;
        public List<ZRBFile> zrbList = new List<ZRBFile>();
        public ZRBFile currentZRB = null;
        public Form1()
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
                tv1.Nodes.Clear();
                tv1.Nodes.Add(basePath);
                tv1.BringToFront();
            }
            foreach (ZRBFile zrb in zrbList)
                if (flatView)
                    listBox1.Items.Add(zrb.myPath.Substring(basePath.Length));
                else
                    AddPathToNode(tv1.Nodes[0], zrb.myPath.Substring(basePath.Length));            
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
            listBox2.Items.Clear();
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
                        s += "Script (\"" + scr.fileName + "\")";
                        break;
                    default:
                        s += "Unknown (0x" + e.resourceType.ToString("X") + ")";
                        break;
                }
                listBox2.Items.Add(s);
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

        private void tv1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode sel = tv1.SelectedNode;
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

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentZRB == null)
                return;
            int n = listBox2.SelectedIndex;
            if (n == -1)
                return;
            rtb1.Text = richTextBox1.Text = "";
            hb1.ByteProvider = new DynamicByteProvider(new byte[0]);
            pb1.Image = null;
            FileStream fs = new FileStream(currentZRB.myPath, FileMode.Open, FileAccess.Read);
            ZRBResourceEntry en = currentZRB.header.entries[n];
            if (en.resource != null)
                en.resource.LoadData(fs);
            switch (en.resourceType)
            {
                case ZRBResourceEntry.ResType.LocalizedText:
                    ZRBResLocalizedText ltex = (ZRBResLocalizedText)en.resource;
                    rtb1.Text = ltex.text;
                    break;
                case ZRBResourceEntry.ResType.Texture:
                    ZRBResTexture tex = (ZRBResTexture)en.resource;
                    hb1.ByteProvider = new DynamicByteProvider(tex.data);
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
                    hb1.ByteProvider = new DynamicByteProvider(scr.data);
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
    }
}
