using UnityEngine;
using System.Collections.Generic;

namespace OmniWorld.World
{
    /// <summary>
    /// Manages zone-specific lore, stories, and narrative content
    /// Procedurally generates zone history and cultural context
    /// </summary>
    public class ZoneLoreManager : MonoBehaviour
    {
        private static ZoneLoreManager _instance;
        public static ZoneLoreManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ZoneLoreManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ZoneLoreManager");
                        _instance = go.AddComponent<ZoneLoreManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Lore Configuration")]
        public bool proceduralLoreEnabled = true;

        private Dictionary<string, ZoneLore> zoneLore = new Dictionary<string, ZoneLore>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeZoneLore();
        }

        private void InitializeZoneLore()
        {
            Debug.Log("ZoneLoreManager initialized - Lore system ready");

            // Initialize lore for OmniVegas zones
            zoneLore["Vegas Strip Zone"] = new ZoneLore
            {
                zoneName = "Vegas Strip Zone",
                description = "The heart of OmniVegas luxury and entertainment",
                history = "Home to OmniLux Auto, the premier dealership for ultra-rare vehicles",
                culture = "High-stakes lifestyle with 24/7 energy"
            };

            zoneLore["OmniDowntown"] = new ZoneLore
            {
                zoneName = "OmniDowntown",
                description = "Business and commercial hub with smart towers",
                history = "Modern financial district with cutting-edge architecture",
                culture = "Fast-paced professional environment"
            };

            zoneLore["OmniSouthside"] = new ZoneLore
            {
                zoneName = "OmniSouthside",
                description = "Underground culture and combat training facilities",
                history = "Home to legendary underground gyms and fight clubs",
                culture = "Gritty, determined, street-level combat scene"
            };

            Debug.Log($"Initialized lore for {zoneLore.Count} zones");
        }

        public ZoneLore GetZoneLore(string zoneName)
        {
            if (zoneLore.ContainsKey(zoneName))
            {
                return zoneLore[zoneName];
            }

            Debug.LogWarning($"No lore found for zone: {zoneName}");
            return null;
        }

        public void DisplayZoneLore(string zoneName)
        {
            ZoneLore lore = GetZoneLore(zoneName);
            if (lore != null)
            {
                Debug.Log($"=== {lore.zoneName} ===");
                Debug.Log(lore.description);
                Debug.Log($"History: {lore.history}");
                Debug.Log($"Culture: {lore.culture}");
            }
        }
    }

    [System.Serializable]
    public class ZoneLore
    {
        public string zoneName;
        public string description;
        public string history;
        public string culture;
    }
}
