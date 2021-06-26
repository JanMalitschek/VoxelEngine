using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelEngine{
    public class TextureAtlas
    {
        public class Part
        {
            public int X { get; private set; }
            public int Y { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }
            public Rect Rect { get; private set; }
            public Vector2[] uvs;
            public float UVWidth{
                get{
                    return uvs[3].x - uvs[0].x;
                }
            }
            public float UVHeight{
                get{
                    return uvs[1].y - uvs[0].y;
                }
            }
            public Part(int x, int y, int width, int height, TextureAtlas atlas){
                X = x;
                Y = y;
                Width = width;
                Height = height;
                Rect = new Rect(x, y, width, height);
                uvs = new Vector2[4]{
                    new Vector2((float)x / (float)atlas.texture.width,           (float)(y) / (float)atlas.texture.height),
                    new Vector2((float)x / (float)atlas.texture.width,           (float)(y + height) / (float)atlas.texture.height),
                    new Vector2((float)(x + width) / (float)atlas.texture.width, (float)(y + height) / (float)atlas.texture.height),
                    new Vector2((float)(x + width) / (float)atlas.texture.width, (float)(y) / (float)atlas.texture.height)
                };
            }
            public bool Overlaps(Part other){
                return Rect.Overlaps(other.Rect);
            }
            public bool Overlaps(Rect other){
                return Rect.Overlaps(other);
            }
        }
        public Texture2D texture;
        private List<Part> parts = new List<Part>();
        private int packingResolution;

        public TextureAtlas(int width, int height, int packingResolution, Color backgroundColor, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Repeat, bool generateMipMaps = true){
            this.packingResolution = packingResolution;

            texture = new Texture2D(width, height, TextureFormat.ARGB32, true);
            Color[] colors = new Color[width * height];
            for(int i = 0; i < width * height; i++)
                colors[i] = backgroundColor;
            texture.SetPixels(0, 0, width, height, colors);
            texture.filterMode = filterMode;
            texture.wrapMode = wrapMode;
            texture.Apply();
        }
        public bool PackMissingTexture(int width, int height, out Part packedPart){
            packedPart = null;
            for (int y = 0; y < texture.height; y += packingResolution){
                for (int x = 0; x < texture.width; x += packingResolution){
                    bool overlaps = false;
                    foreach (Part p in parts){
                        Rect r = new Rect(x, y, width, height);
                        if (p.Overlaps(r)){
                            overlaps = true;
                            break;
                        }
                    }
                    if (!overlaps){
                        Color[] colors = new Color[width * height];
                        int halfWidth = width / 2;
                        int halfHeight = height / 2;
                        for(int i = 0; i < width; i++){
                            for (int j = 0; j < height; j++){
                                if(i < halfWidth && j < halfHeight || i >= halfWidth && j >= halfHeight)
                                    colors[i + j * width] = Color.magenta;
                                else
                                    colors[i + j * width] = Color.black;
                            }
                        }
                        packedPart = new Part(x, y, width, height, this);
                        parts.Add(packedPart);
                        texture.SetPixels(x, y, width, height, colors);
                        texture.Apply();
                        return true;
                    }
                }
            }
            return false;
        }
        public bool Pack(Texture2D tex, out Part packedPart){
            packedPart = null;
            for (int y = 0; y < texture.height; y += packingResolution){
                for (int x = 0; x < texture.width; x += packingResolution){
                    bool overlaps = false;
                    foreach(Part p in parts){
                        Rect r = new Rect(x, y, tex.width, tex.height);
                        if (p.Overlaps(r)){
                            overlaps = true;
                            break;
                        }
                    }
                    if (!overlaps){
                        packedPart = new Part(x, y, tex.width, tex.height, this);
                        parts.Add(packedPart);
                        Color[] colors = tex.GetPixels(0, 0, tex.width, tex.height);
                        texture.SetPixels(x, y, tex.width, tex.height, colors);
                        texture.Apply();
                        return true;
                    }
                }
            }
            return false;
        }
    }
}