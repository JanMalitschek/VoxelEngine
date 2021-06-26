using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelEngine{
    public class Voxel {
        public string VoxelName{
            get{
                return voxelName;
            }
            set{
                voxelName = value;
                nameHash = value.GetHashCode();
            }
        }
        private string voxelName;
        public int nameHash;

        //Obsolete
        public Vector2 faceUVWidth;
        public Sprite topFace;
        public Vector2 topFaceUV;
        public Sprite sideFace;
        public Vector2 sideFaceUV;
        public Sprite bottomFace;
        public Vector2 bottomFaceUV;
        //New
        public TextureAtlas.Part partTop, partSides, partBottom;

        public bool isTransparent = false;
        public bool maintainInnerFaces = false;
        public float illuminationLevel = 0.0f;

        public bool hasCustomModel = false;
        [System.Serializable]
        public class ThreadableMesh{
            public List<Vector3> vertices;
            public List<int> indices;
            public List<Vector2> uvs;
            public List<Vector3> normals;
            public ThreadableMesh(Mesh mesh){
                vertices = new List<Vector3>();
                indices = new List<int>();
                uvs = new List<Vector2>();
                normals = new List<Vector3>();
                mesh.GetVertices(vertices);
                indices = new List<int>(mesh.GetIndices(0));
                mesh.GetUVs(0, uvs);
                mesh.GetNormals(normals);
            }
            public ThreadableMesh(List<Vector3> vertices, List<int> indices, List<Vector2> uvs, List<Vector3> normals){
                this.vertices = vertices;
                this.indices = indices;
                this.uvs = uvs;
                this.normals = normals;
            }
        }
        public ThreadableMesh customModel;
        public float minRandomScale = 1.0f;
        public float maxRandomScale = 1.0f;
        public bool randomRotation = false;
        public float randomOffsetX = 0.0f;
        public float randomOffsetY = 0.0f;
        public float randomOffsetZ = 0.0f;
    }
}