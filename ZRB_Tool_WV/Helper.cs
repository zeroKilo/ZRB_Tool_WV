using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ZRB_Tool_WV
{
    public static class Helper
    {
        public static void WriteU32(Stream s, uint u)
        {
            byte[] data = BitConverter.GetBytes(u);
            for (int i = 0; i < 4; i++)
                s.WriteByte(data[3 - i]);
        }
        public static void WriteU32LE(Stream s, uint u)
        {
            byte[] data = BitConverter.GetBytes(u);
            for (int i = 0; i < 4; i++)
                s.WriteByte(data[i]);
        }

        public static uint ReadU32(Stream s)
        {
            byte[] data = new byte[4];
            for (int i = 0; i < 4; i++)
                data[3 - i] = (byte)s.ReadByte();
            return BitConverter.ToUInt32(data, 0);
        }

        public static ushort ReadU16(Stream s)
        {
            byte[] data = new byte[2];
            for (int i = 0; i < 2; i++)
                data[1 - i] = (byte)s.ReadByte();
            return BitConverter.ToUInt16(data, 0);
        }

        public static float ReadFloat(Stream s)
        {
            byte[] data = new byte[4];
            for (int i = 0; i < 4; i++)
                data[3 - i] = (byte)s.ReadByte();
            return BitConverter.ToSingle(data, 0);
        }

        public static string ReadCString(Stream s)
        {
            uint count = ReadU32(s);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count - 1; i++)
                sb.Append((char)s.ReadByte());
            s.ReadByte();
            return sb.ToString();
        }

        public static string RunShell(string file, string command)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = file;
            startInfo.Arguments = command;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = Path.GetDirectoryName(file) + "\\";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            return process.StandardOutput.ReadToEnd();
        }
    }
}
