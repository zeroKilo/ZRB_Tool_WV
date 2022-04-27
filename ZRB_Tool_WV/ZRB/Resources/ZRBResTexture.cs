using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZRB_Tool_WV
{
    public class ZRBResTexture : ZRBRes
    {
        public enum BufferFormat
        {
            FormatA16 = 335545070,
            FormatA8 = 16777983,
            FormatARGB1555 = 33598180,
            FormatARGB4444 = 50375396,
            FormatARGB8 = 83929828,
            FormatBGRA8 = 83929627,
            FormatD16 = 302033152,
            FormatD24X8 = 268479204,
            FormatDXT1 = 100707044,
            FormatDXT3 = 117484260,
            FormatDXT5 = 134261476,
            FormatGR16F = 520104420,
            FormatL16 = 335587822,
            FormatL16A16 = 352430820,
            FormatL8 = 16820735,
            FormatL8A8 = 184593067,
            FormatR32F = 469764580,
            FormatRGB565 = 67152356,
            FormatRGBA16F = 436251364,
            FormatRGBA32F = 453028580,
            FormatRGBA8 = 83929747
        };

        public byte textureType;
        public byte mipCount;
        public uint dataSize;
        public uint sizeX;
        public uint sizeY;
        public BufferFormat bufferFormat;
        public uint pitch;
        public uint depth;
        public uint states;
        public uint bias;
        public byte[] data;

        private long loadAddress;
        public ZRBResTexture(ZRBFile file, ZRBResourceEntry e,  Stream s)
        {
            textureType = (byte)s.ReadByte();
            mipCount = (byte)s.ReadByte();
            dataSize = Helper.ReadU32(s);
            sizeX = Helper.ReadU32(s);
            sizeY = Helper.ReadU32(s);
            bufferFormat = (BufferFormat)Helper.ReadU32(s);
            pitch = Helper.ReadU32(s);
            depth = Helper.ReadU32(s);
            states = Helper.ReadU32(s);
            bias = Helper.ReadU32(s);
            loadAddress = file.vramOffsetGPU + e.vramOffsetGPU;
        }

        public override void LoadData(Stream s)
        {
            s.Seek(loadAddress, 0);
            data = new byte[dataSize];
            s.Read(data, 0, (int)dataSize);
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\ttextureType = 0x" + textureType.ToString("X"));
            sb.AppendLine("\tmipCount = 0x" + mipCount.ToString("X"));
            sb.AppendLine("\tdataSize = 0x" + dataSize.ToString("X"));
            sb.AppendLine("\tsizeX = 0x" + sizeX.ToString("X"));
            sb.AppendLine("\tsizeY = 0x" + sizeY.ToString("X"));
            sb.AppendLine("\tbufferFormat = " + bufferFormat);
            sb.AppendLine("\tpitch = 0x" + pitch.ToString("X"));
            sb.AppendLine("\tdepth = 0x" + depth.ToString("X"));
            sb.AppendLine("\tstates = 0x" + states.ToString("X"));
            sb.AppendLine("\tbias = 0x" + bias.ToString("X"));
            return sb.ToString();
        }

        public void DumpToPNG()
        {
            switch(bufferFormat)
            {
                case BufferFormat.FormatDXT1:
                    DumpDXTTexture("dump.dds", '1');
                    if (File.Exists("dump.dds"))
                        ConvertDXT2PNG();
                    File.Delete("dump.dds");
                    break;
                case BufferFormat.FormatDXT3:
                    DumpDXTTexture("dump.dds", '3');
                    if (File.Exists("dump.dds"))
                        ConvertDXT2PNG();
                    File.Delete("dump.dds");
                    break;
                case BufferFormat.FormatDXT5:
                    DumpDXTTexture("dump.dds", '5');
                    if (File.Exists("dump.dds"))
                        ConvertDXT2PNG();
                    File.Delete("dump.dds");
                    break;
            }
        }

        public void DumpDXTTexture(string filename, char version, uint flags = 0x07100A00)
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, 0x44445320);
            Helper.WriteU32(m, 0x7C000000);
            Helper.WriteU32(m, flags);
            Helper.WriteU32LE(m, sizeY);
            Helper.WriteU32LE(m, sizeX);
            Helper.WriteU32LE(m, pitch);
            Helper.WriteU32LE(m, depth);
            Helper.WriteU32LE(m, mipCount);
            m.Write(new byte[0x2C], 0, 0x2C);
            Helper.WriteU32LE(m, 0x20);
            Helper.WriteU32LE(m, 4);
            m.WriteByte(0x44); 
            m.WriteByte(0x58); 
            m.WriteByte(0x54);
            m.WriteByte((byte)version);
            m.Write(new byte[0x14], 0, 0x14);
            Helper.WriteU32(m, 0x08104000);
            m.Write(new byte[0x10], 0, 0x10);
            m.Write(data, 0, data.Length);
            File.WriteAllBytes(filename, m.ToArray());
        }

        public void ConvertDXT2PNG()
        {
            Helper.RunShell(Path.GetDirectoryName(Application.ExecutablePath) + "\\texconv.exe", "dump.dds -ft png");
        }
    }
}
