using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZRB_Tool_WV
{
    public class ZRBResMesh : ZRBRes
    {
        public class StaticMeshLODHeader
        {
            public uint meshVersion;
            public uint tag;
            public byte numEdgePartitions;
            public uint[] attributeStreamSize = new uint[4];
            public float[] unk1 = new float[3];
            public int unk2;
            public float[] unk3 = new float[3];
            public int unk4;
            public float radiusFromLocalPivot;
            public float[] localBoundingSphere = new float[4];
            public ushort flags;
            public byte lodIndex;
            public byte lodTags;
            public ushort freqMode;
            public uint numVertices;
            public uint numIndicies;
            public uint unk5;
            public uint vertexFormatDescriptor;
            public uint[] vFD_requestedUsagesStream = new uint[3];
            public uint vFD_freqDivider;
            public byte specialFormat_floatPositions;
            public byte specialFormat_compressedPositions;
            public byte specialFormat_positionHas4Components;
            public byte[] unk6 = new byte[5];
            public uint[] unk7 = new uint[2];
            public uint unk8;
            public float[] unk9 = new float[2];
            public uint unk10;
            public ushort unk11;

            public bool isValid = false;
            public StaticMeshLODHeader(Stream s)
            {
                meshVersion = Helper.ReadU32(s);
                if (meshVersion != 0x1D)
                    return;
                tag = Helper.ReadU32(s);
                numEdgePartitions = (byte)s.ReadByte();
                for (int i = 0; i < 4; i++)
                    attributeStreamSize[i] = Helper.ReadU32(s);
                for (int i = 0; i < 3; i++)
                    unk1[i] = Helper.ReadFloat(s);
                unk2 = (int)Helper.ReadU32(s);
                for (int i = 0; i < 3; i++)
                    unk3[i] = Helper.ReadFloat(s);
                unk4 = (int)Helper.ReadU32(s);
                radiusFromLocalPivot = Helper.ReadFloat(s);
                for (int i = 0; i < 4; i++)
                    localBoundingSphere[i] = Helper.ReadFloat(s);
                flags = Helper.ReadU16(s);
                lodIndex = (byte)s.ReadByte();
                lodTags = (byte)s.ReadByte();
                freqMode = Helper.ReadU16(s);
                numVertices = Helper.ReadU32(s);
                numIndicies = Helper.ReadU32(s);
                unk5 = Helper.ReadU32(s);
                vertexFormatDescriptor = Helper.ReadU32(s);
                if (vertexFormatDescriptor != 0x20B)
                    return;
                for (int i = 0; i < 3; i++)
                    vFD_requestedUsagesStream[i] = Helper.ReadU32(s);
                vFD_freqDivider = Helper.ReadU32(s);
                specialFormat_floatPositions = (byte)s.ReadByte();
                specialFormat_compressedPositions = (byte)s.ReadByte();
                specialFormat_positionHas4Components = (byte)s.ReadByte();
                for (int i = 0; i < 5; i++)
                    unk6[i] = (byte)s.ReadByte();
                for (int i = 0; i < 2; i++)
                    unk7[i] = Helper.ReadU32(s);
                unk8 = Helper.ReadU32(s);
                for (int i = 0; i < 2; i++)
                    unk9[i] = Helper.ReadFloat(s);
                unk10 = Helper.ReadU32(s);
                unk11 = Helper.ReadU16(s);
                isValid = true;
            }
        }
        public class StaticMeshHeader
        {
            public float localBoundingSphereRadius;
            public float[] localBoundingSphere = new float[4];
            public float[] localABBmin = new float[3];
            public float[] localABBmax = new float[3];
            public ushort flags;
            public byte lodCount;
            public uint numAtomicMeshes;
            public uint numAttachments;
            public float unk1;
            public ushort[] unk2 = new ushort[3];
            public StaticMeshLODHeader[] LODs;
            public bool valid = false;
            public StaticMeshHeader(Stream s)
            {
                localBoundingSphereRadius = Helper.ReadFloat(s);
                for(int i = 0; i < 4; i++)
                    localBoundingSphere[i] = Helper.ReadFloat(s);
                for (int i = 0; i < 3; i++)
                    localABBmin[i] = Helper.ReadFloat(s);
                for (int i = 0; i < 3; i++)
                    localABBmax[i] = Helper.ReadFloat(s);
                flags = Helper.ReadU16(s);
                lodCount = (byte)s.ReadByte();
                numAtomicMeshes = Helper.ReadU32(s);
                numAttachments = Helper.ReadU32(s);
                if (numAtomicMeshes != 0 || numAttachments != 0)
                    return;//todo
                unk1 = Helper.ReadFloat(s);
                for (int i = 0; i < 3; i++)
                    unk2[i] = Helper.ReadU16(s);
                LODs = new StaticMeshLODHeader[lodCount];
                for (int i = 0; i < lodCount; i++)
                {
                    LODs[i] = new StaticMeshLODHeader(s);
                    if (!LODs[i].isValid)
                        return;
                }
                valid = true;
            }
        }
        public class StaticMesh
        {
            public StaticMeshHeader header;
            public bool valid = false;
            public StaticMesh(Stream s)
            {
                header = new StaticMeshHeader(s);
                if (!header.valid)
                    return;
                valid = true;
            }
        }
        public bool isStatic = true;
        public StaticMesh stm;
        public long loadAddress;
        public ZRBResMesh(ZRBFile file, ZRBResourceEntry en, Stream s, long startPos)
        {
            loadAddress = file.vramOffsetGPU + en.vramOffsetGPU;
            MemoryStream m = new MemoryStream();
            while (s.Position < startPos + en.nextHeaderPos)
                m.WriteByte((byte)s.ReadByte());
            Data = m.ToArray();
            m.Seek(0, 0);
            uint typeFlags = Helper.ReadU32(m);
            isStatic = typeFlags == 0;
            if (isStatic)
                stm = new StaticMesh(m);
        }
        public override void LoadData(Stream s) { }
    }
}
