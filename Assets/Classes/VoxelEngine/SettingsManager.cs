using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VoxelEngine{
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager instance;
        private World world;

        [Header("Lighting")]
        public Slider lightingIterations;
        public TextMeshProUGUI lightingIterationsLabel;
        public Toggle smoothLighting;

        [Header("Debugging")]
        public Toggle showRegions;
        public Toggle showChunks;

        private void Awake() {
            instance = this;
            world = FindObjectOfType<World>();
        }

        private void Start() {
            smoothLighting.onValueChanged.AddListener(delegate {OnSmoothLightingChange();});
            lightingIterations.onValueChanged.AddListener(delegate {OnLightingIterationsChange();});
            showRegions.onValueChanged.AddListener(delegate {OnShowRegionsChange();});
            showChunks.onValueChanged.AddListener(delegate {OnShowChunksChange();});
            ShowSettings(false);
        }

        //Lighting
        public void OnLightingIterationsChange(){
            int iterations = (int)lightingIterations.value;
            Chunk.lightPropagationIterations = iterations;
            lightingIterationsLabel.text = $"Iterations ({iterations})";
            world.RegenerateChunks();
        }
        public void OnSmoothLightingChange(){
            Chunk.lightMode = smoothLighting.isOn ? Chunk.LightMode.Smooth : Chunk.LightMode.Flat;
            world.RegenerateChunks();
        }
        //Debugging
        public void OnShowRegionsChange(){
            DebugManager.instance.showRegions = showRegions.isOn;
        }
        public void OnShowChunksChange(){
            DebugManager.instance.showChunks = showChunks.isOn;
        }

        public void ShowSettings(bool visible = true){
            gameObject.SetActive(visible);
            Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = visible;
        }
    }
}