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

        private void Awake() {
            instance = this;
            world = FindObjectOfType<World>();
        }

        private void Start() {
            smoothLighting.onValueChanged.AddListener(delegate {OnSmoothLightingChange();});
            lightingIterations.onValueChanged.AddListener(delegate {OnLightingIterationsChange();});
            ShowSettings(false);
        }

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

        public void ShowSettings(bool visible = true){
            gameObject.SetActive(visible);
            Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = visible;
        }
    }
}