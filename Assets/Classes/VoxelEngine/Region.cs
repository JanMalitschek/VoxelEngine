using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

namespace VoxelEngine{
    public class Region : MonoBehaviour
    {
        private Chunk[,,] chunksInRegion = new Chunk[4,4,4];
        public Vector3 worldPosition;
        public World world;

        public Chunk RequestChunk(Vector3Int regionSpaceChunkPos){
            if(AreCoordinatesInBounds(regionSpaceChunkPos)){
                //Chunk does not exist yet
                if(chunksInRegion[regionSpaceChunkPos.x, regionSpaceChunkPos.y, regionSpaceChunkPos.z] == null){
                    Vector3 chunkPosition = worldPosition + new Vector3(regionSpaceChunkPos.x, regionSpaceChunkPos.y, regionSpaceChunkPos.z) * 16.0f;
                    GameObject c = new GameObject($"Chunk_{regionSpaceChunkPos.x}_{regionSpaceChunkPos.y}_{regionSpaceChunkPos.z}", typeof(Chunk));
                    c.transform.position = chunkPosition;
                    c.transform.SetParent(transform);
                    c.GetComponent<MeshRenderer>().sharedMaterial = VoxelContainer.globalDefaultChunkMaterial;
                    Chunk chunk = c.GetComponent<Chunk>();
                    chunk.worldPosition = chunkPosition;
                    chunk.region = this;
                    chunksInRegion[regionSpaceChunkPos.x, regionSpaceChunkPos.y, regionSpaceChunkPos.z] = chunk;
                    chunk.discreteWorldPosition = new Vector3Int(Mathf.FloorToInt(chunk.worldPosition.x),
                                                                 Mathf.FloorToInt(chunk.worldPosition.y),
                                                                 Mathf.FloorToInt(chunk.worldPosition.z));
                    return chunk;
                }
                //Chunk already exists
                else{
                    Chunk chunk = chunksInRegion[regionSpaceChunkPos.x, regionSpaceChunkPos.y, regionSpaceChunkPos.z];
                    return chunk;
                }
            }
            else
                Debug.LogWarning($"Region Space Coordiantes {regionSpaceChunkPos} are out of bounds!", this);
            return null;
        }   
        public Chunk GetChunk(Vector3Int regionSpaceChunkPos){
            return chunksInRegion[regionSpaceChunkPos.x, regionSpaceChunkPos.y, regionSpaceChunkPos.z];
        }
        private bool AreCoordinatesInBounds(Vector3Int coords){
            return coords.x >= 0 && coords.x < 4 && coords.y >= 0 && coords.y < 4 && coords.z >= 0 && coords.z < 4;
        }

        private static Vector3Int ToDiscreteVector(Vector3 v){
            return new Vector3Int(Mathf.RoundToInt(v.x + 0.1f), Mathf.RoundToInt(v.y + 0.1f), Mathf.RoundToInt(v.z + 0.1f));
        }
        public Vector3Int GetRegionWorldPosition(){
            return ToDiscreteVector(transform.position);
        }
        public Vector3Int GetRegionPosition(){
            return ToDiscreteVector(transform.position / 64.0f);
        }
        public static Vector3Int WorldToRegionPosition(Vector3 worldPosition){
            return ToDiscreteVector(worldPosition / 64.0f);
        }
        private static int Repeat(int x, int max){
            while(x >= max)
                x -= max;
            while(x < 0)
                x += max;
            return x;
        }
        public static Vector3Int ChunkToRegionPosition(Vector3Int chunkPos){
            return new Vector3Int(Repeat(chunkPos.x, 4), Repeat(chunkPos.y, 4), Repeat(chunkPos.z, 4));
        }

        #region Saving and Loading
        public void SaveToDisk(){
            using(BinaryWriter writer = new BinaryWriter(File.Open($"./Save/{gameObject.name}.rgn", FileMode.OpenOrCreate))){
                for(int x = 0; x < 4; x++)
                    for(int y = 0; y < 4; y++)
                        for(int z = 0; z < 4; z++){
                            if(chunksInRegion[x,y,z] == null)
                                writer.Write(1);
                            else{
                                Chunk c = chunksInRegion[x,y,z];
                                for(int cx = 0; cx < 16; cx++)
                                    for(int cy = 0; cy < 16; cy++)
                                        for(int cz = 0; cz < 16; cz++)
                                            writer.Write(c.chunkData[cx,cy,cz].voxelHash);
                            }
                        }
            }
        }
        public void LoadFromDisk(){
            if(File.Exists($"./Save/{gameObject.name}.rgn")){
                using(BinaryReader reader = new BinaryReader(File.Open($"./Save/{gameObject.name}.rgn", FileMode.Open))){
                    for(int x = 0; x < 4; x++)
                        for(int y = 0; y < 4; y++)
                            for(int z = 0; z < 4; z++){
                                if(reader.PeekChar() == 1){
                                    chunksInRegion[x,y,z] = null;
                                    reader.ReadInt32();
                                    continue;
                                }
                                Vector3 chunkPosition = transform.position + new Vector3(x, y, z) * 16.0f;
                                GameObject g = new GameObject($"Chunk_{x}_{y}_{z}", typeof(Chunk));
                                g.transform.position = chunkPosition;
                                g.transform.SetParent(transform);
                                g.GetComponent<MeshRenderer>().sharedMaterial = VoxelContainer.globalDefaultChunkMaterial;
                                chunksInRegion[x, y, z] = g.GetComponent<Chunk>();
                                chunksInRegion[x, y, z].worldPosition = chunksInRegion[x, y, z].transform.position;
                                Chunk c = chunksInRegion[x,y,z];
                                // chunksInRegion[x, y, z].InitChunk();
                                for(int cx = 0; cx < 16; cx++)
                                    for(int cy = 0; cy < 16; cy++)
                                        for(int cz = 0; cz < 16; cz++){
                                            c.chunkData[cx,cy,cz] = new Chunk.ChunkVoxel{voxelHash = reader.ReadInt32()};
                                        }
                                c.UpdateChunk();
                                g.SetActive(false);
                            }
                }
            }
            else{
                Debug.LogWarning($"Region File {gameObject.name}.rgn does not exist!");
            }
        }
        #endregion
    }
}