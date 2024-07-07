using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SC2_3DS.Helper;
using static SC2_3DS.Weight;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Collections;
using SharpGLTF.Transforms;
using SharpGLTF.Runtime;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using System.Numerics;
using System.Xml;
using System.Windows.Controls;
using static SC2_3DS.Objects;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using NUnit.Framework;
using SharpGLTF.Memory;
using System.Windows;
using PaintDotNet;
using DdsFileTypePlus;

namespace SC2_3DS
{
    using VERTEX = VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>;
    
    internal class ExportGLTF
    {
        public static void Export(VMXObject vmxobject)
        {
            var mesh = VERTEX.CreateCompatibleMesh();
            AddGeometrySkinned(mesh, vmxobject);
            var scene = new SceneBuilder();
            var root = new NodeBuilder();
            root = root
                .WithLocalTranslation(new Vector3(0, 0, 0))
                .WithLocalRotation(Quaternion.CreateFromYawPitchRoll(0, 90 * (float.Pi / 180), 0))
                .WithLocalScale(new Vector3(1, 1, 1));
            var jointlist = Bones(vmxobject, root);
            scene.AddSkinnedMesh(mesh, Matrix4x4.Identity, jointlist.ToArray());
            // save the model in different formats
            var model = scene.ToGltf2();
            var settings = new WriteSettings
            {
                ImageWriting = ResourceWriteMode.SatelliteFile,
                ImageWriteCallback = imageSharingHook
            };
            string imageSharingHook(WriteContext ctx, string uri, MemoryImage image)
            {
                if (File.Exists(image.SourcePath))
                {
                    // image.SourcePath is an absolute path, we must make it relative to ctx.CurrentDirectory
            
                    var currDir = ctx.CurrentDirectory.FullName + "\\";
                    
                    // if the shared texture can be reached by the model in its directory, reuse the texture.
                    if (image.SourcePath.StartsWith(currDir, StringComparison.OrdinalIgnoreCase))
                    {
                        // we've found the shared texture!, return the uri relative to the model:
                        return image.SourcePath.Substring(currDir.Length);
                    }
                }
            
                // we were unable to reuse the shared texture,
                // default to write our own texture.
            
                image.SaveToFile(Path.Combine(ctx.CurrentDirectory.FullName, uri));
            
                return uri;
            }

            model.SaveGLTF("mesh.gltf");
            model.SaveGLB("mesh.glb");
        }

        static List<NodeBuilder> Bones(VMXObject vmxobject, NodeBuilder root)
        {
            Dictionary<int, NodeBuilder> node_map = new Dictionary<int, NodeBuilder>();
            var jointlist = new List<NodeBuilder>();
            foreach (var joint in vmxobject.BoneTables)
            {
                if (joint.BoneParentIdx == 255) //bones that have the root as parent
                {
                    var rotate = (joint.Rotation * 360) * (float.Pi / 180);
                    var scale = new Vector3(joint.StartPositionScale, joint.StartPositionScale, joint.StartPositionScale);
                    var node = root.CreateNode(joint.Name)
                        .WithLocalTranslation(joint.StartPosition)
                        .WithLocalRotation(Quaternion.CreateFromYawPitchRoll(rotate.Z, rotate.Y, rotate.X))
                        .WithLocalScale(scale);
                    jointlist.Add(node);
                    node_map[joint.BoneIdx] = node;
                }
                else if (joint.BoneNameOffset == 0) //empty bones
                {
                    var parent = node_map[joint.BoneParentIdx];
                    var node = parent.CreateNode()
                        .WithLocalTranslation(joint.StartPosition)
                        .WithLocalRotation(Quaternion.CreateFromYawPitchRoll(0, 0, 0))
                        .WithLocalScale(new Vector3(1, 1, 1));
                    jointlist.Add(node);
                }
                else
                {
                    var parent = node_map[joint.BoneParentIdx]; //any child bone
                    var rotate = (joint.Rotation * 360) * (float.Pi / 180);
                    if (joint.Ukn3 == 3) 
                    {  
                        rotate += (joint.Ukn1 * 360) * (float.Pi / 180); 
                    }
                    if (joint.Ukn3 == 7)
                    {
                        var transform1 = Matrix4x4.Identity;
                        var transform2 = Matrix4x4.Identity;
                        var transform3 = Matrix4x4.Identity;

                        rotate += (joint.Ukn1 * 360) * (float.Pi / 180);
                    }
                    var scale = new Vector3(joint.StartPositionScale, joint.StartPositionScale, joint.StartPositionScale);
                    var node = parent.CreateNode(joint.Name)
                        .WithLocalTranslation(joint.StartPosition)
                        .WithLocalRotation(Quaternion.CreateFromYawPitchRoll(rotate.Z, rotate.Y, rotate.X))
                        .WithLocalScale(scale);
                    jointlist.Add(node);
                    node_map[joint.BoneIdx] = node;
                }
            }
            return jointlist;
        }

        static void AddGeometrySkinned(MeshBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4> mesh, VMXObject vmxobject)
        {
            var mats = CreateMaterialWithTexture(vmxobject);
            var vertices = new List<VERTEX>();
            int vertexIndex = 0;
            int j = 0;
            for (int i = 0; i < vmxobject.WeightTables.VertCount1; i++, vertexIndex++, j+= 1)
            {
                vertices.Add(CreateVertex(vmxobject.Buffer2[vertexIndex], vmxobject.Buffer1[vertexIndex], vmxobject.WeightDef1Bone, j, 1));
            }
            j = 0;
            for (int i = 0; i < vmxobject.WeightTables.VertCount2; i++, vertexIndex++, j += 2)
            {
                vertices.Add(CreateVertex(vmxobject.Buffer2[vertexIndex], vmxobject.Buffer1[vertexIndex], vmxobject.WeightDef2Bone, j, 2));
            }
            j = 0;
            for (int i = 0; i < vmxobject.WeightTables.VertCount3; i++, vertexIndex++, j += 3)
            {
                vertices.Add(CreateVertex(vmxobject.Buffer2[vertexIndex], vmxobject.Buffer1[vertexIndex], vmxobject.WeightDef3Bone, j, 3));
            }
            j = 0;
            for (int i = 0; i < vmxobject.WeightTables.VertCount4; i++, vertexIndex++, j += 4)
            {
                vertices.Add(CreateVertex(vmxobject.Buffer2[vertexIndex], vmxobject.Buffer1[vertexIndex], vmxobject.WeightDef4Bone, j, 4));
            }
            j = 0;
            for (int i = 0; i < vmxobject.Object_0.Length; i++)
            {
                var num = vmxobject.MaterialDictionary.GetValueOrDefault((int)vmxobject.Object_0[i].MaterialOffset);
                var prim = mesh.UsePrimitive(mats.GetValueOrDefault(num));
                AddTrianglesSkinned(prim, vmxobject.Object_0[i], vertices);
            }
            for (int i = 0; i < vmxobject.Object_1.Length; i++)
            {
                var num = vmxobject.MaterialDictionary.GetValueOrDefault((int)vmxobject.Object_1[i].MaterialOffset);
                var prim = mesh.UsePrimitive(mats.GetValueOrDefault(num));
                AddTrianglesSkinned(prim, vmxobject.Object_1[i], vertices);
            }
            for (int i = 0; i < vmxobject.Object_2.Length; i++)
            {
                var num = vmxobject.MaterialDictionary.GetValueOrDefault((int)vmxobject.Object_2[i].MaterialOffset);
                var prim = mesh.UsePrimitive(mats.GetValueOrDefault(num));
                AddTrianglesSkinned(prim, vmxobject.Object_2[i], vertices);
            }
        }

        static void AddTrianglesSkinned(PrimitiveBuilder<MaterialBuilder, VertexPositionNormal, VertexTexture1, VertexJoints4> prim, LayerObjectEntryXbox layerobject, List<VERTEX> verts)
        {
        
            if (layerobject.ObjectType == MeshXboxContent.SKINNED)
            {
                var vertices = new List<Tuple<ushort, ushort, ushort>>();
                if (layerobject.PrimitiveType == PrimitiveXbox.TRIANGLESTRIP)
                    vertices = (TriangleStripToFaceTuple(layerobject.SkinnedMesh.Faces.Data));
                if (layerobject.PrimitiveType == PrimitiveXbox.TRIANGLELIST)
                    vertices = (TriangleListToFaceTuple(layerobject.SkinnedMesh.Faces.Data));
        
                for (int i = 0; i < vertices.Count; i++)
                {
                    prim.AddTriangle(verts[vertices[i].Item1], verts[vertices[i].Item2], verts[vertices[i].Item3]);
                }
            }
        }

        private static VERTEX CreateVertex(Buffer2Xbox buffer2, Buffer1Xbox buffer1, WeightDefXbox[] weightDef, int j, int weightlength)
        {
            var w0 = SparseWeight8.Create(new Vector4(
                                                 weightlength > 0 ? weightDef[j + 0].BoneIdx : 0,
                                                 weightlength > 1 ? weightDef[j + 1].BoneIdx : 0,
                                                 weightlength > 2 ? weightDef[j + 2].BoneIdx : 0,
                                                 weightlength > 3 ? weightDef[j + 3].BoneIdx : 0), 
                                                 new Vector4(0, 0, 0, 0),
                                                 new Vector4(
                                                 weightlength > 0 ? weightDef[j + 0].BoneWeight : 0,
                                                 weightlength > 1 ? weightDef[j + 1].BoneWeight : 0,
                                                 weightlength > 2 ? weightDef[j + 2].BoneWeight : 0,
                                                 weightlength > 3 ? weightDef[j + 3].BoneWeight : 0), 
                                                 new Vector4(0, 0, 0, 0));
            return new VERTEX(
                new VertexPositionNormal(buffer2.Position.X, buffer2.Position.Y, buffer2.Position.Z, buffer2.Normal.X, buffer2.Normal.Y, buffer2.Normal.Z),
                new VertexTexture1(buffer1.TileUV),
                new VertexJoints4((w0))
            );
        }
        static Dictionary<int, MaterialBuilder> CreateMaterialWithTexture(VMXObject vmxobject)//vector4 basecolor
        {
            string pngfolder = "PNG";
            var materials = new Dictionary<int, MaterialBuilder>();
            for (int i = 0; i < vmxobject.MaterialOffsets.Length; i++)
            {
                var num = vmxobject.MaterialDictionary.GetValueOrDefault(vmxobject.MaterialOffsets[i]);
                int texturenum = vmxobject.TextureDictionary.GetValueOrDefault((int)vmxobject.MaterialTables[num].VXTOffset0);

                byte[] fileBytes = File.ReadAllBytes($"{pngfolder}\\Texture{texturenum}.png");
                var imgBuilder = ImageBuilder.From(fileBytes);
                var material = new MaterialBuilder()
                    .WithBaseColor(imgBuilder, vmxobject.MaterialTables[num].DiffuseRGBA);
                if (vmxobject.MaterialTables[num].VXTOffset1 != 0)
                {
                    texturenum = vmxobject.TextureDictionary.GetValueOrDefault((int)vmxobject.MaterialTables[num].VXTOffset1);
                    fileBytes = File.ReadAllBytes($"{pngfolder}\\Texture{texturenum}.png");
                    imgBuilder = ImageBuilder.From(fileBytes);
                    material = material
                        .WithChannelImage(KnownChannel.SpecularColor, imgBuilder)
                        .WithChannelParam(KnownChannel.SpecularColor, KnownProperty.RGB, new Vector3(
                            vmxobject.MaterialTables[num].SpecularRGBA.X,
                            vmxobject.MaterialTables[num].SpecularRGBA.Y, 
                            vmxobject.MaterialTables[num].SpecularRGBA.Z))
                        .WithChannelImage(KnownChannel.SpecularFactor, imgBuilder)
                        .WithChannelParam(KnownChannel.SpecularFactor, KnownProperty.SpecularFactor, vmxobject.MaterialTables[num].SpecularRGBA.W);
                }
                materials.Add(num, material);
            }
            return materials;
        }
    }
}

