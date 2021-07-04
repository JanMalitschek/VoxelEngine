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

        private Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();

        private void Awake() {
            instance = this;
        }   

        private void Start() {
            foreach (string s in Directory.GetFiles(@"Packs\Sounds", "*.ogg")){
                try{
                               
                }
                catch(Exception e){
                    Debug.LogError(e.Message);
                }
            }
        }
    }
}