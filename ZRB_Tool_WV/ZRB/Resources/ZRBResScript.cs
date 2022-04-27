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
        public string fileName;
        public byte[] data;
        public ZRBResScript(ZRBResourceEntry en, Stream s, long startPos)
        {
            fileName = Helper.ReadCString(s);
            MemoryStream m = new MemoryStream();
            while (s.Position < startPos + en.nextHeaderPos) 
                m.WriteByte((byte)s.ReadByte());
            data = m.ToArray();
        }

        public override void LoadData(Stream s)
        { }
    }
}
