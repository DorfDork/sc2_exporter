using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;
using static SC2_3DS.Headers;
using static SC2_3DS.Helper;
using static SC2_3DS.Matrix;
using static SC2_3DS.Objects;
using static SC2_3DS.Textures;
using static SC2_3DS.Weight;

namespace SC2_3DS
{
    internal class ImportSC2
    {
        public static VMXObject ReadVMXObject(BinaryReader reader, MemoryStream input)
        {
            VMXObject vmxobject = new VMXObject();

            //VMX HEADER
            input.Seek(0L, SeekOrigin.Begin);
            vmxobject.VMXheader = ReadVMXHeader(reader);
            //WEIGHTING
            input.Seek(vmxobject.VMXheader.WeightTableOffset, SeekOrigin.Begin);
            uint weightOffsetCheck = ReadUInt32L(reader);
            if (weightOffsetCheck != 0)
            {
                input.Seek(vmxobject.VMXheader.WeightTableOffset, SeekOrigin.Begin);
                vmxobject.WeightTables = ReadWeightTableXbox(reader);
                vmxobject.WeightDef1Bone = new WeightDefXbox[vmxobject.WeightTables.VertCount1 * 1];
                vmxobject.WeightDef2Bone = new WeightDefXbox[vmxobject.WeightTables.VertCount2 * 2];
                vmxobject.WeightDef3Bone = new WeightDefXbox[vmxobject.WeightTables.VertCount3 * 3];
                vmxobject.WeightDef4Bone = new WeightDefXbox[vmxobject.WeightTables.VertCount4 * 4];
                input.Seek(vmxobject.WeightTables.WeightBufferOffset, SeekOrigin.Begin);
                for (uint i = 0; i < vmxobject.WeightTables.VertCount1; i++)
                    vmxobject.WeightDef1Bone[i] = ReadWeightDefXbox(reader);
                for (uint i = 0; i < vmxobject.WeightTables.VertCount2 * 2; i++)
                    vmxobject.WeightDef2Bone[i] = ReadWeightDefXbox(reader);
                for (uint i = 0; i < vmxobject.WeightTables.VertCount3 * 3; i++)
                    vmxobject.WeightDef3Bone[i] = ReadWeightDefXbox(reader);
                for (uint i = 0; i < vmxobject.WeightTables.VertCount4 * 4; i++)
                    vmxobject.WeightDef4Bone[i] = ReadWeightDefXbox(reader);
            }
            //MATRIX Unknown
            input.Seek(vmxobject.VMXheader.MatrixUnkTableOffset, SeekOrigin.Begin);
            vmxobject.MatrixUnk = ReadMatrixUnkXbox(reader);
            //MATRIX
            vmxobject.MatrixTables = new MatrixTable[vmxobject.VMXheader.MatrixCount];
            vmxobject.MatrixDictionary = new Dictionary<int, int>();
            input.Seek(vmxobject.VMXheader.MatrixTableOffset, SeekOrigin.Begin);
            for (int i = 0; i < vmxobject.VMXheader.MatrixCount; i++)
            {
                vmxobject.MatrixTables[i] = ReadMatrixTableXbox(reader);
                vmxobject.MatrixDictionary.Add((int)vmxobject.VMXheader.MatrixTableOffset + (i * 400), i);
            }
            //VXT HEADER
            input.Seek(vmxobject.VMXheader.TextureTableOffset, SeekOrigin.Begin);
            vmxobject.VTXHeader = ReadVXTHeader(reader);
            //MATERIALS
            vmxobject.MaterialOffsets = new int[vmxobject.VMXheader.MaterialCount];
            vmxobject.MaterialTables = new MaterialTable[vmxobject.VMXheader.MaterialCount];
            vmxobject.MaterialDictionary = new Dictionary<int, int>();
            input.Seek(vmxobject.VMXheader.MaterialOffset, SeekOrigin.Begin);
            for (int i = 0; i < vmxobject.VMXheader.MaterialCount; i++)
            {
                vmxobject.MaterialOffsets[i] = (int)vmxobject.VMXheader.MaterialOffset + (i * 80);
                vmxobject.MaterialTables[i] = ReadMaterialTableXbox(reader);
                vmxobject.MaterialDictionary.Add((int)vmxobject.VMXheader.MaterialOffset + (i * 80), i);
            }
            //TEXTURES
            vmxobject.TextureTables = new TextureDataTypeXbox[vmxobject.VTXHeader.TextureCount];
            vmxobject.TextureDictionary = new Dictionary<int, int>();
            uint TextureTableOffset = vmxobject.VMXheader.TextureTableOffset + vmxobject.VTXHeader.HeaderLength;
            int TempSize = 0;
            for (int i = 0; i < vmxobject.VTXHeader.TextureCount; i++)
            {
                if (vmxobject.VTXHeader.Type == 0)
                {
                    input.Seek((int)TextureTableOffset + (i * 32), SeekOrigin.Begin);
                    vmxobject.TextureTables[i] = ReadTextureDataType0Xbox(reader);
                    vmxobject.TextureDictionary.Add((int)TextureTableOffset + (i * 32), i);
                }
                else if (vmxobject.VTXHeader.Type == 2)
                {
                    input.Seek((int)TextureTableOffset + (i * 36), SeekOrigin.Begin);
                    vmxobject.TextureTables[i] = ReadTextureDataType2Xbox(reader);
                    vmxobject.TextureDictionary.Add((int)TextureTableOffset + (i * 36), i);
                }
                input.Seek(vmxobject.VMXheader.TextureTableOffset + vmxobject.TextureTables[i].TextureDataOffset, SeekOrigin.Begin);
                TempSize = vmxobject.TextureTables[i].Width * vmxobject.TextureTables[i].Height;
                switch (vmxobject.TextureTables[i].ImageType)
                {
                    case ImageTypeXBOX.ARGB: vmxobject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, TempSize); break;
                    case ImageTypeXBOX.P8: vmxobject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, TempSize); break;
                    case ImageTypeXBOX.DXT1: TempSize >>= 1; vmxobject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, TempSize); break; //DXT1 textures are half size
                    case ImageTypeXBOX.DXT3: vmxobject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, TempSize); break;
                    case ImageTypeXBOX.DXT5: vmxobject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, TempSize); break;
                }
                vmxobject.TextureTables[i].TextureSize = TempSize;
                if (vmxobject.TextureTables[i].MipMapCount > 1)
                {
                    vmxobject.TextureTables[i].MipMapBytes = new TextureData[vmxobject.TextureTables[i].MipMapCount - 1];
                    for (int j = 1; j < vmxobject.TextureTables[i].MipMapCount; j++)
                    {
                        TempSize = (vmxobject.TextureTables[i].Width / (2 * j)) * (vmxobject.TextureTables[i].Height / (2 * j));
                        vmxobject.TextureTables[i].MipMapBytes[j - 1] = ReadTextureData(reader, TempSize);
                    }
                }
                if (vmxobject.TextureTables[i].ImageType == ImageTypeXBOX.P8) //Palette
                {
                    input.Seek(vmxobject.VMXheader.TextureTableOffset + vmxobject.TextureTables[i].TexturePaletteCLUTOffset, SeekOrigin.Begin);
                    vmxobject.TextureTables[i].Palette = ReadTexturePaletteXbox(reader);
                    input.Seek(vmxobject.VMXheader.TextureTableOffset + vmxobject.TextureTables[i].Palette.PaletteOffset, SeekOrigin.Begin);
                    byte[] palettebuffer = reader.ReadBytes(vmxobject.TextureTables[i].Palette.PaletteCount * sizeof(int));
                    vmxobject.TextureTables[i].Palette.PaletteData = new byte[vmxobject.TextureTables[i].Palette.PaletteCount * 4];
                    Buffer.BlockCopy(palettebuffer, 0, vmxobject.TextureTables[i].Palette.PaletteData, 0, palettebuffer.Length);
                }
            }
            //BONES
            vmxobject.BoneTables = new BoneTable[vmxobject.VMXheader.BoneCount];
            vmxobject.BoneDictionary = new Dictionary<int, int>();
            for (int i = 0; i < vmxobject.VMXheader.BoneCount; i++)
            {
                input.Seek(vmxobject.VMXheader.BoneOffset + (i * 64), SeekOrigin.Begin);
                vmxobject.BoneTables[i] = ReadBoneTableXbox(reader);
                if (vmxobject.BoneTables[i].BoneNameOffset != 0)
                {
                    if (!vmxobject.BoneDictionary.ContainsKey(vmxobject.BoneTables[i].BoneParentIdx))
                    {
                        vmxobject.BoneDictionary.Add(vmxobject.BoneTables[i].BoneParentIdx, i);
                    }
                    input.Seek(vmxobject.BoneTables[i].BoneNameOffset, SeekOrigin.Begin);
                    vmxobject.BoneTables[i].Name = ReadNullTerminatedString(reader);
                }
                if (vmxobject.BoneTables[i].BoneNameOffset == 0)
                {
                    if (!vmxobject.BoneDictionary.ContainsKey(vmxobject.BoneTables[i].BoneParentIdx))
                    {
                        vmxobject.BoneDictionary.Add(vmxobject.BoneTables[i].BoneParentIdx, i);
                    }
                    vmxobject.BoneTables[i].Name = $"Empty_{i}";
                }
            }

            //MESH
            vmxobject.Object_0 = new LayerObjectEntryXbox[vmxobject.VMXheader.Object0Count];
            vmxobject.Object_1 = new LayerObjectEntryXbox[vmxobject.VMXheader.Object1Count];
            vmxobject.Object_2 = new LayerObjectEntryXbox[vmxobject.VMXheader.Object2Count];
            vmxobject.SkinnedMeshList = new List<SkinnedMeshXbox>();
            vmxobject.StaticMeshList = new List<StaticMeshXbox>();
            int[] TempVertSkinned = new int[3];
            TempVertSkinned[1] = 9001; //Min
            TempVertSkinned[2] = 0; //Max
            bool skinned_bool = false;
            for (int i = 0; i < vmxobject.VMXheader.Object0Count; i++)
            {
                input.Seek(vmxobject.VMXheader.Object0Offset + (i * 40), SeekOrigin.Begin);
                vmxobject.Object_0[i] = ReadLayerObjectEntryXbox(reader);
                if (vmxobject.Object_0[i].ObjectType == MeshXboxContent.SKINNED)
                {
                    vmxobject.Object_0[i].SkinnedMesh = new SkinnedMeshXbox();
                    input.Seek(vmxobject.Object_0[i].FaceOffset, SeekOrigin.Begin);
                    vmxobject.Object_0[i].SkinnedMesh.Faces = ReadFacesDataXbox(reader, (int)(vmxobject.Object_0[i].FaceCount));
                    TempVertSkinned = ReadVertXbox((int)vmxobject.Object_0[i].FaceCount, vmxobject.Object_0[i].SkinnedMesh.Faces, TempVertSkinned[1], TempVertSkinned[2]);
                    if (skinned_bool == false)
                    {
                        vmxobject.SkinnedData = vmxobject.Object_0[i];
                        skinned_bool = true;
                    }
                    vmxobject.SkinnedMeshList.Add(vmxobject.Object_0[i].SkinnedMesh);
                }
                else if (vmxobject.Object_0[i].ObjectType == MeshXboxContent.STATIC)
                {
                    vmxobject.Object_0[i].StaticMesh = ObjectStaticXboxHelper(vmxobject.Object_0[i], input, reader);
                    vmxobject.StaticMeshList.Add(vmxobject.Object_0[i].StaticMesh);
                }
            }
            for (int i = 0; i < vmxobject.VMXheader.Object1Count; i++)
            {
                input.Seek(vmxobject.VMXheader.Object1Offset + (i * 40), SeekOrigin.Begin);
                vmxobject.Object_1[i] = ReadLayerObjectEntryXbox(reader);
                if (vmxobject.Object_1[i].ObjectType == MeshXboxContent.SKINNED)
                {
                    vmxobject.Object_1[i].SkinnedMesh = new SkinnedMeshXbox();
                    input.Seek(vmxobject.Object_1[i].FaceOffset, SeekOrigin.Begin);
                    vmxobject.Object_1[i].SkinnedMesh.Faces = ReadFacesDataXbox(reader, (int)(vmxobject.Object_1[i].FaceCount));
                    TempVertSkinned = ReadVertXbox((int)vmxobject.Object_1[i].FaceCount, vmxobject.Object_1[i].SkinnedMesh.Faces, TempVertSkinned[1], TempVertSkinned[2]);
                    if (skinned_bool == false)
                    {
                        vmxobject.SkinnedData = vmxobject.Object_1[i];
                        skinned_bool = true;
                    }
                    vmxobject.SkinnedMeshList.Add(vmxobject.Object_1[i].SkinnedMesh);
                }
                else if (vmxobject.Object_1[i].ObjectType == MeshXboxContent.STATIC)
                {
                    vmxobject.Object_1[i].StaticMesh = ObjectStaticXboxHelper(vmxobject.Object_1[i], input, reader);
                    vmxobject.StaticMeshList.Add(vmxobject.Object_1[i].StaticMesh);
                }
            }
            for (int i = 0; i < vmxobject.VMXheader.Object2Count; i++)
            {
                input.Seek(vmxobject.VMXheader.Object2Offset + (i * 40), SeekOrigin.Begin);
                vmxobject.Object_2[i] = ReadLayerObjectEntryXbox(reader);
                if (vmxobject.Object_2[i].ObjectType == MeshXboxContent.SKINNED)
                {
                    vmxobject.Object_2[i].SkinnedMesh = new SkinnedMeshXbox();
                    input.Seek(vmxobject.Object_2[i].FaceOffset, SeekOrigin.Begin);
                    vmxobject.Object_2[i].SkinnedMesh.Faces = ReadFacesDataXbox(reader, (int)(vmxobject.Object_2[i].FaceCount));
                    TempVertSkinned = ReadVertXbox((int)vmxobject.Object_2[i].FaceCount, vmxobject.Object_2[i].SkinnedMesh.Faces, TempVertSkinned[1], TempVertSkinned[2]);
                    if (skinned_bool == false)
                    {
                        vmxobject.SkinnedData = vmxobject.Object_2[i];
                        skinned_bool = true;
                    }
                    vmxobject.SkinnedMeshList.Add(vmxobject.Object_2[i].SkinnedMesh);
                }
                else if (vmxobject.Object_2[i].ObjectType == MeshXboxContent.STATIC)
                {
                    vmxobject.Object_2[i].StaticMesh = ObjectStaticXboxHelper(vmxobject.Object_2[i], input, reader);
                    vmxobject.StaticMeshList.Add(vmxobject.Object_2[i].StaticMesh);
                }
            }
            //Skinned buffers are set here for convience
            if (skinned_bool) //There is a skinned mesh
            {
                vmxobject.Buffer1 = new Buffer1Xbox[TempVertSkinned[0]];
                vmxobject.Buffer2 = new Buffer2Xbox[TempVertSkinned[0]];
                vmxobject.Buffer3 = new Buffer2Xbox[TempVertSkinned[0]];
                input.Seek(vmxobject.SkinnedData.Buffer1Offset, SeekOrigin.Begin);
                for (int i = 0; i < TempVertSkinned[0]; i++)
                    vmxobject.Buffer1[i] = ReadBuffer1Xbox(reader);
                input.Seek(vmxobject.SkinnedData.Buffer2Offset, SeekOrigin.Begin);
                for (int i = 0; i < TempVertSkinned[0]; i++)
                    vmxobject.Buffer2[i] = ReadBuffer2Xbox(reader);
                input.Seek(vmxobject.SkinnedData.Buffer3Offset, SeekOrigin.Begin);
                for (int i = 0; i < TempVertSkinned[0]; i++)
                    vmxobject.Buffer3[i] = ReadBuffer2Xbox(reader);
            }
            return vmxobject;
        }

        public static VMGObject ReadVMGObject(BinaryReader reader, MemoryStream input)
        {
            VMGObject vmgobject = new VMGObject();

            //VMX HEADER
            input.Seek(0L, SeekOrigin.Begin);
            vmgobject.VMGheader = ReadVMGHeader(reader);
            //WEIGHTING
            input.Seek(vmgobject.VMGheader.WeightTableOffset, SeekOrigin.Begin);
            uint weightOffsetCheck = ReadUInt32B(reader);
            if (weightOffsetCheck != 0)
            {
                input.Seek(vmgobject.VMGheader.WeightTableOffset, SeekOrigin.Begin);
                vmgobject.WeightTables = ReadWeightTableGCN(reader);
                vmgobject.WeightDef1Bone = new WeightDefGCN[vmgobject.WeightTables.VertCount1 * 1];
                vmgobject.WeightDef2Bone = new WeightDefGCN[vmgobject.WeightTables.VertCount2 * 2];
                vmgobject.WeightDef3Bone = new WeightDefGCN[vmgobject.WeightTables.VertCount3 * 3];
                vmgobject.WeightDef4Bone = new WeightDefGCN[vmgobject.WeightTables.VertCount4 * 4];
                input.Seek(vmgobject.WeightTables.WeightBufferOffset, SeekOrigin.Begin);
                for (uint i = 0; i < vmgobject.WeightTables.VertCount1; i++)
                    vmgobject.WeightDef1Bone[i] = ReadWeightDefGCN(reader);
                for (uint i = 0; i < vmgobject.WeightTables.VertCount2 * 2; i++)
                    vmgobject.WeightDef2Bone[i] = ReadWeightDefGCN(reader);
                for (uint i = 0; i < vmgobject.WeightTables.VertCount3 * 3; i++)
                    vmgobject.WeightDef3Bone[i] = ReadWeightDefGCN(reader);
                for (uint i = 0; i < vmgobject.WeightTables.VertCount4 * 4; i++)
                    vmgobject.WeightDef4Bone[i] = ReadWeightDefGCN(reader);
            }
            //MATRIX Unknown
            input.Seek(vmgobject.VMGheader.MatrixUnkTableOffset, SeekOrigin.Begin);
            vmgobject.MatrixUnk = ReadMatrixUnkGCN(reader);
            //MATRIX
            vmgobject.MatrixTables = new MatrixTable[vmgobject.VMGheader.MatrixCount];
            vmgobject.MatrixDictionary = new Dictionary<int, int>();
            input.Seek(vmgobject.VMGheader.MatrixTableOffset, SeekOrigin.Begin);
            for (int i = 0; i < vmgobject.VMGheader.MatrixCount; i++)
            {
                vmgobject.MatrixTables[i] = ReadMatrixTableGCN(reader);
                vmgobject.MatrixDictionary.Add((int)vmgobject.VMGheader.MatrixTableOffset + (i * 400), i);
            }
            //VXT HEADER
            input.Seek(vmgobject.VMGheader.TextureTableOffset, SeekOrigin.Begin);
            vmgobject.VTGHeader = ReadVGTHeader(reader);
            //MATERIALS
            vmgobject.MaterialTables = new MaterialTable[vmgobject.VMGheader.MaterialCount];
            vmgobject.MaterialDictionary = new Dictionary<int, int>();
            input.Seek(vmgobject.VMGheader.MaterialOffset, SeekOrigin.Begin);
            for (int i = 0; i < vmgobject.VMGheader.MaterialCount; i++)
            {
                vmgobject.MaterialTables[i] = ReadMaterialTableGCN(reader);
                vmgobject.MaterialDictionary.Add((int)vmgobject.VMGheader.MaterialOffset + (i * 80), i);
            }
            //TEXTURES
            vmgobject.TextureTables = new TextureDataTypeGCN[vmgobject.VTGHeader.TextureCount];
            vmgobject.TextureDictionary = new Dictionary<int, int>();
            uint TextureTableOffset = vmgobject.VMGheader.TextureTableOffset + vmgobject.VTGHeader.HeaderLength;
            for (int i = 0; i < vmgobject.VTGHeader.TextureCount; i++)
            {
                input.Seek((int)TextureTableOffset + (i * 68), SeekOrigin.Begin);
                vmgobject.TextureTables[i] = ReadTextureDataTypeGCN(reader);
                vmgobject.TextureDictionary.Add((int)TextureTableOffset + (i * 68), i);

                vmgobject.TextureTables[i].Width = (vmgobject.TextureTables[i].Dimensions & 1023) + 1;
                vmgobject.TextureTables[i].Height = (((vmgobject.TextureTables[i].Dimensions >> 10) & 1023) + 1);

                vmgobject.TextureTables[i].AlphaWidth = (vmgobject.TextureTables[i].AlphaDimensions & 1023) + 1;
                vmgobject.TextureTables[i].AlphaHeight = (((vmgobject.TextureTables[i].AlphaDimensions >> 10) & 1023) + 1);


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

                //Diffuse textures
                input.Seek(vmgobject.VMGheader.TextureTableOffset + vmgobject.TextureTables[i].TextureDataOffset, SeekOrigin.Begin);
                vmgobject.TextureTables[i].TextureSize = (int)ImageSizeGCN(vmgobject.TextureTables[i].Unk21);
                switch (vmgobject.TextureTables[i].ImageTypeVisible)
                {
                    case ImageTypeGCN.I4: vmgobject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, vmgobject.TextureTables[i].TextureSize); break;
                    case ImageTypeGCN.I8: vmgobject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, vmgobject.TextureTables[i].TextureSize); break;
                    case ImageTypeGCN.IA4: vmgobject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, vmgobject.TextureTables[i].TextureSize); break;
                    case ImageTypeGCN.IA8: vmgobject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, vmgobject.TextureTables[i].TextureSize); break;
                    case ImageTypeGCN.RGB565: vmgobject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, vmgobject.TextureTables[i].TextureSize); break;
                    case ImageTypeGCN.RGB5A3: vmgobject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, vmgobject.TextureTables[i].TextureSize); break;
                    case ImageTypeGCN.RGBA8: vmgobject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, vmgobject.TextureTables[i].TextureSize); break; //RGBA8 is full texture size, others seem to be half
                    case ImageTypeGCN.CI4: vmgobject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, vmgobject.TextureTables[i].TextureSize); break;
                    case ImageTypeGCN.CI8: vmgobject.TextureTables[i].TextureSize >>= 1; vmgobject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, vmgobject.TextureTables[i].TextureSize); break;
                    case ImageTypeGCN.C14X2: vmgobject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, vmgobject.TextureTables[i].TextureSize); break;
                    case ImageTypeGCN.CMPR: vmgobject.TextureTables[i].TextureSize >>= 1; vmgobject.TextureTables[i].DiffuseBytes.Data = UnSwizzleGCNData(ReadTextureData(reader, vmgobject.TextureTables[i].TextureSize).Data, vmgobject.TextureTables[i].TextureSize, vmgobject.TextureTables[i].Width, vmgobject.TextureTables[i].Height); break;
                    case ImageTypeGCN.XFB: vmgobject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, vmgobject.TextureTables[i].TextureSize); break;
                    case ImageTypeGCN.UNK16: vmgobject.TextureTables[i].DiffuseBytes = ReadTextureData(reader, vmgobject.TextureTables[i].TextureSize); break;
                }
                if (vmgobject.TextureTables[i].Unk1 == 464)
                {
                    vmgobject.TextureTables[i].MipMapSize = vmgobject.TextureTables[i].TextureSize >> 1;
                    vmgobject.TextureTables[i].MipMapBytes = ReadTextureData(reader, vmgobject.TextureTables[i].MipMapSize);
                }
                else if (vmgobject.TextureTables[i].Unk1 == 432)
                {
                    vmgobject.TextureTables[i].MipMapSize = vmgobject.TextureTables[i].TextureSize >> 2;
                    vmgobject.TextureTables[i].MipMapBytes = ReadTextureData(reader, vmgobject.TextureTables[i].MipMapSize);
                }

                if (vmgobject.TextureTables[i].ImageTypeVisible == ImageTypeGCN.CI8) //Palette
                {
                    input.Seek(vmgobject.VMGheader.TextureTableOffset + vmgobject.TextureTables[i].TexturePaletteCLUTOffset, SeekOrigin.Begin);
                    vmgobject.TextureTables[i].Palette = ReadTexturePaletteGCN(reader);

                    input.Seek(vmgobject.VMGheader.TextureTableOffset + vmgobject.TextureTables[i].Palette.PaletteOffset, SeekOrigin.Begin);
                    byte[] palettebuffer = reader.ReadBytes(vmgobject.TextureTables[i].Palette.PaletteCount * sizeof(ushort));
                    vmgobject.TextureTables[i].Palette.PaletteData = new ushort[vmgobject.TextureTables[i].Palette.PaletteCount];
                    Buffer.BlockCopy(palettebuffer, 0, vmgobject.TextureTables[i].Palette.PaletteData, 0, palettebuffer.Length);

                    input.Seek(vmgobject.VMGheader.TextureTableOffset + vmgobject.TextureTables[i].Palette.PaletteOffset2, SeekOrigin.Begin);
                    byte[] palette2buffer = reader.ReadBytes(vmgobject.TextureTables[i].Palette.PaletteCount2 * sizeof(ushort));
                    vmgobject.TextureTables[i].Palette.PaletteData2 = new ushort[vmgobject.TextureTables[i].Palette.PaletteCount2];
                    Buffer.BlockCopy(palette2buffer, 0, vmgobject.TextureTables[i].Palette.PaletteData2, 0, palette2buffer.Length);
                }

                //Alpha textures
                if (vmgobject.TextureTables[i].Unk19 != 0)
                {
                    input.Seek(vmgobject.VMGheader.TextureTableOffset + vmgobject.TextureTables[i].AlphaTextureDataOffset, SeekOrigin.Begin);
                    vmgobject.TextureTables[i].AlphaTextureSize = (int)ImageSizeGCN(vmgobject.TextureTables[i].Unk19);
                    switch (vmgobject.TextureTables[i].ImageTypeVisible)
                    {
                        case ImageTypeGCN.I4: vmgobject.TextureTables[i].AlphaTextureSize >>= 1; vmgobject.TextureTables[i].AlphaBytes = ReadTextureData(reader, vmgobject.TextureTables[i].AlphaTextureSize); break;
                        case ImageTypeGCN.CMPR: vmgobject.TextureTables[i].AlphaTextureSize >>= 1; vmgobject.TextureTables[i].AlphaBytes = ReadTextureData(reader, vmgobject.TextureTables[i].AlphaTextureSize); break;
                    }
                    if (vmgobject.TextureTables[i].Unk10 == 464)
                    {
                        vmgobject.TextureTables[i].AlphaMipMapSize = vmgobject.TextureTables[i].AlphaTextureSize >> 1;
                        vmgobject.TextureTables[i].AlphaMipMapBytes = ReadTextureData(reader, vmgobject.TextureTables[i].AlphaMipMapSize);
                    }
                }


            }
            //BONES
            vmgobject.BoneTables = new BoneTable[vmgobject.VMGheader.BoneCount];
            vmgobject.BoneDictionary = new Dictionary<int, int>();
            for (int i = 0; i < vmgobject.VMGheader.BoneCount; i++)
            {
                input.Seek(vmgobject.VMGheader.BoneOffset + (i * 64), SeekOrigin.Begin);
                vmgobject.BoneTables[i] = ReadBoneTableGCN(reader);
                if (vmgobject.BoneTables[i].BoneNameOffset != 0)
                {
                    if (!vmgobject.BoneDictionary.ContainsKey(vmgobject.BoneTables[i].BoneParentIdx))
                    {
                        vmgobject.BoneDictionary.Add(vmgobject.BoneTables[i].BoneParentIdx, i);
                    }
                    input.Seek(vmgobject.BoneTables[i].BoneNameOffset, SeekOrigin.Begin);
                    vmgobject.BoneTables[i].Name = ReadNullTerminatedString(reader);
                }
                if (vmgobject.BoneTables[i].BoneNameOffset == 0)
                {
                    if (!vmgobject.BoneDictionary.ContainsKey(vmgobject.BoneTables[i].BoneParentIdx))
                    {
                        vmgobject.BoneDictionary.Add(vmgobject.BoneTables[i].BoneParentIdx, i);
                    }
                    vmgobject.BoneTables[i].Name = $"Empty_{i}";
                }
            }

            //MESH
            // vmgobject.Object_0 = new LayerObjectEntryGCN[vmgobject.VMGheader.Object0Count];
            // vmgobject.Object_1 = new LayerObjectEntryGCN[vmgobject.VMGheader.Object1Count];
            // vmgobject.Object_2 = new LayerObjectEntryGCN[vmgobject.VMGheader.Object2Count];
            // int[] TempOffsetSkinned = new int[3];
            // int[] TempVertSkinned = new int[3];
            // TempVertSkinned[1] = 9001; //Min
            // TempVertSkinned[2] = 0; //Max
            // int skinned_bool = 0;
            // for (int i = 0; i < vmgobject.VMGheader.Object0Count; i++)
            // {
            //     input.Seek(vmgobject.VMGheader.Object0Offset + (i * 40), SeekOrigin.Begin);
            //     vmgobject.Object_0[i] = ReadLayerObjectEntryGCN(reader);
            //     if (vmgobject.Object_0[i].Indice.B4 == (byte)MeshGCNContent.SKINNED)
            //     {
            //         vmgobject.Object_0[i].SkinnedMesh = new SkinnedMeshGCN();
            //         input.Seek(vmgobject.Object_0[i].FaceOffset, SeekOrigin.Begin);
            //         vmgobject.Object_0[i].SkinnedMesh.Faces = ReadFacesDataGCN(reader, (int)(vmgobject.Object_0[i].FaceCount));
            //         TempVertSkinned = ReadVertXGCN((int)vmgobject.Object_0[i].FaceCount, vmgobject.Object_0[i].SkinnedMesh.Faces, TempVertSkinned[1], TempVertSkinned[2]);
            //         if (skinned_bool == 0)
            //         {
            //             vmgobject.SkinnedData = vmgobject.Object_0[i];
            //             skinned_bool++;
            //         }
            //     }
            //     else if (vmgobject.Object_0[i].ObjectType == MeshGCNContent.STATIC)
            //         vmgobject.Object_0[i].StaticMesh = ObjectStaticHelper(vmgobject.Object_0[i], input, reader);
            // }
            // for (int i = 0; i < vmgobject.VMGheader.Object1Count; i++)
            // {
            //     input.Seek(vmgobject.VMGheader.Object1Offset + (i * 40), SeekOrigin.Begin);
            //     vmgobject.Object_1[i] = ReadLayerObjectEntryGCN(reader);
            //     if (vmgobject.Object_1[i].ObjectType == MeshGCNContent.SKINNED)
            //     {
            //         vmgobject.Object_1[i].SkinnedMesh = new SkinnedMeshGCN();
            //         input.Seek(vmgobject.Object_1[i].FaceOffset, SeekOrigin.Begin);
            //         vmgobject.Object_1[i].SkinnedMesh.Faces = ReadFacesDataGCN(reader, (int)(vmgobject.Object_1[i].FaceCount));
            //         TempVertSkinned = ReadVertXGCN((int)vmgobject.Object_1[i].FaceCount, vmgobject.Object_1[i].SkinnedMesh.Faces, TempVertSkinned[1], TempVertSkinned[2]);
            //         if (skinned_bool == 0)
            //         {
            //             vmgobject.SkinnedData = vmgobject.Object_1[i];
            //             skinned_bool++;
            //         }
            //     }
            //     else if (vmgobject.Object_1[i].ObjectType == MeshGCNContent.STATIC)
            //         vmgobject.Object_1[i].StaticMesh = ObjectStaticHelper(vmgobject.Object_1[i], input, reader);
            // }
            // for (int i = 0; i < vmgobject.VMGheader.Object2Count; i++)
            // {
            //     input.Seek(vmgobject.VMGheader.Object2Offset + (i * 40), SeekOrigin.Begin);
            //     vmgobject.Object_2[i] = ReadLayerObjectEntryGCN(reader);
            //     if (vmgobject.Object_2[i].ObjectType == MeshGCNContent.SKINNED)
            //     {
            //         vmgobject.Object_2[i].SkinnedMesh = new SkinnedMeshGCN();
            //         input.Seek(vmgobject.Object_2[i].FaceOffset, SeekOrigin.Begin);
            //         vmgobject.Object_2[i].SkinnedMesh.Faces = ReadFacesDataGCN(reader, (int)(vmgobject.Object_2[i].FaceCount));
            //         TempVertSkinned = ReadVertXGCN((int)vmgobject.Object_2[i].FaceCount, vmgobject.Object_2[i].SkinnedMesh.Faces, TempVertSkinned[1], TempVertSkinned[2]);
            //         if (skinned_bool == 0)
            //         {
            //             vmgobject.SkinnedData = vmgobject.Object_2[i];
            //             skinned_bool++;
            //         }
            //     }
            //     else if (vmgobject.Object_2[i].ObjectType == MeshGCNContent.STATIC)
            //         vmgobject.Object_2[i].StaticMesh = ObjectStaticHelper(vmgobject.Object_2[i], input, reader);
            // }
            // //Skinned buffers are set here for convience
            // vmgobject.Buffer1 = new Buffer1GCN[TempVertSkinned[0]];
            // vmgobject.Buffer2 = new Buffer2GCN[TempVertSkinned[0]];
            // vmgobject.Buffer3 = new Buffer2GCN[TempVertSkinned[0]];
            // input.Seek(TempOffsetSkinned[0], SeekOrigin.Begin);
            // for (int i = 0; i < TempVertSkinned[0]; i++)
            //     vmgobject.Buffer1[i] = ReadBuffer1GCN(reader);
            // input.Seek(TempOffsetSkinned[1], SeekOrigin.Begin);
            // for (int i = 0; i < TempVertSkinned[0]; i++)
            //     vmgobject.Buffer2[i] = ReadBuffer2GCN(reader);
            // input.Seek(TempOffsetSkinned[2], SeekOrigin.Begin);
            // for (int i = 0; i < TempVertSkinned[0]; i++)
            //     vmgobject.Buffer3[i] = ReadBuffer2GCN(reader);

            return vmgobject;

        }
    }
}
