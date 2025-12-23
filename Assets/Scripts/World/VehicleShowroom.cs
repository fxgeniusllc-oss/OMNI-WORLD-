using UnityEngine;
using System.Collections.Generic;

namespace OmniWorld.World
{
    /// <summary>
    /// VehicleShowroom - Manages the ultra-modern glass showroom
    /// Features dynamic lighting, lounge zones, and 24/7 window shopping
    /// </summary>
    public class VehicleShowroom : MonoBehaviour
    {
        [Header("Showroom Design")]
        [Tooltip("Ultra-modern glass showroom design enabled")]
        public bool glassShowroomDesign = true;
        
        [Tooltip("Located in prime area")]
        public bool primeLocation = true;
        
        [Header("Lighting System")]
        [Tooltip("Enable dynamic lighting")]
        public bool dynamicLighting = true;
        
        [Tooltip("Spotlight intensity")]
        [Range(0f, 10f)]
        public float spotlightIntensity = 5f;
        
        [Tooltip("Ambient lighting color")]
        public Color ambientColor = new Color(0.2f, 0.2f, 0.3f);
        
        [Tooltip("Accent lighting color")]
        public Color accentColor = new Color(1f, 0.8f, 0.4f);
        
        [Tooltip("Lighting animation speed")]
        [Range(0f, 5f)]
        public float lightingAnimationSpeed = 1f;
        
        [Header("Lounge Zones")]
        [Tooltip("Enable VIP lounge zones")]
        public bool hasLoungeZones = true;
        
        [Tooltip("Number of lounge areas")]
        public int loungeCount = 3;
        
        [Tooltip("Lounge positions in showroom")]
        public List<Vector3> loungePositions = new List<Vector3>();
        
        [Header("Display Configuration")]
        [Tooltip("Vehicle display platforms")]
        public List<Transform> displayPlatforms = new List<Transform>();
        
        [Tooltip("Rotating platforms enabled")]
        public bool rotatingPlatforms = true;
        
        [Tooltip("Platform rotation speed")]
        [Range(0f, 50f)]
        public float rotationSpeed = 10f;
        
        [Tooltip("Spotlight transforms for each platform")]
        public List<Light> spotlights = new List<Light>();
        
        [Header("Window Shopping")]
        [Tooltip("24/7 window shopping enabled")]
        public bool windowShoppingEnabled = true;
        
        [Tooltip("Exterior viewing enabled when closed")]
        public bool exteriorViewingEnabled = true;
        
        [Header("Interactive Features")]
        [Tooltip("Information panels enabled")]
        public bool infoPanelsEnabled = true;
        
        [Tooltip("Virtual test drive enabled")]
        public bool virtualTestDriveEnabled = true;
        
        [Tooltip("360-degree vehicle viewer")]
        public bool vehicle360Viewer = true;
        
        [Header("Audio")]
        [Tooltip("Ambient showroom music")]
        public AudioClip ambientMusic;
        
        [Tooltip("Vehicle engine sound previews")]
        public bool engineSoundPreviews = true;
        
        private AudioSource audioSource;
        private float lightingTimer = 0f;
        private Dictionary<Transform, VehicleNFT> displayedVehicles = new Dictionary<Transform, VehicleNFT>();
        
        private void Start()
        {
            InitializeShowroom();
        }
        
        private void InitializeShowroom()
        {
            Debug.Log("=== Vehicle Showroom Initialized ===");
            Debug.Log($"Design: {(glassShowroomDesign ? "Ultra-Modern Glass" : "Standard")}");
            Debug.Log($"Location: {(primeLocation ? "Prime Area" : "Standard")}");
            Debug.Log($"Display Platforms: {displayPlatforms.Count}");
            Debug.Log($"Lounge Zones: {(hasLoungeZones ? loungeCount.ToString() : "None")}");
            Debug.Log($"24/7 Window Shopping: {(windowShoppingEnabled ? "ENABLED" : "DISABLED")}");
            
            SetupAudioSystem();
            SetupLighting();
            SetupDisplayPlatforms();
            
            if (hasLoungeZones)
            {
                SetupLoungeZones();
            }
        }
        
        private void SetupAudioSystem()
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            if (ambientMusic != null)
            {
                audioSource.clip = ambientMusic;
                audioSource.loop = true;
                audioSource.volume = 0.3f;
                audioSource.Play();
                
                Debug.Log("Ambient showroom music playing");
            }
        }
        
        private void SetupLighting()
        {
            if (!dynamicLighting)
                return;
            
            Debug.Log("Setting up dynamic lighting system...");
            
            // Ensure each display platform has a spotlight
            foreach (var platform in displayPlatforms)
            {
                Light spotlight = platform.GetComponentInChildren<Light>();
                if (spotlight == null)
                {
                    GameObject lightObj = new GameObject("Spotlight");
                    lightObj.transform.SetParent(platform);
                    lightObj.transform.localPosition = new Vector3(0, 5f, 0);
                    lightObj.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                    
                    spotlight = lightObj.AddComponent<Light>();
                    spotlight.type = LightType.Spot;
                    spotlight.intensity = spotlightIntensity;
                    spotlight.range = 15f;
                    spotlight.spotAngle = 60f;
                    spotlight.color = accentColor;
                }
                
                if (!spotlights.Contains(spotlight))
                {
                    spotlights.Add(spotlight);
                }
            }
            
            Debug.Log($"Configured {spotlights.Count} spotlights");
        }
        
        private void SetupDisplayPlatforms()
        {
            Debug.Log($"Setting up {displayPlatforms.Count} display platforms...");
            
            // Initialize platform rotation components
            if (rotatingPlatforms)
            {
                foreach (var platform in displayPlatforms)
                {
                    RotatingPlatform rotator = platform.GetComponent<RotatingPlatform>();
                    if (rotator == null)
                    {
                        rotator = platform.gameObject.AddComponent<RotatingPlatform>();
                        rotator.rotationSpeed = rotationSpeed;
                    }
                }
            }
        }
        
        private void SetupLoungeZones()
        {
            Debug.Log($"Setting up {loungeCount} VIP lounge zones...");
            
            // Initialize lounge positions if empty
            if (loungePositions.Count == 0)
            {
                for (int i = 0; i < loungeCount; i++)
                {
                    // Arrange lounges around the perimeter
                    float angle = (360f / loungeCount) * i;
                    float radius = 25f;
                    Vector3 position = new Vector3(
                        Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                        0f,
                        Mathf.Sin(angle * Mathf.Deg2Rad) * radius
                    );
                    loungePositions.Add(position);
                }
            }
        }
        
        /// <summary>
        /// Display vehicle on platform
        /// </summary>
        public bool DisplayVehicle(VehicleNFT vehicle, int platformIndex)
        {
            if (platformIndex < 0 || platformIndex >= displayPlatforms.Count)
            {
                Debug.LogWarning($"Invalid platform index: {platformIndex}");
                return false;
            }
            
            Transform platform = displayPlatforms[platformIndex];
            
            // Load vehicle prefab (placeholder - actual implementation would load from Resources)
            GameObject vehiclePrefab = LoadVehiclePrefab(vehicle.prefabReference);
            if (vehiclePrefab != null)
            {
                GameObject displayObject = Instantiate(vehiclePrefab, platform);
                displayObject.transform.localPosition = Vector3.zero;
                displayObject.transform.localRotation = Quaternion.identity;
                
                displayedVehicles[platform] = vehicle;
                
                // Activate spotlight
                if (platformIndex < spotlights.Count && spotlights[platformIndex] != null)
                {
                    spotlights[platformIndex].enabled = true;
                }
                
                Debug.Log($"Displaying {vehicle.vehicleName} on platform {platformIndex}");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Remove vehicle from platform
        /// </summary>
        public void RemoveVehicle(int platformIndex)
        {
            if (platformIndex < 0 || platformIndex >= displayPlatforms.Count)
                return;
            
            Transform platform = displayPlatforms[platformIndex];
            
            // Clear platform
            foreach (Transform child in platform)
            {
                if (child.gameObject.name != "Spotlight")
                {
                    Destroy(child.gameObject);
                }
            }
            
            displayedVehicles.Remove(platform);
            
            // Deactivate spotlight
            if (platformIndex < spotlights.Count && spotlights[platformIndex] != null)
            {
                spotlights[platformIndex].enabled = false;
            }
            
            Debug.Log($"Removed vehicle from platform {platformIndex}");
        }
        
        /// <summary>
        /// Get vehicle displayed on platform
        /// </summary>
        public VehicleNFT GetDisplayedVehicle(int platformIndex)
        {
            if (platformIndex < 0 || platformIndex >= displayPlatforms.Count)
                return null;
            
            Transform platform = displayPlatforms[platformIndex];
            if (displayedVehicles.ContainsKey(platform))
            {
                return displayedVehicles[platform];
            }
            
            return null;
        }
        
        /// <summary>
        /// Update dynamic lighting animation
        /// </summary>
        private void Update()
        {
            if (!dynamicLighting)
                return;
            
            lightingTimer += Time.deltaTime * lightingAnimationSpeed;
            
            // Animate spotlight intensity
            for (int i = 0; i < spotlights.Count; i++)
            {
                if (spotlights[i] != null && spotlights[i].enabled)
                {
                    float pulse = Mathf.Sin(lightingTimer + i * 1.5f) * 0.5f + 0.5f;
                    spotlights[i].intensity = spotlightIntensity * (0.8f + pulse * 0.2f);
                }
            }
        }
        
        /// <summary>
        /// Load vehicle prefab (placeholder implementation)
        /// </summary>
        private GameObject LoadVehiclePrefab(string prefabReference)
        {
            // In actual implementation, this would load from Resources or AssetBundle
            // For now, return a placeholder cube
            GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placeholder.name = "VehiclePlaceholder";
            placeholder.transform.localScale = new Vector3(2f, 1f, 4f);
            
            return placeholder;
        }
        
        /// <summary>
        /// Enable/disable window shopping mode
        /// </summary>
        public void SetWindowShoppingMode(bool enabled)
        {
            windowShoppingEnabled = enabled;
            
            if (enabled)
            {
                Debug.Log("24/7 Window Shopping ENABLED");
                // Enable exterior lighting and displays
            }
            else
            {
                Debug.Log("Window Shopping DISABLED");
            }
        }
        
        /// <summary>
        /// Play vehicle engine sound preview
        /// </summary>
        public void PlayEngineSoundPreview(VehicleNFT vehicle, AudioClip engineSound)
        {
            if (!engineSoundPreviews || engineSound == null)
                return;
            
            audioSource.PlayOneShot(engineSound, 0.5f);
            Debug.Log($"Playing engine sound for {vehicle.vehicleName}");
        }
    }
    
    /// <summary>
    /// Rotating platform component for vehicle display
    /// </summary>
    public class RotatingPlatform : MonoBehaviour
    {
        public float rotationSpeed = 10f;
        public bool isRotating = true;
        
        private void Update()
        {
            if (isRotating)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }
        }
        
        public void SetRotating(bool rotating)
        {
            isRotating = rotating;
        }
    }
}
