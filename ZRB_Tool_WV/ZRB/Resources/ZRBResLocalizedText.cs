using System.IO;
using System.Text;

namespace ZRB_Tool_WV
{
    public class ZRBResLocalizedText : ZRBRes
    {
        public string Text { get; set; }

        private long m_LoadAddress;

        public ZRBResLocalizedText(ZRBFile p_ZrbFile, Stream p_ZrbStream, ZRBResourceEntry p_ResourceEntry, Stream p_Stream)
        {
            m_LoadAddress = p_ZrbFile.mainMemOffset + p_ResourceEntry.mainMemOffset;            
        }

        public override void LoadData(Stream s)
        {
            s.Seek(m_LoadAddress, 0);
            MemoryStream m = new MemoryStream();
            int b;
            while ((b = s.ReadByte()) != -1 && b != 0)
                m.WriteByte((byte)b);
            Data = m.ToArray();
            Text = Encoding.UTF8.GetString(Data);
        }
    }
}
