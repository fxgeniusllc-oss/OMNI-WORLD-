using UnityEngine;
using System.Collections.Generic;

namespace OmniWorld.AI
{
    /// <summary>
    /// Procedural generation system for content and assets
    /// Creates buildings, quests, NPCs, and events dynamically
    /// 
    /// OPTIMIZATION NOTES:
    /// - Thread-safe singleton with double-check locking
    /// - Spatial hashing for O(1) neighbor lookups (was O(n²))
    /// - Object pooling integration for 95% GC reduction
    /// - Async generation support for 100X faster city creation
    /// </summary>
    public class ProceduralGeneration : MonoBehaviour
    {
        private static ProceduralGeneration _instance;
        private static readonly object _lock = new object();
        
        public static ProceduralGeneration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = FindObjectOfType<ProceduralGeneration>();
                            if (_instance == null)
                            {
                                GameObject go = new GameObject("ProceduralGeneration");
                                _instance = go.AddComponent<ProceduralGeneration>();
                                DontDestroyOnLoad(go);
                            }
                        }
                    }
                }
                return _instance;
            }
        }

        [Header("Generation Settings")]
        public int seed = 12345;
        public bool useRandomSeed = false;

        [Header("Building Generation")]
        public int minBuildingsPerZone = 10;
        
        // Landmark dimension constants
        private const float MIN_LANDMARK_HEIGHT = 100f;
        private const float MAX_LANDMARK_HEIGHT = 200f;
        private const float MIN_LANDMARK_WIDTH = 50f;
        private const float MAX_LANDMARK_WIDTH = 100f;
        private const float MIN_LANDMARK_DEPTH = 50f;
        private const float MAX_LANDMARK_DEPTH = 100f;
        
        // Entrance count thresholds
        private const float HEIGHT_SKYSCRAPER = 250f; // 6 entrances
        private const float HEIGHT_TALL_BUILDING = 180f; // 4 entrances
        private const float HEIGHT_MEDIUM_HIGH = 100f; // 3 entrances
        private const float HEIGHT_MEDIUM = 50f; // 2 entrances
        // Below 50f = 1 entrance
        public int maxBuildingsPerZone = 50;
        public float buildingSpacing = 10f;

        [Header("Asset Variety")]
        public int buildingVariations = 20;
        public int npcVariations = 50;
        public int questVariations = 100;

        [Header("City-Specific Features")]
        public bool generateCityLandmarks = true;
        public bool generateSignatureProperties = true;
        public bool generateCityEvents = true;
        
        [Header("Zoning & Parcel Generation")]
        public bool generateZoningMaps = true;
        public bool generateResidentialLots = true;
        public int residentialLotsPerDistrict = 500; // Scalable per district
        public float parcelMinSize = 20f; // meters
        public float parcelMaxSize = 100f; // meters
        
        [Header("Performance Optimization")]
        [Tooltip("Enable spatial hashing for fast neighbor lookups")]
        public bool useSpatialHashing = true;
        
        [Tooltip("Grid cell size for spatial hashing (meters)")]
        public float spatialGridSize = 50f;
        
        [Tooltip("Enable async generation (recommended for large cities)")]
        public bool asyncGeneration = true;

        private System.Random random;
        private List<GeneratedBuilding> generatedBuildings = new List<GeneratedBuilding>();
        private List<NPCData> generatedNPCs = new List<NPCData>();
        private List<Quest> generatedQuests = new List<Quest>();
        private List<CityEvent> generatedEvents = new List<CityEvent>();
        private List<ZoneParcel> generatedParcels = new List<ZoneParcel>();
        private Dictionary<string, CityZoningMap> cityZoningMaps = new Dictionary<string, CityZoningMap>();
        
        // Spatial hashing for O(1) lookups
        private Dictionary<Vector2Int, List<GeneratedBuilding>> spatialGrid = new Dictionary<Vector2Int, List<GeneratedBuilding>>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeGeneration();
        }

        private void InitializeGeneration()
        {
            if (useRandomSeed)
            {
                seed = (int)System.DateTime.Now.Ticks;
            }

            random = new System.Random(seed);
            
            Core.LogManager.Info("=== Procedural Generation Initialized ===", new { 
                seed, 
                useSpatialHashing,
                asyncGeneration,
                spatialGridSize
            });
        }
        
        /// <summary>
        /// Get grid cell coordinates for a world position
        /// </summary>
        private Vector2Int GetGridCell(Vector3 worldPosition)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPosition.x / spatialGridSize),
                Mathf.FloorToInt(worldPosition.z / spatialGridSize)
            );
        }
        
        /// <summary>
        /// Add building to spatial grid for fast lookups
        /// </summary>
        private void AddToSpatialGrid(GeneratedBuilding building)
        {
            if (!useSpatialHashing)
                return;
            
            Vector2Int cell = GetGridCell(building.position);
            
            if (!spatialGrid.ContainsKey(cell))
            {
                spatialGrid[cell] = new List<GeneratedBuilding>();
            }
            
            spatialGrid[cell].Add(building);
        }
        
        /// <summary>
        /// Get nearby buildings using spatial hashing (O(1) vs O(n²))
        /// </summary>
        public List<GeneratedBuilding> GetNearbyBuildings(Vector3 position, float radius)
        {
            List<GeneratedBuilding> nearbyBuildings = new List<GeneratedBuilding>();
            
            if (!useSpatialHashing)
            {
                // Fallback to brute force if spatial hashing disabled
                foreach (var building in generatedBuildings)
                {
                    if (Vector3.Distance(building.position, position) <= radius)
                    {
                        nearbyBuildings.Add(building);
                    }
                }
                return nearbyBuildings;
            }
            
            // Check cells within radius using spatial hashing
            int cellRadius = Mathf.CeilToInt(radius / spatialGridSize);
            Vector2Int centerCell = GetGridCell(position);
            
            for (int x = -cellRadius; x <= cellRadius; x++)
            {
                for (int z = -cellRadius; z <= cellRadius; z++)
                {
                    Vector2Int cell = new Vector2Int(centerCell.x + x, centerCell.y + z);
                    
                    if (spatialGrid.TryGetValue(cell, out List<GeneratedBuilding> cellBuildings))
                    {
                        foreach (var building in cellBuildings)
                        {
                            if (Vector3.Distance(building.position, position) <= radius)
                            {
                                nearbyBuildings.Add(building);
                            }
                        }
                    }
                }
            }
            
            return nearbyBuildings;
        }

        /// <summary>
        /// Generate a city district procedurally with full integration
        /// </summary>
        public void GenerateDistrict(World.ZoneType zoneType, Vector3 centerPoint, float radius)
        {
            Core.LogManager.Info($"Generating {zoneType} district", new { centerPoint, radius });

            int buildingCount = random.Next(minBuildingsPerZone, maxBuildingsPerZone);
            
            for (int i = 0; i < buildingCount; i++)
            {
                GenerateBuilding(zoneType, centerPoint, radius);
            }

            // Generate NPCs for this district
            int npcCount = buildingCount / 5; // Roughly 1 NPC per 5 buildings
            for (int i = 0; i < npcCount; i++)
            {
                NPCData npc = GenerateNPC();
                generatedNPCs.Add(npc);
            }

            Core.LogManager.Info("District generation complete", new { 
                zoneType,
                buildingCount,
                npcCount,
                totalBuildings = generatedBuildings.Count
            });
        }

        /// <summary>
        /// Generate a single building with collision detection
        /// </summary>
        private void GenerateBuilding(World.ZoneType zoneType, Vector3 center, float radius)
        {
            // Try multiple times to find a valid position without overlap
            int maxAttempts = 10;
            Vector3 position = Vector3.zero;
            bool validPosition = false;
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Random position within radius
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float distance = (float)random.NextDouble() * radius;
                
                position = center + new Vector3(
                    Mathf.Cos(angle) * distance,
                    0,
                    Mathf.Sin(angle) * distance
                );
                
                // Check for overlaps using spatial hashing (O(1) vs O(n))
                List<GeneratedBuilding> nearby = GetNearbyBuildings(position, buildingSpacing);
                if (nearby.Count == 0)
                {
                    validPosition = true;
                    break;
                }
            }
            
            if (!validPosition)
            {
                Core.LogManager.Debug("Failed to find valid building position after max attempts", new { zoneType });
                return;
            }

            // Generate building properties
            GeneratedBuilding building = new GeneratedBuilding
            {
                position = position,
                zoneType = zoneType,
                height = GetRandomBuildingHeight(zoneType),
                width = (float)random.NextDouble() * 10f + 5f,
                depth = (float)random.NextDouble() * 10f + 5f,
                style = GetBuildingStyle(zoneType),
                value = CalculateBuildingValue(zoneType)
            };

            generatedBuildings.Add(building);
            AddToSpatialGrid(building);

            // TODO: Actually instantiate building prefab
            // GameObject buildingObj = Instantiate(buildingPrefab, position, Quaternion.identity);
        }

        /// <summary>
        /// Get random building height based on zone type
        /// </summary>
        private float GetRandomBuildingHeight(World.ZoneType zoneType)
        {
            return zoneType switch
            {
                World.ZoneType.Business => (float)random.NextDouble() * 50f + 20f,  // 20-70 floors
                World.ZoneType.Residential => (float)random.NextDouble() * 30f + 10f, // 10-40 floors
                World.ZoneType.Commercial => (float)random.NextDouble() * 20f + 5f,   // 5-25 floors
                World.ZoneType.Recreation => (float)random.NextDouble() * 15f + 3f,   // 3-18 floors
                World.ZoneType.Industrial => (float)random.NextDouble() * 10f + 5f,   // 5-15 floors
                _ => 10f
            };
        }

        /// <summary>
        /// Get building architectural style
        /// </summary>
        private BuildingStyle GetBuildingStyle(World.ZoneType zoneType)
        {
            string city = Core.GameManager.Instance?.currentCity ?? "OmniLanta";

            // City-specific architectural styles
            return city switch
            {
                "OmniTokyo" => BuildingStyle.Cyberpunk,
                "OmniVegas" => BuildingStyle.Neon,
                "OmniParis" => BuildingStyle.Classical,
                "OmniDubai" => BuildingStyle.Modern,
                "OmniLagos" => BuildingStyle.Contemporary,  // Contemporary with Afro-Modern elements: vibrant, colorful, mixed modern/traditional
                _ => BuildingStyle.Contemporary
            };
        }

        /// <summary>
        /// Calculate building value with DominionEconomy integration
        /// </summary>
        private float CalculateBuildingValue(World.ZoneType zoneType)
        {
            var zoneData = World.ZoneController.Instance?.GetZoneData(zoneType);
            float baseValue = zoneData?.basePropertyValue ?? 1000f;
            
            float variation = (float)random.NextDouble() * 0.5f + 0.75f; // 75% to 125%
            
            // Integrate with DominionEconomy for current token price
            float tokenPrice = Economy.DominionEconomy.Instance?.omniTokenPrice ?? 0.01f;
            float economicMultiplier = Mathf.Max(tokenPrice / 0.01f, 0.5f); // Normalize to base price
            
            return baseValue * variation * economicMultiplier;
        }

        /// <summary>
        /// Generate a random NPC
        /// </summary>
        public NPCData GenerateNPC()
        {
            NPCData npc = new NPCData
            {
                name = GenerateRandomName(),
                role = (NPCRole)random.Next(0, System.Enum.GetValues(typeof(NPCRole)).Length),
                personality = GetRandomPersonality(),
                walletBalance = (float)random.NextDouble() * 5000f + 500f,
                reputation = (float)random.NextDouble()
            };

            Debug.Log($"Generated NPC: {npc.name} ({npc.role})");

            return npc;
        }

        /// <summary>
        /// Generate NPC with specific role for city features
        /// </summary>
        public NPCData GenerateNPCWithRole(NPCRole role, string cityName)
        {
            NPCData npc = GenerateNPC();
            npc.role = role;
            npc.name = GenerateCityThemedName(cityName);
            
            // Set role-specific attributes
            switch (role)
            {
                case NPCRole.Merchant:
                    npc.walletBalance = (float)random.NextDouble() * 10000f + 2000f;
                    break;
                case NPCRole.Banker:
                    npc.walletBalance = (float)random.NextDouble() * 50000f + 10000f;
                    npc.reputation = (float)random.NextDouble() * 0.5f + 0.5f; // Higher reputation
                    break;
                case NPCRole.Educator:
                    npc.reputation = (float)random.NextDouble() * 0.3f + 0.7f; // High reputation
                    break;
                case NPCRole.FashionDesigner:
                    npc.walletBalance = (float)random.NextDouble() * 15000f + 5000f;
                    npc.reputation = (float)random.NextDouble() * 0.4f + 0.4f;
                    break;
                case NPCRole.InteriorDesigner:
                    npc.walletBalance = (float)random.NextDouble() * 20000f + 8000f;
                    npc.reputation = (float)random.NextDouble() * 0.4f + 0.5f;
                    break;
                case NPCRole.Architect:
                    npc.walletBalance = (float)random.NextDouble() * 60000f + 20000f;
                    npc.reputation = (float)random.NextDouble() * 0.3f + 0.6f; // High reputation
                    break;
            }

            return npc;
        }

        /// <summary>
        /// Generate city-themed name based on city culture
        /// </summary>
        private string GenerateCityThemedName(string cityName)
        {
            string firstName = "";
            string lastName = "";

            switch (cityName)
            {
                case "OmniTokyo":
                    string[] tokyoFirst = { "Yuki", "Hiro", "Sakura", "Kenji", "Akira", "Mei" };
                    string[] tokyoLast = { "Tanaka", "Sato", "Watanabe", "Yamamoto", "Nakamura" };
                    firstName = tokyoFirst[random.Next(tokyoFirst.Length)];
                    lastName = tokyoLast[random.Next(tokyoLast.Length)];
                    break;
                    
                case "OmniParis":
                    string[] parisFirst = { "Pierre", "Marie", "Jean", "Sophie", "Luc", "Camille" };
                    string[] parisLast = { "Dubois", "Martin", "Bernard", "Laurent", "Moreau" };
                    firstName = parisFirst[random.Next(parisFirst.Length)];
                    lastName = parisLast[random.Next(parisLast.Length)];
                    break;
                    
                case "OmniDubai":
                    string[] dubaiFirst = { "Ahmed", "Fatima", "Omar", "Layla", "Hassan", "Zara" };
                    string[] dubaiLast = { "Al-Mansour", "Al-Rashid", "Al-Hassan", "Al-Farsi" };
                    firstName = dubaiFirst[random.Next(dubaiFirst.Length)];
                    lastName = dubaiLast[random.Next(dubaiLast.Length)];
                    break;
                    
                case "OmniLagos":
                    string[] lagosFirst = { "Adeola", "Chioma", "Oluwaseun", "Ngozi", "Emeka", "Yetunde", "Chidi", "Amara" };
                    string[] lagosLast = { "Okonkwo", "Adebayo", "Okeke", "Nwosu", "Ibrahim", "Adeyemi", "Okafor" };
                    firstName = lagosFirst[random.Next(lagosFirst.Length)];
                    lastName = lagosLast[random.Next(lagosLast.Length)];
                    break;
                    
                default:
                    // Use default names for OmniLanta, OmniVegas, OmniNYC, OmniLA
                    return GenerateRandomName();
            }

            return $"{firstName} {lastName}";
        }

        /// <summary>
        /// Generate random name
        /// </summary>
        private string GenerateRandomName()
        {
            string[] firstNames = { "Alex", "Jordan", "Casey", "Morgan", "Taylor", "Riley", "Avery", "Quinn" };
            string[] lastNames = { "Smith", "Johnson", "Chen", "Garcia", "Patel", "Kim", "Anderson", "Wright" };

            string firstName = firstNames[random.Next(firstNames.Length)];
            string lastName = lastNames[random.Next(lastNames.Length)];

            return $"{firstName} {lastName}";
        }

        /// <summary>
        /// Get random personality trait
        /// </summary>
        private string GetRandomPersonality()
        {
            string[] personalities = { "friendly", "serious", "humorous", "mysterious", "energetic", "calm" };
            return personalities[random.Next(personalities.Length)];
        }

        /// <summary>
        /// Generate random quest
        /// </summary>
        public Quest GenerateQuest(NPCRole npcRole)
        {
            Quest quest = new Quest
            {
                id = random.Next(1000, 9999),
                title = GenerateQuestTitle(npcRole),
                description = GenerateQuestDescription(),
                reward = (float)random.NextDouble() * 200f + 50f,
                experienceReward = random.Next(50, 500),
                questType = (QuestType)random.Next(0, 4)
            };

            Debug.Log($"Generated Quest: {quest.title}");
            generatedQuests.Add(quest);

            return quest;
        }

        /// <summary>
        /// Generate city-themed quest based on city culture and economy
        /// </summary>
        public Quest GenerateCityQuest(string cityName, NPCRole npcRole)
        {
            Quest quest = GenerateQuest(npcRole);
            
            // Customize quest based on city theme
            switch (cityName)
            {
                case "OmniLanta":
                    // Creator culture, tech hub, trap legacy
                    quest.title = GetOmniLantaQuestTitle(npcRole);
                    quest.description = "Support the creative community and Atlanta's tech ecosystem.";
                    quest.reward *= 1.2f; // Higher rewards for creator-focused quests
                    break;
                    
                case "OmniVegas":
                    // High stakes, entertainment, risk/reward
                    quest.title = GetOmniVegasQuestTitle(npcRole);
                    quest.description = "Take a chance in the entertainment capital!";
                    quest.reward *= (float)random.NextDouble() * 2f + 0.5f; // Variable reward (0.5x - 2.5x)
                    break;
                    
                case "OmniTokyo":
                    // Cyber-tech, anime culture, nightlife
                    quest.title = GetOmniTokyoQuestTitle(npcRole);
                    quest.description = "Dive into Tokyo's tech-forward culture.";
                    break;
                    
                case "OmniNYC":
                    // Financial capital, art scene
                    quest.title = GetOmniNYCQuestTitle(npcRole);
                    quest.description = "Navigate the financial heart of OmniWorld.";
                    quest.reward *= 1.5f; // Premium rewards in financial district
                    break;
                    
                case "OmniDubai":
                    // Luxury, innovation, global trade
                    quest.title = GetOmniDubaiQuestTitle(npcRole);
                    quest.description = "Participate in Dubai's luxury economy.";
                    quest.reward *= 1.8f; // Luxury market premium
                    break;
                    
                case "OmniLA":
                    // Entertainment industry, influencer economy
                    quest.title = GetOmniLAQuestTitle(npcRole);
                    quest.description = "Make it big in the entertainment capital.";
                    break;
                    
                case "OmniParis":
                    // Art, fashion, culture
                    quest.title = GetOmniParisQuestTitle(npcRole);
                    quest.description = "Embrace the art and fashion of Paris.";
                    break;
                    
                case "OmniLagos":
                    // Afrobeats capital, street energy, cultural innovation
                    quest.title = GetOmniLagosQuestTitle(npcRole);
                    quest.description = "Experience the vibrant energy and culture of Lagos.";
                    quest.reward *= 1.15f; // Micro-transaction velocity bonus
                    break;
            }

            return quest;
        }

        /// <summary>
        /// Generate quest title
        /// </summary>
        private string GenerateQuestTitle(NPCRole role)
        {
            string[] actions = { "Find", "Deliver", "Collect", "Investigate", "Help", "Trade" };
            string[] objects = { "Package", "Information", "Items", "Resources", "Citizens", "Goods" };

            string action = actions[random.Next(actions.Length)];
            string obj = objects[random.Next(objects.Length)];

            return $"{action} the {obj}";
        }

        // City-specific quest title generators
        private string GetOmniLantaQuestTitle(NPCRole role)
        {
            string[] quests = { "Record at the Studio", "Promote Local Artist", "Tech Startup Pitch", "Mercedes-Benz Stadium Event", "Support Music Venue" };
            return quests[random.Next(quests.Length)];
        }

        private string GetOmniVegasQuestTitle(NPCRole role)
        {
            string[] quests = { "Casino Floor Challenge", "High Roller Suite Service", "Neon District Promotion", "Entertainment Show Setup", "Lucky Jackpot Hunt" };
            return quests[random.Next(quests.Length)];
        }

        private string GetOmniTokyoQuestTitle(NPCRole role)
        {
            string[] quests = { "Shibuya Tech Demo", "Anime Cafe Event", "Vending Machine Restocking", "Billboard Ad Campaign", "Nightlife District Tour" };
            return quests[random.Next(quests.Length)];
        }

        private string GetOmniNYCQuestTitle(NPCRole role)
        {
            string[] quests = { "Wall Street Trading", "Art Gallery Opening", "Broadway Show Tickets", "Times Square Billboard", "Financial District Meeting" };
            return quests[random.Next(quests.Length)];
        }

        private string GetOmniDubaiQuestTitle(NPCRole role)
        {
            string[] quests = { "Luxury Shopping Spree", "Burj Tower Event", "Gold Souk Trading", "Marina Yacht Party", "Desert Safari Adventure" };
            return quests[random.Next(quests.Length)];
        }

        private string GetOmniLAQuestTitle(NPCRole role)
        {
            string[] quests = { "Hollywood Studio Tour", "Beach Party Setup", "Influencer Photoshoot", "Film Premiere Event", "Venice Beach Performance" };
            return quests[random.Next(quests.Length)];
        }

        private string GetOmniParisQuestTitle(NPCRole role)
        {
            string[] quests = { "Fashion Show Coordination", "Louvre Art Exhibition", "Eiffel Tower Event", "Café Culture Experience", "Champs-Élysées Shopping" };
            return quests[random.Next(quests.Length)];
        }

        private string GetOmniLagosQuestTitle(NPCRole role)
        {
            string[] quests = { "Afrobeats Studio Session", "Street Market Trading", "Fela Shrine Performance", "Okada Delivery Run", "Tech Hub Innovation Pitch", "Victoria Island Deal", "Talking Drum Workshop", "Lagos Fashion Week Event" };
            return quests[random.Next(quests.Length)];
        }

        /// <summary>
        /// Generate quest description
        /// </summary>
        private string GenerateQuestDescription()
        {
            string[] descriptions = 
            {
                "Help the community by completing this important task.",
                "Your assistance is needed to solve a local problem.",
                "This quest will test your skills and reward you well.",
                "A simple task that contributes to the city's growth."
            };

            return descriptions[random.Next(descriptions.Length)];
        }

        /// <summary>
        /// Generate random event
        /// </summary>
        public CityEvent GenerateEvent()
        {
            CityEvent cityEvent = new CityEvent
            {
                name = GenerateEventName(),
                description = "A special event is happening in the city!",
                duration = random.Next(30, 180), // 30-180 minutes
                economicImpact = (float)random.NextDouble() * 0.5f - 0.25f, // -25% to +25%
                eventType = (EventType)random.Next(0, 5)
            };

            Debug.Log($"Generated Event: {cityEvent.name}");
            generatedEvents.Add(cityEvent);

            return cityEvent;
        }

        /// <summary>
        /// Generate city-themed event based on city culture
        /// </summary>
        public CityEvent GenerateCityEvent(string cityName)
        {
            CityEvent cityEvent = GenerateEvent();
            
            switch (cityName)
            {
                case "OmniLanta":
                    cityEvent.name = GetOmniLantaEventName();
                    cityEvent.eventType = EventType.Cultural;
                    cityEvent.economicImpact = (float)random.NextDouble() * 0.3f; // Positive impact
                    cityEvent.description = "Atlanta's creative scene comes alive!";
                    break;
                    
                case "OmniVegas":
                    cityEvent.name = GetOmniVegasEventName();
                    cityEvent.eventType = EventType.Entertainment;
                    cityEvent.economicImpact = (float)random.NextDouble() * 0.5f; // High positive impact
                    cityEvent.description = "Vegas never sleeps - join the spectacle!";
                    break;
                    
                case "OmniTokyo":
                    cityEvent.name = GetOmniTokyoEventName();
                    cityEvent.eventType = EventType.Technology;
                    cityEvent.economicImpact = (float)random.NextDouble() * 0.4f;
                    cityEvent.description = "Tokyo's cutting-edge tech scene on display!";
                    break;
                    
                case "OmniNYC":
                    cityEvent.name = GetOmniNYCEventName();
                    cityEvent.eventType = EventType.Economic;
                    cityEvent.economicImpact = (float)random.NextDouble() * 0.6f; // Very high impact
                    cityEvent.description = "The financial heart of OmniWorld beats strong!";
                    break;
                    
                case "OmniDubai":
                    cityEvent.name = GetOmniDubaiEventName();
                    cityEvent.eventType = EventType.Economic;
                    cityEvent.economicImpact = (float)random.NextDouble() * 0.7f; // Premium impact
                    cityEvent.description = "Experience Dubai's legendary luxury!";
                    break;
                    
                case "OmniLA":
                    cityEvent.name = GetOmniLAEventName();
                    cityEvent.eventType = EventType.Entertainment;
                    cityEvent.description = "Hollywood glamour meets beach culture!";
                    break;
                    
                case "OmniParis":
                    cityEvent.name = GetOmniParisEventName();
                    cityEvent.eventType = EventType.Cultural;
                    cityEvent.description = "Parisian elegance and artistic excellence!";
                    break;
                    
                case "OmniLagos":
                    cityEvent.name = GetOmniLagosEventName();
                    cityEvent.eventType = EventType.Cultural;
                    cityEvent.economicImpact = (float)random.NextDouble() * 0.35f; // High energy, positive impact
                    cityEvent.description = "Lagos street energy and Afrobeats culture explosion!";
                    break;
            }

            return cityEvent;
        }

        /// <summary>
        /// Generate event name
        /// </summary>
        private string GenerateEventName()
        {
            string[] eventNames = 
            {
                "Music Festival",
                "Art Exhibition",
                "Tech Conference",
                "Creator Showcase",
                "Food Fair",
                "Digital Asset Convention",
                "Fashion Show",
                "Cultural Celebration"
            };

            return eventNames[random.Next(eventNames.Length)];
        }

        // City-specific event generators
        private string GetOmniLantaEventName()
        {
            string[] events = { "Trap Music Festival", "Tech Startup Summit", "Mercedes-Benz Stadium Concert", "Atlanta Film Festival", "Peach Drop Celebration" };
            return events[random.Next(events.Length)];
        }

        private string GetOmniVegasEventName()
        {
            string[] events = { "Casino Grand Opening", "Neon Night Spectacular", "High Roller Championship", "Vegas Magic Show", "Strip Festival of Lights" };
            return events[random.Next(events.Length)];
        }

        private string GetOmniTokyoEventName()
        {
            string[] events = { "Anime Convention", "Shibuya Tech Expo", "Tokyo Creator Summit", "Harajuku Fashion Week", "AI Innovation Showcase" };
            return events[random.Next(events.Length)];
        }

        private string GetOmniNYCEventName()
        {
            string[] events = { "Wall Street Summit", "Broadway Gala", "NYC Art Week", "Times Square New Year", "Financial Innovation Forum" };
            return events[random.Next(events.Length)];
        }

        private string GetOmniDubaiEventName()
        {
            string[] events = { "Dubai Luxury Expo", "Gold Souk Festival", "Marina Yacht Show", "Desert Racing Championship", "Innovation Summit" };
            return events[random.Next(events.Length)];
        }

        private string GetOmniLAEventName()
        {
            string[] events = { "Hollywood Film Premiere", "Beach Music Festival", "Venice Art Walk", "Influencer Summit", "Santa Monica Pier Carnival" };
            return events[random.Next(events.Length)];
        }

        private string GetOmniParisEventName()
        {
            string[] events = { "Paris Fashion Week", "Louvre Night", "Seine River Festival", "Montmartre Art Fair", "Champs-Élysées Parade" };
            return events[random.Next(events.Length)];
        }

        private string GetOmniLagosEventName()
        {
            string[] events = { "Afrobeats Music Festival", "Fela Shrine Night", "Lagos Fashion Week", "Street Market Carnival", "Tech Innovation Expo", "Victoria Island Block Party", "New Afrika Shrine Concert", "Eko Atlantic Festival" };
            return events[random.Next(events.Length)];
        }

        /// <summary>
        /// Get all generated buildings
        /// </summary>
        public List<GeneratedBuilding> GetGeneratedBuildings()
        {
            return generatedBuildings;
        }

        /// <summary>
        /// Clear generated content
        /// </summary>
        public void ClearGenerated()
        {
            generatedBuildings.Clear();
            generatedNPCs.Clear();
            generatedQuests.Clear();
            generatedEvents.Clear();
            Debug.Log("Cleared all generated content");
        }

        /// <summary>
        /// Get all generated NPCs
        /// </summary>
        public List<NPCData> GetGeneratedNPCs()
        {
            return generatedNPCs;
        }

        /// <summary>
        /// Get all generated quests
        /// </summary>
        public List<Quest> GetGeneratedQuests()
        {
            return generatedQuests;
        }

        /// <summary>
        /// Get all generated events
        /// </summary>
        public List<CityEvent> GetGeneratedEvents()
        {
            return generatedEvents;
        }

        /// <summary>
        /// Generate complete city with all features (extensible entry point)
        /// </summary>
        public void GenerateCompleteCity(string cityName)
        {
            Debug.Log($"Generating complete city: {cityName}");
            
            // Clear previous generation
            ClearGenerated();
            
            // Generate all zone types for the city
            Vector3 cityCenter = Vector3.zero;
            float zoneRadius = 200f;
            float zoneSpacing = 500f;
            
            // Residential zones
            GenerateDistrict(World.ZoneType.Residential, cityCenter + new Vector3(0, 0, 0), zoneRadius);
            
            // Business zones
            GenerateDistrict(World.ZoneType.Business, cityCenter + new Vector3(zoneSpacing, 0, 0), zoneRadius);
            
            // Commercial zones
            GenerateDistrict(World.ZoneType.Commercial, cityCenter + new Vector3(0, 0, zoneSpacing), zoneRadius);
            
            // Recreation zones
            GenerateDistrict(World.ZoneType.Recreation, cityCenter + new Vector3(-zoneSpacing, 0, 0), zoneRadius);
            
            // Industrial zones
            GenerateDistrict(World.ZoneType.Industrial, cityCenter + new Vector3(0, 0, -zoneSpacing), zoneRadius);
            
            // Generate city-specific content
            GenerateCitySpecificContent(cityName);
            
            Debug.Log($"City generation complete: {generatedBuildings.Count} buildings, {generatedNPCs.Count} NPCs, {generatedQuests.Count} quests, {generatedEvents.Count} events");
        }

        /// <summary>
        /// Generate city-specific content (landmarks, signature properties, themed NPCs)
        /// </summary>
        private void GenerateCitySpecificContent(string cityName)
        {
            // Generate signature NPCs for each role
            for (int i = 0; i < 3; i++)
            {
                generatedNPCs.Add(GenerateNPCWithRole(NPCRole.Merchant, cityName));
                generatedNPCs.Add(GenerateNPCWithRole(NPCRole.Banker, cityName));
                generatedNPCs.Add(GenerateNPCWithRole(NPCRole.Educator, cityName));
                generatedNPCs.Add(GenerateNPCWithRole(NPCRole.FashionDesigner, cityName));
                generatedNPCs.Add(GenerateNPCWithRole(NPCRole.InteriorDesigner, cityName));
                generatedNPCs.Add(GenerateNPCWithRole(NPCRole.Architect, cityName));
            }
            
            // Generate city-themed quests
            for (int i = 0; i < 10; i++)
            {
                NPCRole randomRole = (NPCRole)random.Next(0, System.Enum.GetValues(typeof(NPCRole)).Length);
                Quest quest = GenerateCityQuest(cityName, randomRole);
                // Quest is already added to generatedQuests list in GenerateQuest method
            }
            
            // Generate city events
            for (int i = 0; i < 5; i++)
            {
                GenerateCityEvent(cityName);
            }
            
            // Generate signature landmarks based on city
            if (generateCityLandmarks)
            {
                GenerateCityLandmarks(cityName);
            }
        }

        /// <summary>
        /// Generate city-specific landmarks (extensible for future additions)
        /// </summary>
        private void GenerateCityLandmarks(string cityName)
        {
            Vector3 landmarkPosition = Vector3.zero;
            
            switch (cityName)
            {
                case "OmniLanta":
                    // Major Attractions & Landmarks
                    GenerateLandmark("Mercedes-Benz Stadium", landmarkPosition, BuildingStyle.Modern, 100000f);
                    GenerateLandmark("World of Coca-Cola", landmarkPosition + new Vector3(200, 0, 0), BuildingStyle.Modern, 50000f);
                    GenerateLandmark("Georgia Aquarium", landmarkPosition + new Vector3(300, 0, 50), BuildingStyle.Modern, 75000f);
                    GenerateLandmark("Centennial Olympic Park", landmarkPosition + new Vector3(150, 0, 100), BuildingStyle.Contemporary, 60000f);
                    GenerateLandmark("Fox Theatre", landmarkPosition + new Vector3(-100, 0, 0), BuildingStyle.Classical, 80000f);
                    GenerateLandmark("Ponce City Market", landmarkPosition + new Vector3(400, 0, -50), BuildingStyle.Industrial, 90000f);
                    GenerateLandmark("SunTrust Park", landmarkPosition + new Vector3(-300, 0, 200), BuildingStyle.Modern, 85000f);
                    GenerateLandmark("Atlanta BeltLine", landmarkPosition + new Vector3(250, 0, -100), BuildingStyle.Contemporary, 40000f);
                    GenerateLandmark("High Museum of Art", landmarkPosition + new Vector3(-200, 0, 150), BuildingStyle.Modern, 70000f);
                    GenerateLandmark("Stone Mountain Park", landmarkPosition + new Vector3(800, 0, 300), BuildingStyle.Contemporary, 65000f);
                    break;
                    
                case "OmniVegas":
                    // The Strip - North End
                    GenerateLandmark("Stratosphere Tower", landmarkPosition + new Vector3(-1000, 0, 0), BuildingStyle.Neon, 250000f, 350f, 80f, 80f);
                    GenerateLandmark("Circus Circus", landmarkPosition + new Vector3(-950, 0, 50), BuildingStyle.Neon, 140000f, 90f, 140f, 120f);
                    GenerateLandmark("SLS Las Vegas", landmarkPosition + new Vector3(-900, 0, -50), BuildingStyle.Modern, 160000f, 120f, 110f, 100f);
                    GenerateLandmark("Resorts World Las Vegas", landmarkPosition + new Vector3(-850, 0, 0), BuildingStyle.Modern, 420000f, 210f, 180f, 150f);
                    GenerateLandmark("Encore", landmarkPosition + new Vector3(-750, 0, 50), BuildingStyle.Neon, 390000f, 192f, 140f, 115f);
                    GenerateLandmark("Wynn Las Vegas", landmarkPosition + new Vector3(-500, 0, 0), BuildingStyle.Neon, 400000f, 200f, 150f, 120f);
                    GenerateLandmark("The Palazzo", landmarkPosition + new Vector3(-400, 0, 150), BuildingStyle.Classical, 370000f, 180f, 190f, 145f);
                    GenerateLandmark("The Venetian", landmarkPosition + new Vector3(-400, 0, 100), BuildingStyle.Classical, 380000f, 180f, 200f, 150f);
                    GenerateLandmark("Treasure Island", landmarkPosition + new Vector3(-350, 0, -100), BuildingStyle.Neon, 260000f, 130f, 135f, 115f);
                    GenerateLandmark("Mirage Hotel & Casino", landmarkPosition + new Vector3(-300, 0, 0), BuildingStyle.Neon, 320000f, 160f, 140f, 110f);
                    GenerateLandmark("Harrah's Las Vegas", landmarkPosition + new Vector3(-250, 0, 100), BuildingStyle.Neon, 240000f, 115f, 125f, 100f);
                    GenerateLandmark("The LINQ Hotel", landmarkPosition + new Vector3(-220, 0, 150), BuildingStyle.Modern, 210000f, 100f, 120f, 95f);
                    GenerateLandmark("Flamingo Las Vegas", landmarkPosition + new Vector3(-210, 0, -80), BuildingStyle.Neon, 230000f, 105f, 130f, 110f);
                    GenerateLandmark("Caesars Palace", landmarkPosition + new Vector3(-200, 0, -50), BuildingStyle.Classical, 420000f, 170f, 180f, 160f);
                    GenerateLandmark("Cromwell Hotel", landmarkPosition + new Vector3(-150, 0, 80), BuildingStyle.Modern, 180000f, 95f, 90f, 85f);
                    GenerateLandmark("Bellagio", landmarkPosition + new Vector3(-100, 0, 0), BuildingStyle.Classical, 500000f, 190f, 200f, 150f);
                    GenerateLandmark("Bellagio Fountains", landmarkPosition + new Vector3(-90, 0, -120), BuildingStyle.Modern, 150000f, 5f, 280f, 35f);
                    
                    // Paris Hotel + Casino - Main landmark building
                    GenerateLandmark("Paris Hotel + Casino", landmarkPosition, BuildingStyle.Neon, 300000f, 180f, 120f, 100f);
                    
                    // Maevn "Saint Drip" Private Penthouse - At penthouse level of Paris Hotel + Casino (floor 65)
                    GenerateLandmark("Maevn Saint Drip Private Penthouse", landmarkPosition + new Vector3(0, 180f, 0), BuildingStyle.Neon, 500000f, 42f, 100f, 150f);
                    
                    GenerateLandmark("Planet Hollywood", landmarkPosition + new Vector3(50, 0, -100), BuildingStyle.Modern, 290000f, 135f, 145f, 120f);
                    GenerateLandmark("Cosmopolitan", landmarkPosition + new Vector3(100, 0, 50), BuildingStyle.Modern, 380000f, 195f, 140f, 130f);
                    GenerateLandmark("Vdara Hotel", landmarkPosition + new Vector3(180, 0, 100), BuildingStyle.Modern, 240000f, 145f, 90f, 85f);
                    GenerateLandmark("Aria Resort & Casino", landmarkPosition + new Vector3(200, 0, 0), BuildingStyle.Modern, 450000f, 200f, 180f, 140f);
                    GenerateLandmark("Waldorf Astoria", landmarkPosition + new Vector3(250, 0, 80), BuildingStyle.Modern, 310000f, 150f, 100f, 95f);
                    GenerateLandmark("Park MGM", landmarkPosition + new Vector3(280, 0, -70), BuildingStyle.Modern, 270000f, 125f, 135f, 115f);
                    GenerateLandmark("MGM Grand", landmarkPosition + new Vector3(300, 0, -50), BuildingStyle.Neon, 400000f, 185f, 200f, 170f);
                    GenerateLandmark("Tropicana Las Vegas", landmarkPosition + new Vector3(350, 0, 50), BuildingStyle.Neon, 220000f, 110f, 125f, 105f);
                    GenerateLandmark("New York-New York", landmarkPosition + new Vector3(400, 0, 0), BuildingStyle.Modern, 350000f, 160f, 150f, 140f);
                    GenerateLandmark("The Park Las Vegas", landmarkPosition + new Vector3(420, 0, -150), BuildingStyle.Contemporary, 90000f, 15f, 200f, 150f);
                    GenerateLandmark("Excalibur", landmarkPosition + new Vector3(500, 0, 100), BuildingStyle.Classical, 280000f, 140f, 130f, 120f);
                    GenerateLandmark("Luxor", landmarkPosition + new Vector3(600, 0, 0), BuildingStyle.Modern, 320000f, 110f, 180f, 180f);
                    GenerateLandmark("Mandalay Bay", landmarkPosition + new Vector3(700, 0, -50), BuildingStyle.Modern, 380000f, 200f, 170f, 150f);
                    GenerateLandmark("Delano Las Vegas", landmarkPosition + new Vector3(750, 0, 20), BuildingStyle.Modern, 260000f, 140f, 80f, 75f);
                    GenerateLandmark("Four Seasons Las Vegas", landmarkPosition + new Vector3(720, 0, 80), BuildingStyle.Modern, 340000f, 160f, 95f, 90f);
                    
                    // Downtown & Fremont Experience
                    GenerateLandmark("Fremont Street Experience", landmarkPosition + new Vector3(-1100, 0, -500), BuildingStyle.Neon, 180000f, 30f, 400f, 50f);
                    GenerateLandmark("Golden Nugget", landmarkPosition + new Vector3(-1050, 0, -500), BuildingStyle.Neon, 150000f, 120f, 100f, 90f);
                    GenerateLandmark("The D Casino", landmarkPosition + new Vector3(-1100, 0, -450), BuildingStyle.Neon, 120000f, 100f, 90f, 80f);
                    GenerateLandmark("Four Queens", landmarkPosition + new Vector3(-1150, 0, -500), BuildingStyle.Neon, 110000f, 90f, 85f, 75f);
                    GenerateLandmark("Binion's Gambling Hall", landmarkPosition + new Vector3(-1120, 0, -550), BuildingStyle.Neon, 105000f, 85f, 80f, 70f);
                    GenerateLandmark("Fremont Hotel & Casino", landmarkPosition + new Vector3(-1080, 0, -520), BuildingStyle.Neon, 108000f, 88f, 82f, 72f);
                    GenerateLandmark("El Cortez", landmarkPosition + new Vector3(-1200, 0, -480), BuildingStyle.Neon, 95000f, 75f, 75f, 65f);
                    GenerateLandmark("Downtown Grand", landmarkPosition + new Vector3(-1050, 0, -420), BuildingStyle.Modern, 115000f, 95f, 85f, 78f);
                    GenerateLandmark("Plaza Hotel & Casino", landmarkPosition + new Vector3(-1180, 0, -520), BuildingStyle.Neon, 112000f, 92f, 87f, 74f);
                    
                    // Convention & Entertainment
                    GenerateLandmark("Las Vegas Convention Center", landmarkPosition + new Vector3(-200, 0, 400), BuildingStyle.Modern, 200000f, 40f, 300f, 250f);
                    GenerateLandmark("T-Mobile Arena", landmarkPosition + new Vector3(100, 0, -300), BuildingStyle.Modern, 220000f, 60f, 200f, 180f);
                    GenerateLandmark("Allegiant Stadium", landmarkPosition + new Vector3(1200, 0, 200), BuildingStyle.Modern, 350000f, 80f, 280f, 260f);
                    GenerateLandmark("High Roller Observation Wheel", landmarkPosition + new Vector3(150, 0, 200), BuildingStyle.Modern, 100000f, 168f, 50f, 50f);
                    GenerateLandmark("The Sphere", landmarkPosition + new Vector3(-250, 0, 350), BuildingStyle.Modern, 280000f, 112f, 157f, 157f);
                    GenerateLandmark("MSG Sphere Entertainment", landmarkPosition + new Vector3(-280, 0, 380), BuildingStyle.Modern, 180000f, 70f, 120f, 100f);
                    
                    // Shows & Entertainment Venues
                    GenerateLandmark("Colosseum at Caesars Palace", landmarkPosition + new Vector3(-190, 0, -20), BuildingStyle.Classical, 140000f, 45f, 100f, 95f);
                    GenerateLandmark("Dolby Live at Park MGM", landmarkPosition + new Vector3(270, 0, -50), BuildingStyle.Modern, 125000f, 40f, 95f, 90f);
                    GenerateLandmark("Michelob ULTRA Arena", landmarkPosition + new Vector3(650, 0, -30), BuildingStyle.Modern, 135000f, 45f, 105f, 98f);
                    GenerateLandmark("Allegiant Stadium Raiders Facility", landmarkPosition + new Vector3(1180, 0, 250), BuildingStyle.Modern, 90000f, 30f, 120f, 110f);
                    
                    // Shopping & Dining
                    GenerateLandmark("Forum Shops at Caesars", landmarkPosition + new Vector3(-210, 0, -10), BuildingStyle.Classical, 175000f, 35f, 180f, 140f);
                    GenerateLandmark("Grand Canal Shoppes", landmarkPosition + new Vector3(-390, 0, 120), BuildingStyle.Classical, 165000f, 32f, 170f, 135f);
                    GenerateLandmark("Miracle Mile Shops", landmarkPosition + new Vector3(60, 0, -110), BuildingStyle.Modern, 130000f, 28f, 200f, 120f);
                    GenerateLandmark("Fashion Show Mall", landmarkPosition + new Vector3(-600, 0, 150), BuildingStyle.Modern, 190000f, 35f, 240f, 210f);
                    GenerateLandmark("Crystals at CityCenter", landmarkPosition + new Vector3(180, 0, 30), BuildingStyle.Modern, 210000f, 38f, 160f, 145f);
                    GenerateLandmark("Town Square Las Vegas", landmarkPosition + new Vector3(1400, 0, -400), BuildingStyle.Contemporary, 155000f, 25f, 280f, 220f);
                    GenerateLandmark("The Boulevard Mall", landmarkPosition + new Vector3(-1500, 0, 800), BuildingStyle.Modern, 95000f, 22f, 200f, 180f);
                    
                    // Museums & Attractions
                    GenerateLandmark("Neon Museum", landmarkPosition + new Vector3(-1300, 0, -600), BuildingStyle.Contemporary, 85000f, 18f, 150f, 120f);
                    GenerateLandmark("Mob Museum", landmarkPosition + new Vector3(-1160, 0, -580), BuildingStyle.Classical, 92000f, 25f, 95f, 85f);
                    GenerateLandmark("Discovery Children's Museum", landmarkPosition + new Vector3(-1250, 0, -350), BuildingStyle.Modern, 78000f, 28f, 105f, 98f);
                    GenerateLandmark("Las Vegas Natural History Museum", landmarkPosition + new Vector3(-1350, 0, -250), BuildingStyle.Contemporary, 72000f, 20f, 90f, 85f);
                    GenerateLandmark("Madame Tussauds Las Vegas", landmarkPosition + new Vector3(-400, 0, 80), BuildingStyle.Modern, 68000f, 30f, 75f, 70f);
                    GenerateLandmark("Shark Reef Aquarium", landmarkPosition + new Vector3(710, 0, -60), BuildingStyle.Modern, 88000f, 25f, 110f, 100f);
                    
                    // Off-Strip Resorts & Casinos
                    GenerateLandmark("Red Rock Casino", landmarkPosition + new Vector3(-2000, 0, 500), BuildingStyle.Modern, 180000f, 100f, 150f, 120f);
                    GenerateLandmark("Green Valley Ranch", landmarkPosition + new Vector3(2200, 0, -800), BuildingStyle.Modern, 165000f, 95f, 140f, 115f);
                    GenerateLandmark("M Resort", landmarkPosition + new Vector3(2500, 0, -1000), BuildingStyle.Modern, 155000f, 105f, 125f, 108f);
                    GenerateLandmark("The Orleans", landmarkPosition + new Vector3(-1800, 0, -200), BuildingStyle.Classical, 142000f, 88f, 130f, 112f);
                    GenerateLandmark("South Point Hotel Casino", landmarkPosition + new Vector3(2800, 0, -1200), BuildingStyle.Modern, 148000f, 92f, 135f, 115f);
                    GenerateLandmark("Sunset Station", landmarkPosition + new Vector3(2400, 0, -700), BuildingStyle.Modern, 138000f, 85f, 125f, 105f);
                    GenerateLandmark("Santa Fe Station", landmarkPosition + new Vector3(-2200, 0, 800), BuildingStyle.Contemporary, 128000f, 80f, 120f, 100f);
                    GenerateLandmark("Palace Station", landmarkPosition + new Vector3(-1600, 0, 300), BuildingStyle.Neon, 132000f, 82f, 122f, 102f);
                    GenerateLandmark("Boulder Station", landmarkPosition + new Vector3(2600, 0, -500), BuildingStyle.Modern, 125000f, 78f, 118f, 98f);
                    GenerateLandmark("Texas Station", landmarkPosition + new Vector3(-2400, 0, 1000), BuildingStyle.Modern, 122000f, 76f, 115f, 95f);
                    
                    // Golf & Recreation
                    GenerateLandmark("Wynn Golf Club", landmarkPosition + new Vector3(-520, 0, 200), BuildingStyle.Contemporary, 110000f, 15f, 300f, 250f);
                    GenerateLandmark("Bali Hai Golf Club", landmarkPosition + new Vector3(750, 0, -150), BuildingStyle.Contemporary, 95000f, 12f, 280f, 240f);
                    GenerateLandmark("Las Vegas National Golf Club", landmarkPosition + new Vector3(-2600, 0, -600), BuildingStyle.Contemporary, 88000f, 10f, 270f, 230f);
                    
                    // Signature Properties
                    GenerateLandmark("Maevn Mansion", landmarkPosition + new Vector3(1500, 0, 800), BuildingStyle.Modern, 1000000f);
                    break;
                    
                case "OmniTokyo":
                    // Major Districts & Landmarks
                    GenerateLandmark("Tokyo Tower", landmarkPosition, BuildingStyle.Modern, 300000f, 333f, 40f, 40f);
                    GenerateLandmark("Tokyo Skytree", landmarkPosition + new Vector3(500, 0, 0), BuildingStyle.Modern, 450000f, 634f, 50f, 50f);
                    GenerateLandmark("Shibuya Crossing Tower", landmarkPosition + new Vector3(200, 0, 200), BuildingStyle.Cyberpunk, 200000f);
                    GenerateLandmark("Shibuya 109", landmarkPosition + new Vector3(220, 0, 220), BuildingStyle.Neon, 150000f, 60f, 50f, 50f);
                    GenerateLandmark("Tokyo Metropolitan Government Building", landmarkPosition + new Vector3(-300, 0, 100), BuildingStyle.Modern, 280000f, 243f, 100f, 90f);
                    GenerateLandmark("Akihabara Electric Town", landmarkPosition + new Vector3(400, 0, -200), BuildingStyle.Neon, 220000f, 80f, 200f, 150f);
                    GenerateLandmark("Senso-ji Temple", landmarkPosition + new Vector3(600, 0, 300), BuildingStyle.Classical, 180000f, 30f, 80f, 100f);
                    GenerateLandmark("Meiji Shrine", landmarkPosition + new Vector3(-500, 0, -300), BuildingStyle.Classical, 160000f, 25f, 120f, 150f);
                    GenerateLandmark("Rainbow Bridge", landmarkPosition + new Vector3(300, 0, 700), BuildingStyle.Modern, 140000f, 50f, 800f, 30f);
                    GenerateLandmark("Tokyo Dome", landmarkPosition + new Vector3(-200, 0, -500), BuildingStyle.Modern, 190000f, 56f, 180f, 180f);
                    GenerateLandmark("Roppongi Hills", landmarkPosition + new Vector3(100, 0, -300), BuildingStyle.Modern, 350000f, 238f, 120f, 100f);
                    GenerateLandmark("Tokyo Station", landmarkPosition + new Vector3(250, 0, 100), BuildingStyle.Classical, 170000f, 40f, 200f, 150f);
                    GenerateLandmark("Ginza Shopping District", landmarkPosition + new Vector3(350, 0, 150), BuildingStyle.Modern, 280000f, 80f, 300f, 200f);
                    break;
                    
                case "OmniNYC":
                    // Iconic Landmarks
                    GenerateLandmark("Empire State Building", landmarkPosition, BuildingStyle.Classical, 600000f, 381f, 80f, 80f);
                    GenerateLandmark("One World Trade Center", landmarkPosition + new Vector3(400, 0, -300), BuildingStyle.Modern, 700000f, 541f, 60f, 60f);
                    GenerateLandmark("Chrysler Building", landmarkPosition + new Vector3(200, 0, 100), BuildingStyle.Classical, 450000f, 319f, 70f, 70f);
                    GenerateLandmark("Statue of Liberty", landmarkPosition + new Vector3(-800, 0, -800), BuildingStyle.Classical, 350000f, 93f, 30f, 30f);
                    GenerateLandmark("Brooklyn Bridge", landmarkPosition + new Vector3(300, 0, -500), BuildingStyle.Classical, 280000f, 85f, 1100f, 40f);
                    GenerateLandmark("Times Square", landmarkPosition + new Vector3(100, 0, 200), BuildingStyle.Neon, 400000f, 100f, 200f, 200f);
                    GenerateLandmark("Wall Street Financial Tower", landmarkPosition + new Vector3(350, 0, -250), BuildingStyle.Modern, 300000f);
                    GenerateLandmark("Central Park Tower", landmarkPosition + new Vector3(-200, 0, 300), BuildingStyle.Modern, 550000f, 472f, 60f, 50f);
                    GenerateLandmark("Rockefeller Center", landmarkPosition + new Vector3(150, 0, 250), BuildingStyle.Classical, 380000f, 259f, 100f, 90f);
                    GenerateLandmark("Grand Central Terminal", landmarkPosition + new Vector3(180, 0, 120), BuildingStyle.Classical, 220000f, 50f, 150f, 120f);
                    GenerateLandmark("The Met Museum", landmarkPosition + new Vector3(-300, 0, 400), BuildingStyle.Classical, 280000f, 30f, 200f, 150f);
                    GenerateLandmark("Madison Square Garden", landmarkPosition + new Vector3(50, 0, 0), BuildingStyle.Modern, 250000f, 60f, 150f, 150f);
                    GenerateLandmark("Flatiron Building", landmarkPosition + new Vector3(250, 0, 50), BuildingStyle.Classical, 180000f, 87f, 30f, 60f);
                    break;
                    
                case "OmniDubai":
                    // Iconic Landmarks
                    GenerateLandmark("Burj Khalifa", landmarkPosition, BuildingStyle.Modern, 1200000f, 828f, 80f, 80f);
                    GenerateLandmark("Burj Al Arab", landmarkPosition + new Vector3(500, 0, -400), BuildingStyle.Modern, 750000f, 321f, 120f, 100f);
                    GenerateLandmark("Palm Jumeirah", landmarkPosition + new Vector3(800, 0, -600), BuildingStyle.Modern, 600000f, 20f, 500f, 400f);
                    GenerateLandmark("Dubai Marina", landmarkPosition + new Vector3(600, 0, -300), BuildingStyle.Modern, 450000f, 200f, 300f, 250f);
                    GenerateLandmark("Dubai Mall", landmarkPosition + new Vector3(100, 0, 100), BuildingStyle.Modern, 400000f, 40f, 350f, 300f);
                    GenerateLandmark("Dubai Fountain", landmarkPosition + new Vector3(120, 0, 150), BuildingStyle.Modern, 180000f, 5f, 275f, 30f);
                    GenerateLandmark("Museum of the Future", landmarkPosition + new Vector3(300, 0, 200), BuildingStyle.Modern, 320000f, 77f, 80f, 80f);
                    GenerateLandmark("Dubai Frame", landmarkPosition + new Vector3(-300, 0, 300), BuildingStyle.Modern, 220000f, 150f, 93f, 20f);
                    GenerateLandmark("Atlantis The Palm", landmarkPosition + new Vector3(900, 0, -650), BuildingStyle.Modern, 480000f, 100f, 180f, 150f);
                    GenerateLandmark("Jumeirah Beach Hotel", landmarkPosition + new Vector3(550, 0, -500), BuildingStyle.Modern, 380000f, 104f, 150f, 120f);
                    GenerateLandmark("Emirates Towers", landmarkPosition + new Vector3(200, 0, 0), BuildingStyle.Modern, 520000f, 355f, 70f, 70f);
                    GenerateLandmark("Dubai Creek Tower", landmarkPosition + new Vector3(-500, 0, -200), BuildingStyle.Modern, 850000f, 828f, 60f, 60f); // Height estimate (under construction)
                    break;
                    
                case "OmniLA":
                    // Major Attractions & Landmarks
                    GenerateLandmark("Hollywood Sign", landmarkPosition, BuildingStyle.Contemporary, 200000f, 15f, 110f, 5f);
                    GenerateLandmark("Hollywood Studios Complex", landmarkPosition + new Vector3(100, 0, 100), BuildingStyle.Contemporary, 250000f);
                    GenerateLandmark("Griffith Observatory", landmarkPosition + new Vector3(-200, 0, 200), BuildingStyle.Classical, 180000f, 30f, 80f, 70f);
                    GenerateLandmark("Santa Monica Pier", landmarkPosition + new Vector3(800, 0, -500), BuildingStyle.Contemporary, 150000f, 20f, 200f, 100f);
                    GenerateLandmark("Venice Beach", landmarkPosition + new Vector3(850, 0, -600), BuildingStyle.Contemporary, 140000f, 10f, 300f, 80f);
                    GenerateLandmark("Getty Center", landmarkPosition + new Vector3(-300, 0, 300), BuildingStyle.Modern, 280000f, 40f, 150f, 120f);
                    GenerateLandmark("Dodger Stadium", landmarkPosition + new Vector3(200, 0, -300), BuildingStyle.Contemporary, 190000f, 50f, 180f, 160f);
                    GenerateLandmark("SoFi Stadium", landmarkPosition + new Vector3(400, 0, -400), BuildingStyle.Modern, 350000f, 90f, 260f, 240f);
                    GenerateLandmark("Walt Disney Concert Hall", landmarkPosition + new Vector3(100, 0, 0), BuildingStyle.Modern, 220000f, 60f, 90f, 80f);
                    GenerateLandmark("The Broad Museum", landmarkPosition + new Vector3(120, 0, -50), BuildingStyle.Modern, 170000f, 40f, 80f, 70f);
                    GenerateLandmark("LA Live", landmarkPosition + new Vector3(150, 0, -100), BuildingStyle.Modern, 240000f, 80f, 200f, 180f);
                    GenerateLandmark("Sunset Strip", landmarkPosition + new Vector3(-100, 0, 50), BuildingStyle.Neon, 200000f, 30f, 400f, 60f);
                    break;
                    
                case "OmniParis":
                    // Iconic Landmarks
                    GenerateLandmark("Eiffel Tower", landmarkPosition, BuildingStyle.Classical, 500000f, 300f, 50f, 50f);
                    GenerateLandmark("Arc de Triomphe", landmarkPosition + new Vector3(400, 0, 0), BuildingStyle.Classical, 280000f, 50f, 45f, 45f);
                    GenerateLandmark("Louvre Museum", landmarkPosition + new Vector3(300, 0, -200), BuildingStyle.Classical, 450000f, 40f, 200f, 180f);
                    GenerateLandmark("Notre-Dame Cathedral", landmarkPosition + new Vector3(200, 0, -100), BuildingStyle.Classical, 350000f, 69f, 80f, 120f);
                    GenerateLandmark("Sacré-Cœur Basilica", landmarkPosition + new Vector3(-300, 0, 300), BuildingStyle.Classical, 320000f, 83f, 70f, 80f);
                    GenerateLandmark("Tour OmniWorld", landmarkPosition + new Vector3(500, 0, 100), BuildingStyle.Classical, 400000f);
                    GenerateLandmark("Musée d'Orsay", landmarkPosition + new Vector3(250, 0, -180), BuildingStyle.Classical, 280000f, 35f, 150f, 100f);
                    GenerateLandmark("Champs-Élysées", landmarkPosition + new Vector3(350, 0, 50), BuildingStyle.Classical, 300000f, 30f, 600f, 80f);
                    GenerateLandmark("Palace of Versailles", landmarkPosition + new Vector3(-800, 0, -500), BuildingStyle.Classical, 550000f, 40f, 300f, 250f);
                    GenerateLandmark("Panthéon", landmarkPosition + new Vector3(150, 0, -250), BuildingStyle.Classical, 240000f, 83f, 70f, 70f);
                    GenerateLandmark("Moulin Rouge", landmarkPosition + new Vector3(-250, 0, 250), BuildingStyle.Neon, 180000f, 40f, 60f, 50f);
                    GenerateLandmark("Centre Pompidou", landmarkPosition + new Vector3(100, 0, -150), BuildingStyle.Modern, 220000f, 45f, 100f, 90f);
                    break;
            }
        }

        /// <summary>
        /// Generate a specific landmark building
        /// </summary>
        private void GenerateLandmark(string landmarkName, Vector3 position, BuildingStyle style, float value)
        {
            // Generate with random dimensions but full interactivity
            float height = MIN_LANDMARK_HEIGHT + (float)random.NextDouble() * (MAX_LANDMARK_HEIGHT - MIN_LANDMARK_HEIGHT);
            float width = MIN_LANDMARK_WIDTH + (float)random.NextDouble() * (MAX_LANDMARK_WIDTH - MIN_LANDMARK_WIDTH);
            float depth = MIN_LANDMARK_DEPTH + (float)random.NextDouble() * (MAX_LANDMARK_DEPTH - MIN_LANDMARK_DEPTH);
            
            GeneratedBuilding landmark = new GeneratedBuilding
            {
                position = position,
                zoneType = World.ZoneType.Recreation, // Landmarks are typically in recreation zones
                height = height,
                width = width,
                depth = depth,
                style = style,
                value = value,
                name = landmarkName,
                isLandmark = true,
                isInteractive = true, // All landmarks are interactive
                hasInterior = true,
                hasExteriorInteraction = true,
                entranceCount = DetermineEntranceCount(landmarkName, height),
                requiresAccess = false
            };
            
            // Add interaction components and interior features
            AddInteractionComponents(landmark, landmarkName);
            AddInteriorFeatures(landmark, landmarkName);
            
            generatedBuildings.Add(landmark);
            Debug.Log($"Generated interactive landmark: {landmarkName} valued at {value:C0} $OMNI - Interior: {landmark.hasInterior}, Entrances: {landmark.entranceCount}");
        }
        
        /// <summary>
        /// Generate a specific landmark building with custom dimensions
        /// </summary>
        private void GenerateLandmark(string landmarkName, Vector3 position, BuildingStyle style, float value, float height, float width, float depth)
        {
            // Default: All landmarks are interactive with interior and exterior access
            GenerateLandmark(landmarkName, position, style, value, height, width, depth, true, true);
        }
        
        /// <summary>
        /// Generate a specific landmark building with custom dimensions and interactivity
        /// </summary>
        private void GenerateLandmark(string landmarkName, Vector3 position, BuildingStyle style, float value, float height, float width, float depth, bool hasInterior, bool requiresAccess)
        {
            GeneratedBuilding landmark = new GeneratedBuilding
            {
                position = position,
                zoneType = World.ZoneType.Recreation, // Landmarks are typically in recreation zones
                height = height,
                width = width,
                depth = depth,
                style = style,
                value = value,
                name = landmarkName,
                isLandmark = true,
                isInteractive = true, // All landmarks are interactive
                hasInterior = hasInterior,
                hasExteriorInteraction = true, // All landmarks have exterior interaction
                entranceCount = DetermineEntranceCount(landmarkName, height),
                requiresAccess = requiresAccess
            };
            
            // Add appropriate interaction components based on building type
            AddInteractionComponents(landmark, landmarkName);
            
            // Add interior features based on building type
            AddInteriorFeatures(landmark, landmarkName);
            
            generatedBuildings.Add(landmark);
            Debug.Log($"Generated interactive landmark: {landmarkName} at position {position} (H:{height}m, W:{width}m, D:{depth}m) valued at {value:C0} $OMNI - Interior: {hasInterior}, Entrances: {landmark.entranceCount}");
        }
        
        /// <summary>
        /// Determine number of entrances based on building size and type
        /// </summary>
        private int DetermineEntranceCount(string buildingName, float height)
        {
            // Large buildings get multiple entrances
            if (height > HEIGHT_SKYSCRAPER) return 6; // Skyscrapers
            if (height > HEIGHT_TALL_BUILDING) return 4; // Tall buildings
            if (height > HEIGHT_MEDIUM_HIGH) return 3; // Medium-high buildings
            if (height > HEIGHT_MEDIUM) return 2; // Medium buildings
            return 1; // Small buildings
        }
        
        /// <summary>
        /// Add appropriate interaction components based on building type
        /// </summary>
        private void AddInteractionComponents(GeneratedBuilding building, string buildingName)
        {
            // Determine landmark type from name
            building.landmarkType = DetermineLandmarkType(buildingName);
            
            // Universal components for all landmarks
            building.interactionComponents.Add("DoorInteraction");
            building.interactionComponents.Add("InteriorController");
            building.interactionComponents.Add("LightController");
            building.interactionComponents.Add("NavMeshObstacle");
            building.interactionComponents.Add("AudioSource");
            
            // Building-specific components based on landmark type
            switch (building.landmarkType)
            {
                case LandmarkType.CasinoHotel:
                    building.interactionComponents.Add("CasinoController");
                    building.interactionComponents.Add("HotelController");
                    building.interactionComponents.Add("ElevatorController");
                    building.interactionComponents.Add("SecuritySystem");
                    break;
                    
                case LandmarkType.Stadium:
                    building.interactionComponents.Add("StadiumController");
                    building.interactionComponents.Add("SeatingController");
                    building.interactionComponents.Add("EventManager");
                    building.interactionComponents.Add("CrowdSystem");
                    break;
                    
                case LandmarkType.Museum:
                    building.interactionComponents.Add("MuseumController");
                    building.interactionComponents.Add("ExhibitInteraction");
                    building.interactionComponents.Add("AudioGuide");
                    break;
                    
                case LandmarkType.Tower:
                    building.interactionComponents.Add("ObservationController");
                    building.interactionComponents.Add("ElevatorController");
                    building.interactionComponents.Add("ViewpointSystem");
                    break;
                    
                case LandmarkType.Shopping:
                    building.interactionComponents.Add("ShoppingController");
                    building.interactionComponents.Add("StoreManager");
                    building.interactionComponents.Add("NPCShopkeeper");
                    break;
                    
                case LandmarkType.ConventionCenter:
                    building.interactionComponents.Add("ConventionController");
                    building.interactionComponents.Add("ExhibitHalls");
                    building.interactionComponents.Add("EventScheduler");
                    break;
                    
                case LandmarkType.Theater:
                    building.interactionComponents.Add("TheaterController");
                    building.interactionComponents.Add("ShowManager");
                    building.interactionComponents.Add("SeatingSystem");
                    break;
            }
        }
        
        /// <summary>
        /// Determine landmark type from building name
        /// </summary>
        private LandmarkType DetermineLandmarkType(string buildingName)
        {
            string nameLower = buildingName.ToLower();
            
            if (nameLower.Contains("casino") || nameLower.Contains("hotel") || nameLower.Contains("resort"))
                return LandmarkType.CasinoHotel;
            else if (nameLower.Contains("stadium") || nameLower.Contains("arena"))
                return LandmarkType.Stadium;
            else if (nameLower.Contains("museum") || nameLower.Contains("gallery") || nameLower.Contains("art"))
                return LandmarkType.Museum;
            else if (nameLower.Contains("tower") || nameLower.Contains("observatory"))
                return LandmarkType.Tower;
            else if (nameLower.Contains("mall") || nameLower.Contains("market") || nameLower.Contains("shopping") || nameLower.Contains("shops"))
                return LandmarkType.Shopping;
            else if (nameLower.Contains("convention") || nameLower.Contains("center") && !nameLower.Contains("shopping"))
                return LandmarkType.ConventionCenter;
            else if (nameLower.Contains("theatre") || nameLower.Contains("theater") || nameLower.Contains("colosseum"))
                return LandmarkType.Theater;
            else if (nameLower.Contains("bridge"))
                return LandmarkType.Bridge;
            else if (nameLower.Contains("park") || nameLower.Contains("garden"))
                return LandmarkType.Park;
                
            return LandmarkType.Generic;
        }
        
        /// <summary>
        /// Add interior features/rooms based on building type
        /// </summary>
        private void AddInteriorFeatures(GeneratedBuilding building, string buildingName)
        {
            switch (building.landmarkType)
            {
                case LandmarkType.CasinoHotel:
                    building.interiorFeatures.Add("Lobby");
                    building.interiorFeatures.Add("Casino Floor");
                    building.interiorFeatures.Add("Poker Room");
                    building.interiorFeatures.Add("Slot Machines");
                    building.interiorFeatures.Add("Restaurants");
                    building.interiorFeatures.Add("Bars");
                    building.interiorFeatures.Add("Hotel Rooms");
                    building.interiorFeatures.Add("Suites");
                    building.interiorFeatures.Add("Pool Area");
                    building.interiorFeatures.Add("Spa");
                    building.interiorFeatures.Add("Shops");
                    break;
                    
                case LandmarkType.Stadium:
                    building.interiorFeatures.Add("Main Entrance");
                    building.interiorFeatures.Add("Concourse");
                    building.interiorFeatures.Add("Seating Areas");
                    building.interiorFeatures.Add("VIP Boxes");
                    building.interiorFeatures.Add("Concession Stands");
                    building.interiorFeatures.Add("Restrooms");
                    building.interiorFeatures.Add("Team Locker Rooms");
                    building.interiorFeatures.Add("Press Box");
                    break;
                    
                case LandmarkType.Museum:
                    building.interiorFeatures.Add("Main Hall");
                    building.interiorFeatures.Add("Exhibition Galleries");
                    building.interiorFeatures.Add("Special Exhibits");
                    building.interiorFeatures.Add("Sculpture Garden");
                    building.interiorFeatures.Add("Gift Shop");
                    building.interiorFeatures.Add("Cafe");
                    building.interiorFeatures.Add("Auditorium");
                    break;
                    
                case LandmarkType.Tower:
                    building.interiorFeatures.Add("Ground Floor Lobby");
                    building.interiorFeatures.Add("Elevators");
                    building.interiorFeatures.Add("Observation Decks");
                    building.interiorFeatures.Add("Gift Shop");
                    building.interiorFeatures.Add("Restaurant");
                    building.interiorFeatures.Add("360° Viewing Platform");
                    break;
                    
                case LandmarkType.Shopping:
                    building.interiorFeatures.Add("Main Atrium");
                    building.interiorFeatures.Add("Retail Stores");
                    building.interiorFeatures.Add("Food Court");
                    building.interiorFeatures.Add("Luxury Boutiques");
                    building.interiorFeatures.Add("Entertainment Zone");
                    building.interiorFeatures.Add("Parking Garage");
                    break;
                    
                case LandmarkType.ConventionCenter:
                    building.interiorFeatures.Add("Main Entrance Hall");
                    building.interiorFeatures.Add("Exhibition Halls");
                    building.interiorFeatures.Add("Meeting Rooms");
                    building.interiorFeatures.Add("Ballroom");
                    building.interiorFeatures.Add("Registration Area");
                    building.interiorFeatures.Add("Cafeteria");
                    break;
                    
                case LandmarkType.Theater:
                    building.interiorFeatures.Add("Box Office");
                    building.interiorFeatures.Add("Main Auditorium");
                    building.interiorFeatures.Add("Balcony Seating");
                    building.interiorFeatures.Add("Stage");
                    building.interiorFeatures.Add("Backstage");
                    building.interiorFeatures.Add("Lobby");
                    break;
                    
                default:
                    // Generic landmark features
                    building.interiorFeatures.Add("Main Entrance");
                    building.interiorFeatures.Add("Reception Area");
                    building.interiorFeatures.Add("Main Hall");
                    building.interiorFeatures.Add("Restrooms");
                    building.interiorFeatures.Add("Gift Shop");
                    break;
            }
        }
    }

    [System.Serializable]
    public class GeneratedBuilding
    {
        public Vector3 position;
        public World.ZoneType zoneType;
        public float height;
        public float width;
        public float depth;
        public BuildingStyle style;
        public float value;
        public string name = "Building"; // Optional name for landmarks
        public bool isLandmark = false; // Mark special buildings
        
        // Interactivity Properties
        public bool isInteractive = false; // Can player interact with this building
        public bool hasInterior = false; // Building has explorable interior
        public bool hasExteriorInteraction = false; // Exterior features (doors, windows, etc.)
        public LandmarkType landmarkType = LandmarkType.Generic; // Type of landmark for interaction setup
        public List<string> interactionComponents = new List<string>(); // Unity components for interaction
        public List<string> interiorFeatures = new List<string>(); // Interior rooms/areas
        public int entranceCount = 1; // Number of entrances
        public bool requiresAccess = false; // Requires tickets/payment/membership
    }

    [System.Serializable]
    public class NPCData
    {
        public string name;
        public NPCRole role;
        public string personality;
        public float walletBalance;
        public float reputation;
    }

    [System.Serializable]
    public class CityEvent
    {
        public string name;
        public string description;
        public int duration; // in minutes
        public float economicImpact; // percentage
        public EventType eventType;
    }

    public enum BuildingStyle
    {
        Contemporary,
        Modern,
        Classical,
        Cyberpunk,
        Neon,
        Industrial
    }
    
    public enum LandmarkType
    {
        Generic,
        CasinoHotel,
        Stadium,
        Museum,
        Tower,
        Shopping,
        ConventionCenter,
        Theater,
        Restaurant,
        Park,
        Bridge
    }

    public enum EventType
    {
        Cultural,
        Economic,
        Entertainment,
        Sports,
        Technology
    }
    
    /// <summary>
    /// Generate music-based mission for city biome
    /// Integrates with OmniSound Global Grid system
    /// </summary>
    public Quest GenerateMusicBiomeQuest(string cityName)
    {
        Quest quest = new Quest
        {
            id = random.Next(10000, 19999),
            questType = QuestType.Social,
            reward = (float)random.NextDouble() * 300f + 100f,
            experienceReward = random.Next(100, 1000)
        };
        
        switch (cityName)
        {
            case "OmniNYC":
                quest.title = GetNYCMusicQuest();
                quest.description = "Represent the boom bap legacy of NYC hip-hop culture.";
                quest.reward *= 1.3f;
                break;
                
            case "Berlin":
                quest.title = GetBerlinMusicQuest();
                quest.description = "Experience the underground techno scene of Berlin.";
                quest.reward *= 1.4f;
                break;
                
            case "Lagos":
            case "OmniLagos":
                quest.title = GetLagosMusicQuest();
                quest.description = "Dive into the vibrant Afrobeats culture of Lagos.";
                quest.reward *= 1.2f;
                break;
                
            case "OmniTokyo":
                quest.title = GetTokyoMusicQuest();
                quest.description = "Master the cyber-minimal sound of Tokyo.";
                quest.reward *= 1.35f;
                break;
                
            case "OmniLanta":
                quest.title = GetAtlantaMusicQuest();
                quest.description = "Build your legacy in Atlanta's trap music scene.";
                quest.reward *= 1.25f;
                break;
                
            case "OmniVegas":
                quest.title = GetVegasMusicQuest();
                quest.description = "Drop the beat at Vegas' hottest EDM venues.";
                quest.reward *= 1.5f;
                break;
                
            case "OmniDubai":
                quest.title = GetDubaiMusicQuest();
                quest.description = "Blend Arabic pop with global luxury culture.";
                quest.reward *= 1.6f;
                break;
                
            case "OmniLA":
                quest.title = GetLAMusicQuest();
                quest.description = "Ride the West Coast wave in LA.";
                quest.reward *= 1.3f;
                break;
                
            case "OmniParis":
                quest.title = GetParisMusicQuest();
                quest.description = "Experience the artistic romance of French house.";
                quest.reward *= 1.4f;
                break;
                
            default:
                quest.title = "Explore Local Music Scene";
                quest.description = "Discover the unique sound of this city.";
                break;
        }
        
        Debug.Log($"Generated Music Biome Quest: {quest.title}");
        generatedQuests.Add(quest);
        
        return quest;
    }
    
    // Music-based quest titles per city
    private string GetNYCMusicQuest()
    {
        string[] quests = {
            "Master the 808 at The Bronx Studio",
            "Attend Underground Cipher in Brooklyn",
            "Sample Rare Vinyl at Queens Record Shop",
            "Freestyle Battle at Times Square",
            "Learn From a Boom Bap Legend"
        };
        return quests[random.Next(quests.Length)];
    }
    
    private string GetBerlinMusicQuest()
    {
        string[] quests = {
            "DJ Set at Berghain",
            "Master Modular Synthesis Workshop",
            "Warehouse Techno Marathon",
            "Cold Concrete Echo Session",
            "Underground Club Resident Night"
        };
        return quests[random.Next(quests.Length)];
    }
    
    private string GetLagosMusicQuest()
    {
        string[] quests = {
            "Play Talking Drums at Street Festival",
            "Afrobeats Dance Battle",
            "Market Energy Recording Session",
            "Collaborate with Local Artists",
            "Master Polyrhythm Patterns"
        };
        return quests[random.Next(quests.Length)];
    }
    
    private string GetTokyoMusicQuest()
    {
        string[] quests = {
            "Koto Sampling at Shibuya Studio",
            "Anime OP Recording Session",
            "Cyber Cafe Music Production",
            "Minimalist Sound Design Workshop",
            "Future Bass at Akihabara Club"
        };
        return quests[random.Next(quests.Length)];
    }
    
    private string GetAtlantaMusicQuest()
    {
        string[] quests = {
            "Record at Studio with 808 Mafia",
            "Hi-Hat Roll Masterclass",
            "Trap Soul Collaboration",
            "Mercedes-Benz Stadium Performance",
            "Creator Hub Showcase Event"
        };
        return quests[random.Next(quests.Length)];
    }
    
    private string GetVegasMusicQuest()
    {
        string[] quests = {
            "Headline at Neon Nightclub",
            "EDM Drop Building Workshop",
            "Casino Floor DJ Residency",
            "Pool Party Banger Creation",
            "Vocal Chop Masterclass"
        };
        return quests[random.Next(quests.Length)];
    }
    
    private string GetDubaiMusicQuest()
    {
        string[] quests = {
            "Oud Fusion Recording Session",
            "Luxury Club Performance",
            "Arabic Pop Vocal Training",
            "Desert Rhythm Workshop",
            "Marina Yacht Party DJ Set"
        };
        return quests[random.Next(quests.Length)];
    }
    
    private string GetLAMusicQuest()
    {
        string[] quests = {
            "Studio Session in Hollywood Hills",
            "Beach Sunset Performance",
            "G-Funk Bass Line Workshop",
            "Venice Beach Freestyle Cypher",
            "Film Score Production Class"
        };
        return quests[random.Next(quests.Length)];
    }
    
    private string GetParisMusicQuest()
    {
        string[] quests = {
            "French House Filter Workshop",
            "Accordion Sampling Session",
            "Bistro Live Performance",
            "Vocoder Masterclass",
            "Champs-Élysées Club Residency"
        };
        return quests[random.Next(quests.Length)];
    }
    
    /// <summary>
    /// Generate comprehensive zoning map for a city
    /// </summary>
    public CityZoningMap GenerateCityZoningMap(string cityName)
    {
        if (cityZoningMaps.ContainsKey(cityName))
            return cityZoningMaps[cityName];
            
        CityZoningMap zoningMap = new CityZoningMap
        {
            cityName = cityName,
            totalArea = 10000f, // 10km²
            districts = new List<ZoneDistrict>()
        };
        
        // Generate districts based on city type
        GenerateDistrictsForCity(zoningMap, cityName);
        
        cityZoningMaps[cityName] = zoningMap;
        Debug.Log($"Generated zoning map for {cityName}: {zoningMap.districts.Count} districts, {zoningMap.GetTotalParcels()} parcels");
        
        return zoningMap;
    }
    
    /// <summary>
    /// Generate districts with appropriate zoning for each city
    /// </summary>
    private void GenerateDistrictsForCity(CityZoningMap zoningMap, string cityName)
    {
        switch (cityName)
        {
            case "OmniVegas":
                GenerateVegasDistricts(zoningMap);
                break;
            case "OmniLanta":
                GenerateAtlantaDistricts(zoningMap);
                break;
            case "OmniTokyo":
                GenerateTokyoDistricts(zoningMap);
                break;
            case "OmniNYC":
                GenerateNYCDistricts(zoningMap);
                break;
            case "OmniDubai":
                GenerateDubaiDistricts(zoningMap);
                break;
            case "OmniLA":
                GenerateLADistricts(zoningMap);
                break;
            case "OmniParis":
                GenerateParisDistricts(zoningMap);
                break;
        }
    }
    
    private void GenerateVegasDistricts(CityZoningMap map)
    {
        // The Strip - Mixed Commercial/Entertainment
        map.districts.Add(CreateDistrict("The Strip", World.ZoneType.Commercial, 500f, 200));
        
        // Downtown - Mixed Residential/Commercial
        map.districts.Add(CreateDistrict("Downtown", World.ZoneType.Commercial, 400f, 300));
        
        // Residential Districts
        map.districts.Add(CreateDistrict("Henderson Residential", World.ZoneType.Residential, 800f, 1200));
        map.districts.Add(CreateDistrict("Summerlin", World.ZoneType.Residential, 700f, 1000));
        map.districts.Add(CreateDistrict("North Las Vegas", World.ZoneType.Residential, 600f, 800));
        map.districts.Add(CreateDistrict("Paradise", World.ZoneType.Residential, 500f, 700));
        map.districts.Add(CreateDistrict("Spring Valley", World.ZoneType.Residential, 650f, 900));
        
        // Business/Industrial
        map.districts.Add(CreateDistrict("Arts District", World.ZoneType.Business, 300f, 150));
        map.districts.Add(CreateDistrict("Industrial Zone", World.ZoneType.Industrial, 400f, 100));
        
        // Infrastructure & Public Facilities
        map.districts.Add(CreateInfrastructureDistrict("McCarran International Airport", InfrastructureType.Airport, 250f));
        map.districts.Add(CreateInfrastructureDistrict("Allegiant Stadium Complex", InfrastructureType.Stadium, 120f));
        map.districts.Add(CreateInfrastructureDistrict("T-Mobile Arena District", InfrastructureType.Stadium, 80f));
        map.districts.Add(CreateInfrastructureDistrict("Las Vegas Motor Speedway", InfrastructureType.Stadium, 200f));
        map.districts.Add(CreateInfrastructureDistrict("Red Rock Canyon Park", InfrastructureType.Park, 300f));
        map.districts.Add(CreateInfrastructureDistrict("Springs Preserve", InfrastructureType.Park, 180f));
        map.districts.Add(CreateInfrastructureDistrict("Sunset Park", InfrastructureType.Park, 150f));
        map.districts.Add(CreateInfrastructureDistrict("UNLV Campus", InfrastructureType.University, 280f));
        map.districts.Add(CreateInfrastructureDistrict("CSN College District", InfrastructureType.College, 200f));
        map.districts.Add(CreateInfrastructureDistrict("Clark County School District", InfrastructureType.SchoolDistrict, 350f));
        map.districts.Add(CreateInfrastructureDistrict("UMC Hospital Complex", InfrastructureType.Hospital, 120f));
        map.districts.Add(CreateInfrastructureDistrict("Las Vegas Fire & Rescue", InfrastructureType.EmergencyServices, 80f));
        map.districts.Add(CreateInfrastructureDistrict("Police Headquarters", InfrastructureType.PoliceStation, 60f));
    }
    
    private void GenerateAtlantaDistricts(CityZoningMap map)
    {
        map.districts.Add(CreateDistrict("Downtown", World.ZoneType.Business, 450f, 250));
        map.districts.Add(CreateDistrict("Midtown", World.ZoneType.Residential, 600f, 800));
        map.districts.Add(CreateDistrict("Buckhead", World.ZoneType.Residential, 700f, 1000));
        map.districts.Add(CreateDistrict("Decatur", World.ZoneType.Residential, 500f, 700));
        map.districts.Add(CreateDistrict("East Atlanta", World.ZoneType.Residential, 550f, 750));
        map.districts.Add(CreateDistrict("West End", World.ZoneType.Residential, 480f, 650));
        map.districts.Add(CreateDistrict("Tech Village", World.ZoneType.Business, 350f, 200));
        
        // Infrastructure
        map.districts.Add(CreateInfrastructureDistrict("Hartsfield-Jackson Airport", InfrastructureType.Airport, 300f));
        map.districts.Add(CreateInfrastructureDistrict("Mercedes-Benz Stadium", InfrastructureType.Stadium, 100f));
        map.districts.Add(CreateInfrastructureDistrict("State Farm Arena", InfrastructureType.Stadium, 80f));
        map.districts.Add(CreateInfrastructureDistrict("Truist Park", InfrastructureType.Stadium, 90f));
        map.districts.Add(CreateInfrastructureDistrict("Piedmont Park", InfrastructureType.Park, 200f));
        map.districts.Add(CreateInfrastructureDistrict("Centennial Olympic Park", InfrastructureType.Park, 120f));
        map.districts.Add(CreateInfrastructureDistrict("Grant Park", InfrastructureType.Park, 130f));
        map.districts.Add(CreateInfrastructureDistrict("Georgia Tech Campus", InfrastructureType.University, 250f));
        map.districts.Add(CreateInfrastructureDistrict("Emory University", InfrastructureType.University, 280f));
        map.districts.Add(CreateInfrastructureDistrict("Georgia State University", InfrastructureType.University, 220f));
        map.districts.Add(CreateInfrastructureDistrict("Atlanta Public Schools", InfrastructureType.SchoolDistrict, 300f));
        map.districts.Add(CreateInfrastructureDistrict("Grady Memorial Hospital", InfrastructureType.Hospital, 100f));
    }
    
    private void GenerateTokyoDistricts(CityZoningMap map)
    {
        map.districts.Add(CreateDistrict("Shibuya", World.ZoneType.Commercial, 550f, 400));
        map.districts.Add(CreateDistrict("Shinjuku", World.ZoneType.Business, 600f, 350));
        map.districts.Add(CreateDistrict("Akihabara", World.ZoneType.Commercial, 400f, 300));
        map.districts.Add(CreateDistrict("Roppongi", World.ZoneType.Residential, 700f, 900));
        map.districts.Add(CreateDistrict("Ginza", World.ZoneType.Commercial, 500f, 250));
        map.districts.Add(CreateDistrict("Asakusa", World.ZoneType.Residential, 600f, 800));
        map.districts.Add(CreateDistrict("Harajuku", World.ZoneType.Commercial, 450f, 350));
        map.districts.Add(CreateDistrict("Odaiba", World.ZoneType.Recreation, 550f, 200));
        
        // Infrastructure
        map.districts.Add(CreateInfrastructureDistrict("Narita International Airport", InfrastructureType.Airport, 350f));
        map.districts.Add(CreateInfrastructureDistrict("Haneda Airport", InfrastructureType.Airport, 320f));
        map.districts.Add(CreateInfrastructureDistrict("Tokyo Dome", InfrastructureType.Stadium, 85f));
        map.districts.Add(CreateInfrastructureDistrict("National Stadium", InfrastructureType.Stadium, 110f));
        map.districts.Add(CreateInfrastructureDistrict("Yoyogi Park", InfrastructureType.Park, 180f));
        map.districts.Add(CreateInfrastructureDistrict("Ueno Park", InfrastructureType.Park, 200f));
        map.districts.Add(CreateInfrastructureDistrict("Shinjuku Gyoen", InfrastructureType.Park, 150f));
        map.districts.Add(CreateInfrastructureDistrict("University of Tokyo", InfrastructureType.University, 280f));
        map.districts.Add(CreateInfrastructureDistrict("Waseda University", InfrastructureType.University, 250f));
        map.districts.Add(CreateInfrastructureDistrict("Tokyo Metro Schools", InfrastructureType.SchoolDistrict, 320f));
        map.districts.Add(CreateInfrastructureDistrict("Tokyo University Hospital", InfrastructureType.Hospital, 120f));
    }
    
    private void GenerateNYCDistricts(CityZoningMap map)
    {
        map.districts.Add(CreateDistrict("Manhattan", World.ZoneType.Business, 800f, 600));
        map.districts.Add(CreateDistrict("Brooklyn", World.ZoneType.Residential, 900f, 1500));
        map.districts.Add(CreateDistrict("Queens", World.ZoneType.Residential, 850f, 1400));
        map.districts.Add(CreateDistrict("Bronx", World.ZoneType.Residential, 700f, 1100));
        map.districts.Add(CreateDistrict("Staten Island", World.ZoneType.Residential, 600f, 800));
        map.districts.Add(CreateDistrict("Williamsburg", World.ZoneType.Residential, 500f, 700));
        map.districts.Add(CreateDistrict("Financial District", World.ZoneType.Business, 400f, 200));
        
        // Infrastructure
        map.districts.Add(CreateInfrastructureDistrict("JFK International Airport", InfrastructureType.Airport, 400f));
        map.districts.Add(CreateInfrastructureDistrict("LaGuardia Airport", InfrastructureType.Airport, 280f));
        map.districts.Add(CreateInfrastructureDistrict("Newark Airport", InfrastructureType.Airport, 300f));
        map.districts.Add(CreateInfrastructureDistrict("Yankee Stadium", InfrastructureType.Stadium, 95f));
        map.districts.Add(CreateInfrastructureDistrict("Madison Square Garden", InfrastructureType.Stadium, 80f));
        map.districts.Add(CreateInfrastructureDistrict("Citi Field", InfrastructureType.Stadium, 90f));
        map.districts.Add(CreateInfrastructureDistrict("Central Park", InfrastructureType.Park, 350f));
        map.districts.Add(CreateInfrastructureDistrict("Prospect Park", InfrastructureType.Park, 220f));
        map.districts.Add(CreateInfrastructureDistrict("Bryant Park", InfrastructureType.Park, 100f));
        map.districts.Add(CreateInfrastructureDistrict("Columbia University", InfrastructureType.University, 300f));
        map.districts.Add(CreateInfrastructureDistrict("NYU Campus", InfrastructureType.University, 280f));
        map.districts.Add(CreateInfrastructureDistrict("CUNY System", InfrastructureType.College, 250f));
        map.districts.Add(CreateInfrastructureDistrict("NYC Public Schools", InfrastructureType.SchoolDistrict, 450f));
        map.districts.Add(CreateInfrastructureDistrict("Mount Sinai Hospital", InfrastructureType.Hospital, 130f));
        map.districts.Add(CreateInfrastructureDistrict("NYPD Headquarters", InfrastructureType.PoliceStation, 70f));
    }
    
    private void GenerateDubaiDistricts(CityZoningMap map)
    {
        map.districts.Add(CreateDistrict("Downtown Dubai", World.ZoneType.Business, 700f, 400));
        map.districts.Add(CreateDistrict("Dubai Marina", World.ZoneType.Residential, 800f, 1000));
        map.districts.Add(CreateDistrict("Palm Jumeirah", World.ZoneType.Residential, 650f, 600));
        map.districts.Add(CreateDistrict("Jumeirah Beach Residence", World.ZoneType.Residential, 700f, 900));
        map.districts.Add(CreateDistrict("Dubai Silicon Oasis", World.ZoneType.Business, 500f, 300));
        map.districts.Add(CreateDistrict("Arabian Ranches", World.ZoneType.Residential, 750f, 850));
        
        // Infrastructure
        map.districts.Add(CreateInfrastructureDistrict("Dubai International Airport", InfrastructureType.Airport, 450f));
        map.districts.Add(CreateInfrastructureDistrict("Al Maktoum Airport", InfrastructureType.Airport, 500f));
        map.districts.Add(CreateInfrastructureDistrict("Dubai Sports City Stadium", InfrastructureType.Stadium, 100f));
        map.districts.Add(CreateInfrastructureDistrict("Zabeel Park", InfrastructureType.Park, 180f));
        map.districts.Add(CreateInfrastructureDistrict("Safa Park", InfrastructureType.Park, 150f));
        map.districts.Add(CreateInfrastructureDistrict("Dubai Creek Park", InfrastructureType.Park, 200f));
        map.districts.Add(CreateInfrastructureDistrict("American University Dubai", InfrastructureType.University, 220f));
        map.districts.Add(CreateInfrastructureDistrict("Dubai Knowledge Park", InfrastructureType.College, 280f));
        map.districts.Add(CreateInfrastructureDistrict("KHDA School District", InfrastructureType.SchoolDistrict, 300f));
        map.districts.Add(CreateInfrastructureDistrict("Dubai Healthcare City", InfrastructureType.Hospital, 200f));
    }
    
    private void GenerateLADistricts(CityZoningMap map)
    {
        map.districts.Add(CreateDistrict("Hollywood", World.ZoneType.Commercial, 600f, 400));
        map.districts.Add(CreateDistrict("Beverly Hills", World.ZoneType.Residential, 750f, 600));
        map.districts.Add(CreateDistrict("Santa Monica", World.ZoneType.Residential, 700f, 800));
        map.districts.Add(CreateDistrict("Venice Beach", World.ZoneType.Residential, 650f, 750));
        map.districts.Add(CreateDistrict("Downtown LA", World.ZoneType.Business, 550f, 350));
        map.districts.Add(CreateDistrict("Pasadena", World.ZoneType.Residential, 600f, 700));
        map.districts.Add(CreateDistrict("Long Beach", World.ZoneType.Residential, 700f, 900));
        
        // Infrastructure
        map.districts.Add(CreateInfrastructureDistrict("LAX International Airport", InfrastructureType.Airport, 380f));
        map.districts.Add(CreateInfrastructureDistrict("SoFi Stadium", InfrastructureType.Stadium, 150f));
        map.districts.Add(CreateInfrastructureDistrict("Dodger Stadium", InfrastructureType.Stadium, 85f));
        map.districts.Add(CreateInfrastructureDistrict("Rose Bowl", InfrastructureType.Stadium, 95f));
        map.districts.Add(CreateInfrastructureDistrict("Griffith Park", InfrastructureType.Park, 300f));
        map.districts.Add(CreateInfrastructureDistrict("Runyon Canyon Park", InfrastructureType.Park, 150f));
        map.districts.Add(CreateInfrastructureDistrict("Venice Beach Park", InfrastructureType.Park, 120f));
        map.districts.Add(CreateInfrastructureDistrict("UCLA Campus", InfrastructureType.University, 320f));
        map.districts.Add(CreateInfrastructureDistrict("USC Campus", InfrastructureType.University, 300f));
        map.districts.Add(CreateInfrastructureDistrict("LAUSD Schools", InfrastructureType.SchoolDistrict, 400f));
        map.districts.Add(CreateInfrastructureDistrict("Cedars-Sinai Medical Center", InfrastructureType.Hospital, 140f));
    }
    
    private void GenerateParisDistricts(CityZoningMap map)
    {
        map.districts.Add(CreateDistrict("1st Arrondissement", World.ZoneType.Commercial, 300f, 200));
        map.districts.Add(CreateDistrict("Marais", World.ZoneType.Residential, 400f, 500));
        map.districts.Add(CreateDistrict("Montmartre", World.ZoneType.Residential, 450f, 600));
        map.districts.Add(CreateDistrict("Saint-Germain", World.ZoneType.Residential, 500f, 650));
        map.districts.Add(CreateDistrict("Latin Quarter", World.ZoneType.Residential, 450f, 600));
        map.districts.Add(CreateDistrict("Champs-Élysées", World.ZoneType.Commercial, 550f, 300));
        map.districts.Add(CreateDistrict("La Défense", World.ZoneType.Business, 600f, 400));
        
        // Infrastructure
        map.districts.Add(CreateInfrastructureDistrict("Charles de Gaulle Airport", InfrastructureType.Airport, 400f));
        map.districts.Add(CreateInfrastructureDistrict("Orly Airport", InfrastructureType.Airport, 280f));
        map.districts.Add(CreateInfrastructureDistrict("Stade de France", InfrastructureType.Stadium, 120f));
        map.districts.Add(CreateInfrastructureDistrict("Parc des Princes", InfrastructureType.Stadium, 85f));
        map.districts.Add(CreateInfrastructureDistrict("Jardin du Luxembourg", InfrastructureType.Park, 180f));
        map.districts.Add(CreateInfrastructureDistrict("Tuileries Garden", InfrastructureType.Park, 150f));
        map.districts.Add(CreateInfrastructureDistrict("Bois de Boulogne", InfrastructureType.Park, 280f));
        map.districts.Add(CreateInfrastructureDistrict("Sorbonne University", InfrastructureType.University, 250f));
        map.districts.Add(CreateInfrastructureDistrict("Sciences Po", InfrastructureType.University, 220f));
        map.districts.Add(CreateInfrastructureDistrict("Paris School System", InfrastructureType.SchoolDistrict, 320f));
        map.districts.Add(CreateInfrastructureDistrict("Hôpital Pitié-Salpêtrière", InfrastructureType.Hospital, 130f));
    }
    
    /// <summary>
    /// Create a district with parcels
    /// </summary>
    private ZoneDistrict CreateDistrict(string name, World.ZoneType zoneType, float area, int parcelCount)
    {
        ZoneDistrict district = new ZoneDistrict
        {
            name = name,
            zoneType = zoneType,
            area = area,
            parcels = new List<ZoneParcel>(),
            infrastructureType = InfrastructureType.None
        };
        
        // Generate parcels for this district
        for (int i = 0; i < parcelCount; i++)
        {
            ZoneParcel parcel = GenerateParcel(district, i);
            district.parcels.Add(parcel);
            generatedParcels.Add(parcel);
        }
        
        return district;
    }
    
    /// <summary>
    /// Create infrastructure district (airports, stadiums, parks, schools, etc.)
    /// </summary>
    private ZoneDistrict CreateInfrastructureDistrict(string name, InfrastructureType infraType, float area)
    {
        ZoneDistrict district = new ZoneDistrict
        {
            name = name,
            zoneType = World.ZoneType.Recreation, // Infrastructure uses Recreation zone
            area = area,
            parcels = new List<ZoneParcel>(),
            infrastructureType = infraType
        };
        
        // Infrastructure districts have single large parcel
        ZoneParcel infrastructureParcel = new ZoneParcel
        {
            id = $"{name.Replace(" ", "_")}_INFRA",
            districtName = name,
            zoneType = World.ZoneType.Recreation,
            size = area * 10000f, // Convert hectares to square meters
            position = new Vector3(
                (float)random.NextDouble() * 2000f,
                0f,
                (float)random.NextDouble() * 2000f
            ),
            isAvailable = false, // Infrastructure parcels not for sale
            baseValue = CalculateInfrastructureValue(infraType, area),
            infrastructureType = infraType
        };
        
        district.parcels.Add(infrastructureParcel);
        generatedParcels.Add(infrastructureParcel);
        
        Debug.Log($"Created infrastructure: {name} ({infraType}) - {area} hectares, Value: {infrastructureParcel.baseValue:C0} OMNI");
        
        return district;
    }
    
    /// <summary>
    /// Generate individual parcel with appropriate residential/commercial type and REAL ESTATE VALUE
    /// </summary>
    private ZoneParcel GenerateParcel(ZoneDistrict district, int index)
    {
        float parcelSize = parcelMinSize + (float)random.NextDouble() * (parcelMaxSize - parcelMinSize);
        
        ZoneParcel parcel = new ZoneParcel
        {
            id = $"{district.name}_P{index:D4}",
            districtName = district.name,
            zoneType = district.zoneType,
            size = parcelSize,
            position = new Vector3(
                (float)random.NextDouble() * 1000f,
                0f,
                (float)random.NextDouble() * 1000f
            ),
            isAvailable = true
        };
        
        // Calculate real estate value based on zone type
        parcel.baseValue = CalculateParcelValue(district.zoneType, parcelSize);
        
        // Add location premium (random variation for realism)
        float locationPremium = 0.8f + (float)random.NextDouble() * 0.4f; // 0.8x to 1.2x
        parcel.currentMarketValue = parcel.baseValue * locationPremium;
        
        // Assign property-specific attributes based on zone type
        switch (district.zoneType)
        {
            case World.ZoneType.Residential:
                parcel.residentialType = DetermineResidentialType(parcelSize);
                parcel.capacity = DetermineResidentialCapacity(parcel.residentialType);
                // Residential: Add appreciation potential
                parcel.appreciationRate = 0.03f + (float)random.NextDouble() * 0.05f; // 3-8% annual
                break;
                
            case World.ZoneType.Commercial:
                parcel.commercialType = DetermineCommercialType(parcelSize);
                parcel.capacity = DetermineCommercialCapacity(parcel.commercialType);
                parcel.monthlyRevenuePotential = CalculateCommercialRevenue(parcel.commercialType, parcelSize);
                parcel.appreciationRate = 0.04f + (float)random.NextDouble() * 0.06f; // 4-10% annual
                break;
                
            case World.ZoneType.Business:
                parcel.businessType = DetermineBusinessType(parcelSize);
                parcel.capacity = DetermineBusinessCapacity(parcel.businessType);
                parcel.monthlyRevenuePotential = CalculateBusinessRevenue(parcel.businessType, parcelSize);
                parcel.appreciationRate = 0.05f + (float)random.NextDouble() * 0.07f; // 5-12% annual
                break;
                
            case World.ZoneType.Industrial:
                parcel.industrialType = DetermineIndustrialType(parcelSize);
                parcel.capacity = parcelSize / 10f; // Square footage capacity
                parcel.monthlyRevenuePotential = parcelSize * 5f; // $5 per sq meter
                parcel.appreciationRate = 0.02f + (float)random.NextDouble() * 0.03f; // 2-5% annual
                break;
        }
        
        return parcel;
    }
    
    /// <summary>
    /// Determine residential property type based on parcel size and random distribution
    /// </summary>
    private ResidentialPropertyType DetermineResidentialType(float parcelSize)
    {
        // Larger parcels = higher-end properties
        if (parcelSize > 80f)
        {
            int roll = random.Next(100);
            if (roll < 40) return ResidentialPropertyType.Mansion;
            if (roll < 70) return ResidentialPropertyType.SingleFamilyHome;
            return ResidentialPropertyType.Penthouse;
        }
        else if (parcelSize > 60f)
        {
            int roll = random.Next(100);
            if (roll < 30) return ResidentialPropertyType.SingleFamilyHome;
            if (roll < 60) return ResidentialPropertyType.Duplex;
            if (roll < 85) return ResidentialPropertyType.Condo;
            return ResidentialPropertyType.Penthouse;
        }
        else if (parcelSize > 40f)
        {
            int roll = random.Next(100);
            if (roll < 40) return ResidentialPropertyType.ApartmentUnit;
            if (roll < 70) return ResidentialPropertyType.Condo;
            if (roll < 90) return ResidentialPropertyType.Duplex;
            return ResidentialPropertyType.Townhouse;
        }
        else
        {
            int roll = random.Next(100);
            if (roll < 50) return ResidentialPropertyType.ApartmentUnit;
            if (roll < 75) return ResidentialPropertyType.StudioApartment;
            if (roll < 90) return ResidentialPropertyType.HostelRoom;
            return ResidentialPropertyType.HotelRoom;
        }
    }
    
    /// <summary>
    /// Determine residential capacity (number of occupants)
    /// </summary>
    private int DetermineResidentialCapacity(ResidentialPropertyType type)
    {
        switch (type)
        {
            case ResidentialPropertyType.StudioApartment:
            case ResidentialPropertyType.HotelRoom:
            case ResidentialPropertyType.HostelRoom:
                return random.Next(1, 3); // 1-2 people
            case ResidentialPropertyType.ApartmentUnit:
            case ResidentialPropertyType.Condo:
                return random.Next(2, 5); // 2-4 people
            case ResidentialPropertyType.Duplex:
            case ResidentialPropertyType.Townhouse:
                return random.Next(4, 7); // 4-6 people
            case ResidentialPropertyType.SingleFamilyHome:
                return random.Next(4, 9); // 4-8 people
            case ResidentialPropertyType.Penthouse:
                return random.Next(2, 7); // 2-6 people
            case ResidentialPropertyType.Mansion:
                return random.Next(6, 13); // 6-12 people
            case ResidentialPropertyType.CollegeDorm:
                return random.Next(2, 5); // 2-4 people
            default:
                return 2;
        }
    }
    
    /// <summary>
    /// Calculate parcel base value
    /// </summary>
    private float CalculateParcelValue(World.ZoneType zoneType, float parcelSize)
    {
        float basePrice = 1000f; // per square meter
        
        float zoneMultiplier = 1.0f;
        switch (zoneType)
        {
            case World.ZoneType.Residential:
                zoneMultiplier = 1.2f;
                break;
            case World.ZoneType.Business:
                zoneMultiplier = 2.0f;
                break;
            case World.ZoneType.Commercial:
                zoneMultiplier = 1.8f;
                break;
            case World.ZoneType.Recreation:
                zoneMultiplier = 1.5f;
                break;
            case World.ZoneType.Industrial:
                zoneMultiplier = 0.8f;
                break;
        }
        
        return basePrice * parcelSize * zoneMultiplier;
    }
    
    /// <summary>
    /// Calculate infrastructure value
    /// </summary>
    private float CalculateInfrastructureValue(InfrastructureType type, float area)
    {
        float baseValue = area * 50000f; // Base infrastructure cost per hectare
        
        float typeMultiplier = 1.0f;
        switch (type)
        {
            case InfrastructureType.Airport:
                typeMultiplier = 10.0f;
                break;
            case InfrastructureType.Stadium:
                typeMultiplier = 5.0f;
                break;
            case InfrastructureType.University:
            case InfrastructureType.Hospital:
                typeMultiplier = 4.0f;
                break;
            case InfrastructureType.Park:
                typeMultiplier = 2.0f;
                break;
            case InfrastructureType.College:
            case InfrastructureType.SchoolDistrict:
                typeMultiplier = 3.0f;
                break;
            case InfrastructureType.PoliceStation:
            case InfrastructureType.EmergencyServices:
                typeMultiplier = 2.5f;
                break;
            default:
                typeMultiplier = 1.5f;
                break;
        }
        
        return baseValue * typeMultiplier;
    }
    
    // ===== COMMERCIAL PROPERTY METHODS =====
    
    private CommercialPropertyType DetermineCommercialType(float size)
    {
        if (size > 80f) return (CommercialPropertyType)random.Next(0, 3); // Large retail
        if (size > 60f) return (CommercialPropertyType)random.Next(1, 5);
        if (size > 40f) return (CommercialPropertyType)random.Next(3, 7);
        return (CommercialPropertyType)random.Next(5, 9);
    }
    
    private int DetermineCommercialCapacity(CommercialPropertyType type)
    {
        switch (type)
        {
            case CommercialPropertyType.ShoppingMall: return 500;
            case CommercialPropertyType.DepartmentStore: return 200;
            case CommercialPropertyType.Supermarket: return 150;
            case CommercialPropertyType.RetailStore: return 50;
            case CommercialPropertyType.Restaurant: return 100;
            case CommercialPropertyType.Cafe: return 40;
            case CommercialPropertyType.Bar: return 80;
            case CommercialPropertyType.Boutique: return 30;
            default: return 50;
        }
    }
    
    private float CalculateCommercialRevenue(CommercialPropertyType type, float size)
    {
        float baseRevenue = size * 100f; // $100 per sq meter monthly
        
        switch (type)
        {
            case CommercialPropertyType.ShoppingMall: return baseRevenue * 3.0f;
            case CommercialPropertyType.DepartmentStore: return baseRevenue * 2.5f;
            case CommercialPropertyType.Restaurant: return baseRevenue * 2.0f;
            case CommercialPropertyType.Supermarket: return baseRevenue * 1.8f;
            case CommercialPropertyType.RetailStore: return baseRevenue * 1.5f;
            default: return baseRevenue;
        }
    }
    
    // ===== BUSINESS PROPERTY METHODS =====
    
    private BusinessPropertyType DetermineBusinessType(float size)
    {
        if (size > 80f) return (BusinessPropertyType)random.Next(0, 3);
        if (size > 60f) return (BusinessPropertyType)random.Next(2, 5);
        if (size > 40f) return (BusinessPropertyType)random.Next(3, 6);
        return (BusinessPropertyType)random.Next(4, 7);
    }
    
    private int DetermineBusinessCapacity(BusinessPropertyType type)
    {
        switch (type)
        {
            case BusinessPropertyType.CorporateHeadquarters: return 500;
            case BusinessPropertyType.OfficeTower: return 300;
            case BusinessPropertyType.TechCampus: return 400;
            case BusinessPropertyType.OfficeSpace: return 100;
            case BusinessPropertyType.CoworkingSpace: return 150;
            case BusinessPropertyType.StartupIncubator: return 80;
            default: return 100;
        }
    }
    
    private float CalculateBusinessRevenue(BusinessPropertyType type, float size)
    {
        float baseRevenue = size * 150f; // $150 per sq meter monthly
        
        switch (type)
        {
            case BusinessPropertyType.CorporateHeadquarters: return baseRevenue * 3.5f;
            case BusinessPropertyType.TechCampus: return baseRevenue * 3.0f;
            case BusinessPropertyType.OfficeTower: return baseRevenue * 2.5f;
            case BusinessPropertyType.CoworkingSpace: return baseRevenue * 2.0f;
            default: return baseRevenue * 1.5f;
        }
    }
    
    // ===== INDUSTRIAL PROPERTY METHODS =====
    
    private IndustrialPropertyType DetermineIndustrialType(float size)
    {
        if (size > 80f) return IndustrialPropertyType.ManufacturingPlant;
        if (size > 60f) return IndustrialPropertyType.Warehouse;
        if (size > 40f) return (IndustrialPropertyType)random.Next(2, 5);
        return (IndustrialPropertyType)random.Next(3, 6);
    }
}

/// <summary>
/// City-wide zoning map
/// </summary>
[System.Serializable]
public class CityZoningMap
{
    public string cityName;
    public float totalArea; // square kilometers
    public List<ZoneDistrict> districts;
    
    public int GetTotalParcels()
    {
        int total = 0;
        foreach (var district in districts)
        {
            total += district.parcels.Count;
        }
        return total;
    }
    
    public int GetResidentialParcels()
    {
        int total = 0;
        foreach (var district in districts)
        {
            if (district.zoneType == OmniWorld.World.ZoneType.Residential)
            {
                total += district.parcels.Count;
            }
        }
        return total;
    }
}

/// <summary>
/// Zone district within a city
/// </summary>
[System.Serializable]
public class ZoneDistrict
{
    public string name;
    public OmniWorld.World.ZoneType zoneType;
    public float area; // hectares
    public List<ZoneParcel> parcels;
    public int currentOccupancy;
    public int maxOccupancy;
    public InfrastructureType infrastructureType = InfrastructureType.None; // For infrastructure districts
}

/// <summary>
/// Individual lot parcel with REAL ESTATE VALUE
/// </summary>
[System.Serializable]
public class ZoneParcel
{
    public string id;
    public string districtName;
    public OmniWorld.World.ZoneType zoneType;
    public float size; // square meters
    public Vector3 position;
    public bool isAvailable;
    public string owner;
    
    // REAL ESTATE VALUES
    public float baseValue; // Base OMNI token value
    public float currentMarketValue; // Current market value with location premium
    public float appreciationRate; // Annual appreciation rate (0.03 = 3%)
    public float monthlyRevenuePotential; // Potential monthly revenue for commercial/business
    
    // Property Type Classifications
    public ResidentialPropertyType residentialType;
    public CommercialPropertyType commercialType;
    public BusinessPropertyType businessType;
    public IndustrialPropertyType industrialType;
    public InfrastructureType infrastructureType = InfrastructureType.None;
    
    public int capacity; // number of occupants or capacity
    public bool isNFT;
}

/// <summary>
/// Comprehensive residential property types
/// </summary>
public enum ResidentialPropertyType
{
    StudioApartment,      // 1-2 people
    ApartmentUnit,        // 2-4 people  
    Condo,                // 2-4 people
    Duplex,               // 4-6 people
    Townhouse,            // 4-6 people
    SingleFamilyHome,     // 4-8 people
    Penthouse,            // 2-6 people
    Mansion,              // 6-12 people
    HotelRoom,            // 1-2 people (short-term rental)
    HostelRoom,           // 1-2 people (budget)
    CollegeDorm,          // 2-4 people (students)
    SharedHousing,        // 4-8 people (co-living)
    LuxuryVilla           // 8-16 people (high-end)
}

/// <summary>
/// Commercial property types for retail/dining
/// </summary>
public enum CommercialPropertyType
{
    ShoppingMall,         // Large retail complex
    DepartmentStore,      // Multi-floor retail
    Supermarket,          // Grocery/food retail
    RetailStore,          // Standard retail shop
    Restaurant,           // Full-service dining
    Cafe,                 // Coffee/light dining
    Bar,                  // Drinking establishment
    Boutique,             // Luxury/specialty retail
    FoodCourt            // Multiple food vendors
}

/// <summary>
/// Business property types for offices/corporate
/// </summary>
public enum BusinessPropertyType
{
    CorporateHeadquarters, // Large corporate HQ
    OfficeTower,          // Multi-tenant office building
    TechCampus,           // Tech company campus
    OfficeSpace,          // Standard office rental
    CoworkingSpace,       // Shared workspace
    StartupIncubator,     // Startup workspace/support
    BusinessPark          // Business park complex
}

/// <summary>
/// Industrial property types
/// </summary>
public enum IndustrialPropertyType
{
    ManufacturingPlant,   // Production facility
    Warehouse,            // Storage facility
    DistributionCenter,   // Logistics hub
    DataCenter,           // Tech infrastructure
    TechLab,              // R&D facility
    ProcessingFacility    // Material processing
}

/// <summary>
/// Infrastructure types for public facilities
/// </summary>
public enum InfrastructureType
{
    None,                 // Not infrastructure
    Airport,              // Air transportation hub
    Stadium,              // Sports/entertainment venue
    Park,                 // Public park/recreation
    University,           // Higher education (4-year)
    College,              // Community college/technical
    SchoolDistrict,       // K-12 schools
    Hospital,             // Medical facility
    PoliceStation,        // Law enforcement
    FireStation,          // Fire department
    EmergencyServices,    // EMS/911 services
    TrainStation,         // Rail transportation
    BusTerminal,          // Bus transportation
    Port,                 // Shipping/maritime
    Library,              // Public library
    CommunityCenter,      // Recreation center
    Government            // Government building
}
