using UnityEngine;
using System.Collections.Generic;

namespace OmniWorld.AI
{
    /// <summary>
    /// Procedural generation system for content and assets
    /// Creates buildings, quests, NPCs, and events dynamically
    /// </summary>
    public class ProceduralGeneration : MonoBehaviour
    {
        private static ProceduralGeneration _instance;
        public static ProceduralGeneration Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ProceduralGeneration>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ProceduralGeneration");
                        _instance = go.AddComponent<ProceduralGeneration>();
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
        public int maxBuildingsPerZone = 50;
        public float buildingSpacing = 10f;

        [Header("Asset Variety")]
        public int buildingVariations = 20;
        public int npcVariations = 50;
        public int questVariations = 100;

        private System.Random random;
        private List<GeneratedBuilding> generatedBuildings = new List<GeneratedBuilding>();

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
            
            Debug.Log($"Procedural Generation Initialized - Seed: {seed}");
        }

        /// <summary>
        /// Generate a city district procedurally
        /// </summary>
        public void GenerateDistrict(World.ZoneType zoneType, Vector3 centerPoint, float radius)
        {
            Debug.Log($"Generating {zoneType} district at {centerPoint}");

            int buildingCount = random.Next(minBuildingsPerZone, maxBuildingsPerZone);
            
            for (int i = 0; i < buildingCount; i++)
            {
                GenerateBuilding(zoneType, centerPoint, radius);
            }

            Debug.Log($"Generated {buildingCount} buildings in {zoneType} district");
        }

        /// <summary>
        /// Generate a single building
        /// </summary>
        private void GenerateBuilding(World.ZoneType zoneType, Vector3 center, float radius)
        {
            // Random position within radius
            float angle = (float)random.NextDouble() * Mathf.PI * 2f;
            float distance = (float)random.NextDouble() * radius;
            
            Vector3 position = center + new Vector3(
                Mathf.Cos(angle) * distance,
                0,
                Mathf.Sin(angle) * distance
            );

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
                _ => BuildingStyle.Contemporary
            };
        }

        /// <summary>
        /// Calculate building value
        /// </summary>
        private float CalculateBuildingValue(World.ZoneType zoneType)
        {
            var zoneData = World.ZoneController.Instance.GetZoneData(zoneType);
            float baseValue = zoneData?.basePropertyValue ?? 1000f;
            
            float variation = (float)random.NextDouble() * 0.5f + 0.75f; // 75% to 125%
            
            return baseValue * variation;
        }

        /// <summary>
        /// Generate a random NPC
        /// </summary>
        public NPCData GenerateNPC()
        {
            NPCData npc = new NPCData
            {
                name = GenerateRandomName(),
                role = (NPCRole)random.Next(0, 7),
                personality = GetRandomPersonality(),
                walletBalance = (float)random.NextDouble() * 5000f + 500f,
                reputation = (float)random.NextDouble()
            };

            Debug.Log($"Generated NPC: {npc.name} ({npc.role})");

            return npc;
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
                "Sports Tournament",
                "Food Fair",
                "Gaming Convention",
                "Fashion Show",
                "Cultural Celebration"
            };

            return eventNames[random.Next(eventNames.Length)];
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
            Debug.Log("Cleared all generated content");
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

    public enum EventType
    {
        Cultural,
        Economic,
        Entertainment,
        Sports,
        Technology
    }
}
