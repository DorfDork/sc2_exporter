using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SC2_3DS.Helper;

namespace SC2_3DS
{
    internal class Matrix
    {
        public struct MatrixTable //400 BYTES FOR VMX, 336 BYTES FOR VMG
        {
            public byte Type;
            public byte ParentBoneIdx; // parent object to this bone?
            public ushort Ukn1;
            public uint Ukn3; // on stages files an offset is present
            public uint Ukn4;
            public uint Ukn5;
            public Matrix4x4 Matrix;
            public byte[] Pad; //320 FOR XBOX, 256 FOR GCN
        }
        public static MatrixTable ReadMatrixTableXbox(BinaryReader reader)
        {
            MatrixTable value = new MatrixTable
            {
                Type = reader.ReadByte(),
                ParentBoneIdx = reader.ReadByte(),
                Ukn1 = ReadUInt16L(reader),
                Ukn3 = ReadUInt32L(reader),
                Ukn4 = ReadUInt32L(reader),
                Ukn5 = ReadUInt32L(reader),
                Matrix = ReadMatrix4x4L(reader),
                Pad = reader.ReadBytes(320)
            };
            return value;
        }
        public static MatrixTable ReadMatrixTableGCN(BinaryReader reader)
        {
            MatrixTable value = new MatrixTable
            {
                Type = reader.ReadByte(),
                ParentBoneIdx = reader.ReadByte(),
                Ukn1 = ReadUInt16B(reader),
                Ukn3 = ReadUInt32B(reader),
                Ukn4 = ReadUInt32B(reader),
                Ukn5 = ReadUInt32B(reader),
                Matrix = ReadMatrix4x4B(reader),
                Pad = reader.ReadBytes(256)
            };
            return value;
        }
        public struct MatrixUnk
        { // data Present in stage files? boundaries?
            public ushort Unk1;
            public ushort Matrix2Count;
            public uint Matrix2Offset; // if count is 0, then this will point to the material block
        }
        public static MatrixUnk ReadMatrixUnkXbox(BinaryReader reader)
        {
            MatrixUnk value = new MatrixUnk
            {
                Unk1 = ReadUInt16L(reader),
                Matrix2Count = ReadUInt16L(reader),
                Matrix2Offset = ReadUInt32L(reader),
            };
            return value;
        }
        public static MatrixUnk ReadMatrixUnkGCN(BinaryReader reader)
        {
            MatrixUnk value = new MatrixUnk
            {
                Unk1 = ReadUInt16B(reader),
                Matrix2Count = ReadUInt16B(reader),
                Matrix2Offset = ReadUInt32B(reader),
            };
            return value;
        }
        
    }
}
