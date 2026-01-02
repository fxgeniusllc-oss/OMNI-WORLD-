using UnityEngine;
using System.Collections.Generic;

namespace OmniWorld.World
{
    /// <summary>
    /// Transit system for moving between zones and cities
    /// Manages teleportation, vehicles, and travel costs
    /// </summary>
    public class TransitSystem : MonoBehaviour
    {
        private static TransitSystem _instance;
        public static TransitSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TransitSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("TransitSystem");
                        _instance = go.AddComponent<TransitSystem>();
                    }
                }
                return _instance;
            }
        }

        [Header("Transit Settings")]
        public float baseTravelCost = 1.0f; // $OMNI
        public float travelCostPerDistance = 0.01f;
        public bool fastTravelUnlocked = true;

        [Header("Available Cities")]
        public List<string> availableCities = new List<string>
        {
            "OmniLanta",
            "OmniVegas",
            "OmniTokyo",
            "OmniNYC",
            "OmniDubai",
            "OmniLA",
            "OmniParis",
            "OmniLagos"
        };

        private Dictionary<string, CityData> cityRegistry = new Dictionary<string, CityData>();
        private string currentCity = "OmniLanta";

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeCities();
        }

        private void InitializeCities()
        {
            Debug.Log("Transit System Initialized");

            // Initialize all 8 metropolises
            cityRegistry["OmniLanta"] = new CityData
            {
                name = "OmniLanta",
                displayName = "OmniLanta (Atlanta, GA)",
                theme = "Creator Culture, Trap Legacy, Tech Hub",
                coordinates = new Vector2(33.7490f, -84.3880f),
                isUnlocked = true
            };

            cityRegistry["OmniVegas"] = new CityData
            {
                name = "OmniVegas",
                displayName = "OmniVegas (Las Vegas, NV)",
                theme = "High Stakes, Neon Capital, Risk/Reward",
                coordinates = new Vector2(36.1699f, -115.1398f),
                isUnlocked = true
            };

            cityRegistry["OmniTokyo"] = new CityData
            {
                name = "OmniTokyo",
                displayName = "OmniTokyo (Tokyo, JP)",
                theme = "Cyber-Tech, Anime Culture, Nightlife",
                coordinates = new Vector2(35.6762f, 139.6503f),
                isUnlocked = false
            };

            cityRegistry["OmniNYC"] = new CityData
            {
                name = "OmniNYC",
                displayName = "OmniNYC (New York, NY)",
                theme = "Financial Capital, Art Scene, Cultural Hub",
                coordinates = new Vector2(40.7128f, -74.0060f),
                isUnlocked = false
            };

            cityRegistry["OmniDubai"] = new CityData
            {
                name = "OmniDubai",
                displayName = "OmniDubai (Dubai, UAE)",
                theme = "Luxury, Innovation, Global Trade",
                coordinates = new Vector2(25.2048f, 55.2708f),
                isUnlocked = false
            };

            cityRegistry["OmniLA"] = new CityData
            {
                name = "OmniLA",
                displayName = "OmniLA (Los Angeles, CA)",
                theme = "Entertainment Industry, Beach Culture",
                coordinates = new Vector2(34.0522f, -118.2437f),
                isUnlocked = false
            };

            cityRegistry["OmniParis"] = new CityData
            {
                name = "OmniParis",
                displayName = "OmniParis (Paris, FR)",
                theme = "Art, Fashion, Culture, Romance",
                coordinates = new Vector2(48.8566f, 2.3522f),
                isUnlocked = false
            };

            cityRegistry["OmniLagos"] = new CityData
            {
                name = "OmniLagos",
                displayName = "OmniLagos (Lagos, NG)",
                theme = "Afrobeats Capital, Street Energy, Cultural Innovation",
                coordinates = new Vector2(6.5244f, 3.3792f),
                isUnlocked = false
            };

            Debug.Log($"Initialized {cityRegistry.Count} cities");
        }

        /// <summary>
        /// Travel to a different city
        /// Integrates with AirportManager for OmniGate Travel Network
        /// </summary>
        public bool TravelToCity(string cityName, string walletAddress)
        {
            if (!cityRegistry.ContainsKey(cityName))
            {
                Debug.LogWarning($"City not found: {cityName}");
                return false;
            }

            var city = cityRegistry[cityName];

            if (!city.isUnlocked)
            {
                Debug.LogWarning($"City not unlocked: {cityName}");
                return false;
            }

            if (cityName == currentCity)
            {
                Debug.Log("Already in this city");
                return false;
            }

            // Calculate travel cost
            float distance = CalculateDistance(currentCity, cityName);
            float cost = CalculateTravelCost(distance);

            Debug.Log($"Traveling from {currentCity} to {cityName}");
            Debug.Log($"Distance: {distance:F0} km | Cost: {cost:F2} $OMNI");

            // TODO: Deduct cost from wallet
            // TODO: Implement actual scene loading

            currentCity = cityName;
            
            // Notify music biome controller
            MusicBiomeController musicBiome = MusicBiomeController.Instance;
            if (musicBiome != null)
            {
                musicBiome.LoadBiomeForCity(cityName);
            }
            
            // Update city reputation - track visit
            CityReputationSystem repSystem = CityReputationSystem.Instance;
            if (repSystem != null)
            {
                repSystem.AddReputation(cityName, 1, "Visited city");
            }
            
            return true;
        }

        /// <summary>
        /// Calculate distance between two cities
        /// </summary>
        private float CalculateDistance(string from, string to)
        {
            if (!cityRegistry.ContainsKey(from) || !cityRegistry.ContainsKey(to))
                return 0f;

            var fromCoords = cityRegistry[from].coordinates;
            var toCoords = cityRegistry[to].coordinates;

            // Simplified distance calculation
            float dx = toCoords.x - fromCoords.x;
            float dy = toCoords.y - fromCoords.y;
            
            return Mathf.Sqrt(dx * dx + dy * dy) * 111f; // Rough conversion to km
        }

        /// <summary>
        /// Calculate cost of travel based on distance
        /// </summary>
        public float CalculateTravelCost(float distance)
        {
            if (!fastTravelUnlocked)
                return 0f; // Free during alpha

            return baseTravelCost + (distance * travelCostPerDistance);
        }

        /// <summary>
        /// Unlock a city for travel
        /// </summary>
        public bool UnlockCity(string cityName)
        {
            if (!cityRegistry.ContainsKey(cityName))
                return false;

            cityRegistry[cityName].isUnlocked = true;
            Debug.Log($"City unlocked: {cityName}");
            
            return true;
        }

        /// <summary>
        /// Get list of unlocked cities
        /// </summary>
        public List<CityData> GetUnlockedCities()
        {
            List<CityData> unlocked = new List<CityData>();
            
            foreach (var city in cityRegistry.Values)
            {
                if (city.isUnlocked)
                    unlocked.Add(city);
            }

            return unlocked;
        }

        /// <summary>
        /// Get current city
        /// </summary>
        public string GetCurrentCity()
        {
            return currentCity;
        }

        /// <summary>
        /// Get city data
        /// </summary>
        public CityData GetCityData(string cityName)
        {
            if (cityRegistry.ContainsKey(cityName))
                return cityRegistry[cityName];
            
            return null;
        }

        /// <summary>
        /// Teleport within the same city (zone to zone)
        /// </summary>
        public bool TeleportToZone(ZoneType targetZone, Vector3 position)
        {
            Debug.Log($"Teleporting to {targetZone} at position {position}");
            
            // Small cost for intra-city teleportation
            float cost = baseTravelCost * 0.1f;
            
            // TODO: Deduct cost and move player
            
            ZoneController.Instance.EnterZone(targetZone);
            
            return true;
        }
    }

    [System.Serializable]
    public class CityData
    {
        public string name;
        public string displayName;
        public string theme;
        public Vector2 coordinates;
        public bool isUnlocked;
        public int population = 0;
        public float economicActivity = 0f;
    }
}
