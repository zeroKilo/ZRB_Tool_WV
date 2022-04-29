using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZRB_Tool_WV
{
    public class ZRBResScript : ZRBRes
    {
        // Script file path
        public string FileName { get; set; }

        public ZRBResScript(ZRBResourceEntry en, Stream s, long startPos)
        {
            FileName = Helper.ReadCString(s);
            MemoryStream m = new MemoryStream();
            while (s.Position < startPos + en.nextHeaderPos) 
                m.WriteByte((byte)s.ReadByte());
            Data = m.ToArray();
        }

        public override void LoadData(Stream s)
        {
        }
    }
}
