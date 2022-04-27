using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZRB_Tool_WV
{
    public static class Log
    {
        public static RichTextBox box;

        public static void Clear()
        {
            box.Text = "";
        }
        public static void Write(string s)
        {
            box.AppendText(s);
            box.SelectionStart = box.Text.Length;
            box.ScrollToCaret();
        }
        public static void WriteLine(string s)
        {
            box.AppendText(s + "\n");
            box.SelectionStart = box.Text.Length;
            box.ScrollToCaret();
        }
    }
}
