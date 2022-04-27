using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZRB_Tool_WV
{
    public class ZRBHeader
    {
        public bool valid = false;
        public long startPos;
        public Dictionary<uint, uint> classes;
        public List<ZRBResourceEntry> entries;
        public ZRBHeader(ZRBFile file, Stream s)
        {
            byte[] data = new byte[file.headerSize];
            s.Read(data, 0, (int)file.headerSize);
            MemoryStream m = new MemoryStream(data);
            classes = new Dictionary<uint, uint>();
            for (int i = 0; i < file.numClassCounts; i++)
                classes.Add(Helper.ReadU32(m), Helper.ReadU32(m));
            uint totalCount = 0;
            foreach (KeyValuePair<uint, uint> pair in classes)
                totalCount += pair.Value;
            entries = new List<ZRBResourceEntry>();
            startPos = m.Position;
            for (int i = 0; i < totalCount; i++)
            {
                ZRBResourceEntry entry = new ZRBResourceEntry(file, s, m, startPos);
                if (!entry.valid)
                    return;
                m.Seek(startPos + entry.nextHeaderPos, 0);
                entry._offset += file.headerOffset;
                entries.Add(entry);
            }
            valid = true;
        }
    }
}
