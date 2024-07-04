using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static SC2_3DS.Helper;
using static SC2_3DS.Matrix;
using static SC2_3DS.Textures;

namespace SC2_3DS
{
    internal class Textures
    {
        public struct TextureData
        {
            public byte[] Data;
        }

        public static TextureData ReadTextureData(BinaryReader reader, int size)
        {
            TextureData value = new TextureData
            {
                Data = reader.ReadBytes(size)
            };
            return value;
        }

        public struct TextureMap
        {
            public TextureMapType Type; // 0x01=Diffuse, 0x02=Effects, 0x03=Animating, 0x04=?, 0x05=SpecBall 
            public short Size; // size of entry?
            public float[] Values;
        }

        public static TextureMap ReadTextureMapXbox(BinaryReader reader)
        {
            TextureMap value = new TextureMap
            {
                Type = (TextureMapType)ReadUInt16L(reader),
                Size = ReadInt16L(reader)
            };
            value.Values = ReadTextureMapValuesL(reader, value.Size, (ushort)value.Type);
            return value;
        }

        public static TextureMap ReadTextureMapGCN(BinaryReader reader)
        {
            TextureMap value = new TextureMap
            {
                Type = (TextureMapType)ReadUInt16B(reader),
                Size = ReadInt16B(reader)
            };
            value.Values = ReadTextureMapValuesB(reader, value.Size, (ushort)value.Type);
            return value;
        }

        public struct MaterialTable //(info,map1,map2,map3,colour) (ambient,diffuse,specular)
        {
            public byte Type;
            public byte Ukn1;
            public byte Ukn2;
            public MaterialTableCull CullMode; // 0x00=DrawnBothSides, 0x08=DrawnOneSide
            public uint OpacitySrc;
            public uint VXTOffset0; // offsets within the VXT block
            public uint VXTOffset1;
            public uint VXTOffset2;
            public uint Map0Offset; //offsets to the texture table entries
            public uint Map1Offset;
            public uint Map2Offset;
            public Vector4 AmbientRGBA;
            public Vector4 DiffuseRGBA;
            public Vector3 SpecularRGB;
            public float SpecularPower;
        } // 80 bytes

        public static MaterialTable ReadMaterialTableXbox(BinaryReader reader)
        {
            MaterialTable value = new MaterialTable
            {
                 Type = reader.ReadByte(),
                 Ukn1 = reader.ReadByte(),
                 Ukn2 = reader.ReadByte(),
                 CullMode = (MaterialTableCull)reader.ReadByte(),
                 OpacitySrc = ReadUInt32L(reader),
                 VXTOffset0 = ReadUInt32L(reader),
                 VXTOffset1 = ReadUInt32L(reader),
                 VXTOffset2 = ReadUInt32L(reader),
                 Map0Offset = ReadUInt32L(reader), 
                 Map1Offset = ReadUInt32L(reader),
                 Map2Offset = ReadUInt32L(reader),
                 AmbientRGBA = ReadVector4L(reader),
                 DiffuseRGBA = ReadVector4L(reader),
                 SpecularRGB = ReadVector3L(reader),
                 SpecularPower = ReadSingleL(reader)
            };
            return value;
        }

        public static MaterialTable ReadMaterialTableGCN(BinaryReader reader)
        {
            MaterialTable value = new MaterialTable
            {
                Type = reader.ReadByte(),
                Ukn1 = reader.ReadByte(),
                Ukn2 = reader.ReadByte(),
                CullMode = (MaterialTableCull)reader.ReadByte(),
                OpacitySrc = ReadUInt32B(reader),
                VXTOffset0 = ReadUInt32B(reader),
                VXTOffset1 = ReadUInt32B(reader),
                VXTOffset2 = ReadUInt32B(reader),
                Map0Offset = ReadUInt32B(reader),
                Map1Offset = ReadUInt32B(reader),
                Map2Offset = ReadUInt32B(reader),
                AmbientRGBA = ReadVector4B(reader),
                SpecularRGB = ReadVector3B(reader),
                SpecularPower = ReadSingleB(reader)
            };
            return value;
        }

        // Thanks Aman
        public struct TextureDataTypeXbox //32 BYTES for type 0, 36 BYTES for type 2
        {
            public uint TexturePaletteCLUTOffset; //TEXTURE PALETTE ROW OFFSET
            public FmtTXVFlag Flags;
            public uint Unk1; //00000000
            public ushort HeightVisible; //visible height ONLY IN TYPE 2
            public ushort WidthVisible; //visible width ONLY IN TYPE 2
            public uint TextureDataOffset; // color look up table, if DXT this is the image data addr
            public ImageTypeXBOX ImageType; //0C = DXT1; 0F=DXT5; 0B=INDEX_COLOR(p8);
            public ushort Height; // dimension but in multiples of 8
            public ushort Width; // dimension but in multiples of 8
            public uint MipMapCount; // mip count range [1 - 6]
            public uint Pad2; // pad to align
            public TextureData DiffuseBytes;
            public TextureData[] MipMapBytes;
            public TexturePaletteXbox Palette;
            public int TextureSize;
        }

        public static TextureDataTypeXbox ReadTextureDataType0Xbox(BinaryReader reader)
        {
            TextureDataTypeXbox value = new TextureDataTypeXbox
            {
                TexturePaletteCLUTOffset = ReadUInt32L(reader),
                Flags = (FmtTXVFlag)ReadUInt32L(reader),
                Unk1 = ReadUInt32L(reader),
                TextureDataOffset = ReadUInt32L(reader),
                ImageType = (ImageTypeXBOX)ReadUInt32L(reader),
                Height = ReadUInt16L(reader),
                Width = ReadUInt16L(reader),
                MipMapCount = ReadUInt32L(reader),
                Pad2 = ReadUInt32L(reader)
            };
            return value;
        }

        public static TextureDataTypeXbox ReadTextureDataType2Xbox(BinaryReader reader)
        {
            TextureDataTypeXbox value = new TextureDataTypeXbox
            {
                TexturePaletteCLUTOffset = ReadUInt32L(reader),
                Flags = (FmtTXVFlag)ReadUInt32L(reader),
                Unk1 = ReadUInt32L(reader),
                HeightVisible = ReadUInt16L(reader),
                WidthVisible = ReadUInt16L(reader),
                TextureDataOffset = ReadUInt32L(reader),
                ImageType = (ImageTypeXBOX)ReadUInt32L(reader),
                Height = ReadUInt16L(reader),
                Width = ReadUInt16L(reader),
                MipMapCount = ReadUInt32L(reader),
                Pad2 = ReadUInt32L(reader)
            };
            return value;
        }

        public struct TextureDataTypeGCN
        {
            public uint TexturePaletteCLUTOffset; //TEXTURE PALETTE ROW OFFSET
            public uint Unk1;
            public ushort Unk2;
            public ushort Unk3;
            public int Dimensions; //This number with bitwise operators gives the x and y. Not accurate when alpha texture doesn't exist.
            public uint TextureDataOffset;
            public ushort Unk6;
            public ushort Unk7;
            public ImageTypeGCN ImageTypeVisible;
            public uint Unk8;
            public ushort Unk21;
            public ushort Unk9;
            public uint Unk10;
            public ushort Unk11;
            public ushort Unk12;
            public int AlphaDimensions;
            public uint AlphaTextureDataOffset;
            public ushort WidthVisible; //When the alpha texture is not existant, these two shorts have the texture dimensions.
            public ushort HeightVisible;
            public ImageTypeGCN AlphaImageTypeVisible; //Looks to be the same as diffuse, is 0 when alpha isn't present.
            public uint Unk18;
            public ushort Unk19;
            public ushort Unk20;
            public TextureData DiffuseBytes;
            public TextureData AlphaBytes;
            public TextureData MipMapBytes;
            public TextureData AlphaMipMapBytes;
            public TexturePaletteGCN Palette;
            public int TextureSize;
            public int AlphaTextureSize;
            public int MipMapSize;
            public int AlphaMipMapSize;
            public int Width;
            public int Height;
            public int AlphaWidth;
            public int AlphaHeight;
        } // 68 BYTES
        public static TextureDataTypeGCN ReadTextureDataTypeGCN(BinaryReader reader)
        {
            TextureDataTypeGCN value = new TextureDataTypeGCN
            {
                TexturePaletteCLUTOffset = ReadUInt32B(reader),
                Unk1 = ReadUInt32B(reader),
                Unk2 = ReadUInt16B(reader),
                Unk3 = ReadUInt16B(reader),
                Dimensions = ReadInt32B(reader),
                TextureDataOffset = ReadUInt32B(reader),
                Unk6 = ReadUInt16B(reader),
                Unk7 = ReadUInt16B(reader),
                ImageTypeVisible = (ImageTypeGCN)ReadUInt32B(reader),
                Unk8 = ReadUInt32B(reader),
                Unk21 = ReadUInt16B(reader),
                Unk9 = ReadUInt16B(reader),
                Unk10 = ReadUInt32B(reader),
                Unk11 = ReadUInt16B(reader),
                Unk12 = ReadUInt16B(reader),
                AlphaDimensions = ReadInt32B(reader),
                AlphaTextureDataOffset = ReadUInt32B(reader),
                WidthVisible = ReadUInt16B(reader),
                HeightVisible = ReadUInt16B(reader),
                AlphaImageTypeVisible = (ImageTypeGCN)ReadUInt32B(reader),
                Unk18 = ReadUInt32B(reader),
                Unk19 = ReadUInt16B(reader),
                Unk20 = ReadUInt16B(reader),
            };
            return value;
        }

        public struct TexturePaletteXbox
        {
            public int PaletteOffset;
            public int PaletteCount;
            public int Pad;
            public byte[] PaletteData;
        }
        public static TexturePaletteXbox ReadTexturePaletteXbox(BinaryReader reader)
        {
            TexturePaletteXbox value = new TexturePaletteXbox
            {
                PaletteOffset = ReadInt32L(reader),
                PaletteCount = ReadInt32L(reader),
                Pad = ReadInt32L(reader)
            };
            return value;
        }

        public struct TexturePaletteGCN
        {
            public uint Unk0;
            public uint PaletteOffset;
            public ushort PaletteCount;
            public ushort Unk1;
            public uint Unk2;
            public uint PaletteOffset2;
            public ushort PaletteCount2;
            public ushort Unk3;
            public ushort[] PaletteData;
            public ushort[] PaletteData2;
        }

        public static TexturePaletteGCN ReadTexturePaletteGCN(BinaryReader reader)
        {
            TexturePaletteGCN value = new TexturePaletteGCN
            {
                Unk0 = ReadUInt32B(reader),
                PaletteOffset = ReadUInt32B(reader),
                PaletteCount = ReadUInt16B(reader),
                Unk1 = ReadUInt16B(reader),
                Unk2 = ReadUInt32B(reader),
                PaletteOffset2 = ReadUInt32B(reader),
                PaletteCount2 = ReadUInt16B(reader),
                Unk3 = ReadUInt16B(reader)
            };
            return value;
        }
    }
}
