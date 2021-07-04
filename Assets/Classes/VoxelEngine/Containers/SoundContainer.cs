using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Networking;

namespace VoxelEngine{
    public class SoundContainer : MonoBehaviour
    {
        public static SoundContainer instance;

        private Dictionary<int, AudioClip> sounds = new Dictionary<int, AudioClip>();

        public AudioSource globalSFXSource;

        public AudioClip[] manualInput;

        private void Awake() {
            instance = this;
        }   

        private void Start() {
            foreach(AudioClip a in manualInput)
                sounds.Add(a.name.GetHashCode(), a);
            return;

            foreach (string s in Directory.GetFiles(@"Packs\Sounds", "*.ogg")){
                try{
                    using(FileStream fs = new FileStream(s, FileMode.Open)){
                        using(BinaryReader br = new BinaryReader(fs)){
                            int counter = 10;
                            while(counter-- > 0){
                                //Capture pattern must be "Oggs"
                                char[] capturePattern = br.ReadChars(4);

                                //Version has to be 0
                                byte version = br.ReadByte();

                                //We have to read some flags from this to know when the stream ends
                                byte headerType = br.ReadByte();
                                //Most of the flags are not super important for the reading process
                                // if((headerType & 1) == 1)
                                //     print("Continued Packet");
                                // else
                                //     print("Fresh Packet");
                                // if((headerType & 2) == 2)
                                //     print("First page of logical bitstream");
                                // else
                                //     print("Not first page of loical bitstream");
                                bool isLastPage = (headerType & 4) == 4;
                                // if((headerType & 4) == 4)
                                    // print("Last page of logical bitstream");
                                // else
                                //     print("Not last page of logical bitstream");

                                //honestly don't really know what this does
                                long granulePosition = br.ReadInt64();

                                //cool serial number
                                int streamSerialNumber = br.ReadInt32();

                                //which page are we currently reading?
                                int pageSequenceNumber = br.ReadInt32();

                                //We don't really care about this. We just hope that all the files we read are corruption free.
                                int pageChecksum = br.ReadInt32();

                                //How many segments are in this page (0-255)
                                byte numSegments = br.ReadByte();
                                //How long are these individual segments
                                byte[] segmentLengths = new byte[numSegments];
                                for(byte i = 0; i < numSegments; i++)
                                    segmentLengths[i] = br.ReadByte();

                                //Read the segments
                                for(byte segment = 0; segment < numSegments; segment++){
                                    byte[] segmentData = new byte[segmentLengths[segment]];
                                    for(byte idx = 0; idx < segmentLengths[segment]; idx++)
                                        segmentData[idx] = br.ReadByte();
                                }
                                
                                if(isLastPage)
                                    break;
                            }
                        }
                    }
                }
                catch(Exception e){
                    Debug.LogError(e.Message);
                }
            }
        }

        public static bool TryGetSound(string name, out AudioClip sound){
            return instance.sounds.TryGetValue(name.GetHashCode(), out sound);
        }
        public static bool TryGetSound(int nameHash, out AudioClip sound){
            return instance.sounds.TryGetValue(nameHash, out sound);
        }
        public static void PlayGlobalSFX(string name){
            if(TryGetSound(name, out AudioClip sound))
                instance.globalSFXSource.PlayOneShot(sound);
        }
        public static void PlayGlobalSFX(int nameHash){
            if(TryGetSound(nameHash, out AudioClip sound))
                instance.globalSFXSource.PlayOneShot(sound);
        }
        public static void PlayMultiGlobalSFX(params string[] names){
            if(names == null || names.Length == 0)
                return;
            if(TryGetSound(names[UnityEngine.Random.Range(0, names.Length)], out AudioClip sound))
                instance.globalSFXSource.PlayOneShot(sound);
        }
        public static void PlayMultiGlobalSFX(params int[] nameHashes){
            if(nameHashes == null || nameHashes.Length == 0)
                return;
            if(TryGetSound(nameHashes[UnityEngine.Random.Range(0, nameHashes.Length)], out AudioClip sound))
                instance.globalSFXSource.PlayOneShot(sound);
        }
    }
}