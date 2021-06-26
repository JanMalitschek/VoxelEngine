using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace VoxelEngine{
    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
    public class Chunk : MonoBehaviour
    {
        public struct ChunkVoxel{
            public int voxelHash;
            public float illuminationLevel;
        }
        public ChunkVoxel[,,] chunkData = new ChunkVoxel[16,16,16];
        private MeshFilter meshFilter;
        private Initializer initializer;
        public Vector3 worldPosition;
        private World world;
        
        [Header("Neighboring Chunks")]
        public Chunk rightChunk;
        public Chunk leftChunk;
        public Chunk topChunk;
        public Chunk bottomChunk;
        public Chunk frontChunk;
        public Chunk backChunk;

        [HideInInspector] public List<Vector3> vertices = new List<Vector3>();
        [HideInInspector] public List<int> indices = new List<int>();
        [HideInInspector] public List<Vector2> uvs = new List<Vector2>();
        [HideInInspector] public List<Vector3> normals = new List<Vector3>();
        [HideInInspector] public List<Color> colors = new List<Color>();
        private bool finishedUpdating = false;

        [HideInInspector] public static Unity.Mathematics.Random random = new Unity.Mathematics.Random(1234);

        public float lightLevelLoss = 0.3f;
        public enum LightMode{
            Flat,
            Smooth
        }
        public LightMode lightMode = LightMode.Smooth;

        public enum State{
            Initializing,
            DataOnly,
            GeneratingMesh,
            Visual,
            PendingUpdate
        }
        public State state = State.Initializing;

        private void Awake() {
            meshFilter = GetComponent<MeshFilter>();
            initializer = new FlatlandInitializer();
            world = FindObjectOfType<World>();
        }

        private void Update() {
            switch(state){
                case State.Initializing:

                break;
                case State.DataOnly:
                    
                break;
                case State.GeneratingMesh:

                break;
                case State.Visual:
                    if(finishedUpdating){
                        finishedUpdating = false;
                        Mesh m = new Mesh();
                        m.SetVertices(vertices);
                        m.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
                        m.SetUVs(0, uvs);
                        m.SetNormals(normals);
                        m.SetColors(colors);
                        m.RecalculateBounds();
                        meshFilter.mesh = m;
                    }
                break;
                case State.PendingUpdate:

                break;
            }

            // if(isUpdating && finishedUpdating){
            //     isUpdating = false;
            //     finishedUpdating = false;
            //     Mesh m = new Mesh();
            //     m.SetVertices(vertices);
            //     m.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
            //     m.SetUVs(0, uvs);
            //     m.SetNormals(normals);
            //     m.SetColors(colors);
            //     m.RecalculateBounds();
            //     meshFilter.mesh = m;
            // }
        }

        public async Task InitChunk(){
            await Task.Run(() => Init(this));
            state = State.DataOnly;
        }
        // private void InitCunkParallel(object stateInfo){
        //     Chunk c = (Chunk)stateInfo;
        //     ChunkMeshGeneration.GenerateMesh(c);
        //     c.finishedUpdating = true;
        // }
        public async Task UpdateChunk(){
            await UpdateChunkParallel(this);
        }
        private async Task UpdateChunkParallel(Chunk c){
            FindSurroundings();
            //Wait for all surroundings to be at least in DataOnly
            await Task.Run(() => {
                while(!AreSurroundingsDataOnly())
                    Task.Delay(25);
            });
            state = State.GeneratingMesh;
            c.vertices.Clear();
            c.indices.Clear();
            c.uvs.Clear();
            c.normals.Clear();
            c.colors.Clear();
            await Task.Run(() => ChunkIllumination.InitializeLighting(c));
            await Task.Run(() => ChunkIllumination.PropagateLighting(c));
            await Task.Run(() => ChunkMeshGeneration.GenerateMesh(c));
            c.finishedUpdating = true;
            state = State.Visual;
        }

        public static void Init(Chunk c){
            for(int x = 0; x < 16; x++)
                for(int y = 0; y < 16; y++)
                    for(int z = 0; z < 16; z++)
                        c.chunkData[x,y,z] = new ChunkVoxel{voxelHash = 0, illuminationLevel = 0.0f};
            c.initializer.Initialize(c);
            // ChunkIllumination.InitializeLighting(c);
            // ChunkIllumination.PropagateLighting(c);
        }

        private void FindSurroundings(){
            Vector3Int chunkSpacePos = Conversions.WorldToChunkPosition(worldPosition);
            rightChunk = world.RequestChunk(chunkSpacePos + new Vector3Int(1, 0, 0));
            leftChunk = world.RequestChunk(chunkSpacePos + new Vector3Int(-1, 0, 0));
            topChunk = world.RequestChunk(chunkSpacePos + new Vector3Int(0, 1, 0));
            bottomChunk = world.RequestChunk(chunkSpacePos + new Vector3Int(0, -1, 0));
            frontChunk = world.RequestChunk(chunkSpacePos + new Vector3Int(0, 0, 1));
            backChunk = world.RequestChunk(chunkSpacePos + new Vector3Int(0, 0, -1));
        }

        private bool AreSurroundingsDataOnly(){
            if(rightChunk == null || rightChunk.state < State.DataOnly) return false;
            if(leftChunk == null || leftChunk.state < State.DataOnly) return false;
            if(topChunk == null || topChunk.state < State.DataOnly) return false;
            if(bottomChunk == null || bottomChunk.state < State.DataOnly) return false;
            if(frontChunk == null || frontChunk.state < State.DataOnly) return false;
            if(backChunk == null || backChunk.state < State.DataOnly) return false;
            return true;
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.white;
            Gizmos.matrix = Matrix4x4.Translate(transform.position);
            Gizmos.DrawWireCube(Vector3.one * 8.0f, Vector3.one * 16.0f);
        }

        #region Chunk Access
        public static bool AreCoordinatesInBounds(int x, int y, int z){
            return x >= 0 && x < 16 && y >= 0 && y < 16 && z >= 0 && z < 16;
        }
        public static void ClampCoordinates(ref int x, ref int y, ref int z){
            x = Mathf.Clamp(x, 0, 16);
            y = Mathf.Clamp(y, 0, 16);
            z = Mathf.Clamp(z, 0, 16);
        }
        public int GetVoxelHashAtPosition(int x, int y, int z){
            if(AreCoordinatesInBounds(x, y, z))
                return chunkData[x,y,z].voxelHash;
            else{
                if(x >= 16) return rightChunk.GetVoxelHashAtPosition(x - 16, y, z);
                if(x < 0) return leftChunk.GetVoxelHashAtPosition(x + 16, y, z);
                if(y >= 16) return topChunk.GetVoxelHashAtPosition(x, y - 16, z);
                if(y < 0) return bottomChunk.GetVoxelHashAtPosition(x, y + 16, z);
                if(z >= 16) return frontChunk.GetVoxelHashAtPosition(x, y, z - 16);
                if(z < 0) return backChunk.GetVoxelHashAtPosition(x, y, z + 16);
            }
            return 0;
        }
        public static void SetVoxel(Chunk c, int x, int y, int z, Voxel v){
            ClampCoordinates(ref x, ref y, ref z);
            if(v == null){
                c.chunkData[x,y,z].voxelHash = 0;
                c.chunkData[x,y,z].illuminationLevel = 0;
                UpdateNeighboringChunksIfNecessary(c, x, y, z);    
                return;
            }
            c.chunkData[x,y,z].voxelHash = v.nameHash;
            c.chunkData[x,y,z].illuminationLevel = v.illuminationLevel;
            UpdateNeighboringChunksIfNecessary(c, x, y, z);
        }
        public static void SetVoxelSafe(Chunk c, int x, int y, int z, Voxel v){
            ClampCoordinates(ref x, ref y, ref z);
            if(v == null){
                c.chunkData[x,y,z].voxelHash = 0;
                c.chunkData[x,y,z].illuminationLevel = 0;
                UpdateNeighboringChunksIfNecessary(c, x, y, z);
                return;
            }
            else if(c.chunkData[x,y,z].voxelHash == 0){
                c.chunkData[x,y,z].voxelHash = v.nameHash;
                c.chunkData[x,y,z].illuminationLevel = v.illuminationLevel;
                UpdateNeighboringChunksIfNecessary(c, x, y, z);
            }
        }
        public static void UpdateNeighboringChunksIfNecessary(Chunk c, int x, int y, int z){
            if(x <= 0 && c.leftChunk.state >= State.GeneratingMesh) c.leftChunk.UpdateChunk();
            else if(x >= 15 && c.rightChunk.state >= State.GeneratingMesh) c.rightChunk.UpdateChunk();
            if(y <= 0 && c.bottomChunk.state >= State.GeneratingMesh) c.bottomChunk.UpdateChunk();
            else if(y >= 15 && c.topChunk.state >= State.GeneratingMesh) c.topChunk.UpdateChunk();
            if(z <= 0 && c.backChunk.state >= State.GeneratingMesh) c.backChunk.UpdateChunk();
            else if(z >= 15 && c.frontChunk.state >= State.GeneratingMesh) c.frontChunk.UpdateChunk();
        }
        private static void Swap(ref int a, ref int b){
            int c = a;
            b = a;
            a = c;
        }
        public static void FillVoxels(Chunk c, int x0, int y0, int z0, int x1, int y1, int z1, params Voxel[] vs){
            if(x0 > x1)
                Swap(ref x0, ref x1);
            if(y0 > y1)
                Swap(ref y0, ref y1);
            if(z0 > z1)
                Swap(ref z0, ref z1);
            ClampCoordinates(ref x0, ref y0, ref z0);
            ClampCoordinates(ref x1, ref y1, ref z1);
            for(int x = x0; x <= x1; x++)
                for(int y = y0; y <= y1; y++)
                    for(int z = z0; z <= z1; z++){
                        Voxel choice = vs[random.NextInt(0, vs.Length)];
                        if(choice != null){
                            c.chunkData[x,y,z].voxelHash = choice.nameHash;
                            c.chunkData[x,y,z].illuminationLevel = choice.illuminationLevel;
                        }
                    }
        }
        #endregion

        #region Tracing
        public struct ChunkTraceResult{
            public Chunk chunk;
            public Vector3Int localVoxel;
            public Chunk hitFaceChunk;
            public Vector3Int hitFaceVoxel;
        }
        public static Vector3 GetMinAngle(Vector3 direction, Vector3 a, Vector3 b){
            return Vector3.Dot(direction, a) < Vector3.Dot(direction, b) ? b : a;
        }
        public bool Trace(Vector3 origin, Vector3 direction, float length, out ChunkTraceResult hit, float stepSize = 0.1f){
            Chunk currentChunk = this;
            float distance = 0.0f;
            while(distance < length){
                Vector3 currentPosition = (origin - currentChunk.transform.position) + direction * distance;
                Vector3Int currentVoxel = new Vector3Int(Mathf.FloorToInt(currentPosition.x), Mathf.FloorToInt(currentPosition.y), Mathf.FloorToInt(currentPosition.z));
                if(!AreCoordinatesInBounds(currentVoxel.x, currentVoxel.y, currentVoxel.z)){
                    currentChunk = world.GetChunk(world.WorldPositionToChunk(origin + direction * distance));
                }
                else{
                    if(currentChunk.chunkData[currentVoxel.x, currentVoxel.y, currentVoxel.z].voxelHash != 0){
                        Vector3 impactDirection = (((origin - currentChunk.transform.position) + direction * distance) - (currentVoxel + Vector3.one * 0.5f)).normalized;
                        Vector3 hitNormal = Vector3.right;
                        hitNormal = GetMinAngle(impactDirection, Vector3.left, hitNormal);
                        hitNormal = GetMinAngle(impactDirection, Vector3.up, hitNormal);
                        hitNormal = GetMinAngle(impactDirection, Vector3.down, hitNormal);
                        hitNormal = GetMinAngle(impactDirection, Vector3.forward, hitNormal);
                        hitNormal = GetMinAngle(impactDirection, Vector3.back, hitNormal);
                        Vector3Int hitVoxel = Conversions.ToDiscreteVector(currentVoxel + hitNormal);
                        Chunk hitAdjacentFaceChunk = currentChunk;
                        if(!AreCoordinatesInBounds(hitVoxel.x, hitVoxel.y, hitVoxel.z)){
                            hitAdjacentFaceChunk = world.GetChunk(world.WorldPositionToChunk(Conversions.ToDiscreteVector(currentChunk.transform.position + hitVoxel)));
                            if(hitVoxel.x < 0)
                                hitVoxel.x += 16;
                            else if(hitVoxel.x >= 15)
                                hitVoxel.x -= 16;
                            else if(hitVoxel.y < 0)
                                hitVoxel.y += 16;
                            else if(hitVoxel.y >= 15)
                                hitVoxel.y -= 16;
                            else if(hitVoxel.z < 0)
                                hitVoxel.z += 16;
                            else if(hitVoxel.z >= 15)
                                hitVoxel.z -= 16;
                        }
                        
                        hit = new ChunkTraceResult{
                            chunk = currentChunk,
                            localVoxel = currentVoxel,
                            hitFaceChunk = hitAdjacentFaceChunk,
                            hitFaceVoxel = hitVoxel
                        };
                        return true;
                    }
                }
                distance += stepSize;
            }
            hit = new ChunkTraceResult{
                chunk = currentChunk,
                localVoxel = Vector3Int.zero
            };
            return false;
        }
        #endregion
    }
}