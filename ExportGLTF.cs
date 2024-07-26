using static SC2_3DS.Helper;
using static SC2_3DS.Weight;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Transforms;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using System.Numerics;
using static SC2_3DS.Objects;
using System.IO;
using SharpGLTF.Memory;
using System;
using System.Collections.Generic; // For List

namespace SC2_3DS
{
    using VERTEXSKINNED = VertexBuilder<VertexPositionNormal, VertexColor1Texture1, VertexJoints4>;
    using VERTEXSTATIC = VertexBuilder<VertexPositionNormal, VertexColor1Texture1, VertexJoints4>;

    internal class ExportGLTF
    {
        public static void Export(VMXObject vmxobject)
        {
            var scene = new SceneBuilder();
            var root = new NodeBuilder()
                .WithLocalTranslation(new Vector3())
                .WithLocalRotation(Quaternion.CreateFromYawPitchRoll(
                    DegToRad(180),
                    0,
                    DegToRad(90)))
                .WithLocalScale(new Vector3(1, 1, 1));

            Dictionary<int, NodeBuilder> node_map = new Dictionary<int, NodeBuilder>();
            var joint_list = CreateSkeleton(vmxobject, root, node_map);
            AddGeometrySkinned(scene, vmxobject, node_map, joint_list);
            AddGeometryStatic(scene, vmxobject, node_map, joint_list);

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

        static List<NodeBuilder> CreateSkeleton(VMXObject vmxobject, NodeBuilder root, Dictionary<int, NodeBuilder> node_map)
        {
            var joint_list = new List<NodeBuilder>();
            foreach (var joint in vmxobject.BoneTables)
            {
                if (joint.BoneParentIdx == 255) //bones that have the root as parent
                {
                    var scale = new Vector3(joint.StartPositionScale, joint.StartPositionScale, joint.StartPositionScale);
                    var node = root.CreateNode(joint.Name)
                        .WithLocalTranslation(new Vector3(joint.StartPosition.Y, joint.StartPosition.Z, joint.StartPosition.X))
                        .WithLocalRotation(Quaternion.CreateFromYawPitchRoll(
                            DegToRad(EulerToDeg(joint.Rotation.Z)),
                            DegToRad(EulerToDeg(joint.Rotation.Y)),
                            DegToRad(EulerToDeg(joint.Rotation.X))))
                        .WithLocalScale(scale);
                    joint_list.Add(node);
                    node_map[joint.BoneIdx] = node;
                }
                else if (joint.BoneNameOffset == 0) //empty bones
                {
                    var parent = node_map[joint.BoneParentIdx];
                    var node = parent.CreateNode()
                        .WithLocalTranslation(joint.StartPosition)
                        .WithLocalRotation(Quaternion.CreateFromYawPitchRoll(0, 0, 0))
                        .WithLocalScale(new Vector3(1, 1, 1));
                    joint_list.Add(node);
                }
                else
                {
                    var parent = node_map[joint.BoneParentIdx];
                    var scale = new Vector3(joint.StartPositionScale, joint.StartPositionScale, joint.StartPositionScale);
                    var node = parent.CreateNode(joint.Name)
                        .WithLocalTranslation(new Vector3(joint.StartPosition.Y, joint.StartPosition.Z, joint.StartPosition.X))
                        .WithLocalRotation(Quaternion.CreateFromYawPitchRoll(
                            DegToRad(EulerToDeg(joint.Rotation.Z)),
                            DegToRad(EulerToDeg(joint.Rotation.Y)),
                            DegToRad(EulerToDeg(joint.Rotation.X))))
                        .WithLocalScale(scale);
                    joint_list.Add(node);
                    node_map[joint.BoneIdx] = node;
                }
            }
            return joint_list;
        }

        static void AddGeometrySkinned(SceneBuilder scene, VMXObject vmxobject, Dictionary<int, NodeBuilder> node_map, List<NodeBuilder> joint_list)
        {
            var mats = CreateMaterialWithTexture(vmxobject);
            var vertices = new List<VERTEXSKINNED>();
            int vertex_index = 0;
            int j = 0;
            for (int i = 0; i < vmxobject.WeightTables.VertCount1; i++, vertex_index++, j += 1)
            {
                vertices.Add(CreateVertex(vmxobject.Buffer2[vertex_index], vmxobject.Buffer1[vertex_index], vmxobject.WeightDef1Bone, j, 1));
            }
            j = 0;
            for (int i = 0; i < vmxobject.WeightTables.VertCount2; i++, vertex_index++, j += 2)
            {
                vertices.Add(CreateVertex(vmxobject.Buffer2[vertex_index], vmxobject.Buffer1[vertex_index], vmxobject.WeightDef2Bone, j, 2));
            }
            j = 0;
            for (int i = 0; i < vmxobject.WeightTables.VertCount3; i++, vertex_index++, j += 3)
            {
                vertices.Add(CreateVertex(vmxobject.Buffer2[vertex_index], vmxobject.Buffer1[vertex_index], vmxobject.WeightDef3Bone, j, 3));
            }
            j = 0;
            for (int i = 0; i < vmxobject.WeightTables.VertCount4; i++, vertex_index++, j += 4)
            {
                vertices.Add(CreateVertex(vmxobject.Buffer2[vertex_index], vmxobject.Buffer1[vertex_index], vmxobject.WeightDef4Bone, j, 4));
            }
            j = 0;

            Vector3 centroid = new Vector3(0, 1.15f, 0); //to correct mesh to center
            for (int i = 0; i < vertices.Count; i++)
            {
                var vertex = vertices[i];
                vertex.Position -= centroid;
                vertices[i] = vertex;
            }

            for (int i = 0; i < vmxobject.SkinnedMeshList.Count; i++, vertex_index++)
            {
                var mesh_skinned = VERTEXSKINNED.CreateCompatibleMesh();
                var num = vmxobject.MaterialDictionary.GetValueOrDefault((int)vmxobject.SkinnedMeshList[i].MaterialOffset);
                var matrix = vmxobject.MatrixTables[vmxobject.MatrixDictionary.GetValueOrDefault((int)vmxobject.SkinnedMeshList[i].MatrixOffset)];
                var prim_skinned = mesh_skinned.UsePrimitive(mats.GetValueOrDefault(num));
                var uniqueVertices = new HashSet<int>();

                for (int k = 0; k < vmxobject.SkinnedMeshList[i].SkinnedMesh.Indicies.Count; k++)
                {
                    prim_skinned.AddTriangle(
                        vertices[vmxobject.SkinnedMeshList[i].SkinnedMesh.Indicies[k].Item1],
                        vertices[vmxobject.SkinnedMeshList[i].SkinnedMesh.Indicies[k].Item2],
                        vertices[vmxobject.SkinnedMeshList[i].SkinnedMesh.Indicies[k].Item3]);
                }
                scene.AddSkinnedMesh(mesh_skinned, matrix.Matrix, joint_list.ToArray());
            }
        }

        static void AddGeometryStatic(SceneBuilder scene, VMXObject vmxobject, Dictionary<int, NodeBuilder> node_map, List<NodeBuilder> joint_list)
        {
            var mats = CreateMaterialWithTexture(vmxobject);
            var vertices = new List<VERTEXSTATIC>();
            for (int i = 0; i < vmxobject.StaticMeshList.Count; i++)
            {
                var matrix = vmxobject.MatrixTables[vmxobject.MatrixDictionary.GetValueOrDefault((int)vmxobject.StaticMeshList[i].MatrixOffset)];
                foreach (var buffer4 in vmxobject.StaticMeshList[i].StaticMesh.Buffer4Data)
                {
                    vertices.Add(CreateVertex(buffer4, matrix.ParentBoneIdx));
                }
                var mesh_static = VERTEXSTATIC.CreateCompatibleMesh();
                var num = vmxobject.MaterialDictionary.GetValueOrDefault((int)vmxobject.StaticMeshList[i].MaterialOffset);
                var prim_static = mesh_static.UsePrimitive(mats.GetValueOrDefault(num));
                matrix.Matrix.Translation = node_map[matrix.ParentBoneIdx].WorldMatrix.Translation;// have to change the matrix translation to bone translation?

                for (int k = 0; k < vmxobject.StaticMeshList[i].StaticMesh.Indicies.Count; k++)
                {
                    prim_static.AddTriangle(
                        vertices[vmxobject.StaticMeshList[i].StaticMesh.Indicies[k].Item1],
                        vertices[vmxobject.StaticMeshList[i].StaticMesh.Indicies[k].Item2],
                        vertices[vmxobject.StaticMeshList[i].StaticMesh.Indicies[k].Item3]);
                }
                //scene.AddRigidMesh(mesh_static, matrix.Matrix);
                scene.AddSkinnedMesh(mesh_static, matrix.Matrix, joint_list.ToArray());
                vertices = new List<VERTEXSTATIC>();
            }
        }

        private static VERTEXSKINNED CreateVertex(Buffer2Xbox buffer2, Buffer1Xbox buffer1, WeightDefXbox[] weight_def, int j, int weight_length)
        {
            var weights = SparseWeight8.Create(new Vector4(
                                                 weight_length > 0 ? weight_def[j + 0].BoneIdx : 0,
                                                 weight_length > 1 ? weight_def[j + 1].BoneIdx : 0,
                                                 weight_length > 2 ? weight_def[j + 2].BoneIdx : 0,
                                                 weight_length > 3 ? weight_def[j + 3].BoneIdx : 0), 
                                                 new Vector4(0, 0, 0, 0),
                                                 new Vector4(
                                                 weight_length > 0 ? weight_def[j + 0].BoneWeight : 0,
                                                 weight_length > 1 ? weight_def[j + 1].BoneWeight : 0,
                                                 weight_length > 2 ? weight_def[j + 2].BoneWeight : 0,
                                                 weight_length > 3 ? weight_def[j + 3].BoneWeight : 0), 
                                                 new Vector4(0, 0, 0, 0));
            return new VERTEXSKINNED(
                new VertexPositionNormal(
                    buffer2.Position.X, 
                    buffer2.Position.Y, 
                    buffer2.Position.Z, 
                    buffer2.Normal.X, 
                    buffer2.Normal.Y, 
                    buffer2.Normal.Z),
                new VertexColor1Texture1(new Vector4(
                (float)buffer1.ColorRGBA.B1 / (float)255, 
                (float)buffer1.ColorRGBA.B2 / (float)255, 
                (float)buffer1.ColorRGBA.B3 / (float)255,
                (float)buffer1.ColorRGBA.B4 / (float)255), buffer1.TileUV),
                new VertexJoints4(weights)
            );
        }

        private static VERTEXSTATIC CreateVertex(Buffer4Xbox buffer4, byte bone_idx)
        {
            var weights = SparseWeight8.Create(new Vector4(
                                                 bone_idx,
                                                 0,0,0),
                                                 new Vector4(0, 0, 0, 0),
                                                 new Vector4(
                                                 1,
                                                 0,0,0),
                                                 new Vector4(0, 0, 0, 0));
            return new VERTEXSTATIC(
                new VertexPositionNormal(
                    buffer4.Position.X,
                    buffer4.Position.Y,
                    buffer4.Position.Z,
                    buffer4.Normal.X,
                    buffer4.Normal.Y,
                    buffer4.Normal.Z),
                new VertexColor1Texture1(new Vector4(
                (float)buffer4.VertDef.ColorRGBA.B1 / (float)255,
                (float)buffer4.VertDef.ColorRGBA.B2 / (float)255,
                (float)buffer4.VertDef.ColorRGBA.B3 / (float)255,
                (float)buffer4.VertDef.ColorRGBA.B4 / (float)255), buffer4.VertDef.TileUV),
                new VertexJoints4(weights)
            );
        }
        static Dictionary<int, MaterialBuilder> CreateMaterialWithTexture(VMXObject vmxobject)
        {
            string png_folder = "PNG";
            var materials = new Dictionary<int, MaterialBuilder>();
            for (int i = 0; i < vmxobject.MaterialOffsets.Length; i++)
            {
                var num = vmxobject.MaterialDictionary.GetValueOrDefault(vmxobject.MaterialOffsets[i]);
                int texture_num = vmxobject.TextureDictionary.GetValueOrDefault((int)vmxobject.MaterialTables[num].VXTOffset0);
                bool cull = (vmxobject.MaterialTables[num].CullMode == MaterialTableCull.DRAWNBOTHSIDES);
                byte[] file_data = File.ReadAllBytes($"{png_folder}\\Texture{texture_num}.png");
                var image = ImageBuilder.From(file_data);
                var material = new MaterialBuilder()
                    .WithDoubleSide(cull)
                    .WithBaseColor(image, vmxobject.MaterialTables[num].DiffuseRGBA);
                if (vmxobject.MaterialTables[num].VXTOffset1 != 0)
                {
                    texture_num = vmxobject.TextureDictionary.GetValueOrDefault((int)vmxobject.MaterialTables[num].VXTOffset1);
                    file_data = File.ReadAllBytes($"{png_folder}\\Texture{texture_num}.png");
                    image = ImageBuilder.From(file_data);
                    material = material
                        .WithChannelImage(KnownChannel.SpecularColor, image)
                        .WithChannelParam(KnownChannel.SpecularColor, KnownProperty.RGB, new Vector3(
                            vmxobject.MaterialTables[num].SpecularRGBA.X,
                            vmxobject.MaterialTables[num].SpecularRGBA.Y, 
                            vmxobject.MaterialTables[num].SpecularRGBA.Z))
                        .WithChannelImage(KnownChannel.SpecularFactor, image)
                        .WithChannelParam(KnownChannel.SpecularFactor, KnownProperty.SpecularFactor, vmxobject.MaterialTables[num].SpecularRGBA.W);
                }
                materials.Add(num, material);
            }
            return materials;
        }
    }
}

