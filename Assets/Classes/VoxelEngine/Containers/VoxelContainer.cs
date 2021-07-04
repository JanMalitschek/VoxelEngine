using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

namespace VoxelEngine{
    public class VoxelContainer : MonoBehaviour
    {
        public static VoxelContainer instance;
        private TextureContainer textureContainer;
        private ModelContainer modelContainer;

        public Material defaultChunkMaterial;
        public static Material globalDefaultChunkMaterial;
        private static Dictionary<int, Voxel> container;
        public static Voxel MissingVoxel { get; private set; }

        private void Awake() {
            instance = this;
            textureContainer = TextureContainer.instance;
            modelContainer = ModelContainer.instance;
        }

        private void Start() {
            globalDefaultChunkMaterial = defaultChunkMaterial;
            globalDefaultChunkMaterial.SetTexture("_MainTex", textureContainer.atlas.texture);
            container = new Dictionary<int, Voxel>();
            
            MissingVoxel = new Voxel();
            MissingVoxel.VoxelName = "Std_Missing";
            MissingVoxel.partTop = MissingVoxel.partSides = MissingVoxel.partBottom = textureContainer.GetTexture("Std_MissingTexture");
            foreach(string s in Directory.GetFiles("Packs/Voxels", "*.vxl")){
                Voxel v = new Voxel();
                v.VoxelName = Path.GetFileNameWithoutExtension(s);
                XMLAbstraction xml = new XMLAbstraction(string.Empty, s);
                XMLAbstraction.Node texturesNode = xml.GetNode("//Voxel/Textures");
                if(texturesNode != null){
                    List<XMLAbstraction.Node> textureNodes = texturesNode.GetNodes("*[local-name()='Texture']");
                    foreach(XMLAbstraction.Node n in textureNodes){
                        string type = n.GetAttribute("type");
                        if (type == "top")
                            v.partTop = textureContainer.GetTexture(n.GetAttribute("path"));
                        else if (type == "sides")
                            v.partSides = textureContainer.GetTexture(n.GetAttribute("path"));
                        else if (type == "bottom")
                            v.partBottom= textureContainer.GetTexture(n.GetAttribute("path"));
                    }
                }
                XMLAbstraction.Node modelsNode = xml.GetNode("//Voxel/Models");
                if(modelsNode != null){
                    List<XMLAbstraction.Node> modelNodes = modelsNode.GetNodes("*[local-name()='Model']");
                    foreach(XMLAbstraction.Node n in modelNodes){
                        string type = n.GetAttribute("type");
                        if (type == "default"){
                            v.hasCustomModel = true;
                            v.customModel = modelContainer.GetModel(n.GetAttribute("path"));
                        }
                    }
                }
                XMLAbstraction.Node propertiesNode = xml.GetNode("//Voxel/Properties");
                if(propertiesNode != null){
                    List<XMLAbstraction.Node> propertyNodes = propertiesNode.GetNodes("*[local-name()='Property']");
                    foreach (XMLAbstraction.Node n in propertyNodes){
                        string type = n.GetAttribute("type");
                        if (type == "transparent")
                            v.isTransparent = bool.Parse(n.GetAttribute("value"));
                        else if (type == "maintainInnerFaces")
                            v.maintainInnerFaces = bool.Parse(n.GetAttribute("value"));
                        else if (type == "illuminationLevel")
                            v.illuminationLevel= float.Parse(n.GetAttribute("value"), NumberStyles.Any, CultureInfo.InvariantCulture);
                        else if (type == "cmMinScale")
                            v.minRandomScale = float.Parse(n.GetAttribute("value"), NumberStyles.Any, CultureInfo.InvariantCulture);
                        else if (type == "cmMaxScale")
                            v.maxRandomScale = float.Parse(n.GetAttribute("value"), NumberStyles.Any, CultureInfo.InvariantCulture);
                        else if (type == "cmRandomRotation")
                            v.randomRotation = bool.Parse(n.GetAttribute("value"));
                        else if (type == "cmRandomOffsetX")
                            v.randomOffsetX = float.Parse(n.GetAttribute("value"), NumberStyles.Any, CultureInfo.InvariantCulture);
                        else if (type == "cmRandomOffsetY")
                            v.randomOffsetY = float.Parse(n.GetAttribute("value"), NumberStyles.Any, CultureInfo.InvariantCulture);
                        else if (type == "cmRandomOffsetZ")
                            v.randomOffsetZ = float.Parse(n.GetAttribute("value"), NumberStyles.Any, CultureInfo.InvariantCulture);
                    }
                }
                XMLAbstraction.Node soundsNode = xml.GetNode("//Voxel/Sounds");
                if(soundsNode != null){
                    XMLAbstraction.Node breakSoundsNode = soundsNode.GetNode("*[local-name()='BreakSounds']");
                    if(breakSoundsNode != null)
                        v.breakSoundHashes = GetSoundNodes(breakSoundsNode);
                    XMLAbstraction.Node placeSoundsNode = soundsNode.GetNode("*[local-name()='PlaceSounds']");
                    if(placeSoundsNode != null)
                        v.placeSoundHashes = GetSoundNodes(placeSoundsNode);
                }
                container.Add(v.nameHash, v);
            }
        }

        private int[] GetSoundNodes(XMLAbstraction.Node node){
            List<XMLAbstraction.Node> soundNodes = node.GetNodes("*[local-name()='Sound']");
            int[] soundHashes = new int[soundNodes.Count];
            for(int i = 0; i < soundNodes.Count; i++)
                soundHashes[i] = soundNodes[i].GetAttribute("name").GetHashCode();
            return soundHashes;
        }

        public static bool TryGetVoxel(int hash, out Voxel voxel){
            if(container.TryGetValue(hash, out voxel))
                return true;
            voxel = null;
            return false;
        }
        public static bool TryGetVoxel(string voxelName, out Voxel voxel){
            if(container.TryGetValue(voxelName.GetHashCode(), out voxel))
                return true;
            voxel = null;
            return false;
        }
        public static Voxel GetVoxel(int hash){
            if(container.TryGetValue(hash, out Voxel v))
                return v;
            return null;
        }
        public static Voxel GetVoxel(string voxelName){
            if(container.TryGetValue(voxelName.GetHashCode(), out Voxel v))
                return v;
            return null;
        }
    }
}