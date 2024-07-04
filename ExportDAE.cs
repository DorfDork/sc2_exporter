using System.Text;
using System.Xml;
using static SC2_3DS.Helper;
using static SC2_3DS.Objects;
using static SC2_3DS.Textures;
using static SC2_3DS.Weight;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Diagnostics.Metrics;
using System.Xml.Linq;
using System;
using System.Windows.Media.Effects;


namespace SC2_3DS
{
    internal class ExportDAE
    {
        public static void Export(VMXObject vmxobject)
        {
            var sb = new StringBuilder();
            int static_count = 0;
            string[] InverseBindPose = new string[vmxobject.BoneTables.Length];
            string[] BoneNames = new string[vmxobject.BoneTables.Length];

            for (int i = 0; i < vmxobject.BoneTables.Length; i++)
            {

          //     double[] matrix3 = new double[]
{         //
          // -4.37114e-10, -0.01, 4.37114e-10, 0, 0, -4.37114e-10, -0.01, 0, 0.01, -4.37114e-10, 0, 0, 0, 0, 0, 1
};        //
          //
          //     // Extract position from the matrix
          //     var position = new Vector3((float)matrix3[3], (float)matrix3[7], (float)matrix3[11]);
          //
          //     // Extract rotation using the rotation part of the matrix
          //     // Assuming the matrix is in column-major order
          //     double m11 = matrix3[0], m12 = matrix3[4], m13 = matrix3[8];
          //     double m21 = matrix3[1], m22 = matrix3[5], m23 = matrix3[9];
          //     double m31 = matrix3[2], m32 = matrix3[6], m33 = matrix3[10];
          //
          //     // Calculate the angle of rotation (theta)
          //     double trace = m11 + m22 + m33;
          //     double theta = Math.Acos((trace - 1) / 2);
          //
          //     // Calculate the rotation axis (k)
          //     double kx = (m32 - m23) / (2 * Math.Sin(theta));
          //     double ky = (m13 - m31) / (2 * Math.Sin(theta));
          //     double kz = (m21 - m12) / (2 * Math.Sin(theta));
          //     var axis = new Vector3((float)kx, (float)ky, (float)kz);
          //     Matrix4x4 RotationMatrix33 = Matrix4x4.CreateFromYawPitchRoll((float)kz, (float)ky, (float)kx);
          //     // Reconstruct the rotation matrix using Rodrigues' formula
          //     Matrix4x4 RotationMatrix3 = Matrix4x4.CreateFromAxisAngle(axis, (float)theta);
          //     Matrix4x4 TranslationMatrix3 = CreateTranslationMatrix(position);
          //     Matrix4x4 LocalTransform3 = TranslationMatrix3 * RotationMatrix3;
          //
          //     // Print the reconstructed matrix
          //     var matrixString = CreateMatrixRowString(LocalTransform3);
          //     Console.WriteLine(matrixString);

                var rotate = (vmxobject.BoneTables[i].Rotation * 360) * (float.Pi / 180);
                Matrix4x4 ScaleMatrix = Matrix4x4.CreateScale(vmxobject.BoneTables[i].StartPositionScale, vmxobject.BoneTables[i].StartPositionScale, vmxobject.BoneTables[i].StartPositionScale);
                Matrix4x4 RotationMatrix = CreateRotationMatrix(rotate);
                Matrix4x4 TranslationMatrix = CreateTranslationMatrix(vmxobject.BoneTables[i].StartPosition);
                Matrix4x4 LocalTransform = ScaleMatrix * RotationMatrix * TranslationMatrix;
                Matrix4x4 GlobalTransform = new Matrix4x4();
                Matrix4x4.Invert(LocalTransform, out Matrix4x4 InvertBindPose);
                if (vmxobject.BoneTables[i].BoneParentIdx != 255)
                {
                    GlobalTransform = vmxobject.BoneTables[vmxobject.BoneTables[i].BoneParentIdx].LocalTransform * LocalTransform;
                    vmxobject.BoneTables[i].LocalTransform = LocalTransform;
                    vmxobject.BoneTables[i].GlobalTransform = GlobalTransform;
                }
                else
                {
                    GlobalTransform = LocalTransform * vmxobject.MatrixTables[vmxobject.MatrixDictionary[(int)vmxobject.SkinnedData.MatrixOffset]].Matrix;
                    vmxobject.BoneTables[i].LocalTransform = LocalTransform;
                    vmxobject.BoneTables[i].GlobalTransform = GlobalTransform;
                }
                

                InverseBindPose[i] = CreateMatrixRowString(InvertBindPose);
                BoneNames[i] = vmxobject.BoneTables[i].Name;
            }
            List<float> WeightValues = new List<float>();
            List<int> VData = new List<int>();
            WeightDefToArray(vmxobject, out WeightValues, out VData);
            float[] WeightValuesArray = WeightValues.ToArray();

            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(xmlDeclaration);

            XmlElement collada = doc.CreateElement("COLLADA");
            collada.SetAttribute("xmlns", "http://www.collada.org/2005/11/COLLADASchema");
            collada.SetAttribute("version", "1.4.1");
            doc.AppendChild(collada);

            XmlElement asset = doc.CreateElement("asset");
            collada.AppendChild(asset);
            XmlElement contributor = doc.CreateElement("contributor");
            asset.AppendChild(contributor);
            XmlElement authoring_tool = doc.CreateElement("authoring_tool");
            authoring_tool.InnerText = "C# Script";
            contributor.AppendChild(authoring_tool);

            XmlElement unit = doc.CreateElement("unit");
            unit.SetAttribute("name", "meter");
            unit.SetAttribute("meter", "1");
            asset.AppendChild(unit);

            //library_effects
            XmlElement library_effects = CreateAndAppendElement(doc, collada, "library_effects");
            AddEffect(doc, library_effects, vmxobject);

            //library_images
            XmlElement library_images = CreateAndAppendElement(doc, collada, "library_images");
            AddImage(doc, library_images, vmxobject);

            //library_materials
            XmlElement library_materials = CreateAndAppendElement(doc, collada, "library_materials");
            AddMaterial(doc, library_materials, vmxobject);

            //library_geometries
            XmlElement library_geometries = CreateAndAppendElement(doc, collada, "library_geometries");

            if (vmxobject.SkinnedData.ObjectType == MeshXboxContent.SKINNED)
            {
                AddGeometrySkinned(doc, library_geometries, vmxobject);
            }

            static_count = 0;
            for (int i = 0; i < vmxobject.Object_0.Length; i++)
            {
                if (vmxobject.Object_0[i].ObjectType == MeshXboxContent.STATIC)
                {
                    var num = vmxobject.MaterialDictionary.GetValueOrDefault((int)(vmxobject.Object_0[i].MaterialOffset));
                    AddGeometryStatic(doc, library_geometries, vmxobject.Object_0[i], num, static_count);
                    static_count++;
                }
            }

            for (int i = 0; i < vmxobject.Object_1.Length; i++)
            {
                if (vmxobject.Object_1[i].ObjectType == MeshXboxContent.STATIC)
                {
                    var num = vmxobject.MaterialDictionary.GetValueOrDefault((int)(vmxobject.Object_1[i].MaterialOffset));
                    AddGeometryStatic(doc, library_geometries, vmxobject.Object_1[i], num, static_count);
                    static_count++;
                }
            }

            for (int i = 0; i < vmxobject.Object_2.Length; i++)
            {
                if (vmxobject.Object_2[i].ObjectType == MeshXboxContent.STATIC)
                {
                    var num = vmxobject.MaterialDictionary.GetValueOrDefault((int)(vmxobject.Object_2[i].MaterialOffset));
                    AddGeometryStatic(doc, library_geometries, vmxobject.Object_2[i], num, static_count);
                    static_count++;
                }
            }

            //library_controller
            XmlElement library_controllers = CreateAndAppendElement(doc, collada, "library_controllers");
            XmlElement controller = CreateAndAppendElement(doc, library_controllers, "controller");
            controller.SetAttribute("id", "controller0");
            controller.SetAttribute("name", "armature0");
            XmlElement skin = CreateAndAppendElement(doc, controller, "skin");
            skin.SetAttribute("source", "#geometry0-mesh");
            XmlElement bind_shape_matrix = CreateAndAppendElement(doc, skin, "bind_shape_matrix");
            bind_shape_matrix.InnerText = CreateMatrixColumnString(Matrix4x4.Identity);
            AddSkinSourceElement(doc, skin, "controller0-joints", "JOINT", "name", "Name_array", BoneNames.Length, BoneNames, 1);
            AddSkinSourceElement(doc, skin, "controller0-bind_poses", "TRANSFORM", "float4x4", "float_array", BoneNames.Length * 16, InverseBindPose.Select(f => f.ToString()).ToArray(), 16);
            AddSkinSourceElement(doc, skin, "controller0-weights", "WEIGHT", "float", "float_array", WeightValuesArray.Length, WeightValuesArray.Select(f => f.ToString()).ToArray(), 1);

            int total_weight_vert = (int)vmxobject.WeightTables.VertCount1 +
                ((int)vmxobject.WeightTables.VertCount2) +
                ((int)vmxobject.WeightTables.VertCount3) +
                ((int)vmxobject.WeightTables.VertCount4);
            //joints
            XmlElement joints = CreateAndAppendElement(doc, skin, "joints");
            XmlElement input_joints1 = CreateAndAppendElement(doc, joints, "input");
            input_joints1.SetAttribute("semantic", "JOINT");
            input_joints1.SetAttribute("source", "#controller0-joints");
            XmlElement input_joints2 = CreateAndAppendElement(doc, joints, "input");
            input_joints2.SetAttribute("semantic", "INV_BIND_MATRIX");
            input_joints2.SetAttribute("source", "#controller0-bind_poses");


            //vertex_weights
            XmlElement vertex_weights = CreateAndAppendElement(doc, skin, "vertex_weights");
            vertex_weights.SetAttribute("count", total_weight_vert.ToString());
            XmlElement input_weights1 = CreateAndAppendElement(doc, vertex_weights, "input");
            input_weights1.SetAttribute("semantic", "JOINT");
            input_weights1.SetAttribute("source", "#controller0-joints");
            input_weights1.SetAttribute("offset", "0");
            XmlElement input_weights2 = CreateAndAppendElement(doc, vertex_weights, "input");
            input_weights2.SetAttribute("semantic", "WEIGHT");
            input_weights2.SetAttribute("source", "#controller0-weights");
            input_weights2.SetAttribute("offset", "1");
            XmlElement vcount = CreateAndAppendElement(doc, vertex_weights, "vcount");
            vcount.InnerText = string.Join(" ",
                                Enumerable.Repeat(1, (int)vmxobject.WeightTables.VertCount1).Concat(
                                    Enumerable.Repeat(2, (int)vmxobject.WeightTables.VertCount2)).Concat(
                                    Enumerable.Repeat(3, (int)vmxobject.WeightTables.VertCount3)).Concat(
                                    Enumerable.Repeat(4, (int)vmxobject.WeightTables.VertCount4)));
            // v = (bone id, weight)
            XmlElement v = CreateAndAppendElement(doc, vertex_weights, "v");
            v.InnerText = string.Join(" ", VData);

            //library_visual_scenes
            XmlElement library_visual_scenes = CreateAndAppendElement(doc, collada, "library_visual_scenes");
            XmlElement visual_scene = CreateAndAppendElement(doc, library_visual_scenes, "visual_scene");
            visual_scene.SetAttribute("id", "Scene");
            visual_scene.SetAttribute("name", "Scene");
            XmlElement node_armature = CreateAndAppendElement(doc, visual_scene, "node");
            node_armature.SetAttribute("id", "armature0");
            node_armature.SetAttribute("name", "armature0");
            node_armature.SetAttribute("type", "NODE");
            XmlElement matrix_armature = CreateAndAppendElement(doc, node_armature, "matrix");
            matrix_armature.SetAttribute("sid", "transform");
            matrix_armature.InnerText = CreateMatrixColumnString(Matrix4x4.Identity);

            Bones(vmxobject, doc, node_armature);

            if (vmxobject.SkinnedData.ObjectType == MeshXboxContent.SKINNED)
            {
                XmlElement node = CreateAndAppendElement(doc, node_armature, "node");
                node.SetAttribute("id", "geometry0-mesh");
                node.SetAttribute("name", "geometry0-mesh");
                node.SetAttribute("type", "NODE");
                XmlElement matrix = CreateAndAppendElement(doc, node, "matrix");
                matrix.SetAttribute("sid", "transform");
                matrix.InnerText = CreateMatrixColumnString(vmxobject.MatrixTables[vmxobject.MatrixDictionary[(int)vmxobject.SkinnedData.MatrixOffset]].Matrix);
                XmlElement instance_controller = CreateAndAppendElement(doc, node, "instance_controller");
                foreach (var value in vmxobject.BoneTables)
                {
                    if (value.BoneParentIdx == 255)
                    {
                        XmlElement skeleton = CreateAndAppendElement(doc, instance_controller, "skeleton");
                        skeleton.InnerText = $"#armature0_{value.Name}";
                    }
                }

                instance_controller.SetAttribute("url", "#controller0");
                XmlElement bind_material = CreateAndAppendElement(doc, instance_controller, "bind_material");
                XmlElement technique_common = CreateAndAppendElement(doc, bind_material, "technique_common");
                for (int i = 0; i < vmxobject.Object_0.Length; i++)
                {
                    if (vmxobject.Object_0[i].ObjectType == MeshXboxContent.SKINNED)
                    {
                        var num = vmxobject.MaterialDictionary.GetValueOrDefault((int)(vmxobject.Object_0[i].MaterialOffset));
                        AddInstanceMaterialElement(doc, technique_common, $"Material{num}-material");
                    }
                }
                for (int i = 0; i < vmxobject.Object_1.Length; i++)
                {
                    if (vmxobject.Object_1[i].ObjectType == MeshXboxContent.SKINNED)
                    {
                        var num = vmxobject.MaterialDictionary.GetValueOrDefault((int)(vmxobject.Object_1[i].MaterialOffset));
                        AddInstanceMaterialElement(doc, technique_common, $"Material{num}-material");
                    }
                }
                for (int i = 0; i < vmxobject.Object_2.Length; i++)
                {
                    if (vmxobject.Object_2[i].ObjectType == MeshXboxContent.SKINNED)
                    {
                        var num = vmxobject.MaterialDictionary.GetValueOrDefault((int)(vmxobject.Object_2[i].MaterialOffset));
                        AddInstanceMaterialElement(doc, technique_common, $"Material{num}-material");
                    }
                }
            }
            static_count = 0;
            for (int i = 0; i < vmxobject.Object_0.Length; i++)
            {
                if (vmxobject.Object_0[i].ObjectType == MeshXboxContent.STATIC)
                {
                    XmlElement node = CreateAndAppendElement(doc, node_armature, "node");
                    node.SetAttribute("id", $"geometry-static{static_count}-mesh");
                    node.SetAttribute("name", $"geometry-static{static_count}-mesh");
                    XmlElement matrix = CreateAndAppendElement(doc, node, "matrix");
                    matrix.SetAttribute("sid", "transform");
                    var matrixs = vmxobject.MatrixDictionary[(int)vmxobject.Object_0[i].MatrixOffset];
                    var matrix43 = CreateMatrixColumn(vmxobject.MatrixTables[matrixs].Matrix);
                    //matrix43.M14 += (vmxobject.Object_0[i].StaticMesh.CenterRadius.X);
                    //matrix43.M24 += (vmxobject.Object_0[i].StaticMesh.CenterRadius.Y);
                    //matrix43.M34 += (vmxobject.Object_0[i].StaticMesh.CenterRadius.Z);
                    //matrix43 *= vmxobject.BoneTables[vmxobject.MatrixTables[matrixs].ParentBoneIdx].LocalTransform;

                    //matrix43.Translation += center;
                    //matrix.InnerText = CreateMatrixString(vmxobject.MatrixTables[vmxobject.MatrixDictionary[(int)vmxobject.Object_0[i].MatrixOffset]].Matrix);
                    matrix.InnerText = CreateMatrixRowString(matrix43);
                    XmlElement instance_geometry = CreateAndAppendElement(doc, node, "instance_geometry");
                    instance_geometry.SetAttribute("url", $"#geometry-static{static_count}-mesh");
                    instance_geometry.SetAttribute("name", $"geometry-static{static_count}");
                    XmlElement bind_material = CreateAndAppendElement(doc, instance_geometry, "bind_material");
                    XmlElement technique_common = CreateAndAppendElement(doc, bind_material, "technique_common");
                    var num = vmxobject.MaterialDictionary.GetValueOrDefault((int)(vmxobject.Object_0[i].MaterialOffset));
                    AddInstanceMaterialElement(doc, technique_common, $"Material{num}-material");
                    static_count++;
                }
            }
            for (int i = 0; i < vmxobject.Object_1.Length; i++)
            {
                if (vmxobject.Object_1[i].ObjectType == MeshXboxContent.STATIC)
                {
                    XmlElement node = CreateAndAppendElement(doc, node_armature, "node");
                    node.SetAttribute("id", $"geometry-static{static_count}-mesh");
                    node.SetAttribute("name", $"geometry-static{static_count}-mesh");
                    XmlElement matrix = CreateAndAppendElement(doc, node, "matrix");
                    XmlElement instance_geometry = CreateAndAppendElement(doc, node, "instance_geometry");
                    instance_geometry.SetAttribute("url", $"#geometry-static{static_count}-mesh");
                    instance_geometry.SetAttribute("name", $"geometry-static{static_count}");
                    XmlElement bind_material = CreateAndAppendElement(doc, instance_geometry, "bind_material");
                    XmlElement technique_common = CreateAndAppendElement(doc, bind_material, "technique_common");
                    var num = vmxobject.MaterialDictionary.GetValueOrDefault((int)(vmxobject.Object_1[i].MaterialOffset));
                    AddInstanceMaterialElement(doc, technique_common, $"Material{num}-material");
                    static_count++;
                }
            }
            for (int i = 0; i < vmxobject.Object_2.Length; i++)
            {
                if (vmxobject.Object_2[i].ObjectType == MeshXboxContent.STATIC)
                {
                    XmlElement node = CreateAndAppendElement(doc, node_armature, "node");
                    node.SetAttribute("id", $"geometry-static{static_count}-mesh");
                    node.SetAttribute("name", $"geometry-static{static_count}-mesh");
                    XmlElement matrix = CreateAndAppendElement(doc, node, "matrix");
                    XmlElement instance_geometry = CreateAndAppendElement(doc, node, "instance_geometry");
                    instance_geometry.SetAttribute("url", $"#geometry-static{static_count}-mesh");
                    instance_geometry.SetAttribute("name", $"geometry-static{static_count}");
                    XmlElement bind_material = CreateAndAppendElement(doc, instance_geometry, "bind_material");
                    XmlElement technique_common = CreateAndAppendElement(doc, bind_material, "technique_common");
                    var num = vmxobject.MaterialDictionary.GetValueOrDefault((int)(vmxobject.Object_2[i].MaterialOffset));
                    AddInstanceMaterialElement(doc, technique_common, $"Material{num}-material");
                    static_count++;
                }
            }

            //Scene
            XmlElement scene = CreateAndAppendElement(doc, collada, "scene");
            XmlElement instance_visual_scene = CreateAndAppendElement(doc, scene, "instance_visual_scene");
            instance_visual_scene.SetAttribute("url", "#Scene");
            doc.Save("faces_export.dae");
        }

        static void AddEffect(XmlDocument doc, XmlElement parent, VMXObject vmxobject)
        {
            for (int i = 0; i < vmxobject.MaterialOffsets.Length; i++)
            {
                var num = vmxobject.MaterialDictionary.GetValueOrDefault(vmxobject.MaterialOffsets[i]);
                XmlElement effect = CreateAndAppendElement(doc, parent, "effect");
                effect.SetAttribute("id", $"Material{num}-effect");
                XmlElement profile_common = CreateAndAppendElement(doc, effect, "profile_COMMON");
                if (vmxobject.MaterialTables[num].VXTOffset0 != 0)
                {
                    AddSurfaceElement(doc, profile_common, $"Texture{num}_0-surface", $"Texture{num}_0");
                    AddSamplerElement(doc, profile_common, $"Texture{num}_0-sampler", $"Texture{num}_0-surface");
                }
                if (vmxobject.MaterialTables[num].VXTOffset1 != 0)
                {
                    AddSurfaceElement(doc, profile_common, $"Texture{num}_1-surface", $"Texture{num}_1");
                    AddSamplerElement(doc, profile_common, $"Texture{num}_1-sampler", $"Texture{num}_1-surface");
                }
                if (vmxobject.MaterialTables[num].VXTOffset2 != 0)
                {
                    AddSurfaceElement(doc, profile_common, $"Texture{num}_2-surface", $"Texture{num}_2");
                    AddSamplerElement(doc, profile_common, $"Texture{num}_2-sampler", $"Texture{num}_2-surface");
                }
                XmlElement technique = CreateAndAppendElement(doc, profile_common, "technique");
                technique.SetAttribute("sid", "common");
                XmlElement phong = CreateAndAppendElement(doc, technique, "phong");
                AddColorElement(doc, phong, "ambient", Vector4ToArray(vmxobject.MaterialTables[num].AmbientRGBA));
                AddColorElement(doc, phong, "diffuse", Vector4ToArray(vmxobject.MaterialTables[num].DiffuseRGBA));
                //AddColorElement(doc, phong, "specular", Vector4ToArray(vmxobject.MaterialTables[num].SpecularRGB));
                XmlElement diffuse = CreateAndAppendElement(doc, phong, "diffuse");
                if (vmxobject.MaterialTables[num].VXTOffset0 != 0)
                    AddTextureElement(doc, diffuse, $"Texture{num}_0", "UVMap");
                if (vmxobject.MaterialTables[num].VXTOffset1 != 0)
                    AddTextureElement(doc, diffuse, $"Texture{num}_1", "UVMap");
                if (vmxobject.MaterialTables[num].VXTOffset2 != 0)
                    AddTextureElement(doc, diffuse, $"Texture{num}_2", "UVMap");
            }
        }

        static void AddImage(XmlDocument doc, XmlElement parent, VMXObject vmxobject)
        {
            for (int i = 0; i < vmxobject.MaterialOffsets.Length; i++)
            {
                var num = vmxobject.MaterialDictionary.GetValueOrDefault(vmxobject.MaterialOffsets[i]);
                int texturenum = 0;
                if (vmxobject.MaterialTables[num].VXTOffset0 != 0)
                {
                    texturenum = vmxobject.TextureDictionary.GetValueOrDefault((int)vmxobject.MaterialTables[num].VXTOffset0);
                    AddImageElement(doc, parent, $"Texture{num}_0", $"Texture{texturenum}.dds");
                }
                if (vmxobject.MaterialTables[num].VXTOffset1 != 0)
                {
                    texturenum = vmxobject.TextureDictionary.GetValueOrDefault((int)vmxobject.MaterialTables[num].VXTOffset1);
                    AddImageElement(doc, parent, $"Texture{num}_1", $"Texture{texturenum}.dds");
                }
                if (vmxobject.MaterialTables[num].VXTOffset2 != 0)
                {
                    texturenum = vmxobject.TextureDictionary.GetValueOrDefault((int)vmxobject.MaterialTables[num].VXTOffset2);
                    AddImageElement(doc, parent, $"Texture{num}_2", $"Texture{texturenum}.dds");
                }
            }
        }

        static void AddMaterial(XmlDocument doc, XmlElement parent, VMXObject vmxobject)
        {
            for (int i = 0; i < vmxobject.MaterialOffsets.Length; i++)
            {
                var num = vmxobject.MaterialDictionary.GetValueOrDefault(vmxobject.MaterialOffsets[i]);
                XmlElement material = CreateAndAppendElement(doc, parent, "material");
                material.SetAttribute("id", $"Material{num}-material");
                material.SetAttribute("name", $"Material{num}");
                XmlElement instance_effect = CreateAndAppendElement(doc, material, "instance_effect");
                instance_effect.SetAttribute("url", $"#Material{num}-effect");
            }
        }

        static void AddGeometrySkinned(XmlDocument doc, XmlElement parent, VMXObject vmxobject)
        {
            XmlElement geometry = CreateAndAppendElement(doc, parent, "geometry");
            geometry.SetAttribute("id", "geometry0-mesh");
            geometry.SetAttribute("name", "geometry0");
            XmlElement mesh = CreateAndAppendElement(doc, geometry, "mesh");
           // List<float[]> position_skinned = Buffer2PositionToArray(vmxobject.Buffer2);
           // List<float[]> normal_skinned = Buffer2NormalToArray(vmxobject.Buffer2);
           // List<float[]> texcoord_skinned = Buffer1TexcoordToArray(vmxobject.Buffer1);
           // AddSourceElement(doc, mesh, "geometry0-positions", "float_array", position_skinned, "position");
           // AddSourceElement(doc, mesh, "geometry0-normals", "float_array", normal_skinned, "normal");
           // AddSourceElement(doc, mesh, "geometry0-texcoords", "float_array", texcoord_skinned, "texcoord");
            //Verts
            XmlElement vertices_element = CreateAndAppendElement(doc, mesh, "vertices");
            vertices_element.SetAttribute("id", "geometry0-vertices");
            XmlElement input_position = CreateAndAppendElement(doc, vertices_element, "input");
            input_position.SetAttribute("semantic", "POSITION");
            input_position.SetAttribute("source", "#geometry0-positions");
            XmlElement input_normal = CreateAndAppendElement(doc, vertices_element, "input");
            input_normal.SetAttribute("semantic", "NORMAL");
            input_normal.SetAttribute("source", "#geometry0-normals");
            for (int i = 0; i < vmxobject.Object_0.Length; i++)
            {
                if (vmxobject.Object_0[i].ObjectType == MeshXboxContent.SKINNED)
                {
                    var num = vmxobject.MaterialDictionary.GetValueOrDefault((int)(vmxobject.Object_0[i].MaterialOffset));
                    AddTriangles(doc, mesh, vmxobject.Object_0[i], num, 0);
                }
            }
            for (int i = 0; i < vmxobject.Object_1.Length; i++)
            {
                if (vmxobject.Object_1[i].ObjectType == MeshXboxContent.SKINNED)
                {
                    var num = vmxobject.MaterialDictionary.GetValueOrDefault((int)(vmxobject.Object_1[i].MaterialOffset));
                    AddTriangles(doc, mesh, vmxobject.Object_1[i], num, 0);
                }
            }
            for (int i = 0; i < vmxobject.Object_2.Length; i++)
            {
                if (vmxobject.Object_2[i].ObjectType == MeshXboxContent.SKINNED)
                {
                    var num = vmxobject.MaterialDictionary.GetValueOrDefault((int)(vmxobject.Object_2[i].MaterialOffset));
                    AddTriangles(doc, mesh, vmxobject.Object_2[i], num, 0);
                }
            }
        }

        static void AddGeometryStatic(XmlDocument doc, XmlElement parent, LayerObjectEntryXbox layerobject, int num, int static_count)
        {
            XmlElement geometry = CreateAndAppendElement(doc, parent, "geometry");
            geometry.SetAttribute("id", $"geometry-static{static_count}-mesh");
            geometry.SetAttribute("name", $"geometry-static{static_count}");
            XmlElement mesh = CreateAndAppendElement(doc, geometry, "mesh");
            var position_static = Buffer4PositionToArray(layerobject.StaticMesh.Buffer4Data);
            var normal_static = Buffer4NormalToArray(layerobject.StaticMesh.Buffer4Data);
            var texcoord_static = Buffer4TexcoordToArray(layerobject.StaticMesh.Buffer4Data);
            AddSourceElement(doc, mesh, $"geometry-static{static_count}-positions", "float_array", position_static, "position");
            AddSourceElement(doc, mesh, $"geometry-static{static_count}-normals", "float_array", normal_static, "normal");
            AddSourceElement(doc, mesh, $"geometry-static{static_count}-texcoords", "float_array", texcoord_static, "texcoord");
            //Verts
            XmlElement vertices_element = CreateAndAppendElement(doc, mesh, "vertices");
            vertices_element.SetAttribute("id", $"geometry-static{static_count}-vertices");
            XmlElement input_position = CreateAndAppendElement(doc, vertices_element, "input");
            input_position.SetAttribute("semantic", "POSITION");
            input_position.SetAttribute("source", $"#geometry-static{static_count}-positions");
            XmlElement input_normal = CreateAndAppendElement(doc, vertices_element, "input");
            input_normal.SetAttribute("semantic", "NORMAL");
            input_normal.SetAttribute("source", $"#geometry-static{static_count}-normals");
            //Triangles
            AddTriangles(doc, mesh, layerobject, num, static_count);
        }

        static void AddTriangles(XmlDocument doc, XmlElement parent, LayerObjectEntryXbox layerobject, int num, int static_count)
        {
            if (layerobject.ObjectType == MeshXboxContent.SKINNED)
            {
                var face = new List<Tuple<ushort, ushort, ushort>>();
                if (layerobject.PrimitiveType == PrimitiveXbox.TRIANGLESTRIP)
                    face = (TriangleStripToFaceTuple(layerobject.SkinnedMesh.Faces.Data));
                if (layerobject.PrimitiveType == PrimitiveXbox.TRIANGLELIST)
                    face = (TriangleListToFaceTuple(layerobject.SkinnedMesh.Faces.Data));

                XmlElement triangles = CreateAndAppendElement(doc, parent, "triangles");
                triangles.SetAttribute("count", face.Count.ToString());
                triangles.SetAttribute("material", $"Material{num}-material");
                XmlElement input_vertex = CreateAndAppendElement(doc, triangles, "input");
                input_vertex.SetAttribute("semantic", "VERTEX");
                input_vertex.SetAttribute("source", "#geometry0-vertices");
                input_vertex.SetAttribute("offset", "0");
                XmlElement input_texcoord = CreateAndAppendElement(doc, triangles, "input");
                input_texcoord.SetAttribute("semantic", "TEXCOORD");
                input_texcoord.SetAttribute("source", "#geometry0-texcoords");
                input_texcoord.SetAttribute("offset", "1");
                XmlElement p = CreateAndAppendElement(doc, triangles, "p");
                p.InnerText = string.Join(" ", Flatten(face));
            }

            if (layerobject.ObjectType == MeshXboxContent.STATIC)
            {
                var face = new List<Tuple<ushort, ushort, ushort>>();
                if (layerobject.PrimitiveType == PrimitiveXbox.TRIANGLESTRIP)
                    face = (TriangleStripToFaceTuple(layerobject.StaticMesh.Faces.Data));
                if (layerobject.PrimitiveType == PrimitiveXbox.TRIANGLELIST)
                    face = (TriangleListToFaceTuple(layerobject.StaticMesh.Faces.Data));

                XmlElement triangles = CreateAndAppendElement(doc, parent, "triangles");
                triangles.SetAttribute("count", face.Count.ToString());
                triangles.SetAttribute("material", $"Material{num}-material");
                XmlElement input_vertex = CreateAndAppendElement(doc, triangles, "input");
                input_vertex.SetAttribute("semantic", "VERTEX");
                input_vertex.SetAttribute("source", $"#geometry-static{static_count}-vertices");//change
                input_vertex.SetAttribute("offset", "0");
                XmlElement input_texcoord = CreateAndAppendElement(doc, triangles, "input");
                input_texcoord.SetAttribute("semantic", "TEXCOORD");
                input_texcoord.SetAttribute("source", $"#geometry-static{static_count}-texcoords");//change
                input_texcoord.SetAttribute("offset", "1");
                XmlElement p = CreateAndAppendElement(doc, triangles, "p");
                p.InnerText = string.Join(" ", Flatten(face));
            }
        }

        static void Bones(VMXObject vmxobject, XmlDocument doc, XmlElement rootElement)
        {
            Dictionary<int, XmlElement> idToElementMap = new Dictionary<int, XmlElement>();
            for (int i = 0; i < vmxobject.VMXheader.BoneCount; i++)
            {
                //if (vmxobject.BoneTables[i].BoneNameOffset != 0)
                //{
                if (vmxobject.BoneTables[i].BoneParentIdx == 255)
                {
                    XmlElement nodeElement = doc.CreateElement("node");
                    nodeElement.SetAttribute("id", $"armature0_{vmxobject.BoneTables[i].Name}");
                    nodeElement.SetAttribute("name", vmxobject.BoneTables[i].Name);
                    nodeElement.SetAttribute("sid", vmxobject.BoneTables[i].Name);
                    nodeElement.SetAttribute("type", "JOINT");
                    XmlElement matrixElement = CreateAndAppendElement(doc, nodeElement, "matrix");
                    matrixElement.SetAttribute("sid", "transform");
                    matrixElement.InnerText = CreateMatrixRowString(vmxobject.BoneTables[i].GlobalTransform);
                    rootElement.AppendChild(nodeElement);
                    idToElementMap[vmxobject.BoneTables[i].BoneIdx] = nodeElement;
                }
                else if (vmxobject.BoneTables[i].BoneNameOffset == 0)
                {
                    XmlElement nodeElement = doc.CreateElement("node");
                    nodeElement.SetAttribute("id", $"{vmxobject.BoneTables[i].Name}");
                    nodeElement.SetAttribute("name", vmxobject.BoneTables[i].Name);
                    nodeElement.SetAttribute("sid", vmxobject.BoneTables[i].Name);
                    nodeElement.SetAttribute("type", "JOINT");
                    XmlElement matrixElement = CreateAndAppendElement(doc, nodeElement, "matrix");
                    matrixElement.SetAttribute("sid", "transform");
                    var parentElement = idToElementMap[vmxobject.BoneTables[i].BoneParentIdx];
                    matrixElement.InnerText = CreateMatrixRowString(vmxobject.BoneTables[i].LocalTransform);
                    parentElement.AppendChild(nodeElement);
                    //idToElementMap[vmxobject.BoneTables[i].BoneIdx] = nodeElement;
                }
                else
                {
                    XmlElement nodeElement = doc.CreateElement("node");
                    nodeElement.SetAttribute("id", $"armature0_{vmxobject.BoneTables[i].Name}");
                    nodeElement.SetAttribute("name", vmxobject.BoneTables[i].Name);
                    nodeElement.SetAttribute("sid", vmxobject.BoneTables[i].Name);
                    nodeElement.SetAttribute("type", "JOINT");
                    XmlElement matrixElement = CreateAndAppendElement(doc, nodeElement, "matrix");
                    matrixElement.SetAttribute("sid", "transform");
                    if (!vmxobject.BoneDictionary.ContainsKey(vmxobject.BoneTables[i].BoneIdx))
                    {
                        //   XmlElement extra = CreateAndAppendElement(doc, nodeElement, "extra");
                        //   XmlElement technique = CreateAndAppendElement(doc, extra, "technique");
                        //   technique.SetAttribute("profile", "blender");
                        //   XmlElement connect = CreateAndAppendElement(doc, technique, "connect");
                        //   connect.SetAttribute("sid", "connect");
                        //   connect.SetAttribute("type", "bool");
                        //   connect.InnerText = "1";
                        //   XmlElement roll = CreateAndAppendElement(doc, technique, "roll");
                        //   roll.SetAttribute("sid", "roll");
                        //   roll.SetAttribute("type", "float");
                        //   roll.InnerText = $"{(vmxobject.BoneTables[i].Rotation.Z * 360) * (float.Pi / 180)}";
                        //   XmlElement tip_x = CreateAndAppendElement(doc, technique, "tip_x");
                        //   tip_x.SetAttribute("sid", "tip_x");
                        //   tip_x.SetAttribute("type", "float");
                        //   tip_x.InnerText = $"{vmxobject.BoneTables[i].StartPosition.X}";
                        //   XmlElement tip_y = CreateAndAppendElement(doc, technique, "tip_y");
                        //   tip_y.SetAttribute("sid", "tip_y");
                        //   tip_y.SetAttribute("type", "float");
                        //   tip_y.InnerText = $"{vmxobject.BoneTables[i].StartPosition.Y}";
                        //   XmlElement tip_z = CreateAndAppendElement(doc, technique, "tip_z");
                        //   tip_z.SetAttribute("sid", "tip_z");
                        //   tip_z.SetAttribute("type", "float");
                        //   tip_z.InnerText = $"{vmxobject.BoneTables[i].StartPosition.Z}";
                    }
                    else
                    {
                        //   XmlElement extra = CreateAndAppendElement(doc, nodeElement, "extra");
                        //   XmlElement technique = CreateAndAppendElement(doc, extra, "technique");
                        //   technique.SetAttribute("profile", "blender");
                        //   XmlElement connect = CreateAndAppendElement(doc, technique, "connect");
                        //   connect.SetAttribute("sid", "connect");
                        //   connect.SetAttribute("type", "bool");
                        //   connect.InnerText = "1";
                    }
                    var parentElement = idToElementMap[vmxobject.BoneTables[i].BoneParentIdx];
                    matrixElement.InnerText = CreateMatrixRowString(vmxobject.BoneTables[i].LocalTransform);
                    parentElement.AppendChild(nodeElement);
                    idToElementMap[vmxobject.BoneTables[i].BoneIdx] = nodeElement;
                }
                //}
            }
        }

        private static Matrix4x4 CreateMatrixColumn(Matrix4x4 matrix)
        {
            Matrix4x4 value = new Matrix4x4();
            value.M11 = matrix.M11;
            value.M12 = matrix.M21;
            value.M13 = matrix.M31;
            value.M14 = matrix.M41;
            value.M21 = matrix.M12;
            value.M22 = matrix.M22;
            value.M23 = matrix.M32;
            value.M24 = matrix.M42;
            value.M31 = matrix.M13;
            value.M32 = matrix.M23;
            value.M33 = matrix.M33;
            value.M34 = matrix.M43;
            value.M41 = matrix.M14;
            value.M42 = matrix.M24;
            value.M43 = matrix.M34;
            value.M44 = matrix.M44;
            return value;
        }

        private static string CreateMatrixColumnString(Matrix4x4 matrix)
        {
            return $"{matrix.M11} {matrix.M21} {matrix.M31} {matrix.M41} " +
                    $"{matrix.M12} {matrix.M22} {matrix.M32} {matrix.M42} " +
                    $"{matrix.M13} {matrix.M23} {matrix.M33} {matrix.M43} " +
                    $"{matrix.M14} {matrix.M24} {matrix.M34} {matrix.M44}";
        }


        private static Matrix4x4 CreateTranslationMatrix(Vector3 value)
        {
            Matrix4x4 matrix = Matrix4x4.Identity;
            matrix.M14 = value.X;
            matrix.M24 = value.Y;
            matrix.M34 = value.Z;
            return matrix;
        }
        private static string CreateMatrixRowString(Matrix4x4 matrix)
        {
            return $"{matrix.M11} {matrix.M12} {matrix.M13} {matrix.M14} " +
                    $"{matrix.M21} {matrix.M22} {matrix.M23} {matrix.M24} " +
                    $"{matrix.M31} {matrix.M32} {matrix.M33} {matrix.M34} " +
                    $"{matrix.M41} {matrix.M42} {matrix.M43} {matrix.M44}";
        }

        private static Matrix4x4 CreateRotationMatrix(Vector3 value)
        {
            Matrix4x4 matrix = Matrix4x4.Identity;
            Matrix4x4 Yaw = Matrix4x4.Identity;
            Yaw.M11 = MathF.Cos(value.Z);
            Yaw.M12 = -MathF.Sin(value.Z);
            Yaw.M21 = MathF.Sin(value.Z);
            Yaw.M22 = MathF.Cos(value.Z);
            matrix *= Yaw;

            Matrix4x4 Pitch = Matrix4x4.Identity;
            Pitch.M11 = MathF.Cos(value.Y);
            Pitch.M13 = MathF.Sin(value.Y);
            Pitch.M31 = -MathF.Sin(value.Y);
            Pitch.M33 = MathF.Cos(value.Y);
            matrix *= Pitch;

            Matrix4x4 Roll = Matrix4x4.Identity;
            Roll.M22 = MathF.Cos(value.X);
            Roll.M23 = -MathF.Sin(value.X);
            Roll.M32 = MathF.Sin(value.X);
            Roll.M33 = MathF.Cos(value.X);
            matrix *= Roll;

            return matrix;
        }

        // Method to compute the inverse of the matrix
        public static Matrix4x4 Invert(Matrix4x4 m)
        {
            Matrix4x4 inv = new Matrix4x4();

            // Inverse of the rotation part (transpose of the 3x3 top-left submatrix)
            inv.M11 = m.M11;
            inv.M12 = m.M21;
            inv.M13 = m.M31;
            inv.M21 = m.M12;
            inv.M22 = m.M22;
            inv.M23 = m.M32;
            inv.M31 = m.M13;
            inv.M32 = m.M23;
            inv.M33 = m.M33;

            // Inverse of the translation part
            inv.M14 = -(inv.M11 * m.M14 + inv.M12 * m.M24 + inv.M13 * m.M34);
            inv.M24 = -(inv.M21 * m.M14 + inv.M22 * m.M24 + inv.M23 * m.M34);
            inv.M34 = -(inv.M31 * m.M14 + inv.M32 * m.M24 + inv.M33 * m.M34);

            // Last row remains the same
            inv.M41 = 0f;
            inv.M42 = 0f;
            inv.M43 = 0f;
            inv.M44 = 1f;

            return inv;
        }

        static XmlElement CreateAndAppendElement(XmlDocument doc, XmlElement parent, string name)
        {
            XmlElement element = doc.CreateElement(name);
            parent.AppendChild(element);
            return element;
        }

        static void AddInstanceMaterialElement(XmlDocument doc, XmlElement parent, string material)
        {
            XmlElement instance_material = CreateAndAppendElement(doc, parent, "instance_material");
            instance_material.SetAttribute("symbol", $"{material}");
            instance_material.SetAttribute("target", $"#{material}");
            XmlElement bind_vertex_input = CreateAndAppendElement(doc, instance_material, "bind_vertex_input");
            bind_vertex_input.SetAttribute("semantic", "UVMap");
            bind_vertex_input.SetAttribute("input_semantic", "TEXCOORD");
            bind_vertex_input.SetAttribute("input_set", "0");
        }


        static void AddSourceElement(XmlDocument doc, XmlElement parent, string id, string array_type, List<float[]> data, string param_name)
        {
            XmlElement source = CreateAndAppendElement(doc, parent, "source");
            source.SetAttribute("id", id);
            XmlElement float_array = CreateAndAppendElement(doc, source, array_type);
            float_array.SetAttribute("id", $"{id}-array");
            float_array.SetAttribute("count", (data.Count * data[0].Length).ToString());
            float_array.InnerText = string.Join(" ", Flatten2(data));
            XmlElement technique_common = CreateAndAppendElement(doc, source, "technique_common");
            XmlElement accessor = CreateAndAppendElement(doc, technique_common, "accessor");
            accessor.SetAttribute("source", $"#{id}-array");
            accessor.SetAttribute("count", data.Count.ToString());
            accessor.SetAttribute("stride", data[0].Length.ToString());

            for (int i = 0; i < data[0].Length; i++)
            {
                XmlElement param = CreateAndAppendElement(doc, accessor, "param");
                param.SetAttribute("name", param_name.ToUpper() + (i + 1).ToString());
                param.SetAttribute("type", "float");
            }
        }

        static void AddImageElement(XmlDocument doc, XmlElement parent, string id, string file_path)
        {
            XmlElement image = CreateAndAppendElement(doc, parent, "image");
            image.SetAttribute("id", id);
            image.SetAttribute("name", id);
            XmlElement initFrom = CreateAndAppendElement(doc, image, "init_from");
            initFrom.InnerText = file_path;
        }

        static void AddSurfaceElement(XmlDocument doc, XmlElement parent, string sid, string sourceId)
        {
            XmlElement newparam = CreateAndAppendElement(doc, parent, "newparam");
            newparam.SetAttribute("sid", sid);
            XmlElement surface = CreateAndAppendElement(doc, newparam, "surface");
            surface.SetAttribute("type", "2D");
            XmlElement source = CreateAndAppendElement(doc, surface, "init_from");
            source.InnerText = sourceId;
        }

        static void AddSamplerElement(XmlDocument doc, XmlElement parent, string sid, string source_id)
        {
            XmlElement newparam = CreateAndAppendElement(doc, parent, "newparam");
            newparam.SetAttribute("sid", sid);
            XmlElement sampler2D = CreateAndAppendElement(doc, newparam, "sampler2D");
            XmlElement source = CreateAndAppendElement(doc, sampler2D, "source");
            source.InnerText = source_id;
        }

        static void AddTextureElement(XmlDocument doc, XmlElement parent, string texture_id, string texcoord)
        {
            XmlElement texture = CreateAndAppendElement(doc, parent, "texture");
            texture.SetAttribute("texture", $"{texture_id}-sampler");
            texture.SetAttribute("texcoord", texcoord);
        }

        static void AddColorElement(XmlDocument doc, XmlElement parent, string tag, float[] color)
        {
            XmlElement color_element = CreateAndAppendElement(doc, parent, tag);
            XmlElement color_value = CreateAndAppendElement(doc, color_element, "color");
            color_value.InnerText = string.Join(" ", color);
        }

        static void AddInputElement(XmlDocument doc, XmlElement parent, string semantic, string source, string offset = null)
        {
            XmlElement input = CreateAndAppendElement(doc, parent, "input");
            input.SetAttribute("semantic", semantic);
            input.SetAttribute("source", source);
            if (offset != null)
            {
                input.SetAttribute("offset", offset);
            }
        }
        static void AddSkinSourceElement(XmlDocument doc, XmlElement parent, string id, string param_name, string param_type, string array_name, int length, string[] array, int stride)
        {
            XmlElement source = CreateAndAppendElement(doc, parent, "source");
            source.SetAttribute("id", $"{id}");
            XmlElement name_array = CreateAndAppendElement(doc, source, $"{array_name}");
            name_array.SetAttribute("id", $"{id}-array");
            name_array.SetAttribute("count", $"{length}");
            name_array.InnerText = string.Join(" ", array);
            XmlElement technique_common_bone = CreateAndAppendElement(doc, source, "technique_common");
            XmlElement accessor = CreateAndAppendElement(doc, technique_common_bone, "accessor");
            accessor.SetAttribute("source", $"#{id}-array");
            accessor.SetAttribute("count", array.Length.ToString());
            accessor.SetAttribute("stride", $"{stride}");
            XmlElement param = CreateAndAppendElement(doc, accessor, "param");
            param.SetAttribute("name", $"{param_name}");
            param.SetAttribute("type", $"{param_type}");
        }

        static string Flatten2(List<float[]> data)
        {
            List<string> result = new List<string>();
            foreach (var array in data)
            {
                result.AddRange(Array.ConvertAll(array, item => item.ToString()));
            }
            return string.Join(" ", result);
        }

        static string Flatten(List<Tuple<ushort, ushort, ushort>> faces)
        {
            List<string> result = new List<string>();
            foreach (var face in faces)
            {
                result.Add(face.Item1.ToString());
                result.Add(face.Item1.ToString());
                result.Add(face.Item2.ToString());
                result.Add(face.Item2.ToString());
                result.Add(face.Item3.ToString());
                result.Add(face.Item3.ToString());
            }
            return string.Join(" ", result);
        }
    }
}