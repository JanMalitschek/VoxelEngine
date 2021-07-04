using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace VoxelEngine
{
    public class VoxelWorldAPI : MonoBehaviour
    {
        public class Operation{
            public enum Type{
                SetVoxel,
                SetMultiVoxel,
                FillVoxels,
                FillMutliVoxels_Distributed
            }
            public Type type = Type.SetVoxel;
            public Voxel[] voxels;
            public int x0, y0, z0;
            public int x1, y1, z1;
            public Operation(Type type, int x0, int y0, int z0, params Voxel[] voxels){
                this.type = type;
                this.x0 = this.x1 = x0;
                this.y0 = this.y1 = y0;
                this.z0 = this.z1 = z0;
                this.voxels = voxels;
            }
            public Operation(Type type, Vector3Int chunkSpacePos, params Voxel[] voxels){
                this.type = type;
                this.x0 = this.x1 = chunkSpacePos.x;
                this.y0 = this.y1 = chunkSpacePos.y;
                this.z0 = this.z1 = chunkSpacePos.z;
                this.voxels = voxels;
            }
            public Operation(Type type, int x0, int y0, int z0, int x1, int y1, int z1, params Voxel[] voxels){
                this.type = type;
                this.x0 = x0;
                this.y0 = y0;
                this.z0 = z0;
                this.x1 = x1;
                this.y1 = y1;
                this.z1 = z1;
                this.voxels = voxels;
            }
            public Operation(Type type, Vector3Int chunkSpacePos0, Vector3Int chunkSpacePos1, params Voxel[] voxels){
                this.type = type;
                this.x0 = chunkSpacePos0.x;
                this.y0 = chunkSpacePos0.y;
                this.z0 = chunkSpacePos0.z;
                this.x1 = chunkSpacePos1.x;
                this.y1 = chunkSpacePos1.y;
                this.z1 = chunkSpacePos1.z;
                this.voxels = voxels;
            }
        }
        public static Dictionary<Vector3Int, List<Operation>> chunkOperationQueue = new Dictionary<Vector3Int, List<Operation>>();
        private static readonly object addLocker = new object();
        private static readonly object getLocker = new object();

        // private static Chunk chunkCache = null;
        // private static Vector3Int lastChunkPos = Vector3Int.zero;
        // private static Vector3Int chunkSpaceVoxelPos = Vector3Int.zero;
        public static World world;
        private static Unity.Mathematics.Random random = new Unity.Mathematics.Random(1);

        #region Voxel Access
        //Single Voxels
        public static void SetVoxel(int x, int y, int z, Voxel v){
            AddOperation(world.WorldPositionToChunk(new Vector3(x, y, z)), 
                         new Operation(Operation.Type.SetVoxel, Conversions.WorldToChunkSpace(new Vector3Int(x, y, z)), v));
            // Chunk.SetVoxel(chunkCache, x, y, z, v);
        }
        public static void SetVoxel(Vector3Int worldPos, Voxel v){
            SetVoxel(worldPos.x, worldPos.y, worldPos.z, v);
        }
        public static void SetMultiVoxel(int x, int y, int z, params Voxel[] v){
            SetVoxel(x, y, z, ChooseVoxel(v));
        }
        public static void SetMultiVoxel(Vector3Int worldPos, params Voxel[] v){
            SetVoxel(worldPos, ChooseVoxel(v));
        }

        //Filling Voxels
        public static List<Operation> FillVoxels(int x0, int y0, int z0, int x1, int y1, int z1, Voxel v){
            if(x0 > x1)
                Swap(ref x0, ref x1);
            if(y0 > y1)
                Swap(ref y0, ref y1);
            if(z0 > z1)
                Swap(ref z0, ref z1);
            //Which chunks will be affected by the fill operation
            Vector3Int minChunk = world.WorldPositionToChunk(new Vector3(x0, y0, z0));
            Vector3Int maxChunk = world.WorldPositionToChunk(new Vector3(x1, y1, z1));
            List<Vector3Int> affectedChunks = new List<Vector3Int>();
            for(int x = minChunk.x; x <= maxChunk.x; x++)
                for(int y = minChunk.y; y <= maxChunk.y; y++)
                    for(int z = minChunk.z; z <= maxChunk.z; z++)
                        affectedChunks.Add(new Vector3Int(x, y, z));
            List<Operation> resultingOperations = new List<Operation>();
            foreach(Vector3Int c in affectedChunks){
                Vector3Int minVoxel = Vector3Int.zero;
                Vector3Int maxVoxel = new Vector3Int(15, 15, 15);
                Vector3Int discreteWorldPosition = c * 16;

                //Which part of the chunk is affected by the fill operation?
                if(minVoxel.x + discreteWorldPosition.x < x0)
                    minVoxel.x += (x0 - (minVoxel.x + discreteWorldPosition.x));
                if(minVoxel.y + discreteWorldPosition.y < y0)
                    minVoxel.y += (y0 - (minVoxel.y + discreteWorldPosition.y));
                if(minVoxel.z + discreteWorldPosition.z < z0)
                    minVoxel.z += (z0 - (minVoxel.z + discreteWorldPosition.z));

                if(maxVoxel.x + discreteWorldPosition.x > x1)
                    maxVoxel.x -= ((maxVoxel.x + discreteWorldPosition.x) - x1);
                if(maxVoxel.y + discreteWorldPosition.y > y1)
                    maxVoxel.y -= ((maxVoxel.y + discreteWorldPosition.y) - y1);
                if(maxVoxel.z + discreteWorldPosition.z > z1)
                    maxVoxel.z -= ((maxVoxel.z + discreteWorldPosition.z) - z1);
                
                Operation o = new Operation(Operation.Type.FillVoxels, minVoxel, maxVoxel, v);
                resultingOperations.Add(o);
                AddOperation(c, o);
            }
            return resultingOperations;
        }
        public static List<Operation> FillVoxels(Vector3Int worldPos0, Vector3Int worldPos1, Voxel v){
            return FillVoxels(worldPos0.x, worldPos0.y, worldPos0.z, worldPos1.x, worldPos1.y, worldPos1.z, v);
        }
        public static void FillMultiVoxelsSame(int x0, int y0, int z0, int x1, int y1, int z1, params Voxel[] v){
            FillVoxels(x0, y0, z0, x1, y1, z1, ChooseVoxel(v));
        }
        public static void FillMultiVoxelsSame(Vector3Int worldPos0, Vector3Int worldPos1, params Voxel[] v){
            FillVoxels(worldPos0, worldPos1, ChooseVoxel(v));
        }
        public static void FillMultiVoxelsDistributed(int x0, int y0, int z0, int x1, int y1, int z1, params Voxel[] v){
            var operations = FillVoxels(x0, y0, z0, x1, y1, z1, v[0]);
            foreach(Operation o in operations){
                o.type = Operation.Type.FillMutliVoxels_Distributed;
                o.voxels = v;
            }
        }
        public static void FillMultiVoxelsDistributed(Vector3Int worldPos0, Vector3Int worldPos1, params Voxel[] v){
            var operations = FillVoxels(worldPos0, worldPos1, v[0]);
            foreach(Operation o in operations){
                o.type = Operation.Type.FillMutliVoxels_Distributed;
                o.voxels = v;
            }
        }
        #endregion

        #region Operation Access
        private static void AddOperation(Vector3Int chunk, Operation operation){
            lock(addLocker){
                if(chunkOperationQueue.TryGetValue(chunk, out List<Operation> operations)){
                    operations.Add(operation);
                }
                else{
                    List<Operation> initialOperations = new List<Operation>();
                    initialOperations.Add(operation);
                    chunkOperationQueue.Add(chunk, initialOperations);
                }
            }
        }
        public static List<Operation> GetOperations(Vector3Int chunk){
            lock(getLocker){
                if(chunkOperationQueue.TryGetValue(chunk, out List<Operation> operations)){
                    chunkOperationQueue.Remove(chunk);
                    return operations;
                }
                else
                    return new List<Operation>();
            }
        }
        public static bool TryGetOperations(Vector3Int chunk, out List<Operation> operations){
            lock(getLocker){
                if(chunkOperationQueue.TryGetValue(chunk, out operations)){
                    chunkOperationQueue.Remove(chunk);
                    return true;
                }
                return false;
            }
        }
        #endregion

        #region Operation Execution
        public static void ExecuteOperations(Chunk c){
            if(TryGetOperations(c.discreteWorldPosition / 16, out List<Operation> operations)){
                foreach(Operation p in operations){
                    switch(p.type){
                        case Operation.Type.SetVoxel:
                            Chunk.SetVoxel(c, p.x0, p.y0, p.z0, p.voxels[0]);
                        break;
                        case Operation.Type.SetMultiVoxel:
                            Chunk.SetVoxel(c, p.x0, p.y0, p.z0, ChooseVoxel(p.voxels));
                        break;
                        case Operation.Type.FillVoxels:
                            Chunk.FillVoxels(c, p.x0, p.y0, p.z0, p.x1, p.y1, p.z1, p.voxels[0]);
                        break;
                        case Operation.Type.FillMutliVoxels_Distributed:
                            Chunk.FillVoxels(c, p.x0, p.y0, p.z0, p.x1, p.y1, p.z1, p.voxels);
                        break;
                    }
                }
            }
        }
        #endregion

        #region Utility
        // public static void UpdateCache(int x, int y, int z){
        //     Vector3Int currentChunkPos = world.WorldPositionToChunk(new Vector3(x, y, z));
        //     chunkSpaceVoxelPos = new Vector3Int(x, y, z) - currentChunkPos;
        //     if(currentChunkPos != lastChunkPos){
        //         lastChunkPos = currentChunkPos;
        //         chunkCache = world.RequestChunk(currentChunkPos);
        //     }
        // }
        public static Voxel ChooseVoxel(params Voxel[] choices){
            return choices[random.NextInt(0, choices.Length)];
        }
        private static void Swap(ref int a, ref int b){
            int c = a;
            b = a;
            a = c;
        }
        #endregion
    }   
}