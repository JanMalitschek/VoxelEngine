using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelEngine{
    public static class Conversions
    {
        public static Vector3Int ChunkToWorldSpace(Chunk c, Vector3Int chunkSpace)
        {
            return ToDiscreteVector(c.worldPosition) + chunkSpace;
        }
        public static Vector3Int WorldToChunkSpace(Vector3Int worldSpace){
            return worldSpace - new Vector3Int((worldSpace.x / 16) * 16, (worldSpace.y / 16) * 16, (worldSpace.z / 16) * 16);
        }
        public static Vector3Int WorldToChunkPosition(Vector3 worldPos)
        {
            return new Vector3Int(Mathf.FloorToInt(worldPos.x / 16.0f), Mathf.FloorToInt(worldPos.y / 16.0f), Mathf.FloorToInt(worldPos.z / 16.0f));
        }
        public static Vector3Int WorldToRegionPosition(Vector3 worldPos)
        {
            return new Vector3Int(Mathf.FloorToInt(worldPos.x / 64.0f), Mathf.FloorToInt(worldPos.y / 64.0f), Mathf.FloorToInt(worldPos.z / 64.0f));
        }
        public static Vector3 ChunkToWorldPosition(Vector3Int chunkPos)
        {
            return new Vector3(chunkPos.x * 16.0f, chunkPos.y * 16.0f, chunkPos.z * 16.0f);
        }
        public static Vector3Int GetWorldPosition(Chunk c)
        {
            return ToDiscreteVector(c.worldPosition);
        }
        public static Vector3Int ToDiscreteVector(Vector3 v){
            return new Vector3Int(Mathf.RoundToInt(v.x + 0.1f), Mathf.RoundToInt(v.y + 0.1f), Mathf.RoundToInt(v.z + 0.1f));
        }
    }
}