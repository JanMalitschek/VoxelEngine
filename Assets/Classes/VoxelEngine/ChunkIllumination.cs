using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelEngine{
    public static class ChunkIllumination
    {
        public static void TransferLighting(Chunk c, int x, int y, int z, float lightLevel){
            if(Chunk.AreCoordinatesInBounds(x, y, z) && (c.chunkData[x,y,z].voxelHash == 0 || VoxelContainer.GetVoxel(c.chunkData[x,y,z].voxelHash).isTransparent))
                c.chunkData[x,y,z].illuminationLevel = Mathf.Max(c.chunkData[x,y,z].illuminationLevel, lightLevel);
        }
        public static float GetIlluminationLevel(Chunk c, int x, int y, int z){
            if(Chunk.AreCoordinatesInBounds(x, y, z)){
                return c.chunkData[x,y,z].illuminationLevel;
            }
            else
                return 1.0f;
        }
        public static float GetIlluminationLevel(Chunk c, Vector3Int coords){
            if(Chunk.AreCoordinatesInBounds(coords.x, coords.y, coords.z)){
                return c.chunkData[coords.x,coords.y,coords.z].illuminationLevel;
            }
            else
                return 1.0f;
        }
        public static float SampleCornerPointIllumination(Chunk c, Vector3Int corner){
            int numSamples = 0;
            float sum = 0.0f;
            for(int x = corner.x - 1; x <= corner.x; x++)
                for(int y = corner.y - 1; y <= corner.y; y++)
                    for(int z = corner.z - 1; z <= corner.z; z++){
                        if(Chunk.AreCoordinatesInBounds(x, y, z)){
                            if(c.chunkData[x,y,z].voxelHash == 0 || (c.chunkData[x,y,z].voxelHash != 0 && VoxelContainer.GetVoxel(c.chunkData[x,y,z].voxelHash).isTransparent)){
                                numSamples++;
                                sum += c.chunkData[x,y,z].illuminationLevel;
                            }
                        }
                        else{
                            numSamples++;
                            sum += 1.0f;
                        }
                    }
            return numSamples == 0 ? 0.0f : sum / (float)numSamples;
        }
        public static float SampleCornerPointIllumination(Chunk c, Vector3Int corner, Vector3Int direction){
            int numSamples = 0;
            float sum = 0.0f;
            int minX = direction.x == 0 ? corner.x - 1 : corner.x + Mathf.Min(direction.x, 0);
            int maxX = direction.x == 0 ? corner.x + 1 : corner.x + Mathf.Max(direction.x, 0);
            int minY = direction.y == 0 ? corner.y - 1 : corner.y + Mathf.Min(direction.y, 0);
            int maxY = direction.y == 0 ? corner.y + 1 : corner.y + Mathf.Max(direction.y, 0);
            int minZ = direction.z == 0 ? corner.z - 1 : corner.z + Mathf.Min(direction.z, 0);
            int maxZ = direction.z == 0 ? corner.z + 1 : corner.z + Mathf.Max(direction.z, 0);
            for(int x = minX; x < maxX; x++)
                for(int y = minY; y < maxY; y++)
                    for(int z = minZ; z < maxZ; z++){
                        if(Chunk.AreCoordinatesInBounds(x, y, z)){
                            if(c.chunkData[x,y,z].voxelHash == 0 || (c.chunkData[x,y,z].voxelHash != 0 && VoxelContainer.GetVoxel(c.chunkData[x,y,z].voxelHash).isTransparent)){
                                numSamples++;
                                sum += c.chunkData[x,y,z].illuminationLevel;
                            }
                        }
                        else{
                            numSamples++;
                            sum += 1.0f;
                        }
                    }
            return numSamples == 0 ? 0.0f : (sum / (float)numSamples);
        }
        public static void InitializeLighting(Chunk c){
            for(int x = 0; x < 16; x++)
                for(int y = 0; y < 16; y++)
                    for(int z = 0; z < 16; z++){
                        if(c.chunkData[x,y,z].voxelHash == 0 || VoxelContainer.GetVoxel(c.chunkData[x,y,z].voxelHash).isTransparent)
                            c.chunkData[x,y,z].illuminationLevel = 0.0f;
                    }
            //Glowing Voxels
            for(int x = 0; x < 16; x++)
                for(int y = 0; y < 16; y++)
                    for(int z = 0; z < 16; z++){
                        if(c.chunkData[x,y,z].voxelHash != 0 && c.chunkData[x,y,z].illuminationLevel > 0.0f){
                            TransferLighting(c, x + 1, y, z, c.chunkData[x,y,z].illuminationLevel);
                            TransferLighting(c, x - 1, y, z, c.chunkData[x,y,z].illuminationLevel);
                            TransferLighting(c, x, y + 1, z, c.chunkData[x,y,z].illuminationLevel);
                            TransferLighting(c, x, y - 1, z, c.chunkData[x,y,z].illuminationLevel);
                            TransferLighting(c, x, y, z + 1, c.chunkData[x,y,z].illuminationLevel);
                            TransferLighting(c, x, y, z - 1, c.chunkData[x,y,z].illuminationLevel);
                        }
                    }
            //Sky Lighting
            for(int x = 0; x < 16; x++){
                for(int z = 0; z < 16; z++){
                    if(c.chunkData[x,15,z].voxelHash == 0){
                        int yTemp = 15;
                        while(yTemp >= 0){
                            if(c.chunkData[x,yTemp,z].voxelHash != 0 && !VoxelContainer.GetVoxel(c.chunkData[x,yTemp,z].voxelHash).isTransparent)
                                break;
                            c.chunkData[x,yTemp,z].illuminationLevel = 1.0f;
                            yTemp--;
                        }
                    }
                }
            }
        }
        public static void PropagateLighting(Chunk c){
            for(int i = 0; i < Mathf.CeilToInt(1.0f / c.lightLevelLoss); i++){
                for(int x = 0; x < 16; x++)
                    for(int y = 0; y < 16; y++)
                        for(int z = 0; z < 16; z++){
                            if((c.chunkData[x,y,z].voxelHash == 0 || VoxelContainer.GetVoxel(c.chunkData[x,y,z].voxelHash).isTransparent) && c.chunkData[x,y,z].illuminationLevel > 0.0f){
                                TransferLighting(c, x + 1, y, z, c.chunkData[x,y,z].illuminationLevel - c.lightLevelLoss);
                                TransferLighting(c, x - 1, y, z, c.chunkData[x,y,z].illuminationLevel - c.lightLevelLoss);
                                TransferLighting(c, x, y + 1, z, c.chunkData[x,y,z].illuminationLevel - c.lightLevelLoss);
                                TransferLighting(c, x, y - 1, z, c.chunkData[x,y,z].illuminationLevel - c.lightLevelLoss);
                                TransferLighting(c, x, y, z + 1, c.chunkData[x,y,z].illuminationLevel - c.lightLevelLoss);
                                TransferLighting(c, x, y, z - 1, c.chunkData[x,y,z].illuminationLevel - c.lightLevelLoss);
                            }
                        }
            }
        }   
    }
}