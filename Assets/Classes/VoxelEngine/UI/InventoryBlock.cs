using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelEngine{
    public class InventoryBlock : MonoBehaviour
    {
        private PlayerController player;
        public Voxel voxel;

        private void Start() {
            player = FindObjectOfType<PlayerController>();
        }

        public void Select(){
            player.SelectBuildingBlock(voxel);
        }
    }
}