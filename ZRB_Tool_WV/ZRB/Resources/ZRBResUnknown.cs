using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZRB_Tool_WV
{
    public class ZRBResUnknown : ZRBRes
    {
        public ZRBResUnknown(ZRBResourceEntry en, Stream s, long startPos)
        {
            MemoryStream m = new MemoryStream();
            while (s.Position < startPos + en.nextHeaderPos)
                m.WriteByte((byte)s.ReadByte());
            Data = m.ToArray();
        }
        public override void LoadData(Stream s) { }
    }
}
