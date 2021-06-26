using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelEngine{
    public class FlatlandInitializer : Initializer
    {
        public override void Initialize(Chunk chunk){
            if(Conversions.GetWorldPosition(chunk).y == 0){
                Chunk.FillVoxels(chunk, 0, 0, 0, 15, 0, 15, VoxelContainer.GetVoxel("Std_Stone"));
                Chunk.FillVoxels(chunk, 0, 1, 0, 15, 1, 15, VoxelContainer.GetVoxel("Std_Dirt"));
                Chunk.FillVoxels(chunk, 0, 2, 0, 15, 2, 15, VoxelContainer.GetVoxel("Std_Grass"));
                Chunk.FillVoxels(chunk, 0, 3, 0, 15, 3, 15, VoxelContainer.GetVoxel("Std_Tall_Grass"), VoxelContainer.GetVoxel("Std_Tombstone"), null, null);
                Chunk.FillVoxels(chunk, 6, 5, 6, 9, 5, 9, VoxelContainer.GetVoxel("Std_Iron_Block"));
            }
            else if(Conversions.GetWorldPosition(chunk).y < 0){
                Chunk.FillVoxels(chunk, 0, 0, 0, 15, 15, 15, VoxelContainer.GetVoxel("Std_Stone"));
            }
        }
    }
}