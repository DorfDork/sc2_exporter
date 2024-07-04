using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using static SC2_3DS.Helper;
using static SC2_3DS.Objects;
using static SC2_3DS.Textures;

namespace SC2_3DS
{
    internal class ExportTextures
    {
        static public void TextureExportXbox(VMXObject vmxobject)
        {
            for (int i = 0; i < vmxobject.TextureTables.Length; i++)
            {
                string name = $"Texture{i}.dds";

                using (FileStream fs = new FileStream(name, FileMode.Create))
                using (BinaryWriter writer = new BinaryWriter(fs))
                    WriteTextureXbox(writer, vmxobject.TextureTables[i]);
            }
        }

        static public void TextureExportGCN(VMGObject vmgobject)
        {
            for (int i = 0; i < vmgobject.TextureTables.Length; i++)
            {
                string Name = $"Texture{i}.dds";
                string NameAlpha = $"Texture{i}_Alpha.dds";

                using (FileStream fs = new FileStream(Name, FileMode.Create))
                using (BinaryWriter writer = new BinaryWriter(fs))
                    WriteTextureGCN(writer, vmgobject.TextureTables[i]);

                if (vmgobject.TextureTables[i].AlphaTextureDataOffset != 0)
                    using (FileStream fs = new FileStream(NameAlpha, FileMode.Create))
                    using (BinaryWriter writer = new BinaryWriter(fs))
                        WriteTextureAlphaGCN(writer, vmgobject.TextureTables[i]);
            }
        }
        //https://learn.microsoft.com/en-us/windows/win32/direct3ddds/dds-header
        static public void WriteTextureXbox(BinaryWriter writer, TextureDataTypeXbox texture)
        {
            writer.Write(new byte[] { 0x44, 0x44, 0x53, 0x20 }); // "DDS "
            writer.Write(124); // Size of DDS header (must be 124 for DX10 format)
            if (texture.MipMapCount > 1)
                writer.Write(DDSHeaderFlags.DDSD_CAPS |
                    DDSHeaderFlags.DDSD_HEIGHT | 
                    DDSHeaderFlags.DDSD_WIDTH | 
                    DDSHeaderFlags.DDSD_PIXELFORMAT | 
                    DDSHeaderFlags.DDSD_MIPMAPCOUNT | 
                    DDSHeaderFlags.DDSD_LINEARSIZE);
            else
                writer.Write(DDSHeaderFlags.DDSD_CAPS |
                    DDSHeaderFlags.DDSD_HEIGHT |
                    DDSHeaderFlags.DDSD_WIDTH |
                    DDSHeaderFlags.DDSD_PIXELFORMAT |
                    DDSHeaderFlags.DDSD_LINEARSIZE);

            writer.Write((int)texture.Width); // Width
            writer.Write((int)texture.Height); // Height
            writer.Write(texture.TextureSize);   // Pitch
            writer.Write(0); // Depth
            if (texture.MipMapCount > 1)
                writer.Write(texture.MipMapCount - 1); // Mipmaps
            else
                writer.Write(0); // Mipmaps
            writer.Write(new byte[44]); // Reserved
            writer.Write(32); // Size of pixel format structure
            if (texture.ImageType == ImageTypeXBOX.P8)
            {
                writer.Write(DDSPixelFlags.DDPF_PALETTEINDEXED8); // Pixel format flags
                writer.Write(0); // 
                writer.Write(8); // RGB bit count
                writer.Write(0xFF); // R bitmask
                writer.Write(0xFF00); // G bitmask
                writer.Write(0xFF0000); // B bitmask
                writer.Write(0xFF000000); // Alpha bitmask
            }
            else
            {
                string type = "";
                switch (texture.ImageType)
                {
                    //case ImageTypeXBOX.ARGB: VMXObject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, TempSize); break;
                    case ImageTypeXBOX.DXT1: type = "DXT1"; break;
                    case ImageTypeXBOX.DXT3: type = "DXT3"; break;
                    case ImageTypeXBOX.DXT5: type = "DXT5"; break;
                }
                writer.Write(DDSPixelFlags.DDPF_FOURCC); // Pixel format flags
                writer.Write(type.ToCharArray()); // FourCC (DXT1)
                writer.Write(0); // RGB bit count
                writer.Write(0); // R bitmask
                writer.Write(0); // G bitmask
                writer.Write(0); // B bitmask
                writer.Write(0); // Alpha bitmask
            }
            if (texture.MipMapCount > 1)
                writer.Write(DDSCapFlags.DDSCAPS_COMPLEX |
                    DDSCapFlags.DDSCAPS_TEXTURE |
                    DDSCapFlags.DDSCAPS_MIPMAP);
            else
                writer.Write(DDSCapFlags.DDSCAPS_TEXTURE);

            writer.Write(0);      // Reserved
            writer.Write(0);      // Reserved
            writer.Write(0);      // Reserved
            writer.Write(0);      // Reserved
            if (texture.ImageType == ImageTypeXBOX.P8)
            {
                writer.Write(ByteSwapAC(texture.Palette.PaletteData));
                writer.Write(UnSwizzleP8Bytes(texture.DiffuseBytes.Data, texture.Width, texture.Height, 1));
                if (texture.MipMapCount > 1)
                {
                    for (int i = 1; i < texture.MipMapCount; i++)
                    {
                        writer.Write(UnSwizzleP8Bytes(texture.MipMapBytes[i - 1].Data, (texture.Width / (2 * i)), (texture.Height / (2 * i)), 1));
                    }
                }
            }
            else
            {
                writer.Write(texture.DiffuseBytes.Data);
                if (texture.MipMapCount > 1)
                {
                    for (int i = 1; i < texture.MipMapCount; i++)
                    {
                        writer.Write(texture.MipMapBytes[i - 1].Data);
                    }
                }
            }

        }

        static public void WriteTextureGCN(BinaryWriter writer, TextureDataTypeGCN texture)
        {
            writer.Write(new byte[] { 0x44, 0x44, 0x53, 0x20 }); // "DDS "
            writer.Write(124); // Size of DDS header (must be 124 for DX10 format)
            writer.Write(DDSHeaderFlags.DDSD_CAPS |
                    DDSHeaderFlags.DDSD_HEIGHT |
                    DDSHeaderFlags.DDSD_WIDTH |
                    DDSHeaderFlags.DDSD_PIXELFORMAT |
                    DDSHeaderFlags.DDSD_LINEARSIZE);
            writer.Write((int)texture.Height); // Height
            writer.Write((int)texture.Width); // Width
            writer.Write(texture.TextureSize);   // Pitch
            writer.Write(0); // Depth
            writer.Write(0); // Mipmaps
            writer.Write(new byte[44]); // Reserved
            writer.Write(32); // Size of pixel format structure
            if (texture.ImageTypeVisible == ImageTypeGCN.CI8)
            {
                writer.Write(DDSPixelFlags.DDPF_PALETTEINDEXED8); // Pixel format flags
                writer.Write(0); // 
                writer.Write(8); // RGB bit count
                writer.Write(0xFF); // R bitmask
                writer.Write(0xFF00); // G bitmask
                writer.Write(0xFF0000); // B bitmask
                writer.Write(0xFF000000); // Alpha bitmask
            }
            else
            {
                string type = "";
                switch (texture.ImageTypeVisible)
                {
                    //case ImageTypeXBOX.ARGB: texture.DiffuseBytes = ReadTextureData(reader, TempSize); break;
                    case ImageTypeGCN.CMPR: type = "DXT1"; break;
                }
                writer.Write(DDSPixelFlags.DDPF_FOURCC); // Pixel format flags
                writer.Write(type.ToCharArray()); // FourCC (DXT1)
                writer.Write(0); // RGB bit count
                writer.Write(0); // R bitmask
                writer.Write(0); // G bitmask
                writer.Write(0); // B bitmask
                writer.Write(0); // Alpha bitmask
            }
            //if (texture.MipMapCount > 1)
            //    writer.Write(DDSCapFlags.DDSCAPS_COMPLEX |
            //        DDSCapFlags.DDSCAPS_TEXTURE |
            //        DDSCapFlags.DDSCAPS_MIPMAP);
            //else
            writer.Write(DDSCapFlags.DDSCAPS_TEXTURE);
            writer.Write(0);      // Reserved
            writer.Write(0);      // Reserved
            writer.Write(0);      // Reserved
            writer.Write(0);      // Reserved
            if (texture.ImageTypeVisible == ImageTypeGCN.CI8)
            {
                ushort[] mergedPalette = new ushort[512];
                Array.Copy(texture.Palette.PaletteData, 0, mergedPalette, 0, 256);
                Array.Copy(texture.Palette.PaletteData2, 0, mergedPalette, 256, 256);
                ushort[] mergedPalette2 = new ushort[512];
                writer.Write(DecodeC8(texture.DiffuseBytes.Data, texture.Width, texture.Height, (int)ImageTypeGCN.CI8, texture.Palette.PaletteData2, 0));
            }
            else if (texture.ImageTypeVisible == ImageTypeGCN.I4)
            {
                writer.Write(UnSwizzleGCNData(texture.DiffuseBytes.Data, texture.TextureSize, texture.Width, texture.Height));
            }
            else if (texture.ImageTypeVisible == ImageTypeGCN.CMPR)
            {
                writer.Write(texture.DiffuseBytes.Data);
            }
            else
            {
                writer.Write(UnSwizzleGCNData(texture.DiffuseBytes.Data, texture.TextureSize, texture.Width, texture.Height));
            }
        }




        static public void WriteTextureAlphaGCN(BinaryWriter writer, TextureDataTypeGCN texture)
        {
            writer.Write(new byte[] { 0x44, 0x44, 0x53, 0x20 }); // "DDS "
            writer.Write(124); // Size of DDS header (must be 124 for DX10 format)
            writer.Write(0x1 | 0x2 | 0x4 | 0x1000); // Required flags
            writer.Write((int)texture.AlphaHeight); // Height
            writer.Write((int)texture.AlphaWidth); // Width
            writer.Write(texture.AlphaTextureSize);   // Pitch
            writer.Write(0); // Depth
            writer.Write(0); // Mipmaps
            writer.Write(new byte[44]); // Reserved
            writer.Write(32); // Size of pixel format structure
            if (texture.AlphaImageTypeVisible == ImageTypeGCN.I4 || texture.AlphaImageTypeVisible == ImageTypeGCN.CI8)
            {
                writer.Write(DDSPixelFlags.DDPF_PALETTEINDEXED8); // Pixel format flags
                writer.Write(0); // 
                writer.Write(8); // RGB bit count
                writer.Write(0xFF); // R bitmask
                writer.Write(0xFF00); // G bitmask
                writer.Write(0xFF0000); // B bitmask
                writer.Write(0xFF000000); // Alpha bitmask
            }
            else
            {
                string type = "";
                switch (texture.AlphaImageTypeVisible)
                {
                    //case ImageTypeXBOX.ARGB: texture.DiffuseBytes = ReadTextureData(reader, TempSize); break;
                    case ImageTypeGCN.CMPR: type = "DXT1"; break;
                }
                writer.Write(0x4); // Pixel format flags
                writer.Write(type.ToCharArray()); // FourCC
                writer.Write(0); // RGB bit count
                writer.Write(0); // R bitmask
                writer.Write(0); // G bitmask
                writer.Write(0); // B bitmask
                writer.Write(0); // Alpha bitmask
            }
            writer.Write(0x1000); // Required caps
            writer.Write(0);      // Reserved
            writer.Write(0);      // Reserved
            writer.Write(0);      // Reserved
            writer.Write(0);      // Reserved
            if (texture.AlphaImageTypeVisible == ImageTypeGCN.CI8)
            {
                writer.Write(DecodeC8(texture.AlphaBytes.Data, texture.AlphaWidth, texture.AlphaHeight, (int)texture.AlphaImageTypeVisible, texture.Palette.PaletteData2, 0));
            }
            else if (texture.AlphaImageTypeVisible == ImageTypeGCN.I4)
            {
                byte[] mergedPalette = new byte[texture.AlphaBytes.Data.Length];
                int temp = 0;
                //Fix8x4(ref mergedPalette, texture.AlphaBytes.Data, temp, texture.AlphaWidth, texture.AlphaHeight);
                //writer.Write(mergedPalette);
                writer.Write(texture.AlphaBytes.Data);
                //writer.Write(UnSwizzleGCNData(texture.AlphaBytes.Data, texture.AlphaTextureSize, texture.AlphaWidth, texture.AlphaHeight));
            }
            else
            {
                writer.Write(UnSwizzleGCNData(texture.AlphaBytes.Data, texture.AlphaTextureSize, texture.AlphaWidth, texture.AlphaHeight));
            }
        }

        //     I4 = 0, // (4 bit intensity, 8x8 tiles)
        // I8 = 1, // (8 bit intensity, 8x4 tiles)
        // IA4 = 2, // (4 bit intensity with 4 bit alpha, 8x4 tiles)
        // IA8 = 3, // (8 bit intensity with 8 bit alpha, 4x4 tiles)
        // RGB565 = 4, // (4x4 tiles)
        // RGB5A3 = 5, // (*) (4x4 tiles)
        // RGBA8 = 6, // (4x4 tiles in two cache lines - first is AR and second is GB)
        // CI4 = 8, // (4 bit color index, 8x8 tiles)
        // CI8 = 9, // (8 bit color index, 8x4 tiles)
        // C14X2 = 10, // (14 bit color index, 4x4 tiles)
        // CMPR = 14, // (S3TC compressed, 2x2 blocks of 4x4 tiles)
        // XFB = 15, // 0x0F
        // UNK16 = 16 // 0x10

        
    }
}





    //private void SwizBlock(ref byte[] outData, ref byte[] inData, ref long offsOut, long offs, long blWidth, long blHeight, long stride)
    //{
    //    if (offs > inData.Length - 1) return;
    //
    //    if (blWidth < 2 || blHeight < 2)
    //    {
    //        Array.Copy(inData, offs, outData, offsOut, blWidth * blHeight);
    //        offsOut += blWidth * blHeight;
    //    }
    //    else if (blWidth == 2 && blHeight == 2)
    //    {
    //        outData[offsOut] = inData[offs];
    //        outData[offsOut + 1] = inData[offs + 1];
    //        outData[offsOut + 2] = inData[offs + stride];
    //        outData[offsOut + 3] = inData[offs + stride + 1];
    //        offsOut += 4;
    //    }
    //    else
    //    {
    //        SwizBlock(ref outData, ref inData, ref offsOut, offs, blWidth / 2, blHeight / 2, stride);
    //        SwizBlock(ref outData, ref inData, ref offsOut, offs + (blWidth / 2), blWidth / 2, blHeight / 2, stride);
    //        SwizBlock(ref outData, ref inData, ref offsOut, offs + (stride * (blHeight / 2)), blWidth / 2, blHeight / 2, stride);
    //        SwizBlock(ref outData, ref inData, ref offsOut, offs + (stride * (blHeight / 2)) + (blWidth / 2), blWidth / 2, blHeight / 2, stride);
    //    }
    //}
    //
    //public byte[] Swizzle(CTpkTexture inTex, byte[] data)
    //{
    //    byte[] bytData = data;
    //    byte[] bytDataOut = new byte[bytData.Length];
    //    long lOffs = 0;
    //    long width = inTex.TexWidth;
    //    long height = inTex.TexHeight;
    //    long minMax = Math.Max(inTex.TexHeight, inTex.TexWidth);
    //    long mipLevels = (long)(Math.Log(minMax) / Math.Log(2) + 1);
    //
    //    for (long i = 1; i <= mipLevels; i++)
    //    {
    //        SwizBlock(ref bytDataOut, ref bytData, ref lOffs, lOffs, width, height, width);
    //        width /= 2;
    //        height /= 2;
    //        if (lOffs > bytData.Length - 1) break;
    //    }
    //
    //    return bytDataOut;
    //}

    //public static void UnswizBlock(ref List<int> collection, ref List<int> collection2, ref byte[] inData, ref byte[] outData, ref long offs, long offsOut, long blWidth, long blHeight, long stride)
    //{
    //    if (offs >= inData.Length) return;
    //        //    if (blWidth < 2 || blHeight < 2)
    //    {
    //        long length = Math.Min(blWidth * blHeight, inData.Length - offs);
    //        Array.Copy(inData, offs, outData, offsOut, length);
    //        collection.Add(inData[offs]);
    //        collection2.Add(outData[offsOut]);
    //        offs += length;
    //    }
    //    else if (blWidth == 2 && blHeight == 2)
    //    {
    //        if (offs + 3 < inData.Length)
    //        {
    //            outData[offsOut] = inData[offs];
    //            outData[offsOut + 1] = inData[offs + 1];
    //            outData[offsOut + stride] = inData[offs + 2];
    //            outData[offsOut + stride + 1] = inData[offs + 3];
    //            offs += 4;
    //        }
    //    }
    //    else
    //    {
    //        UnswizBlock(ref collection,ref collection2,ref inData, ref outData, ref offs, offsOut, blWidth / 2, blHeight / 2, stride);
    //        UnswizBlock(ref collection,ref collection2,ref inData, ref outData, ref offs, offsOut + (stride * (blHeight / 2)), blWidth / 2, blHeight / 2, stride);
    //        UnswizBlock(ref collection,ref collection2,ref inData, ref outData, ref offs, offsOut + (blWidth / 2), blWidth / 2, blHeight / 2, stride);
    //        UnswizBlock(ref collection,ref collection2, ref inData, ref outData, ref offs, offsOut + (stride * (blHeight / 2)) + (blWidth / 2), blWidth / 2, blHeight / 2, stride);
    //    }
    //}


    //public static byte[] Unswizzle(TextureDataTypeXbox inTex)
    //{
    //    byte[] bytData = inTex.DiffuseBytes.Data;
    //    byte[] bytDataOut = new byte[bytData.Length];
    //    long lOffs = 0;
    //    long width = inTex.Width;
    //    long height = inTex.Height;
    //    long minMax = Math.Max(inTex.Height, inTex.Width);
    //    long mipLevels = (long)(Math.Log(minMax) / Math.Log(2) + 1);
    //    List<int> collection = new List<int>();
    //    List<int> collection2 = new List<int>();
    //
    //    for (long i = 1; i <= mipLevels; i++)
    //    {
    //        UnswizBlock(ref collection, ref collection2, ref bytData, ref bytDataOut, ref lOffs, lOffs, width, height, width);
    //        width /= 2;
    //        height /= 2;
    //        if (lOffs > bytData.Length - 1) break;
    //    }
    //
    //    return bytDataOut;
    //}

