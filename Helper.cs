using System.IO;
using System.Numerics;
using static SC2_3DS.Headers;
using static SC2_3DS.Matrix;
using static SC2_3DS.Objects;
using static SC2_3DS.Textures;
using static SC2_3DS.Weight;

namespace SC2_3DS
{
    internal class Helper
    {
        //Enum
        public enum Endianness
        {
            LittleEndian = 0,
            BigEndian = 1
        }
        public enum ConsoleVersion : byte
        {
            UNK0 = 0,
            UNK1 = 1,
            UNK2 = 2,
            GAMECUBE = 3,
            XBOX = 4
        }
        public enum ModelContent : byte
        {
            STAGE = 0,
            CHARACTER = 1,
            WEAPON = 2
        }
        public enum MeshXboxContent : ushort
        {
            STATIC = 0,
            SKINNED = 4
        }
        public enum MeshGCNContent : byte
        {
            STATIC = 2, // (2, 2, 2, 2)
            SKINNED = 3 // (3, 3, 2, 3)  //(3, 3, 3, 3)
        }
        public enum PrimitiveXbox : ushort
        {
            TRIANGLESTRIP = 0,
            TRIANGLELIST = 1
        }
        public enum ImageTypeGCN : uint
        {
            I4 = 0, // (4 bit intensity, 8x8 tiles)
            I8 = 1, // (8 bit intensity, 8x4 tiles)
            IA4 = 2, // (4 bit intensity with 4 bit alpha, 8x4 tiles)
            IA8 = 3, // (8 bit intensity with 8 bit alpha, 4x4 tiles)
            RGB565 = 4, // (4x4 tiles)
            RGB5A3 = 5, // (*) (4x4 tiles)
            RGBA8 = 6, // (4x4 tiles in two cache lines - first is AR and second is GB)
            CI4 = 8, // (4 bit color index, 8x8 tiles)
            CI8 = 9, // (8 bit color index, 8x4 tiles)
            C14X2 = 10, // (14 bit color index, 4x4 tiles)
            CMPR = 14, // (S3TC compressed, 2x2 blocks of 4x4 tiles)
            XFB = 15, // 0x0F
            UNK16 = 16 // 0x10
        }
        public enum ImageTypeXBOX : uint
        {
            ARGB = 6,
            P8 = 11, // INDEX_COLOR
            DXT1 = 12,
            DXT3 = 14,
            DXT5 = 15
        }
        public enum TextureMapType : ushort
        {
            UNKTT0 = 0,
            DIFFUSE = 1,
            EFFECTS = 2,
            ANIMATE = 3,
            UNKTT = 4,
            SPECBALL = 5
        }
        public enum MaterialTableCull : byte
        {
            DRAWNBOTHSIDES = 0,
            DRAWNONESIDE = 8
        }
        [Flags]
        public enum FmtTXVFlag : uint
        {
            BIT01 = 1 << 0,
            CLAMP = 1 << 1,
            BIT03 = 1 << 2,
            BIT04 = 1 << 3,
            TWOSIDED = 1 << 4,
            BIT06 = 1 << 5,
            BIT07 = 1 << 6,
            BIT08 = 1 << 7,
            SPEC = 1 << 8,
            BIT10 = 1 << 9,
            BIT11 = 1 << 10,
            BIT12 = 1 << 11,
            BIT13 = 1 << 12,
            BIT14 = 1 << 13
        }
        public struct DDSHeaderFlags
        {
           public const int DDSD_CAPS = 0x1;
           public const int DDSD_HEIGHT = 0x2;
           public const int DDSD_WIDTH = 0x4;
           public const int DDSD_PITCH = 0x8;
           public const int DDSD_PIXELFORMAT = 0x1000;
           public const int DDSD_MIPMAPCOUNT = 0x20000;
           public const int DDSD_LINEARSIZE = 0x80000;
           public const int DDSD_DEPTH = 0x800000;
        }
        public struct DDSPixelFlags
        {
            public const int DDPF_ALPHAPIXELS = 0x1;
            public const int DDPF_ALPHA = 0x2;
            public const int DDPF_FOURCC = 0x4;
            public const int DDPF_PALETTEINDEXED8 = 0x20;
            public const int DDPF_RGB = 0x40;
            public const int DDPF_YUV = 0x200;
            public const int DDPF_LUMINANCE = 0x20000;
        }
        public struct DDSCapFlags
        {
            public const int DDSCAPS_COMPLEX = 0x8;
            public const int DDSCAPS_TEXTURE = 0x1000;
            public const int DDSCAPS_MIPMAP = 0x400000;
        }
        public struct FacesData
        {
            public ushort[] Data;
        }
        public struct Vector3Half
        {
            public Half X;
            public Half Y;
            public Half Z;
        }
        public static Vector3Half ReadVector3Half(BinaryReader reader)
        {
            Vector3Half value = new Vector3Half
            {
                X = ReadHalfB(reader),
                Y = ReadHalfB(reader),
                Z = ReadHalfB(reader)
            };
            return value;
        }
        public static Vector2 ReadVector2L(BinaryReader reader)
        {
            Vector2 value = new Vector2
            {
                X = ReadSingleL(reader),
                Y = ReadSingleL(reader)
            };
            return value;
        }
        public static Vector2 ReadVector2B(BinaryReader reader)
        {
            Vector2 value = new Vector2
            {
                X = ReadSingleB(reader),
                Y = ReadSingleB(reader)
            };
            return value;
        }
        public static Vector3 ReadVector3L(BinaryReader reader)
        {
            Vector3 value = new Vector3
            {
                X = ReadSingleL(reader),
                Y = ReadSingleL(reader),
                Z = ReadSingleL(reader)
            };
            return value;
        }
        public static Vector3 ReadVector3B(BinaryReader reader)
        {
            Vector3 value = new Vector3
            {
                X = ReadSingleB(reader),
                Y = ReadSingleB(reader),
                Z = ReadSingleB(reader)
            };
            return value;
        }
        public static Vector4 ReadVector4L(BinaryReader reader)
        {
            Vector4 value = new Vector4
            {
                X = ReadSingleL(reader),
                Y = ReadSingleL(reader),
                Z = ReadSingleL(reader),
                W = ReadSingleL(reader)
            };
            return value;
        }
        public static Vector4 ReadVector4B(BinaryReader reader)
        {
            Vector4 value = new Vector4
            {
                X = ReadSingleB(reader),
                Y = ReadSingleB(reader),
                Z = ReadSingleB(reader),
                W = ReadSingleB(reader)
            };
            return value;
        }
        public static Matrix4x4 ReadMatrix4x4L(BinaryReader reader)
        {
            Matrix4x4 value = new Matrix4x4
            {
                M11 = ReadSingleL(reader),
                M12 = ReadSingleL(reader),
                M13 = ReadSingleL(reader),
                M14 = ReadSingleL(reader),

                M21 = ReadSingleL(reader),
                M22 = ReadSingleL(reader),
                M23 = ReadSingleL(reader),
                M24 = ReadSingleL(reader),

                M31 = ReadSingleL(reader),
                M32 = ReadSingleL(reader),
                M33 = ReadSingleL(reader),
                M34 = ReadSingleL(reader),

                M41 = ReadSingleL(reader),
                M42 = ReadSingleL(reader),
                M43 = ReadSingleL(reader),
                M44 = ReadSingleL(reader),
            };
            return value;
        }
        public static Matrix4x4 ReadMatrix4x4B(BinaryReader reader)
        {
            Matrix4x4 value = new Matrix4x4
            {
                M11 = ReadSingleB(reader),
                M12 = ReadSingleB(reader),
                M13 = ReadSingleB(reader),
                M14 = ReadSingleB(reader),

                M21 = ReadSingleB(reader),
                M22 = ReadSingleB(reader),
                M23 = ReadSingleB(reader),
                M24 = ReadSingleB(reader),

                M31 = ReadSingleB(reader),
                M32 = ReadSingleB(reader),
                M33 = ReadSingleB(reader),
                M34 = ReadSingleB(reader),

                M41 = ReadSingleB(reader),
                M42 = ReadSingleB(reader),
                M43 = ReadSingleB(reader),
                M44 = ReadSingleB(reader),
            };
            return value;
        }
        public struct Byte4Array
        {
            public byte B1;
            public byte B2;
            public byte B3;
            public byte B4;
        }
        public static Byte4Array ReadByte4Array(BinaryReader reader)
        {
            Byte4Array value = new Byte4Array
            {
                B1 = reader.ReadByte(),
                B2 = reader.ReadByte(),
                B3 = reader.ReadByte(),
                B4 = reader.ReadByte()
            };
            return value;
        }
        public static ushort ReadUInt16L(BinaryReader reader)
        {
            return reader.ReadUInt16();
        }
        public static ushort ReadUInt16B(BinaryReader reader)
        {
            var data = reader.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }
        public static short ReadInt16L(BinaryReader reader)
        {
            return reader.ReadInt16();
        }
        public static short ReadInt16B(BinaryReader reader)
        {
            var data = reader.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }
        public static uint ReadUInt32L(BinaryReader reader)
        {
            return reader.ReadUInt32();
        }
        public static uint ReadUInt32B(BinaryReader reader)
        {
            var data = reader.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }
        public static int ReadInt32L(BinaryReader reader)
        {
            return reader.ReadInt32();
        }
        public static int ReadInt32B(BinaryReader reader)
        {
            var data = reader.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }
        public static Half ReadHalfL(BinaryReader reader)
        {
            return reader.ReadHalf();
        }
        public static Half ReadHalfB(BinaryReader reader)
        {
            var data = reader.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToHalf(data, 0);
        }
        public static float ReadSingleL(BinaryReader reader)
        {
            return reader.ReadSingle();
        }
        public static float ReadSingleB(BinaryReader reader)
        {
            var data = reader.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToSingle(data, 0);
        }



        public static string ReadNullTerminatedString(BinaryReader reader)
        {
            string str = "";
            char ch;
            while ((int)(ch = reader.ReadChar()) != 0)
                str = str + ch;
            return str;
        }

        public static int[] ReadVertXbox(int size, FacesData data, int TempVertMin, int TempVertMax)
        {
            int[] TempVert = new int[3];
            for (int j = 0; j < size; j++)
            {
                if (data.Data[j] > TempVertMax) TempVertMax = data.Data[j];
                if (data.Data[j] < TempVertMin) TempVertMin = data.Data[j];
            }
            TempVert[0] = TempVertMax - TempVertMin + 1;
            TempVert[1] = TempVertMin;
            TempVert[2] = TempVertMax;
            return TempVert;
        }

        public static float[] ReadTextureMapValuesL(BinaryReader reader, int size, ushort type)
        {
            float[] value = Array.Empty<float>();
            if (size != -1 && size > 4) {
                if (type == 1 && size == 36) {
                    value = new Single[8];
                    for (int i = 0; i < 8; i++)
                    {
                        value[i] = ReadSingleL(reader); //First two floats are Tile UV
                    }
                }
                else {
                    value = new Single[(size - 4) / 4];
                    for (int i = 0; i < (size - 4) / 4; i++)
                    {
                        value[i] = ReadSingleL(reader);
                    }
                }
            }
            return value;
        }

        static public float[] ReadTextureMapValuesB(BinaryReader reader, int size, ushort type)
        {
            float[] value = Array.Empty<float>();
            if (size != -1 && size > 4)
            {
                if (type == 1 && size == 36)
                {
                    value = new Single[8];
                    for (int i = 0; i < 8; i++)
                    {
                        value[i] = ReadSingleB(reader); //First two floats are Tile UV
                    }
                }
                else
                {
                    value = new Single[(size - 4) / 4];
                    for (int i = 0; i < (size - 4) / 4; i++)
                    {
                        value[i] = ReadSingleB(reader);
                    }
                }
            }
            return value;
        }

        static public void WeightDefToArray(VMXObject vmxobject, out List<float> weightValues, out List<int> vData)
        {
            int index = 0;
            weightValues = new List<float>();
            vData = new List<int>();
            foreach (var value in vmxobject.WeightDef1Bone)
            {
                weightValues.Add(value.BoneWeight);
                vData.Add((int)value.BoneIdx);
                vData.Add(index);
                index++;
            }

            foreach (var value in vmxobject.WeightDef2Bone)
            {
                weightValues.Add(value.BoneWeight);
                vData.Add((int)value.BoneIdx);
                vData.Add(index);
                index++;
            }

            foreach (var value in vmxobject.WeightDef3Bone)
            {
                weightValues.Add(value.BoneWeight);
                vData.Add((int)value.BoneIdx);
                vData.Add(index);
                index++;
            }

            foreach (var value in vmxobject.WeightDef4Bone)
            {
                weightValues.Add(value.BoneWeight);
                vData.Add((int)value.BoneIdx);
                vData.Add(index);
                index++;
            }
        }

        public static List<float[]> Buffer2PositionToArray(Buffer2Xbox[] buffer2)
        {
            List<float[]> positions = new List<float[]> { };
            foreach (var value in buffer2)
            {
                float[] position = { value.Position.X, value.Position.Y, value.Position.Z };
                positions.Add(position);
            }
            return positions;
        }
        public static List<float[]> Buffer2NormalToArray(Buffer2Xbox[] buffer2)
        {
            List<float[]> normals = new List<float[]> { };
            foreach (var value in buffer2)
            {
                float[] normal = { value.Normal.X, value.Normal.Y, value.Normal.Z };
                normals.Add(normal);
            }
            return normals;
        }
        public static List<float[]> Buffer4PositionToArray(Buffer4Xbox[] buffer4)
        {
            List<float[]> positions = new List<float[]> { };
            foreach (var value in buffer4)
            {
                float[] position = { value.Position.X, value.Position.Y, value.Position.Z };
                positions.Add(position);
            }
            return positions;
        }
        public static List<float[]> Buffer4NormalToArray(Buffer4Xbox[] buffer4)
        {
            List<float[]> normals = new List<float[]> { };
            foreach (var value in buffer4)
            {
                float[] normal = { value.Normal.X, value.Normal.Y, value.Normal.Z };
                normals.Add(normal);
            }
            return normals;
        }
        public static List<float[]> Buffer1TexcoordToArray(Buffer1Xbox[] buffer1)
        {
            List<float[]> TexCoords = new List<float[]> { };
            foreach (var value in buffer1)
            {
                float[] TexCoord =
                {
                    value.TileUV.X,
                    1.0f - value.TileUV.Y //Flip UV,
                };
                TexCoords.Add(TexCoord);
            }
            return TexCoords;
        }
        public static List<float[]> Buffer4TexcoordToArray(Buffer4Xbox[] buffer4)
        {
            List<float[]> TexCoords = new List<float[]> { };
            foreach (var value in buffer4)
            {
                float[] TexCoord = 
                {
                    value.VertDef.TileUV.X,
                    1.0f - value.VertDef.TileUV.Y //Flip UV,
                };
                TexCoords.Add(TexCoord);
            }
            return TexCoords;
        }

        //GCN Textures
        static public uint ImageSizeGCN(uint num)
        {
            uint value = 0;
            switch (num)
            {
                case 2: value = 2 << 6; break; // 8 x 8 
                case 4: value = 2 << 7; break; // 16 x 8 
                case 8: value = 2 << 8; break; // 16 x 16 
                case 16: value = 2 << 9; break; // 32 x 16 
                case 32: value = 2 << 10; break; // 32 x 32
                case 64: value = 2 << 11; break; // 64 x 32
                case 128: value = 2 << 12; break; // 64 x 64
                case 256: value = 2 << 13; break; // 128 x 64
                case 512: value = 2 << 14; break; // 128 x 128
                case 1024: value = 2 << 15; break; // 256 x 128
                case 2048: value = 2 << 16; break; // 256 x 256
                case 4096: value = 2 << 17; break; // 512 x 256
                case 8192: value = 2 << 18; break; // 512 x 512
            }
            return value;
        }
        static public byte[] UnSwizzleGCNData(byte[] srcBuf, int size, int width, int height)
        {
            byte[] newBuf = new byte[size];
            List<byte> chunk1 = new List<byte>();
            List<byte> chunk2 = new List<byte>();
            List<byte> chunk3 = new List<byte>();
            List<byte> chunk4 = new List<byte>();
            List<byte> totalchunks = new List<byte>();

            srcBuf = ByteSwapAlternateBADC(srcBuf);
            for (int i = 0; i < size; i += 8)
            {
                newBuf[i] = srcBuf[i];
                newBuf[i + 1] = srcBuf[i + 1];
                newBuf[i + 2] = srcBuf[i + 2];
                newBuf[i + 3] = srcBuf[i + 3];
                newBuf[i + 4] = SwapByte(srcBuf[i + 4]);
                newBuf[i + 5] = SwapByte(srcBuf[i + 5]);
                newBuf[i + 6] = SwapByte(srcBuf[i + 6]);
                newBuf[i + 7] = SwapByte(srcBuf[i + 7]);
            }

            for (int index1 = 0; index1 < height >> 3; index1++)
            {
                chunk1 = new List<byte>(); // we need to clear them every loop
                chunk2 = new List<byte>();
                chunk3 = new List<byte>();
                chunk4 = new List<byte>();
                int offset = (index1 * (width * 4));
                for (int index2 = 0; index2 < width >> 2; index2++)
                {
                    if (index2 % 2 == 0)
                    {
                        if (index2 >= width >> 3)
                        {
                            for (int j = 0; j < 16; j++)
                                chunk2.Add(newBuf[offset + (index2 * 16) + j]);
                        }
                        else
                        {
                            for (int j = 0; j < 16; j++)
                                chunk1.Add(newBuf[offset + (index2 * 16) + j]);
                        }
                    }
                    else if (index2 % 2 == 1)
                    {
                        if (index2 >= width >> 3)
                        {
                            for (int j = 0; j < 16; j++)
                                chunk4.Add(newBuf[offset + (index2 * 16) + j]);
                        }
                        else
                        {
                            for (int j = 0; j < 16; j++)
                                chunk3.Add(newBuf[offset + (index2 * 16) + j]);
                        }
                    }
                }
                totalchunks.AddRange(chunk1);
                totalchunks.AddRange(chunk2);
                totalchunks.AddRange(chunk3);
                totalchunks.AddRange(chunk4);
            }
            return totalchunks.ToArray();
        }
        public static byte[] ConvertI4ToRGBA(byte[] i4Data)
        {
            int rgbaLength = i4Data.Length * 4;
            byte[] rgbaData = new byte[rgbaLength];

            for (int i = 0, j = 0; i < i4Data.Length; i++)
            {
                byte i4Value2 = (byte)(i4Data[i] & 0x0F);

                // Convert second 4-bit value
                byte grayscale2 = (byte)(i4Value2 * 0x11);
                rgbaData[j++] = grayscale2; // R
                rgbaData[j++] = grayscale2; // G
                rgbaData[j++] = grayscale2; // B
                rgbaData[j++] = 0xFF;       // A
            }

            return rgbaData;
        }
        public static byte[] UnSwizzleP8Bytes(byte[] srcBuf, int width, int height, int bytesPerPixel)
        {
            (int maskX, int maskY, int maskZ) = GenerateSwizzleMasks(height, width);
            byte[] dstBuf = new byte[srcBuf.Length];

            for (int z = 0; z < 1; z++)
            {
                for (int y = 0; y < width; y++)
                {
                    for (int x = 0; x < height; x++)
                    {
                        int srcIndex = GetSwizzledOffset(x, y, z, maskX, maskY, maskZ, bytesPerPixel);
                        int dstIndex = (y * height + x) * bytesPerPixel;
                        Buffer.BlockCopy(srcBuf, srcIndex, dstBuf, dstIndex, bytesPerPixel);
                    }
                }
            }
            return dstBuf;
        }
        private static (int, int, int) GenerateSwizzleMasks(int height, int width)
        {
            int maskX = 0, maskY = 0, maskZ = 0;
            int bit = 1, depth = 1, maskBit = 1;

            while (bit < height || bit < width || bit < depth)
            {
                if (bit < height)
                {
                    maskX |= maskBit;
                    maskBit <<= 1;
                }
                if (bit < width)
                {
                    maskY |= maskBit;
                    maskBit <<= 1;
                }
                if (bit < depth)
                {
                    maskZ |= maskBit;
                    maskBit <<= 1;
                }
                bit <<= 1;
            }
            return (maskX, maskY, maskZ);
        }
        private static int FillPattern(int pattern, int value)
        {
            int result = 0;
            int bit = 1;

            while (value != 0)
            {
                if ((pattern & bit) != 0)
                {
                    if ((value & 1) != 0)
                        result |= bit;
                    value >>= 1;
                }
                bit <<= 1;
            }
            return result;
        }
        private static int GetSwizzledOffset(int x, int y, int z, int maskX, int maskY, int maskZ, int bytesPerPixel)
        {
            return bytesPerPixel * (FillPattern(maskX, x) | FillPattern(maskY, y) | FillPattern(maskZ, z));
        }
        public static byte SwapByte(byte srcByte)
        {
            byte newByte = (byte)((srcByte & 51) << 2 | (srcByte & 204) >> 2);
            return (byte)(((newByte & 15) << 4 | (newByte & 240) >> 4) & 255);
        }
        public static byte[] ByteSwapAlternateBADC(byte[] srcBuf)
        {
            byte[] outBuf = new byte[srcBuf.Length];
            Array.Copy(srcBuf, outBuf, srcBuf.Length);
            for (int i = 0; i < srcBuf.Length; i += 8) // every other 4 bytes
            {
                outBuf[i] = srcBuf[i + 1]; //B
                outBuf[i + 1] = srcBuf[i]; //A
                outBuf[i + 2] = srcBuf[i + 3]; //D
                outBuf[i + 3] = srcBuf[i + 2]; //C
            }
            return outBuf;
        }
        static public byte[] ByteSwapAC(byte[] srcBuf)
        {
            byte[] outBuf = new byte[srcBuf.Length];
            for (int i = 0; i < srcBuf.Length; i += 4)
            {
                outBuf[i] = srcBuf[i + 2]; //C
                outBuf[i + 1] = srcBuf[i + 1]; //B
                outBuf[i + 2] = srcBuf[i]; //A
                outBuf[i + 3] = srcBuf[i + 3]; //D
            }
            return outBuf;
        }

        //Xbox Textures
        public static byte[] DecodeC8(byte[] src, int width, int height, int textureFormat, ushort[] tlut, int tlutFmt)
        {
            byte[] dst = new byte[src.Length];
            int num1 = (width + 3) / 4;
            int num2 = (width + 7) / 8;
            if (textureFormat == 9)
            {
                for (int index = 0; index < height; index += 4)
                {
                    int num3 = 0;
                    int num4 = index / 4 * num2;
                    while (num3 < width)
                    {
                        int num5 = 0;
                        int num6 = 4 * num4;
                        while (num5 < 4)
                        {
                            DecodeBytesC8(dst, src, (index + num5) * width + num3, 8 * num6, tlut, tlutFmt);
                            num5++;
                            num6++;
                        }
                        num3 += 8;
                        num4++;
                    }
                }
            }
            return dst;
        }
        private static void DecodeBytesC8(byte[] dst, byte[] src, int dstPos, int srcPos, ushort[] tlut, int tlutFmt)
        {
            for (int i = 0; i < 8; i++)
            {
                byte index = src[srcPos + i];
                dst[dstPos] = DecodePixelPaletted(tlut[index], tlutFmt);
                dstPos++;
            }
        }
        private static byte DecodePixelPaletted(ushort pixel, int tlutFmt)
        {
            switch (tlutFmt)
            {
                case 0:
                    return DecodePixelIA8(pixel);
                case 1:
                    return DecodePixelRGB565(pixel);
                case 2:
                    return DecodePixelRGB5A3(pixel);
                default:
                    return 0;
            }
        }
        private static byte DecodePixelIA8(ushort value)
        {
            byte num1 = (byte)(value & 0xFF);
            byte num2 = (byte)(value >> 8);
            return (byte)(num2 | (num2 << 8) | (num2 << 16) | (num1 << 24));
        }
        private static byte DecodePixelRGB565(ushort value)
        {
            byte num1 = Convert5To8((value >> 11) & 31);
            byte num2 = Convert6To8((value >> 5) & 63);
            byte num3 = Convert5To8(value & 31);
            return (byte)(num1 | (num2 << 8) | (num3 << 16) | (0xFF << 24));
        }
        private static byte DecodePixelRGB5A3(ushort value)
        {
            byte num1, num2, num3, num4;
            if ((value & 0x8000) != 0)
            {
                num1 = Convert5To8((value >> 10) & 31);
                num2 = Convert5To8((value >> 5) & 31);
                num3 = Convert5To8(value & 31);
                num4 = 0xFF;
            }
            else
            {
                num4 = Convert3To8((value >> 12) & 7);
                num1 = Convert4To8((value >> 8) & 15);
                num2 = Convert4To8((value >> 4) & 15);
                num3 = Convert4To8(value & 15);
            }
            return (byte)(num1 | (num2 << 8) | (num3 << 16) | (num4 << 24));
        }
        private static byte Convert3To8(int v)
        {
            return (byte)((v << 5) | (v << 2) | (v >> 1));
        }
        private static byte Convert4To8(int v)
        {
            return (byte)((v << 4) | v);
        }
        private static byte Convert5To8(int v)
        {
            return (byte)((v << 3) | (v >> 2));
        }
        private static byte Convert6To8(int v)
        {
            return (byte)((v << 2) | (v >> 4));
        }
        public static void R5G6B5ToRGBA8(ushort SrcPixel, ref byte[] Dest, int Offset)
        {
            byte num1 = (byte)(((int)SrcPixel & 61696) >> 11);
            byte num2 = (byte)(((int)SrcPixel & 2016) >> 5);
            byte num3 = (byte)((uint)SrcPixel & 31U);
            byte num4 = (byte)((int)num1 << 3 | (int)num1 >> 2);
            byte num5 = (byte)((int)num2 << 2 | (int)num2 >> 4);
            byte num6 = (byte)((int)num3 << 3 | (int)num3 >> 2);
            Dest[Offset] = num4;
            Dest[Offset + 1] = num5;
            Dest[Offset + 2] = num6;
            Dest[Offset + 3] = byte.MaxValue;
        }
        public static void GRBA8ToRGBA8(ushort SrcPixel, ushort SrcPixel2, ref byte[] Dest, int Offset)
        {
            byte num1 = (byte)((uint)SrcPixel & (uint)byte.MaxValue);
            byte num2 = (byte)(((int)SrcPixel & 65280) >> 8);
            byte num3 = (byte)((uint)SrcPixel2 & (uint)byte.MaxValue);
            byte num4 = (byte)(((int)SrcPixel2 & 65280) >> 8);
            Dest[Offset] = num1;
            Dest[Offset + 1] = num2;
            Dest[Offset + 2] = num3;
            Dest[Offset + 3] = num4;
        }
        public static void RGB5A3ToRGBA8(ushort SrcPixel, ref byte[] Dest, int Offset)
        {
            byte num1;
            byte num2;
            byte num3;
            byte num4;
            if (((int)SrcPixel & 32768) == 32768)
            {
                num1 = byte.MaxValue;
                byte num5 = (byte)(((int)SrcPixel & 31744) >> 10);
                num2 = (byte)((int)num5 << 3 | (int)num5 >> 2);
                byte num6 = (byte)(((int)SrcPixel & 992) >> 5);
                num3 = (byte)((int)num6 << 3 | (int)num6 >> 2);
                byte num7 = (byte)((uint)SrcPixel & 31U);
                num4 = (byte)((int)num7 << 3 | (int)num7 >> 2);
            }
            else
            {
                byte num8 = (byte)(((int)SrcPixel & 28672) >> 12);
                num1 = (byte)((int)num8 << 5 | (int)num8 << 2 | (int)num8 >> 1);
                byte num9 = (byte)(((int)SrcPixel & 3840) >> 8);
                num2 = (byte)((uint)num9 << 4 | (uint)num9);
                byte num10 = (byte)(((int)SrcPixel & 240) >> 4);
                num3 = (byte)((uint)num10 << 4 | (uint)num10);
                byte num11 = (byte)((uint)SrcPixel & 15U);
                num4 = (byte)((uint)num11 << 4 | (uint)num11);
            }
            Dest[Offset] = num2;
            Dest[Offset + 1] = num3;
            Dest[Offset + 2] = num4;
            Dest[Offset + 3] = num1;
        }
        public static void Fix4x4(ref byte[] dest, byte[] src, int s, int width, int height)
        {
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 4)
                {
                    for (int index3 = 0; index3 < 4; ++index3)
                    {
                        int num = 0;
                        while (num < 4)
                        {
                            int destIndex = 2 * (width * (index1 + index3) + index2 + num);
                            if (index2 + num < width && index1 + index3 < height && s + 1 < src.Length && destIndex + 1 < dest.Length)
                            {
                                dest[destIndex] = src[s + 1];
                                dest[destIndex + 1] = src[s];
                            }
                            ++num;
                            s += 2;
                        }
                    }
                }
            }
        }
        public static void Fix8x4(ref byte[] dest, byte[] src, int s, int width, int height)
        {
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 8)
                {
                    for (int index3 = 0; index3 < 4; ++index3)
                    {
                        int num = 0;
                        while (num < 8)
                        {
                            int destIndex = width * (index1 + index3) + index2 + num;
                            if (index2 + num < width && index1 + index3 < height && s < src.Length && destIndex < dest.Length)
                            {
                                dest[destIndex] = src[s];
                                ++s;
                            }
                            ++num;
                        }
                    }
                }
            }
        }
        public static void Fix8x4Expand(ref byte[] dest, byte[] src, int s, int width, int height)
        {
            for (int index1 = 0; index1 < height; index1 += 4)
            {
                for (int index2 = 0; index2 < width; index2 += 8)
                {
                    for (int index3 = 0; index3 < 4; index3++)
                    {
                        for (int num1 = 0; num1 < 8; num1++)
                        {
                            int destIndex = width * (index1 + index3) + index2 + num1;
                            if (index2 + num1 < width && index1 + index3 < height && s < src.Length && destIndex < dest.Length - 1)
                            {
                                byte num2 = (byte)(src[s] & 0x0F);
                                byte num3 = (byte)((num2 << 4) | num2);
                                byte num4 = (byte)(src[s] & 0xF0);
                                byte num5 = (byte)(num4 | (num4 >> 4));
                                dest[destIndex] = num3;
                                dest[destIndex + 1] = num5;
                                s++;
                            }
                        }
                    }
                }
            }
        }
        public static void Fix8x8Expand(ref byte[] dest, byte[] src, int s, int width, int height)
        {
            for (int index1 = 0; index1 < height; index1 += 8)
            {
                for (int index2 = 0; index2 < width; index2 += 8)
                {
                    for (int index3 = 0; index3 < 8; ++index3)
                    {
                        for (int num1 = 0; num1 < 8; num1++)
                        {
                            int destIndex = width * (index1 + index3) + index2 + num1;
                            if (index2 + num1 < width && index1 + index3 < height && s < src.Length && destIndex < dest.Length)
                            {
                                byte num2 = (byte)((uint)src[s] & 0xF0);
                                dest[destIndex] = (byte)((uint)num2 | (uint)(num2 >> 4));
                                if (destIndex + 1 < dest.Length) // Additional check to prevent out of bounds access
                                {
                                    byte num3 = (byte)((uint)src[s] & 0x0F);
                                    dest[destIndex + 1] = (byte)((uint)(num3 << 4) | (uint)num3);
                                }
                                s++;
                            }
                        }
                    }
                }
            }
        }
        public static void Fix8x8NoExpand(ref byte[] dest, byte[] src, int s, int width, int height)
        {
            for (int index1 = 0; index1 < height; index1 += 8)
            {
                for (int index2 = 0; index2 < width; index2 += 8)
                {
                    for (int index3 = 0; index3 < 8; index3++)
                    {
                        for (int num1 = 0; num1 < 8; num1++)
                        {
                            int destIndex = width * (index1 + index3) + index2 + num1;
                            if (index2 + num1 < width && index1 + index3 < height && s < src.Length && destIndex < dest.Length)
                            {
                                byte num2 = (byte)(src[s] & 0xF0);
                                dest[destIndex] = (byte)(num2 >> 4);

                                if (destIndex + 1 < dest.Length) // Ensure this index is within bounds
                                {
                                    byte num3 = (byte)(src[s] & 0x0F);
                                    dest[destIndex + 1] = num3;
                                }
                                s++;
                            }
                        }
                    }
                }
            }
        }
        //public static void FixRGB5A3(ref byte[] Dest, byte[] Src, int S, int Width, int Height)
        //{
        //    for (int index1 = 0; index1 < Height; index1 += 4)
        //    {
        //        for (int index2 = 0; index2 < Width; index2 += 4)
        //        {
        //            for (int index3 = 0; index3 < 4; ++index3)
        //            {
        //                int num = 0;
        //                while (num < 4)
        //                {
        //                    if (index2 + num < Width && index1 + index3 < Height)
        //                        RGB5A3ToRGBA8(Helpers.Read16(Src, S), ref Dest, 4 * (Width * (index1 + index3) + index2 + num));
        //                    ++num;
        //                    S += 2;
        //                }
        //            }
        //        }
        //    }
        //}
        //
        //public static void FixR5G6B5(ref byte[] Dest, byte[] Src, int S, int Width, int Height)
        //{
        //    for (int index1 = 0; index1 < Height; index1 += 4)
        //    {
        //        for (int index2 = 0; index2 < Width; index2 += 4)
        //        {
        //            for (int index3 = 0; index3 < 4; ++index3)
        //            {
        //                int num = 0;
        //                while (num < 4)
        //                {
        //                    if (index2 + num < Width && index1 + index3 < Height)
        //                        R5G6B5ToRGBA8(Helpers.Read16(Src, S), ref Dest, 4 * (Width * (index1 + index3) + index2 + num));
        //                    ++num;
        //                    S += 2;
        //                }
        //            }
        //        }
        //    }
        //}
        public static void FixRGBA8(ref byte[] Dest, byte[] Src, int S, int Width, int Height)
        {
            for (int index1 = 0; index1 < Height; index1 += 4)
            {
                for (int index2 = 0; index2 < Width; index2 += 4)
                {
                    for (int index3 = 0; index3 < 4; ++index3)
                    {
                        int num = 0;
                        while (num < 4)
                        {
                            if (index2 + num < Width && index1 + index3 < Height)
                            {
                                uint index4 = (uint)(4 * (Width * (index1 + index3) + index2 + num));
                                Dest[(int)index4] = Src[S + 1];
                                Dest[(int)index4 + 3] = Src[S];
                            }
                            ++num;
                            S += 2;
                        }
                    }
                    for (int index5 = 0; index5 < 4; ++index5)
                    {
                        int num1 = 0;
                        while (num1 < 4)
                        {
                            if (index2 + num1 < Width && index1 + index5 < Height)
                            {
                                uint num2 = (uint)(4 * (Width * (index1 + index5) + index2 + num1));
                                Dest[(int)num2 + 1] = Src[S];
                                Dest[(int)num2 + 2] = Src[S + 1];
                            }
                            ++num1;
                            S += 2;
                        }
                    }
                }
            }
        }

        //Model Xbox
        public static List<Tuple<ushort, ushort, ushort>> TriangleStripToFaceTuple(ushort[] strip)
        {
            var faces = new List<Tuple<ushort, ushort, ushort>>();
            int StartDirection = -1;
            int faceDirection = StartDirection;

            ushort fa = strip[0];
            ushort fb = strip[1];
            ushort fc;

            for (int i = 2; i < strip.Length; i++)
            {
                fc = strip[i];
                if (fc == 0xFFFF)
                {
                    // Handle end-of-strip marker
                    i++;
                    if (i < strip.Length)
                    {
                        fa = strip[i];
                        i++;
                        if (i < strip.Length)
                        {
                            fb = strip[i];
                        }
                    }
                    faceDirection = StartDirection;
                }
                else
                {
                    faceDirection *= -1;
                    if (fa != fb && fb != fc && fc != fa)
                    {
                        if (faceDirection > 0)
                            faces.Add(Tuple.Create((ushort)fa, (ushort)fb, (ushort)fc));
                        else
                            faces.Add(Tuple.Create((ushort)fa, (ushort)fc, (ushort)fb));
                    }
                    fa = fb;
                    fb = fc;
                }
            }
            return faces;
        }
        public static List<Tuple<ushort, ushort, ushort>> TriangleListToFaceTuple(ushort[] strip)
        {
            var faces = new List<Tuple<ushort, ushort, ushort>>();
            for (int i = 0; i < strip.Length; i += 3)
                faces.Add(Tuple.Create((ushort)strip[i], (ushort)strip[i + 1], (ushort)strip[i + 2])); // fa fb fc
            return faces;
        }
        public static void IndiceDataIndexGCN(BinaryReader reader, MemoryStream input, VMGObject vmgobject, LayerObjectEntryGCN mesh)
        {
            int flag = 0;
            List<int> indice1 = new List<int>();
            List<int> indice2 = new List<int>();
            List<int> indice3 = new List<int>();
            List<int> indice4 = new List<int>();
            int maxoffset = (int)mesh.FaceOffset + (mesh.FaceCount * 32);
            input.Seek(mesh.FaceOffset, SeekOrigin.Begin);
            while ((int)reader.BaseStream.Position < maxoffset)
            {
                if (mesh.Unk0[0] == 2)
                    flag = ReadUInt16B(reader);
                else if (mesh.Unk0[0] == 2)
                    flag = reader.ReadByte();

                if (flag == 152 || flag == 144)
                {
                    int indexcount = ReadUInt16B(reader);
                    for (int index = 0; index < indexcount; index++)
                    {
                        indice1.Add(ReadIndiceDataGCN(reader, mesh.Indice.B1));
                        indice2.Add(ReadIndiceDataGCN(reader, mesh.Indice.B2));
                        indice3.Add(ReadIndiceDataGCN(reader, mesh.Indice.B3));
                        indice4.Add(ReadIndiceDataGCN(reader, mesh.Indice.B4));
                    }
                }
            }
        }
        public static int ReadIndiceDataGCN(BinaryReader reader, byte indice)
        {
            int value = 0;
            if (indice == 2)
                value = reader.ReadByte();
            else if (indice == 3)
                value = ReadUInt16B(reader);

            return value;
        }
        public static StaticMeshXbox ObjectStaticXboxHelper(LayerObjectEntryXbox layer_object, MemoryStream input, BinaryReader reader)
        {
            layer_object.StaticMesh = new StaticMeshXbox();
            input.Seek(layer_object.FaceOffset, SeekOrigin.Begin);
            layer_object.StaticMesh.Faces = ReadFacesDataXbox(reader, (int)(layer_object.FaceCount));
            int[] TempVert = ReadVertXbox((int)layer_object.FaceCount, layer_object.StaticMesh.Faces, 9001, 0);
            input.Seek(layer_object.Buffer4Offset, SeekOrigin.Begin);
            layer_object.StaticMesh.Buffer4Data = new Buffer4Xbox[TempVert[0]];
            for (int j = 0; j < TempVert[0]; j++)
                layer_object.StaticMesh.Buffer4Data[j] = ReadBuffer4Xbox(reader);

            input.Seek(layer_object.CenterRadiusOffset, SeekOrigin.Begin);
            layer_object.StaticMesh.CenterRadius = ReadVector4L(reader);

            return layer_object.StaticMesh;
        }
        //public static StaticMeshGCN ObjectStaticGCNHelper(LayerObjectEntryGCN layer_object, MemoryStream input, BinaryReader reader)
        //{
        //    layer_object.StaticMesh = new StaticMeshXbox();
        //    input.Seek(layer_object.FaceOffset, SeekOrigin.Begin);
        //    layer_object.StaticMesh.Faces = ReadFacesDataXbox(reader, (int)(layer_object.FaceCount));
        //    int[] TempVert = ReadVertXbox((int)layer_object.FaceCount, layer_object.StaticMesh.Faces, 9001, 0);
        //    input.Seek(layer_object.Buffer4Offset, SeekOrigin.Begin);
        //    layer_object.StaticMesh.Buffer4Data = new Buffer4Xbox[TempVert[0]];
        //    for (int j = 0; j < TempVert[0]; j++)
        //        layer_object.StaticMesh.Buffer4Data[j] = ReadBuffer4Xbox(reader);
        //
        //    input.Seek(layer_object.CenterRadiusOffset, SeekOrigin.Begin);
        //    layer_object.StaticMesh.CenterRadius = ReadVector4L(reader);
        //
        //    return layer_object.StaticMesh;
        //}
        public struct VMXObject 
        {
            public VMXHeader VMXheader;
            public WeightTableXbox WeightTables;
            public WeightDefXbox[] WeightDef1Bone;
            public WeightDefXbox[] WeightDef2Bone;
            public WeightDefXbox[] WeightDef3Bone;
            public WeightDefXbox[] WeightDef4Bone;
            public MatrixUnk MatrixUnk;
            public MatrixTable[] MatrixTables;
            public Dictionary<int, int> MatrixDictionary;
            public VXTHeader VTXHeader;
            public MaterialTable[] MaterialTables;
            public Dictionary<int, int> MaterialDictionary;
            public int[] MaterialOffsets;
            public TextureDataTypeXbox[] TextureTables;
            public Dictionary<int, int> TextureDictionary;
            public BoneTable[] BoneTables;
            public Dictionary<int, int> BoneDictionary;
            public LayerObjectEntryXbox[] Object_0;
            public LayerObjectEntryXbox[] Object_1;
            public LayerObjectEntryXbox[] Object_2;
            public Buffer1Xbox[] Buffer1;
            public Buffer2Xbox[] Buffer2;
            public Buffer2Xbox[] Buffer3;
            public List<LayerObjectEntryXbox> StaticMeshList;
            public List<LayerObjectEntryXbox> SkinnedMeshList;
            public LayerObjectEntryXbox SkinnedData;

        }
        public struct VMGObject
        {
            public VMXHeader VMGheader;
            public WeightTableGCN WeightTables;
            public WeightDefGCN[] WeightDef1Bone;
            public WeightDefGCN[] WeightDef2Bone;
            public WeightDefGCN[] WeightDef3Bone;
            public WeightDefGCN[] WeightDef4Bone;
            public MatrixUnk MatrixUnk;
            public MatrixTable[] MatrixTables;
            public Dictionary<int, int> MatrixDictionary;
            public VXTHeader VTGHeader;
            public MaterialTable[] MaterialTables;
            public Dictionary<int, int> MaterialDictionary;
            public TextureDataTypeGCN[] TextureTables;
            public Dictionary<int, int> TextureDictionary;
            public BoneTable[] BoneTables;
            public Dictionary<int, int> BoneDictionary;
            public LayerObjectEntryGCN[] Object_0;
            public LayerObjectEntryGCN[] Object_1;
            public LayerObjectEntryGCN[] Object_2;
            //public Buffer1GCN[] Buffer1;
            //public Buffer2GCN[] Buffer2;
            //public Buffer2GCN[] Buffer3;
            public LayerObjectEntryGCN SkinnedData;

        }
    }
}
