using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;
using System.Linq;

namespace VoxelEngine{
    public class ModelContainer : MonoBehaviour
    {
        public static ModelContainer instance;

        private Dictionary<string, Voxel.ThreadableMesh> meshes = new Dictionary<string, Voxel.ThreadableMesh>();

        private void Awake() {
            instance = this;
        }

        private void Start(){
            foreach (string s in Directory.GetFiles(@"Packs\Models", "*.obj")){
                try
                {
                    List<Vector3> verts = new List<Vector3>();
                    List<Vector2> uvs = new List<Vector2>();
                    Vector2[] indexedUvs = null;
                    List<Vector3> normals = new List<Vector3>();
                    Vector3[] indexedNormals = null;
                    List<int> indices = new List<int>();
                    FileStream stream = File.OpenRead(s);
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        while (sr.Peek() > -1)
                        {
                            string[] splitLine = sr.ReadLine().Split(' ');
                            switch (splitLine[0])
                            {
                                case "v":
                                    verts.Add(new Vector3(Convert.ToSingle(splitLine[1], CultureInfo.InvariantCulture),
                                                        Convert.ToSingle(splitLine[2], CultureInfo.InvariantCulture),
                                                        Convert.ToSingle(splitLine[3], CultureInfo.InvariantCulture)));
                                    break;
                                case "vt":
                                    uvs.Add(new Vector2(Convert.ToSingle(splitLine[1], CultureInfo.InvariantCulture),
                                                        Convert.ToSingle(splitLine[2], CultureInfo.InvariantCulture)));
                                    break;
                                case "vn":
                                    normals.Add(new Vector3(Convert.ToSingle(splitLine[1], CultureInfo.InvariantCulture),
                                                        Convert.ToSingle(splitLine[2], CultureInfo.InvariantCulture),
                                                        Convert.ToSingle(splitLine[3], CultureInfo.InvariantCulture)));
                                    break;
                                //This has to happen last! Make sure the f section is the last one in the .obj file!
                                case "f":
                                    if(indexedUvs == null)
                                        indexedUvs = new Vector2[verts.Count];
                                    if(indexedNormals == null)
                                        indexedNormals = new Vector3[verts.Count];
                                    for (int i = 1; i <= 3; i++)
                                    {
                                        string[] splitIndexSet = splitLine[i].Split('/');
                                        int vertIdx = Convert.ToInt32(splitIndexSet[0]) - 1;
                                        int uvIdx = Convert.ToInt32(splitIndexSet[1]) - 1;
                                        int normalIdx = Convert.ToInt32(splitIndexSet[2]) - 1;
                                        indices.Add(vertIdx);
                                        indexedUvs[vertIdx] = uvs[uvIdx];
                                        indexedNormals[vertIdx] = normals[normalIdx];
                                    }
                                    break;
                            }
                        }
                    }
                    meshes.Add(s.Replace(@"Packs\Models\", string.Empty), new Voxel.ThreadableMesh(verts,
                                                                                                    indices,
                                                                                                    new List<Vector2>(indexedUvs),
                                                                                                    new List<Vector3>(indexedNormals)));
                }
                catch(Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        public Voxel.ThreadableMesh GetModel(string modelID){
            if(meshes.TryGetValue(modelID, out Voxel.ThreadableMesh mesh))
                return mesh;
            return null;
        }
    }
}