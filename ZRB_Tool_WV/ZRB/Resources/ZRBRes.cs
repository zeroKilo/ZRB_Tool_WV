using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZRB_Tool_WV
{
    public abstract class ZRBRes
    {
        public byte[] Data { get; set; }

        public abstract void LoadData(Stream s);
    }
}
