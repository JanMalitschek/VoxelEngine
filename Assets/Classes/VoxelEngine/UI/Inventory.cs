using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VoxelEngine{
    public class Inventory : MonoBehaviour
    {
        public RawImage block;
        private Transform inventory;

        private void Start() {
            PopulateInventory();
        }

        public void PopulateInventory(){
            inventory = GetComponent<Transform>();
            foreach(Voxel v in VoxelContainer.container.Values){
                RawImage inventoryBlock = Instantiate(block, inventory);
                inventoryBlock.texture = VoxelThumbnailContainer.thumbnails[v.nameHash];
                inventoryBlock.GetComponent<InventoryBlock>().voxel = v;
            }
            ShowInventory(false);
        }

        public void ShowInventory(bool visible = true){
            gameObject.SetActive(visible);
            Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = visible;
        }
    }
}