using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using static SC2_3DS.Helper;
using static SC2_3DS.Objects;
using static SC2_3DS.Textures;
using static SC2_3DS.Weight;

namespace SC2_3DS
{
    class Objects
    {
        //VERTEX DEF
        public struct Buffer1Xbox
        {
            public Byte4Array ColorRGBA; // divide 255 to get FLOAT
            public Vector2 TileUV; //tu = readfloat f  tv = (readfloat f*-1)+1  static_mesh.texture[idx][x] = [tu,tv,0]
        }

        public static Buffer1Xbox ReadBuffer1Xbox(BinaryReader reader)
        {
            Buffer1Xbox value = new Buffer1Xbox
            {
                ColorRGBA = ReadByte4Array(reader),
                TileUV = ReadVector2L(reader)
            };
            return value;
        }

        public struct Buffer2Xbox
        {
            public Vector3 Position;
            public float PositionScale;
            public Vector3 Normal;
            public float NormalScale;
        }

        public static Buffer2Xbox ReadBuffer2Xbox(BinaryReader reader)
        {
            Buffer2Xbox value = new Buffer2Xbox
            {
                Position = ReadVector3L(reader),
                PositionScale = ReadSingleL(reader),
                Normal = ReadVector3L(reader),
                NormalScale = ReadSingleL(reader)
            };
            return value;
        }

        public struct Buffer4Xbox //40 BYTES IN SIZE
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Buffer1Xbox VertDef;
            public int Pad; //some index? bone parent?
        }

        public static Buffer4Xbox ReadBuffer4Xbox(BinaryReader reader)
        {
            Buffer4Xbox value = new Buffer4Xbox
            {
                Position = ReadVector3L(reader), 
                Normal = ReadVector3L(reader),
                VertDef = ReadBuffer1Xbox(reader),
                Pad = ReadInt32L(reader)
            };
            return value;
        }

        //vmxGeometryStatic (position,Normal,colour,texture,center_radius,faces)
        public static FacesData ReadFacesDataXbox(BinaryReader reader, int size)
        {
            FacesData value = new FacesData();
            byte[] buffer = reader.ReadBytes(size * sizeof(ushort));
            value.Data = new ushort[size];
            Buffer.BlockCopy(buffer, 0, value.Data, 0, buffer.Length);
            return value;
        }

        public struct StaticMeshXbox
        {
            public FacesData Faces;
            public List<Tuple<ushort, ushort, ushort>> Indicies;
            public Buffer4Xbox[] Buffer4Data;
            public Vector4 CenterRadius;
        }

        public static StaticMeshXbox ReadStaticMeshXbox(BinaryReader reader)
        {
            StaticMeshXbox value = new StaticMeshXbox();
            return value;
        }

        //vmxGeometrySkinned (position,Normal,colour,texture,weight,boneid,faces,matids)
        public struct SkinnedMeshXbox
        {
            public FacesData Faces;
            public List<Tuple<ushort, ushort, ushort>> Indicies;
        }

        public static SkinnedMeshXbox ReadSkinnedMeshXbox(BinaryReader reader, int size)
        {
            SkinnedMeshXbox value = new SkinnedMeshXbox();
            return value;
        }


        public struct StaticMeshGCN
        {
            public FacesData Faces;
            public Byte4Array ColorRGBA; // divide 255 to get FLOAT
            public Vector2 TileUV; //tu = readfloat f  tv = (readfloat f*-1)+1  static_mesh.texture[idx][x] = [tu,tv,0]
            public Buffer4Xbox[] Buffer4Data;
            public Vector4 CenterRadius; //UnkE, possibly same as the xbox
        }

        public static StaticMeshGCN ReadStaticMeshGCN(BinaryReader reader)
        {
            StaticMeshGCN value = new StaticMeshGCN
            {

            };
            return value;
        }


        public struct SkinnedMeshGCN
        {
            public FacesData Faces;
        }

        public static SkinnedMeshGCN ReadSkinnedMeshGCN(BinaryReader reader, int size)
        {
            SkinnedMeshGCN value = new SkinnedMeshGCN();
            return value;
        }

        //LAYEROBJECTS
        public struct LayerObjectEntryXbox //40 BYTES (XBOX) (info,offset)
        {
            public MeshXboxContent ObjectType; // 0x00=Static, 0x04=Skinned
            public PrimitiveXbox PrimitiveType; // 0x00=TriangleStrip, 0x01=TriangleList
            public uint FaceCount; // Number of Index in Index Buffer
            public uint MatrixOffset;
            public uint MaterialOffset;
            public uint FaceOffset; // Index Buffer Offset
            public uint Buffer1Offset; // for skinned and static meshes this is the vertex buffer
            public uint Buffer2Offset;
            public uint Buffer3Offset;
            public uint Buffer4Offset;
            //Center+Radius Offset. The value maybe zero when object type equal to four(skinned mesh). besides, the offset address are alway followed the main vertex buffer,
            //it maybe use to calculate the size of vertex buffer of static mesh. ie. VertexBufferSize = CenterRadius
            public uint CenterRadiusOffset; // "Aman" describes this section as "Center+Radius Offset"
            public StaticMeshXbox StaticMesh;
            public SkinnedMeshXbox SkinnedMesh;
        }
        public static LayerObjectEntryXbox ReadLayerObjectEntryXbox(BinaryReader reader)
        {
            LayerObjectEntryXbox value = new LayerObjectEntryXbox
            {
                ObjectType = (MeshXboxContent)ReadUInt16L(reader),
                PrimitiveType = (PrimitiveXbox)ReadUInt16L(reader),
                FaceCount = ReadUInt32L(reader),
                MatrixOffset = ReadUInt32L(reader),
                MaterialOffset = ReadUInt32L(reader),
                FaceOffset = ReadUInt32L(reader),
                Buffer1Offset = ReadUInt32L(reader),
                Buffer2Offset = ReadUInt32L(reader),
                Buffer3Offset = ReadUInt32L(reader),
                Buffer4Offset = ReadUInt32L(reader),
                CenterRadiusOffset = ReadUInt32L(reader)
            };
            return value;
        }

        public struct LayerObjectEntryGCN //56 BYTES (GCN) (info,offset)
        {
            public byte[] Unk0; // These unk are consistent with skinned/static meshes
            public Byte4Array Indice;
            public ushort FaceCount;
            public uint MatrixOffset;
            public uint MaterialOffset;
            public uint Position1Offset; //Vertex Buffer1
            public uint Position2Offset; //Vertex Buffer2
            public uint Normal1Offset; //Normal Buffer1
            public uint Normal2Offset; //Normal Buffer2
            public uint ColorOffset;
            public uint TexCoordOffset;
            public uint FaceOffset;
            public uint UnkE;
            //public StaticMeshGCN StaticMesh;
            public SkinnedMeshGCN SkinnedMesh;
            public int[] IndiceData;
        }

        public static LayerObjectEntryGCN ReadLayerObjectEntryGCN(BinaryReader reader)
        {
            LayerObjectEntryGCN value = new LayerObjectEntryGCN
            {
                Unk0 = reader.ReadBytes(10),
                Indice = ReadByte4Array(reader),
                FaceCount = ReadUInt16B(reader),
                MatrixOffset = ReadUInt32B(reader),
                MaterialOffset = ReadUInt32B(reader),
                Position1Offset = ReadUInt32B(reader),
                Position2Offset = ReadUInt32B(reader),
                Normal1Offset = ReadUInt32B(reader),
                Normal2Offset = ReadUInt32B(reader),
                ColorOffset = ReadUInt32B(reader),
                TexCoordOffset = ReadUInt32B(reader),
                FaceOffset = ReadUInt32B(reader),
                UnkE = ReadUInt32B(reader)
            };
            return value;
        }
    }
}
