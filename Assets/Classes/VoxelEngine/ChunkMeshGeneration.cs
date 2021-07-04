using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelEngine{
    public static class ChunkMeshGeneration
    {
        public static void InitChunkVoxelSeed(Chunk c, int x, int y, int z){
            Vector3 nonZeroWorldPos = c.worldPosition + Vector3.one;
            Chunk.random = new Unity.Mathematics.Random((uint)Mathf.Abs(nonZeroWorldPos.x + nonZeroWorldPos.y + nonZeroWorldPos.z) + (uint)(x + y * 16 + z * 256));
        }
        public static void GenerateMesh(Chunk c){
            for(int x = 0; x < 16; x++){
                for(int y = 0; y < 16; y++){
                    for(int z = 0; z < 16; z++){
                        Voxel currentVoxel = VoxelContainer.GetVoxel(c.chunkData[x,y,z].voxelHash);
                        if(currentVoxel != null && !currentVoxel.hasCustomModel){
                            //Right Face
                            Voxel adjacentVoxel;
                            VoxelContainer.TryGetVoxel(c.GetVoxelHashAtPosition(x + 1, y, z), out adjacentVoxel);
                            if(adjacentVoxel == null || adjacentVoxel.isTransparent && (currentVoxel.nameHash != adjacentVoxel.nameHash || !currentVoxel.isTransparent || currentVoxel.maintainInnerFaces))
                                GenerateFace(c, x, y, z, Vector3.right, Vector3.back, Vector3.up, ref c.vertices, ref c.indices, ref c.uvs, currentVoxel.partSides, ref c.normals, ref c.colors, currentVoxel.sideFaceUV, currentVoxel.faceUVWidth);
                            //Left Face
                            VoxelContainer.TryGetVoxel(c.GetVoxelHashAtPosition(x - 1, y, z), out adjacentVoxel);
                            if(adjacentVoxel == null || adjacentVoxel.isTransparent && (currentVoxel.nameHash != adjacentVoxel.nameHash || !currentVoxel.isTransparent || currentVoxel.maintainInnerFaces))
                                GenerateFace(c, x, y, z, Vector3.left, Vector3.forward, Vector3.up, ref c.vertices, ref c.indices, ref c.uvs, currentVoxel.partSides, ref c.normals, ref c.colors, currentVoxel.sideFaceUV, currentVoxel.faceUVWidth);

                            //Front Face
                            VoxelContainer.TryGetVoxel(c.GetVoxelHashAtPosition(x, y, z + 1), out adjacentVoxel);
                            if(adjacentVoxel == null || adjacentVoxel.isTransparent && (currentVoxel.nameHash != adjacentVoxel.nameHash || !currentVoxel.isTransparent || currentVoxel.maintainInnerFaces))
                                GenerateFace(c, x, y, z, Vector3.forward, Vector3.right, Vector3.up, ref c.vertices, ref c.indices, ref c.uvs, currentVoxel.partSides, ref c.normals, ref c.colors, currentVoxel.sideFaceUV, currentVoxel.faceUVWidth);
                            //Back Face
                            VoxelContainer.TryGetVoxel(c.GetVoxelHashAtPosition(x, y, z - 1), out adjacentVoxel);
                            if(adjacentVoxel == null || adjacentVoxel.isTransparent && (currentVoxel.nameHash != adjacentVoxel.nameHash || !currentVoxel.isTransparent || currentVoxel.maintainInnerFaces))
                                GenerateFace(c, x, y, z, Vector3.back, Vector3.left, Vector3.up, ref c.vertices, ref c.indices, ref c.uvs, currentVoxel.partSides, ref c.normals, ref c.colors, currentVoxel.sideFaceUV, currentVoxel.faceUVWidth);

                            //Top Face
                            VoxelContainer.TryGetVoxel(c.GetVoxelHashAtPosition(x, y + 1, z), out adjacentVoxel);
                            if(adjacentVoxel == null || adjacentVoxel.isTransparent && (currentVoxel.nameHash != adjacentVoxel.nameHash || !currentVoxel.isTransparent || currentVoxel.maintainInnerFaces))
                                GenerateFace(c, x, y, z, Vector3.up, Vector3.right, Vector3.back, ref c.vertices, ref c.indices, ref c.uvs, currentVoxel.partTop, ref c.normals, ref c.colors, currentVoxel.topFaceUV, currentVoxel.faceUVWidth);
                            //Bottom Face
                            VoxelContainer.TryGetVoxel(c.GetVoxelHashAtPosition(x, y - 1, z), out adjacentVoxel);
                            if(adjacentVoxel == null || adjacentVoxel.isTransparent && (currentVoxel.nameHash != adjacentVoxel.nameHash || !currentVoxel.isTransparent || currentVoxel.maintainInnerFaces))
                                GenerateFace(c, x, y, z, Vector3.down, Vector3.right, Vector3.forward, ref c.vertices, ref c.indices, ref c.uvs, currentVoxel.partBottom, ref c.normals, ref c.colors, currentVoxel.bottomFaceUV, currentVoxel.faceUVWidth);
                        }
                        else if(currentVoxel != null && currentVoxel.hasCustomModel){
                            GenerateCustomModel(c,
                                                currentVoxel.customModel,
                                                currentVoxel.minRandomScale,
                                                currentVoxel.maxRandomScale,
                                                currentVoxel.randomRotation,
                                                currentVoxel.randomOffsetX, 
                                                currentVoxel.randomOffsetY,
                                                currentVoxel.randomOffsetZ,
                                                x, y, z, ref c.vertices, ref c.indices, ref c.uvs, currentVoxel.partSides, ref c.normals, ref c.colors);
                        }
                    }
                }
            }
        }
        private static Vector3 half = Vector3.one * 0.5f;
        private static void GenerateFace(Chunk c,
                                         int x,
                                         int y,
                                         int z,
                                         Vector3 forward,
                                         Vector3 right,
                                         Vector3 up,
                                         ref List<Vector3> vertices,
                                         ref List<int> indices,
                                         ref List<Vector2> uvs,
                                         TextureAtlas.Part part,
                                         ref List<Vector3> normals,
                                         ref List<Color> colors,
                                         Vector2 uvOffset,
                                         Vector2 uvSize){
            normals.Add(forward);
            normals.Add(forward);
            normals.Add(forward);
            normals.Add(forward);

            forward *= 0.5f;
            right *= 0.5f;
            up *= 0.5f;

            int currentIndex = vertices.Count;
            Vector3 offset = new Vector3(x, y, z);
            vertices.Add(offset + forward + right - up + half);
            vertices.Add(offset + forward + right + up + half);
            vertices.Add(offset + forward - right + up + half);
            vertices.Add(offset + forward - right - up + half);

            Vector3Int coords = new Vector3Int(x, y, z) + new Vector3Int(Mathf.RoundToInt(forward.x * 1.2f), Mathf.RoundToInt(forward.y * 1.2f), Mathf.RoundToInt(forward.z * 1.2f));
            Vector3Int discreteRight = new Vector3Int(Mathf.RoundToInt(right.x * 1.2f), Mathf.RoundToInt(right.y * 1.2f), Mathf.RoundToInt(right.z * 1.2f));
            Vector3Int discreteUp = new Vector3Int(Mathf.RoundToInt(up.x * 1.2f), Mathf.RoundToInt(up.y * 1.2f), Mathf.RoundToInt(up.z * 1.2f));
            switch(c.lightMode){
                case Chunk.LightMode.Flat:
                    Color lightColor = Color.Lerp(Color.black, Color.white, ChunkIllumination.GetIlluminationLevel(c, coords));
                    colors.Add(lightColor);
                    colors.Add(lightColor);
                    colors.Add(lightColor);
                    colors.Add(lightColor);
                break;
                case Chunk.LightMode.Smooth:
                    Vector3Int lightSamplingDirection = new Vector3Int(Mathf.RoundToInt(forward.x * 1.1f), Mathf.RoundToInt(forward.y * 1.1f), Mathf.RoundToInt(forward.z * 1.1f));
                    colors.Add(Color.Lerp(Color.black, Color.white, ChunkIllumination.SampleCornerPointIllumination(c, Conversions.ToDiscreteVector(vertices[vertices.Count - 4]), lightSamplingDirection)));
                    colors.Add(Color.Lerp(Color.black, Color.white, ChunkIllumination.SampleCornerPointIllumination(c, Conversions.ToDiscreteVector(vertices[vertices.Count - 3]), lightSamplingDirection)));
                    colors.Add(Color.Lerp(Color.black, Color.white, ChunkIllumination.SampleCornerPointIllumination(c, Conversions.ToDiscreteVector(vertices[vertices.Count - 2]), lightSamplingDirection)));
                    colors.Add(Color.Lerp(Color.black, Color.white, ChunkIllumination.SampleCornerPointIllumination(c, Conversions.ToDiscreteVector(vertices[vertices.Count - 1]), lightSamplingDirection)));
                break;
            }

            uvs.Add(part.uvs[0]);
            uvs.Add(part.uvs[1]);
            uvs.Add(part.uvs[2]);
            uvs.Add(part.uvs[3]);

            indices.Add(currentIndex);
            indices.Add(currentIndex + 1);
            indices.Add(currentIndex + 2);
            indices.Add(currentIndex);
            indices.Add(currentIndex + 2);
            indices.Add(currentIndex + 3);
        }
        private static void GenerateCustomModel(Chunk c,
                                                Voxel.ThreadableMesh customModel,
                                                float minScale,
                                                float maxScale,
                                                bool randomRotation,
                                                float randomOffsetX,
                                                float randomOffsetY,
                                                float randomOffsetZ,
                                                int x,
                                                int y,
                                                int z,
                                                ref List<Vector3> vertices,
                                                ref List<int> indices,
                                                ref List<Vector2> uvs,
                                                TextureAtlas.Part part,
                                                ref List<Vector3> normals,
                                                ref List<Color> colors){
            if(customModel == null) return;

            InitChunkVoxelSeed(c, x, y, z);

            Vector3 center = new Vector3(x, y, z) + new Vector3(0.5f, 0.0f, 0.5f);

            int currentIndex = vertices.Count;

            List<Vector3> customVertices = customModel.vertices;
            Matrix4x4 trs = Matrix4x4.TRS(new Vector3(Chunk.random.NextFloat(-randomOffsetX, randomOffsetX),
                                                      Chunk.random.NextFloat(-randomOffsetY, randomOffsetY),
                                                      Chunk.random.NextFloat(-randomOffsetZ, randomOffsetZ)),
                                        Quaternion.Euler(new Vector3(0.0f, Chunk.random.NextFloat(0.0f, 360.0f), 0.0f)), 
                                        Vector3.one * Chunk.random.NextFloat(minScale, maxScale));
            foreach(Vector3 v in customVertices){
                Vector4 v4 = new Vector4(v.x, v.y, v.z, 1.0f);
                v4 = trs * v4;
                vertices.Add(center + new Vector3(v4.x, v4.y, v4.z));
            }

            List<int> customIndices = customModel.indices;
            foreach(int i in customIndices)
                indices.Add(currentIndex + i);

            List<Vector2> customUVs = customModel.uvs;
            foreach(Vector2 cuv in customUVs)
                uvs.Add(new Vector2(part.uvs[0].x + cuv.x * part.UVWidth, part.uvs[0].y + cuv.y * part.UVHeight));

            List<Vector3> customNormals = customModel.normals;
            normals.AddRange(customNormals);

            Vector3Int coords = new Vector3Int(x, y, z);
            Color lightColor = Color.Lerp(Color.black, Color.white, ChunkIllumination.GetIlluminationLevel(c, coords));
            for(int i = 0; i < customVertices.Count; i++)
                colors.Add(lightColor);
        }
    }
}