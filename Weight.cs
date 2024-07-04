using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using static SC2_3DS.Helper;

namespace SC2_3DS
{
    internal class Weight
    {
        public struct WeightTableXbox
        {
            public uint VertCount1; // number of vertices with 1 bone weight per vertex bone_id1
            public uint VertCount2; // number of vertices with 2 bone weight per vertex bone_id2
            public uint VertCount3; // number of vertices with 3 bone weight per vertex bone_id3
            public uint VertCount4; // number of vertices with 4 bone weight per vertex bone_id4
            public uint WeightBufferOffset; // weight buffer, also contains vertices and Normals
            public uint VertBuffer1Offset; // Vertex Buffer
            public uint VertBuffer2Offset; // Vertex Buffer2, redundant?
        }

        public static WeightTableXbox ReadWeightTableXbox(BinaryReader reader)
        {
            WeightTableXbox value = new WeightTableXbox
            {
                VertCount1 = ReadUInt32L(reader),
                VertCount2 = ReadUInt32L(reader),
                VertCount3 = ReadUInt32L(reader),
                VertCount4 = ReadUInt32L(reader),
                WeightBufferOffset = ReadUInt32L(reader),
                VertBuffer1Offset = ReadUInt32L(reader),
                VertBuffer2Offset = ReadUInt32L(reader),
            };
            return value;
        }

        public struct WeightTableGCN
        {
            public uint VertCount1; // number of vertices with 1 bone weight per vertex bone_id1
            public uint VertCount2; // number of vertices with 2 bone weight per vertex bone_id2
            public uint VertCount3; // number of vertices with 3 bone weight per vertex bone_id3
            public uint VertCount4; // number of vertices with 4 bone weight per vertex bone_id4
            public uint WeightBufferOffset; // weight buffer, also contains vertices and Normals
            public uint PositionBuffer1Offset; // Vertex Buffer
            public uint PositionBuffer2Offset; // Vertex Buffer2, redundant?
            public uint NormalBuffer1Offset;
            public uint NormalBuffer2Offset;
        }

        public static WeightTableGCN ReadWeightTableGCN(BinaryReader reader)
        {
            WeightTableGCN value = new WeightTableGCN
            {
                VertCount1 = ReadUInt32B(reader),
                VertCount2 = ReadUInt32B(reader),
                VertCount3 = ReadUInt32B(reader),
                VertCount4 = ReadUInt32B(reader),
                WeightBufferOffset = ReadUInt32B(reader),
                PositionBuffer1Offset = ReadUInt32B(reader),
                PositionBuffer2Offset = ReadUInt32B(reader),
                NormalBuffer1Offset = ReadUInt32B(reader),
                NormalBuffer2Offset = ReadUInt32B(reader)
            };
            return value;
        }
        public struct WeightDefXbox
        {
            public Vector3 PositonXYZ; // positions relative to assigned bone
            public float BoneWeight;
            public Vector3 NormalXYZ;
            public byte BoneIdx;
            public byte Unk1;
            public byte Unk2;
            public byte Unk3;
        }

        public static WeightDefXbox ReadWeightDefXbox(BinaryReader reader)
        {
            WeightDefXbox value = new WeightDefXbox
            {
                PositonXYZ = ReadVector3L(reader),
                BoneWeight = ReadSingleL(reader),
                NormalXYZ = ReadVector3L(reader),
                BoneIdx = reader.ReadByte(),
                Unk1 = reader.ReadByte(),
                Unk2 = reader.ReadByte(),
                Unk3 = reader.ReadByte()
            };
            return value;
        }

        public struct WeightDefGCN
        {
            public Vector3Half PositonXYZ; // positions relative to assigned bone
            public Half BoneWeight;
            public Vector3Half NormalXYZ;
            public Half BoneIdx;
        }

        public static WeightDefGCN ReadWeightDefGCN(BinaryReader reader)
        {
            WeightDefGCN value = new WeightDefGCN
            {
                PositonXYZ = ReadVector3Half(reader),
                BoneWeight = ReadHalfB(reader),
                NormalXYZ = ReadVector3Half(reader),
                BoneIdx = ReadHalfB(reader)
            };
            return value;
        }
        public struct BoneTable // bone positions relative to its parent 64 BYTES
        {
            public Vector3 EndPosition;
            public float EndPositionScale;
            public Vector3 StartPosition;
            public float StartPositionScale;
            public Vector3 Rotation; // eulerangles, degrees (n*360)
            public uint BoneNameOffset;
            public Vector3 Ukn1;
            public byte Ukn2;
            public byte BoneParentIdx;
            public byte BoneIdx; // used for name look up?
            public byte Ukn3;
            public string Name;
            public Matrix4x4 LocalTransform;
            public Matrix4x4 GlobalTransform;
        }

        public static BoneTable ReadBoneTableXbox(BinaryReader reader)
        {
            BoneTable value = new BoneTable
            {
                EndPosition = ReadVector3L(reader),
                EndPositionScale = ReadSingleL(reader),
                StartPosition = ReadVector3L(reader),
                StartPositionScale = ReadSingleL(reader),
                Rotation = ReadVector3L(reader),
                BoneNameOffset = ReadUInt32L(reader),
                Ukn1 = ReadVector3L(reader),
                Ukn2 = reader.ReadByte(),
                BoneParentIdx = reader.ReadByte(),
                BoneIdx = reader.ReadByte(),
                Ukn3 = reader.ReadByte(),
                LocalTransform = Matrix4x4.Identity,
                GlobalTransform = Matrix4x4.Identity
            };
            return value;
        }

        public static BoneTable ReadBoneTableGCN(BinaryReader reader)
        {
            BoneTable value = new BoneTable
            {
                EndPosition = ReadVector3B(reader),
                EndPositionScale = ReadSingleB(reader),
                StartPosition = ReadVector3B(reader),
                StartPositionScale = ReadSingleB(reader),
                Rotation = ReadVector3B(reader),
                BoneNameOffset = ReadUInt32B(reader),
                Ukn1 = ReadVector3B(reader),
                Ukn2 = reader.ReadByte(),
                BoneParentIdx = reader.ReadByte(),
                BoneIdx = reader.ReadByte(),
                Ukn3 = reader.ReadByte(),
                LocalTransform = Matrix4x4.Identity,
                GlobalTransform = Matrix4x4.Identity
            };
            return value;
        }
    }
}
