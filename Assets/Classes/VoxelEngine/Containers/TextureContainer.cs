using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace VoxelEngine{
    public class TextureContainer : MonoBehaviour
    {
        public static TextureContainer instance;

        public TextureAtlas atlas { get; private set; }
        private Dictionary<string, TextureAtlas.Part> textures = new Dictionary<string, TextureAtlas.Part>();
        public TextureAtlas.Part MissingTexture { get; private set; }
        public enum AtlasResolution{
            x64 = 64,
            x128 = 128,
            x256 = 256,
            x512 = 512,
            x1024 = 1024,
            x2048 = 2048
        }
        public AtlasResolution atlasResolution = AtlasResolution.x512;
        public Texture2D texture;

        private void Awake() {
            instance = this;
        }

        private void Start(){
            atlas = new TextureAtlas((int)atlasResolution, (int)atlasResolution, 16, Color.black, FilterMode.Point, TextureWrapMode.Clamp, false);

            TextureAtlas.Part missingTexturePart = null;
            atlas.PackMissingTexture(16, 16, out missingTexturePart);
            MissingTexture = missingTexturePart;

            foreach(string s in Directory.GetFiles(@"Packs\Textures", "*.png")){
                byte[] texData = File.ReadAllBytes(s);
                Texture2D tex = new Texture2D(16, 16, TextureFormat.ARGB32, true);
                tex.LoadImage(texData);
                if(atlas.Pack(tex, out TextureAtlas.Part part))
                    textures.Add(s.Replace(@"Packs\Textures\", string.Empty), part);
            }
            texture = atlas.texture;
        }

        public TextureAtlas.Part GetTexture(string texID){
            if(textures.TryGetValue(texID, out TextureAtlas.Part tex))
                return tex;
            return MissingTexture;
        }
    }
}