using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZRB_Tool_WV
{
    public class ZRBFile
    {
        public uint version;
        public uint headerOffset;
        public uint headerSize;
        public uint mainMemOffset;
        public uint mainMemSize;
        public uint tempMemOffset;
        public uint tempMemSize;
        public uint vramOffsetGPU;
        public uint vramSizeGPU;
        public uint vramOffsetIO;
        public uint vramSizeIO;
        public uint numClassCounts;
        public bool localized;
        public bool hasTableOfContents;
        public ZRBHeader header;

        public bool valid = false;
        public string myPath;
        public ZRBFile(string path)
        {
            myPath = path;
            byte[] data = File.ReadAllBytes(path);
            MemoryStream m = new MemoryStream(data);
            version = Helper.ReadU32(m);
            if (version != 6) return;
            headerOffset = Helper.ReadU32(m);
            headerSize = Helper.ReadU32(m);
            mainMemOffset = Helper.ReadU32(m);
            mainMemSize = Helper.ReadU32(m);
            tempMemOffset = Helper.ReadU32(m);
            tempMemSize = Helper.ReadU32(m);
            vramOffsetGPU = Helper.ReadU32(m);
            vramSizeGPU = Helper.ReadU32(m);
            vramOffsetIO = Helper.ReadU32(m);
            vramSizeIO = Helper.ReadU32(m);
            numClassCounts = Helper.ReadU32(m);
            byte t = (byte)m.ReadByte();
            localized = (t & 1) != 0;
            hasTableOfContents = (t & 2) != 0;
            m.ReadByte();
            m.Seek(headerOffset, 0);
            header = new ZRBHeader(this, m);
            if (!header.valid) return;
            valid = true;
        }
    }
}
