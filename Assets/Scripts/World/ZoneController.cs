using UnityEngine;
using System.Collections.Generic;

namespace OmniWorld.World
{
    /// <summary>
    /// Zone controller manages city districts and their properties
    /// Handles zone-specific economics and activities
    /// </summary>
    public class ZoneController : MonoBehaviour
    {
        private static ZoneController _instance;
        public static ZoneController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ZoneController>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ZoneController");
                        _instance = go.AddComponent<ZoneController>();
                    }
                }
                return _instance;
            }
        }

        [Header("Current Zone")]
        public ZoneType currentZone = ZoneType.Residential;
        public string zoneName = "Downtown District";
        
        [Header("Zone Properties")]
        public float propertyValueMultiplier = 1.0f;
        public float activityLevel = 0.5f;
        public int playerCount = 0;

        private Dictionary<ZoneType, ZoneData> zoneRegistry = new Dictionary<ZoneType, ZoneData>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeZones();
        }

        private void InitializeZones()
        {
            Debug.Log("Zone Controller Initialized");
            
            // Initialize zone data for each type
            zoneRegistry[ZoneType.Residential] = new ZoneData
            {
                type = ZoneType.Residential,
                name = "Residential District",
                basePropertyValue = 1000f,
                economicMultiplier = 1.0f,
                description = "Apartments, Condos, Mansions, Penthouses"
            };

            zoneRegistry[ZoneType.Business] = new ZoneData
            {
                type = ZoneType.Business,
                name = "Business District",
                basePropertyValue = 2000f,
                economicMultiplier = 1.5f,
                description = "Offices, Co-working Spaces, Corporate HQs"
            };

            zoneRegistry[ZoneType.Commercial] = new ZoneData
            {
                type = ZoneType.Commercial,
                name = "Commercial District",
                basePropertyValue = 1500f,
                economicMultiplier = 1.3f,
                description = "E-commerce, DApps, Retail Stores"
            };

            zoneRegistry[ZoneType.Recreation] = new ZoneData
            {
                type = ZoneType.Recreation,
                name = "Entertainment District",
                basePropertyValue = 1200f,
                economicMultiplier = 1.2f,
                description = "Parks, Venues, Sports Facilities"
            };

            zoneRegistry[ZoneType.Industrial] = new ZoneData
            {
                type = ZoneType.Industrial,
                name = "Industrial Zone",
                basePropertyValue = 800f,
                economicMultiplier = 1.1f,
                description = "Manufacturing, Processing, Tech Labs"
            };

            Debug.Log($"Initialized {zoneRegistry.Count} zone types");
        }

        /// <summary>
        /// Enter a specific zone
        /// </summary>
        public void EnterZone(ZoneType zone, string customName = "")
        {
            currentZone = zone;
            
            if (zoneRegistry.ContainsKey(zone))
            {
                var zoneData = zoneRegistry[zone];
                zoneName = string.IsNullOrEmpty(customName) ? zoneData.name : customName;
                propertyValueMultiplier = zoneData.economicMultiplier;
                
                Debug.Log($"Entered {zoneName} ({zone})");
                Debug.Log($"Property Value Multiplier: {propertyValueMultiplier}x");
            }
        }

        /// <summary>
        /// Get zone data for a specific zone type
        /// </summary>
        public ZoneData GetZoneData(ZoneType zone)
        {
            if (zoneRegistry.ContainsKey(zone))
            {
                return zoneRegistry[zone];
            }
            
            Debug.LogWarning($"Zone data not found for {zone}");
            return null;
        }

        /// <summary>
        /// Calculate property value based on zone and demand
        /// </summary>
        public float CalculatePropertyValue(ZoneType zone, float baseValue, int demandLevel)
        {
            if (!zoneRegistry.ContainsKey(zone))
                return baseValue;

            var zoneData = zoneRegistry[zone];
            float zoneMultiplier = zoneData.economicMultiplier;
            float demandMultiplier = 1f + (demandLevel * 0.1f);
            
            return baseValue * zoneMultiplier * demandMultiplier;
        }

        /// <summary>
        /// Update activity level based on player count
        /// </summary>
        public void UpdateActivityLevel(int currentPlayers)
        {
            playerCount = currentPlayers;
            activityLevel = Mathf.Clamp01(currentPlayers / 100f); // Max at 100 players
            
            Debug.Log($"Zone activity level: {activityLevel:P0} ({playerCount} players)");
        }

        /// <summary>
        /// Get recommended property types for current zone
        /// </summary>
        public List<string> GetRecommendedPropertyTypes()
        {
            List<string> types = new List<string>();

            switch (currentZone)
            {
                case ZoneType.Residential:
                    types.AddRange(new[] { "Apartment", "Condo", "Penthouse", "Mansion" });
                    break;
                case ZoneType.Business:
                    types.AddRange(new[] { "Office", "Co-working Space", "Corporate HQ" });
                    break;
                case ZoneType.Commercial:
                    types.AddRange(new[] { "Retail Store", "Restaurant", "Shopping Mall" });
                    break;
                case ZoneType.Recreation:
                    types.AddRange(new[] { "Park", "Stadium", "Theater", "Club" });
                    break;
                case ZoneType.Industrial:
                    types.AddRange(new[] { "Factory", "Warehouse", "Tech Lab", "Data Center" });
                    break;
            }

            return types;
        }
    }

    [System.Serializable]
    public class ZoneData
    {
        public ZoneType type;
        public string name;
        public float basePropertyValue;
        public float economicMultiplier;
        public string description;
        public int maxOccupancy = 1000;
        public int currentOccupancy = 0;
    }

    public enum ZoneType
    {
        Residential,    // Apartments, Condos, Mansions
        Business,       // Offices, Corporate HQs
        Commercial,     // Retail, E-commerce
        Recreation,     // Parks, Entertainment
        Industrial      // Manufacturing, Tech Labs
    }
}
