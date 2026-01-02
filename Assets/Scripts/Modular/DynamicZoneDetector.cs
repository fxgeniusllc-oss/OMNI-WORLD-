using UnityEngine;

namespace OmniWorld.World
{
    /// <summary>
    /// Detects and manages dynamic zone transitions
    /// Handles player zone changes and triggers zone-specific events
    /// </summary>
    public class DynamicZoneDetector : MonoBehaviour
    {
        private static DynamicZoneDetector _instance;
        public static DynamicZoneDetector Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<DynamicZoneDetector>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("DynamicZoneDetector");
                        _instance = go.AddComponent<DynamicZoneDetector>();
                    }
                }
                return _instance;
            }
        }

        [Header("Detection Configuration")]
        public string currentZone = "OmniDowntown";
        public float detectionRadius = 50f;
        public bool autoDetectZones = true;

        [Header("Zone Boundaries")]
        public string[] availableZones = {
            "OmniDowntown",
            "OmniHollywood",
            "OmniCoastline",
            "OmniSuburbs",
            "OmniSouthside",
            "OmniDesert",
            "Vegas Strip Zone"
        };

        public event System.Action<string, string> OnZoneChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("DynamicZoneDetector initialized - Zone detection system ready");
            Debug.Log($"Current Zone: {currentZone}");
            Debug.Log($"Auto-Detection: {(autoDetectZones ? "Enabled" : "Disabled")}");
        }

        public void DetectZone(Vector3 position)
        {
            // Placeholder zone detection logic
            // In production, this would use trigger colliders or spatial partitioning
            Debug.Log($"Detecting zone at position {position}");
        }

        public void ChangeZone(string newZone)
        {
            if (currentZone == newZone)
                return;

            string previousZone = currentZone;
            currentZone = newZone;

            Debug.Log($"=== Zone Changed ===");
            Debug.Log($"Previous: {previousZone}");
            Debug.Log($"Current: {currentZone}");

            // Trigger zone-specific events
            OnZoneChanged?.Invoke(previousZone, currentZone);

            // Update ZoneController
            if (ZoneController.Instance != null)
            {
                // Determine zone type based on name
                ZoneType zoneType = DetermineZoneType(currentZone);
                ZoneController.Instance.EnterZone(zoneType, currentZone);
            }

            // Display zone lore
            if (ZoneLoreManager.Instance != null)
            {
                ZoneLoreManager.Instance.DisplayZoneLore(currentZone);
            }
        }

        private ZoneType DetermineZoneType(string zoneName)
        {
            // Simple zone type determination based on name
            if (zoneName.Contains("Downtown") || zoneName.Contains("Strip"))
                return ZoneType.Commercial;
            else if (zoneName.Contains("Southside"))
                return ZoneType.Recreation;
            else if (zoneName.Contains("Suburbs"))
                return ZoneType.Residential;
            else
                return ZoneType.Commercial;
        }

        public bool IsInZone(string zoneName)
        {
            return currentZone == zoneName;
        }
    }
}
