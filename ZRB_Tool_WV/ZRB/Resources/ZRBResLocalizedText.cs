using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZRB_Tool_WV
{
    public class ZRBResLocalizedText : ZRBRes
    {
        public string text;
        public byte[] data;

        private long loadAddress;
        public ZRBResLocalizedText(ZRBFile file, Stream zs, ZRBResourceEntry e, Stream s)
        {
            loadAddress = file.mainMemOffset + e.mainMemOffset;            
        }

        public override void LoadData(Stream s)
        {
            s.Seek(loadAddress, 0);
            MemoryStream m = new MemoryStream();
            int b;
            while ((b = s.ReadByte()) != -1 && b != 0)
                m.WriteByte((byte)b);
            data = m.ToArray();
            text = Encoding.UTF8.GetString(data);
        }
    }
}
