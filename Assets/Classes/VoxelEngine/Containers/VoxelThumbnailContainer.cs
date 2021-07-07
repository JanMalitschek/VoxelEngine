using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace VoxelEngine{
    public class VoxelThumbnailContainer : MonoBehaviour
    {
        public Camera thumbnailRenderer;
        public RenderTexture thumbnailRenderTexture;
        public static Dictionary<int, Texture2D> thumbnails = new Dictionary<int, Texture2D>();

        private void Start() {
            foreach(Voxel v in VoxelContainer.container.Values)
                CreateThumbnail(v);
        }

        public static Mesh GeneratePreviewMesh(Voxel v){
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            if(v.hasCustomModel)
                GenerateCustomModel(v.customModel, ref vertices, ref indices, ref uvs, v.partSides, ref normals);
            else{
                GenerateFace(Vector3.forward, Vector3.right, Vector3.up, ref vertices, ref indices, ref uvs, v.partSides, ref normals);
                GenerateFace(-Vector3.forward, -Vector3.right, Vector3.up, ref vertices, ref indices, ref uvs, v.partSides, ref normals);
                GenerateFace(Vector3.right, -Vector3.forward, Vector3.up, ref vertices, ref indices, ref uvs, v.partSides, ref normals);
                GenerateFace(-Vector3.right, Vector3.forward, Vector3.up, ref vertices, ref indices, ref uvs, v.partSides, ref normals);
                GenerateFace(Vector3.up, Vector3.right, -Vector3.forward, ref vertices, ref indices, ref uvs, v.partTop, ref normals);
                GenerateFace(-Vector3.up, Vector3.right, Vector3.forward, ref vertices, ref indices, ref uvs, v.partBottom, ref normals);
            }
            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.RecalculateBounds();
            return mesh;
        }

        public void CreateThumbnail(Voxel v){
            //Construct Voxel Mesh
            Mesh mesh = GeneratePreviewMesh(v);
            //Position Camera
            float camDistance = mesh.bounds.size.magnitude;
            thumbnailRenderer.transform.position = -thumbnailRenderer.transform.forward * (camDistance + 2.0f);
            thumbnailRenderer.orthographicSize = camDistance * 0.5f;
            //Render Thumbnail
            Graphics.DrawMesh(mesh, Matrix4x4.Translate(-mesh.bounds.center), VoxelContainer.globalDefaultChunkMaterial, 9, thumbnailRenderer, 0);
            thumbnailRenderer.Render();
            //Copy Texture
            Texture2D thumbnail = new Texture2D(thumbnailRenderTexture.width, thumbnailRenderTexture.height, GraphicsFormat.R8G8B8A8_UNorm, TextureCreationFlags.None);
            RenderTexture.active = thumbnailRenderTexture;
            thumbnail.ReadPixels(new Rect(0, 0, thumbnail.width, thumbnail.height), 0, 0);
            thumbnail.Apply();
            print(v.nameHash);
            thumbnails.Add(v.nameHash, thumbnail);
        }
        private static void GenerateFace(Vector3 forward,
                                         Vector3 right,
                                         Vector3 up,
                                         ref List<Vector3> vertices,
                                         ref List<int> indices,
                                         ref List<Vector2> uvs,
                                         TextureAtlas.Part part,
                                         ref List<Vector3> normals){
            normals.Add(forward);
            normals.Add(forward);
            normals.Add(forward);
            normals.Add(forward);

            forward *= 0.5f;
            right *= 0.5f;
            up *= 0.5f;

            int currentIndex = vertices.Count;
            vertices.Add(forward + right - up);
            vertices.Add(forward + right + up);
            vertices.Add(forward - right + up);
            vertices.Add(forward - right - up);

            uvs.Add(part.uvs[0]);
            uvs.Add(part.uvs[1]);
            uvs.Add(part.uvs[2]);
            uvs.Add(part.uvs[3]);

            indices.Add(currentIndex);
            indices.Add(currentIndex + 1);
            indices.Add(currentIndex + 2);
            indices.Add(currentIndex);
            indices.Add(currentIndex + 2);
            indices.Add(currentIndex + 3);
        }
        private static void GenerateCustomModel(Voxel.ThreadableMesh customModel,
                                                ref List<Vector3> vertices,
                                                ref List<int> indices,
                                                ref List<Vector2> uvs,
                                                TextureAtlas.Part part,
                                                ref List<Vector3> normals){
            if(customModel == null) return;

            Vector3 center = new Vector3(0.5f, 0.0f, 0.5f);

            int currentIndex = vertices.Count;

            List<Vector3> customVertices = customModel.vertices;
            foreach(Vector3 v in customVertices){
                Vector4 v4 = new Vector4(v.x, v.y, v.z, 1.0f);
                vertices.Add(center + new Vector3(v4.x, v4.y, v4.z));
            }

            List<int> customIndices = customModel.indices;
            foreach(int i in customIndices)
                indices.Add(currentIndex + i);

            List<Vector2> customUVs = customModel.uvs;
            foreach(Vector2 cuv in customUVs)
                uvs.Add(new Vector2(part.uvs[0].x + cuv.x * part.UVWidth, part.uvs[0].y + cuv.y * part.UVHeight));

            List<Vector3> customNormals = customModel.normals;
            normals.AddRange(customNormals);
        }
    }
}