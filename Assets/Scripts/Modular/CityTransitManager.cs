using UnityEngine;
using System.Collections.Generic;

namespace OmniWorld.World
{
    /// <summary>
    /// Manages city transit system across urban zones
    /// Handles fast travel, public transportation, and vehicle spawning
    /// </summary>
    public class CityTransitManager : MonoBehaviour
    {
        private static CityTransitManager _instance;
        public static CityTransitManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CityTransitManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CityTransitManager");
                        _instance = go.AddComponent<CityTransitManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Transit Configuration")]
        public float fastTravelCost = 25f;
        public float taxiBaseFare = 10f;
        public float taxiPerMileCost = 2f;

        [Header("Available Zones")]
        public string[] urbanZones = {
            "OmniDowntown",
            "OmniHollywood",
            "OmniCoastline",
            "OmniSuburbs",
            "OmniSouthside",
            "OmniDesert"
        };

        // Distance matrix for deterministic zone-to-zone distances (in miles)
        private Dictionary<string, Dictionary<string, float>> zoneDistances;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeDistanceMatrix();
            
            Debug.Log("CityTransitManager initialized - Transit system ready");
            Debug.Log($"Available zones: {string.Join(", ", urbanZones)}");
        }

        private void InitializeDistanceMatrix()
        {
            // Initialize deterministic distance matrix between zones
            zoneDistances = new Dictionary<string, Dictionary<string, float>>();
            
            // OmniDowntown distances
            zoneDistances["OmniDowntown"] = new Dictionary<string, float>
            {
                { "OmniDowntown", 0f },
                { "OmniHollywood", 3.5f },
                { "OmniCoastline", 5.2f },
                { "OmniSuburbs", 8.1f },
                { "OmniSouthside", 4.8f },
                { "OmniDesert", 12.3f }
            };

            // OmniHollywood distances
            zoneDistances["OmniHollywood"] = new Dictionary<string, float>
            {
                { "OmniDowntown", 3.5f },
                { "OmniHollywood", 0f },
                { "OmniCoastline", 6.7f },
                { "OmniSuburbs", 5.4f },
                { "OmniSouthside", 7.2f },
                { "OmniDesert", 15.8f }
            };

            // OmniCoastline distances
            zoneDistances["OmniCoastline"] = new Dictionary<string, float>
            {
                { "OmniDowntown", 5.2f },
                { "OmniHollywood", 6.7f },
                { "OmniCoastline", 0f },
                { "OmniSuburbs", 9.3f },
                { "OmniSouthside", 8.9f },
                { "OmniDesert", 18.1f }
            };

            // OmniSuburbs distances
            zoneDistances["OmniSuburbs"] = new Dictionary<string, float>
            {
                { "OmniDowntown", 8.1f },
                { "OmniHollywood", 5.4f },
                { "OmniCoastline", 9.3f },
                { "OmniSuburbs", 0f },
                { "OmniSouthside", 6.5f },
                { "OmniDesert", 10.7f }
            };

            // OmniSouthside distances
            zoneDistances["OmniSouthside"] = new Dictionary<string, float>
            {
                { "OmniDowntown", 4.8f },
                { "OmniHollywood", 7.2f },
                { "OmniCoastline", 8.9f },
                { "OmniSuburbs", 6.5f },
                { "OmniSouthside", 0f },
                { "OmniDesert", 14.2f }
            };

            // OmniDesert distances
            zoneDistances["OmniDesert"] = new Dictionary<string, float>
            {
                { "OmniDowntown", 12.3f },
                { "OmniHollywood", 15.8f },
                { "OmniCoastline", 18.1f },
                { "OmniSuburbs", 10.7f },
                { "OmniSouthside", 14.2f },
                { "OmniDesert", 0f }
            };

            Debug.Log("Zone distance matrix initialized with deterministic values");
        }

        public void FastTravel(string destinationZone, string walletAddress)
        {
            Debug.Log($"Fast traveling to {destinationZone}");
            Debug.Log($"Cost: {fastTravelCost} OMNI");
            // TODO: Process payment and teleport player
        }

        public void CallTaxi(string destinationZone, string walletAddress)
        {
            float distance = CalculateDistance(DynamicZoneDetector.Instance?.currentZone ?? "OmniDowntown", destinationZone);
            float totalCost = taxiBaseFare + (distance * taxiPerMileCost);
            Debug.Log($"Taxi called to {destinationZone}");
            Debug.Log($"Distance: {distance:F2} miles");
            Debug.Log($"Cost: {totalCost:F2} OMNI");
            // TODO: Spawn taxi and process payment
        }

        private float CalculateDistance(string fromZone, string toZone)
        {
            // Use distance matrix for deterministic distances
            if (zoneDistances != null && 
                zoneDistances.ContainsKey(fromZone) && 
                zoneDistances[fromZone].ContainsKey(toZone))
            {
                return zoneDistances[fromZone][toZone];
            }
            
            // Fallback for unknown zones
            Debug.LogWarning($"Distance not found for {fromZone} to {toZone}, using default");
            return 5f;
        }
    }
}
