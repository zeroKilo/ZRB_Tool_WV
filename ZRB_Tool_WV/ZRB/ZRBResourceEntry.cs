using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZRB_Tool_WV
{
    public class ZRBResourceEntry
    {
        public enum ResType
        {
            LocalizedText = 4,
            Texture = 5,
            Script = 7,
            Mesh = 63,
        }

        public long _offset;
        public uint resourceVersion;
        public uint resourceHeaderMagic;
        public uint nextHeaderPos;
        public ResType resourceType;
        public uint mainMemOffset;
        public uint tempMemOffset;
        public uint vramOffsetGPU;
        public uint vramOffsetIO;
        public uint classTokenVal;
        public uint resourceNameTokenVal;
        public uint priority;

        public ZRBRes resource;

        public bool valid = false;

        public ZRBResourceEntry(ZRBFile file, Stream zs, Stream s, long startPos)
        {
            _offset = s.Position;
            resourceVersion = Helper.ReadU32(s);
            resourceHeaderMagic = Helper.ReadU32(s);
            nextHeaderPos = Helper.ReadU32(s);
            resourceType = (ResType)Helper.ReadU32(s);
            mainMemOffset = Helper.ReadU32(s);
            tempMemOffset = Helper.ReadU32(s);
            vramOffsetGPU = Helper.ReadU32(s);
            vramOffsetIO = Helper.ReadU32(s);
            classTokenVal = Helper.ReadU32(s);
            resourceNameTokenVal = Helper.ReadU32(s);
            priority = Helper.ReadU32(s);
            valid = resourceHeaderMagic == 0x600DF00D;
            if(valid)
                switch(resourceType)
                {
                    case ResType.LocalizedText:
                        resource = new ZRBResLocalizedText(file, zs, this, s);
                        break;
                    case ResType.Texture:
                        resource = new ZRBResTexture(file, this, s);
                        break;
                    case ResType.Script:
                        resource = new ZRBResScript(this, s, startPos);
                        break;
                    case ResType.Mesh:
                        resource = new ZRBResMesh(file, this, s, startPos);
                        break;
                    default:
                        resource = new ZRBResUnknown(this, s, startPos);
                        break;
                }
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Offset = 0x" + _offset.ToString("X"));
            sb.AppendLine("resourceVersion = 0x" + resourceVersion.ToString("X"));
            sb.AppendLine("resourceHeaderMagic = 0x" + resourceHeaderMagic.ToString("X"));
            sb.AppendLine("nextHeaderPos = 0x" + nextHeaderPos.ToString("X"));
            sb.AppendLine("resourceType = 0x" + resourceType.ToString("X"));
            sb.AppendLine("mainMemOffset = 0x" + mainMemOffset.ToString("X"));
            sb.AppendLine("tempMemOffset = 0x" + tempMemOffset.ToString("X"));
            sb.AppendLine("vramOffsetGPU = 0x" + vramOffsetGPU.ToString("X"));
            sb.AppendLine("vramOffsetIO = 0x" + vramOffsetIO.ToString("X"));
            sb.AppendLine("classTokenVal = 0x" + classTokenVal.ToString("X"));
            sb.AppendLine("resourceNameTokenVal = 0x" + resourceNameTokenVal.ToString("X"));
            sb.AppendLine("priority = 0x" + priority.ToString("X"));
            if (valid)
                switch (resourceType)
                {
                    case ResType.LocalizedText:
                        break;
                    case ResType.Texture:
                        ZRBResTexture tex = (ZRBResTexture)resource;
                        sb.Append(tex.GetDetails());
                        break;
                    case ResType.Script:
                        break;
                }
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
