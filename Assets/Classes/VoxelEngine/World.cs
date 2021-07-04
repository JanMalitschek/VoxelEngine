using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelEngine{
    public class World : MonoBehaviour
    {
        public int chunkDistance = 4;
        public int maxChunkUpdatesPerFrame = 8;
        private Vector3Int currentChunk = Vector3Int.zero;
        private Dictionary<Vector3Int, Region> loadedRegions = new Dictionary<Vector3Int, Region>();
        //Queues
        private Queue<Vector3Int> chunkInitQueue = new Queue<Vector3Int>();
        private Queue<Chunk> chunkUpdateQueue = new Queue<Chunk>();

        private void Awake() {
            VoxelWorldAPI.world = this;
        }

        private void Start() {
            OnCurrentChunkChange();
        }

        private void Update() {
            Vector3 currentPlayerPosition = Camera.main.transform.position;
            Vector3Int currentPlayerChunk = new Vector3Int(Mathf.FloorToInt(currentPlayerPosition.x / 16.0f),
                                                            Mathf.FloorToInt(currentPlayerPosition.y / 16.0f),
                                                            Mathf.FloorToInt(currentPlayerPosition.z / 16.0f));
            if(currentPlayerChunk != currentChunk){
                currentChunk = currentPlayerChunk;
                OnCurrentChunkChange();
            }

            int availableUpdates = maxChunkUpdatesPerFrame;
            while(chunkInitQueue.Count > 0 && availableUpdates > 0){
                Chunk requestedChunk = RequestChunk(chunkInitQueue.Dequeue());
                requestedChunk.InitChunk();
                chunkUpdateQueue.Enqueue(requestedChunk);
                availableUpdates--;
            }
            while(chunkUpdateQueue.Count > 0 && availableUpdates > 0){
                if(chunkUpdateQueue.Peek().state >= Chunk.State.DataOnly)
                    chunkUpdateQueue.Dequeue().UpdateChunk();
                availableUpdates--;
            }
        }   

        private void OnCurrentChunkChange(){
            for(int x = currentChunk.x - chunkDistance; x <= currentChunk.x + chunkDistance; x++)
                for(int y = currentChunk.y - chunkDistance; y <= currentChunk.y + chunkDistance; y++)
                    for(int z = currentChunk.z - chunkDistance; z <= currentChunk.z + chunkDistance; z++)
                        chunkInitQueue.Enqueue(new Vector3Int(x, y, z));
        }

        public Chunk GetChunk(Vector3Int chunkPos){
            Vector3Int region = ChunkPositionToRegion(chunkPos);
            try{
                Region r = loadedRegions[region];
                return r.GetChunk(Region.ChunkToRegionPosition(chunkPos));
            }
            catch(System.Exception){
                return null;
            }
        }

        public Chunk RequestChunk(Vector3Int chunkPos){
            Vector3Int region = ChunkPositionToRegion(chunkPos);
            return RequestRegion(region).RequestChunk(Region.ChunkToRegionPosition(chunkPos));
        }
        public Region RequestRegion(Vector3Int regionPos){
            if(!loadedRegions.ContainsKey(regionPos)){
                GameObject r = new GameObject($"Region_{regionPos.x}_{regionPos.y}_{regionPos.z}", typeof(Region));
                r.transform.position = new Vector3(regionPos.x, regionPos.y, regionPos.z) * 64.0f;
                r.transform.SetParent(transform);
                Region region = r.GetComponent<Region>();
                region.worldPosition = r.transform.position;
                region.world = this;
                region.LoadFromDisk();
                loadedRegions.Add(regionPos, region);
                return region;
            }
            else
                return loadedRegions[regionPos];
        }

        private Vector3Int ToDiscreteVector(Vector3 v){
            return new Vector3Int(Mathf.FloorToInt(v.x + 0.1f), Mathf.FloorToInt(v.y + 0.1f), Mathf.FloorToInt(v.z + 0.1f));
        }
        private Vector3Int ChunkPositionToRegion(Vector3Int chunkPos){
            return ToDiscreteVector(new Vector3(chunkPos.x / 4.0f, chunkPos.y / 4.0f, chunkPos.z / 4.0f));
        }
        public Vector3Int WorldPositionToChunk(Vector3 worldPosition){
            return new Vector3Int(Mathf.FloorToInt(worldPosition.x / 16.0f), Mathf.FloorToInt(worldPosition.y / 16.0f), Mathf.FloorToInt(worldPosition.z / 16.0f));
        }

        #region Saving and Loading
        public void SaveWorld(){
            foreach(Region r in loadedRegions.Values)
                r.SaveToDisk();
        }
        #endregion
    }
}