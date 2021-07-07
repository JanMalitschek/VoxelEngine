using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

namespace VoxelEngine{
    public class ExportManager : MonoBehaviour
    {
        public static ExportManager instance;

        private void Awake() {
            instance = this;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }

        private static void DisassembleMesh(Mesh mesh,
                                            out List<Vector3> vertices,
                                            out List<int> indices,
                                            out List<Vector2> uvs,
                                            out List<Vector3> normals,
                                            out List<Color> colors){
            vertices = new List<Vector3>();
            mesh.GetVertices(vertices);
            indices = new List<int>(mesh.GetIndices(0));
            uvs = new List<Vector2>();
            mesh.GetUVs(0, uvs);
            normals = new List<Vector3>();
            mesh.GetNormals(normals);
            colors = new List<Color>();
            mesh.GetColors(colors);
        }
        public static void ExportMesh(Mesh mesh, string name){
            DisassembleMesh(mesh, out List<Vector3> vertices, out List<int> indices, out List<Vector2> uvs, out List<Vector3> normals, out List<Color> colors);
            MeshToCollada(name, vertices, indices, uvs, normals, colors);
        }
        private static string DataToString(List<Vector3> data){
            string result = string.Empty;
            foreach(Vector3 v in data)
                result += string.Format(CultureInfo.InvariantCulture, " {0} {1} {2}", v.x, v.y, v.z);
            result.Remove(0);
            return result;
        }
        private static string DataToString(List<Vector2> data){
            string result = string.Empty;
            foreach(Vector3 v in data)
                result += string.Format(CultureInfo.InvariantCulture, " {0} {1}", v.x, v.y);
            result.Remove(0);
            return result;
        }
        private static string DataToString(List<Color> data){
            string result = string.Empty;
            foreach(Color c in data)
                result += string.Format(CultureInfo.InvariantCulture, " {0} {1} {2} {3}", c.r, c.g, c.b, c.a);
            result.Remove(0);
            return result;
        }
        private static string IndicesToString(List<int> indices){
            string result = string.Empty;
            foreach(int i in indices){
                result += $" {i} {i} {i} {i}";
            }
            result.Remove(0);
            return result;
        }
        public static void MeshToCollada(string name,
                                    List<Vector3> vertices,
                                    List<int> indices,
                                    List<Vector2> uvs,
                                    List<Vector3> normals,
                                    List<Color> colors){
            if(!Directory.Exists("./Exports"))
                Directory.CreateDirectory("./Exports");
            List<string> lines = new List<string>();

            //Header Stuff
            lines.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            lines.Add("<COLLADA xmlns=\"http://www.collada.org/2005/11/COLLADASchema\" version=\"1.4.1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            //Asset Info
            lines.Add("\t<asset>");
            lines.Add("\t\t<unit name=\"meter\" meter=\"1.0\"/>");
            lines.Add("\t\t<up_axis>Y_UP</up_axis>");
            lines.Add("\t</asset>");
            //Textures; we have none so leave this empty
            lines.Add("\t<library_images/>");
            //Geometry
            lines.Add("\t<library_geometries>");
            lines.Add($"\t\t<geometry id=\"{name}-mesh\" name=\"{name}\">");
            lines.Add("\t\t\t<mesh>");
            //Vertices
            lines.Add($"\t\t\t\t<source id=\"{name}-mesh-positions\">");
            lines.Add($"\t\t\t\t\t<float_array id=\"{name}-mesh-positions-array\" count=\"{vertices.Count}\">{DataToString(vertices)}</float_array>");
            lines.Add("\t\t\t\t\t<technique_common>");
            lines.Add($"\t\t\t\t\t\t<accessor source=\"#{name}-mesh-positions-array\" count=\"{vertices.Count / 3}\" stride=\"3\">");
            lines.Add("\t\t\t\t\t\t\t<param name=\"X\" type=\"float\"/>");
            lines.Add("\t\t\t\t\t\t\t<param name=\"Y\" type=\"float\"/>");
            lines.Add("\t\t\t\t\t\t\t<param name=\"Z\" type=\"float\"/>");
            lines.Add("\t\t\t\t\t\t</accessor>");
            lines.Add("\t\t\t\t\t</technique_common>");
            lines.Add($"\t\t\t\t</source>");
            //Normals
            lines.Add($"\t\t\t\t<source id=\"{name}-mesh-normals\">");
            lines.Add($"\t\t\t\t\t<float_array id=\"{name}-mesh-normals-array\" count=\"{normals.Count}\">{DataToString(normals)}</float_array>");
            lines.Add("\t\t\t\t\t<technique_common>");
            lines.Add($"\t\t\t\t\t\t<accessor source=\"#{name}-mesh-normals-array\" count=\"{normals.Count / 3}\" stride=\"3\">");
            lines.Add("\t\t\t\t\t\t\t<param name=\"X\" type=\"float\"/>");
            lines.Add("\t\t\t\t\t\t\t<param name=\"Y\" type=\"float\"/>");
            lines.Add("\t\t\t\t\t\t\t<param name=\"Z\" type=\"float\"/>");
            lines.Add("\t\t\t\t\t\t</accessor>");
            lines.Add("\t\t\t\t\t</technique_common>");
            lines.Add($"\t\t\t\t</source>");
            //UVs
            lines.Add($"\t\t\t\t<source id=\"{name}-mesh-map-0\">");
            lines.Add($"\t\t\t\t\t<float_array id=\"{name}-mesh-map-0-array\" count=\"{uvs.Count}\">{DataToString(uvs)}</float_array>");
            lines.Add("\t\t\t\t\t<technique_common>");
            lines.Add($"\t\t\t\t\t\t<accessor source=\"#{name}-mesh-map-0-array\" count=\"{uvs.Count / 2}\" stride=\"2\">");
            lines.Add("\t\t\t\t\t\t\t<param name=\"S\" type=\"float\"/>");
            lines.Add("\t\t\t\t\t\t\t<param name=\"T\" type=\"float\"/>");
            lines.Add("\t\t\t\t\t\t</accessor>");
            lines.Add("\t\t\t\t\t</technique_common>");
            lines.Add($"\t\t\t\t</source>");
            //Colors
            lines.Add($"\t\t\t\t<source id=\"{name}-mesh-colors-Col\" name=\"Col\">");
            lines.Add($"\t\t\t\t\t<float_array id=\"{name}-mesh-colors-Col-array\" count=\"{colors.Count}\">{DataToString(colors)}</float_array>");
            lines.Add("\t\t\t\t\t<technique_common>");
            lines.Add($"\t\t\t\t\t\t<accessor source=\"#{name}-mesh-color-Col-array\" count=\"{colors.Count / 4}\" stride=\"4\">");
            lines.Add("\t\t\t\t\t\t\t<param name=\"R\" type=\"float\"/>");
            lines.Add("\t\t\t\t\t\t\t<param name=\"G\" type=\"float\"/>");
            lines.Add("\t\t\t\t\t\t\t<param name=\"B\" type=\"float\"/>");
            lines.Add("\t\t\t\t\t\t\t<param name=\"A\" type=\"float\"/>");
            lines.Add("\t\t\t\t\t\t</accessor>");
            lines.Add("\t\t\t\t\t</technique_common>");
            lines.Add($"\t\t\t\t</source>");
            //Vertex Source
            lines.Add($"\t\t\t\t<vertices id=\"{name}-mesh-vertices\">");
            lines.Add($"\t\t\t\t\t<input semantic=\"POSITION\" source=\"#{name}-mesh-positions\"/>");
            lines.Add("\t\t\t\t</vertices>");
            //Indices
            lines.Add($"\t\t\t\t<triangles count=\"{indices.Count / 3}\">");
            lines.Add($"\t\t\t\t\t<input semantic=\"VERTEX\" source=\"#{name}-mesh-vertices\" offset=\"0\"/>");
            lines.Add($"\t\t\t\t\t<input semantic=\"NORMAL\" source=\"#{name}-mesh-normals\" offset=\"1\"/>");
            lines.Add($"\t\t\t\t\t<input semantic=\"TEXCOORD\" source=\"#{name}-mesh-map-0\" offset=\"2\" set=\"0\"/>");
            lines.Add($"\t\t\t\t\t<input semantic=\"COLOR\" source=\"#{name}-mesh-colors-Col\" offset=\"3\" set=\"0\"/>");
            lines.Add($"\t\t\t\t\t<p>{IndicesToString(indices)}</p>");
            lines.Add("\t\t\t\t</triangles>");
            //End Geometry
            lines.Add("\t\t\t</mesh>");
            lines.Add("\t\t</geometry>");
            lines.Add("\t</library_geometries>");
            //Scene
            lines.Add("\t<library_visual_scenes>");
            lines.Add("\t\t<visual_scene id=\"Scene\" name=\"Scene\">");
            lines.Add("\t\t\t<node id=\"Plane\" name=\"Plane\" type=\"NODE\">");
            lines.Add("\t\t\t\t<matrix sid=\"transform\">1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1</matrix>");
            lines.Add($"\t\t\t\t<instance_geometry url=\"#{name}-mesh\" name=\"{name}\"/>");
            lines.Add("\t\t\t</node>");
            lines.Add("\t\t</visual_scene>");
            lines.Add("\t</library_visual_scenes>");
            lines.Add("\t<scene>");
            lines.Add("\t\t<instance_visual_scene url=\"#Scene\"/>");
            lines.Add("\t</scene>");

            lines.Add("</COLLADA>");

            File.WriteAllLines($"./Exports/{name}.dae", lines);
            print($"Exported Mesh to {name}");
        }
        public static void ExportTextureAtlas(){
            byte[] bytes = TextureContainer.instance.texture.EncodeToPNG();
            if(!Directory.Exists("./Exports"))
                Directory.CreateDirectory("./Exports");
            using(FileStream fs = File.Open($"Exports/Atlas.png", FileMode.OpenOrCreate)){
                fs.Write(bytes, 0, bytes.Length);
            }
        }
    }
}