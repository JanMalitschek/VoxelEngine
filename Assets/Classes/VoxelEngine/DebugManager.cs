using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelEngine{
    public class DebugManager : MonoBehaviour
    {
        public static DebugManager instance;
        private static Transform player;

        [Header("Visualizations")]
        public bool showRegions;
        public bool showChunks;
        public Material debugMaterial;

        private void Awake() {
            instance = this;
            player = FindObjectOfType<PlayerController>().transform;
        }

        private void Start() {
            Camera.onPostRender += OnPostRenderCallback;
        }

        private void OnPostRenderCallback(Camera cam) {
                print("Drawing Regions");
            if(showRegions){
                Vector3 currentRegion = Conversions.WorldToRegionPosition(player.position) * 64;
                DrawWireBox(currentRegion, Vector3.one * 64.0f);
            }
            if(showChunks){
                Vector3 currentChunk = Conversions.WorldToChunkPosition(player.position) * 16;
                DrawWireBox(currentChunk, Vector3.one * 16.0f);
            }
        }

        public static void DrawWireBox(Vector3 origin, Vector3 dimensions, float inflation = 0.0f){
            origin -= Vector3.one * inflation;
            dimensions += Vector3.one * inflation * 2.0f;
            GL.PushMatrix();
            instance.debugMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Color.black);

            //Bottom
            GL.Vertex(origin);
            GL.Vertex(origin + Vector3.right * dimensions.x);
            GL.Vertex(origin);
            GL.Vertex(origin + Vector3.forward * dimensions.z);
            GL.Vertex(origin + new Vector3(dimensions.x, 0.0f, dimensions.z));
            GL.Vertex(origin + Vector3.right * dimensions.x);
            GL.Vertex(origin + new Vector3(dimensions.x, 0.0f, dimensions.z));
            GL.Vertex(origin + Vector3.forward * dimensions.z);

            //Top
            GL.Vertex(origin + Vector3.up * dimensions.y);
            GL.Vertex(origin + new Vector3(dimensions.x, dimensions.y, 0.0f));
            GL.Vertex(origin + Vector3.up * dimensions.y);
            GL.Vertex(origin + new Vector3(0.0f, dimensions.y, dimensions.z));
            GL.Vertex(origin + dimensions);
            GL.Vertex(origin + new Vector3(dimensions.x, dimensions.y, 0.0f));
            GL.Vertex(origin + dimensions);
            GL.Vertex(origin + new Vector3(0.0f, dimensions.y, dimensions.z));

            //Sides
            GL.Vertex(origin);
            GL.Vertex(origin + Vector3.up * dimensions.y);
            GL.Vertex(origin + Vector3.right * dimensions.x);
            GL.Vertex(origin + new Vector3(dimensions.x, dimensions.y, 0.0f));
            GL.Vertex(origin + Vector3.forward * dimensions.z);
            GL.Vertex(origin + new Vector3(0.0f, dimensions.y, dimensions.z));
            GL.Vertex(origin + new Vector3(dimensions.x, 0.0f, dimensions.z));
            GL.Vertex(origin + dimensions);

            GL.End();
            GL.PopMatrix();
        }
    }
}