using UnityEngine;
using OmniWorld.AI;
using OmniWorld.Core;
using OmniWorld.World;

namespace OmniWorld.Examples
{
    /// <summary>
    /// Example script demonstrating ProceduralGeneration usage
    /// Attach to a GameObject in your city scene to generate content
    /// </summary>
    public class CityGenerator : MonoBehaviour
    {
        [Header("Generation Settings")]
        [Tooltip("Name of the city to generate (e.g., OmniLanta, OmniVegas)")]
        public string cityName = "OmniLanta";
        
        [Tooltip("Generate city content on Start")]
        public bool generateOnStart = true;
        
        [Tooltip("Show generation statistics in console")]
        public bool showStatistics = true;

        [Header("Prefab References (Optional)")]
        [Tooltip("Building prefab to instantiate (leave null for data-only generation)")]
        public GameObject buildingPrefab;
        
        [Tooltip("NPC prefab to instantiate (leave null for data-only generation)")]
        public GameObject npcPrefab;

        private void Start()
        {
            if (generateOnStart)
            {
                GenerateCity();
            }
        }

        /// <summary>
        /// Generate the complete city
        /// </summary>
        [ContextMenu("Generate City")]
        public void GenerateCity()
        {
            Debug.Log($"Starting city generation for {cityName}...");

            // Set the current city in GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetCurrentCity(cityName);
            }

            // Generate the complete city
            ProceduralGeneration.Instance.GenerateCompleteCity(cityName);

            // Get generated content
            var buildings = ProceduralGeneration.Instance.GetGeneratedBuildings();
            var npcs = ProceduralGeneration.Instance.GetGeneratedNPCs();
            var quests = ProceduralGeneration.Instance.GetGeneratedQuests();
            var events = ProceduralGeneration.Instance.GetGeneratedEvents();

            if (showStatistics)
            {
                ShowGenerationStatistics(buildings.Count, npcs.Count, quests.Count, events.Count);
            }

            // Optionally instantiate prefabs (if provided)
            if (buildingPrefab != null)
            {
                InstantiateBuildings(buildings);
            }

            if (npcPrefab != null)
            {
                InstantiateNPCs(npcs);
            }

            Debug.Log($"City generation complete for {cityName}!");
        }

        /// <summary>
        /// Generate a specific district at runtime
        /// </summary>
        public void GenerateDistrict(ZoneType zoneType, Vector3 position, float radius = 200f)
        {
            Debug.Log($"Generating {zoneType} district at {position}...");
            
            ProceduralGeneration.Instance.GenerateDistrict(zoneType, position, radius);
            
            var newBuildings = ProceduralGeneration.Instance.GetGeneratedBuildings();
            
            if (buildingPrefab != null)
            {
                // Instantiate only the new buildings
                // (In a real implementation, you'd track which buildings are new)
                InstantiateBuildings(newBuildings);
            }
        }

        /// <summary>
        /// Generate a city-themed event
        /// </summary>
        [ContextMenu("Generate City Event")]
        public void GenerateCityEvent()
        {
            var cityEvent = ProceduralGeneration.Instance.GenerateCityEvent(cityName);
            
            Debug.Log($"Event Generated: {cityEvent.name}");
            Debug.Log($"Duration: {cityEvent.duration} minutes");
            Debug.Log($"Economic Impact: {cityEvent.economicImpact:P1}");
            Debug.Log($"Type: {cityEvent.eventType}");
        }

        /// <summary>
        /// Clear all generated content
        /// </summary>
        [ContextMenu("Clear Generated Content")]
        public void ClearGeneratedContent()
        {
            ProceduralGeneration.Instance.ClearGenerated();
            Debug.Log("All generated content cleared.");
        }

        /// <summary>
        /// Show generation statistics
        /// </summary>
        private void ShowGenerationStatistics(int buildingCount, int npcCount, int questCount, int eventCount)
        {
            Debug.Log("=== Generation Statistics ===");
            Debug.Log($"City: {cityName}");
            Debug.Log($"Buildings: {buildingCount}");
            Debug.Log($"NPCs: {npcCount}");
            Debug.Log($"Quests: {questCount}");
            Debug.Log($"Events: {eventCount}");
            Debug.Log("============================");
        }

        /// <summary>
        /// Instantiate building prefabs
        /// </summary>
        private void InstantiateBuildings(System.Collections.Generic.List<GeneratedBuilding> buildings)
        {
            GameObject buildingsParent = new GameObject("Generated Buildings");
            buildingsParent.transform.SetParent(transform);

            foreach (var building in buildings)
            {
                GameObject buildingObj = Instantiate(buildingPrefab, building.position, Quaternion.identity);
                buildingObj.name = building.isLandmark ? building.name : $"Building_{building.zoneType}";
                buildingObj.transform.SetParent(buildingsParent.transform);
                
                // Scale based on dimensions
                buildingObj.transform.localScale = new Vector3(
                    building.width / 10f,  // Normalize to reasonable scale
                    building.height / 10f,
                    building.depth / 10f
                );

                // You could add more custom logic here:
                // - Set building material based on style
                // - Add colliders based on zone type
                // - Set up property ownership systems
                // - Add lighting for landmarks
            }

            Debug.Log($"Instantiated {buildings.Count} buildings");
        }

        /// <summary>
        /// Instantiate NPC prefabs
        /// </summary>
        private void InstantiateNPCs(System.Collections.Generic.List<NPCData> npcs)
        {
            GameObject npcsParent = new GameObject("Generated NPCs");
            npcsParent.transform.SetParent(transform);

            int spawnCount = 0;
            foreach (var npcData in npcs)
            {
                // Spawn NPCs in a grid pattern for demonstration
                Vector3 spawnPos = new Vector3(
                    (spawnCount % 10) * 5f,
                    0,
                    (spawnCount / 10) * 5f
                );

                GameObject npcObj = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
                npcObj.name = npcData.name;
                npcObj.transform.SetParent(npcsParent.transform);

                // Apply NPC data to NPCBrain component if it exists
                var npcBrain = npcObj.GetComponent<NPCBrain>();
                if (npcBrain != null)
                {
                    npcBrain.npcName = npcData.name;
                    npcBrain.role = npcData.role;
                    npcBrain.personality = npcData.personality;
                    npcBrain.walletBalance = npcData.walletBalance;
                }

                spawnCount++;
            }

            Debug.Log($"Instantiated {npcs.Count} NPCs");
        }

        /// <summary>
        /// Example: Generate content based on player position
        /// </summary>
        public void GenerateNearbyContent(Vector3 playerPosition, float radius = 300f)
        {
            // Determine appropriate zone type based on location
            // (In a real implementation, you'd have zone boundaries)
            ZoneType nearbyZone = DetermineZoneType(playerPosition);
            
            // Generate district near player
            GenerateDistrict(nearbyZone, playerPosition, radius);
            
            // Generate some NPCs
            for (int i = 0; i < 5; i++)
            {
                var npc = ProceduralGeneration.Instance.GenerateNPCWithRole(
                    GetRoleForZone(nearbyZone),
                    cityName
                );
                
                // Spawn NPC near player (if prefab provided)
                if (npcPrefab != null)
                {
                    Vector3 npcPos = playerPosition + Random.insideUnitSphere * radius * 0.5f;
                    npcPos.y = 0; // Keep on ground
                    
                    GameObject npcObj = Instantiate(npcPrefab, npcPos, Quaternion.identity);
                    var npcBrain = npcObj.GetComponent<NPCBrain>();
                    if (npcBrain != null)
                    {
                        npcBrain.npcName = npc.name;
                        npcBrain.role = npc.role;
                        npcBrain.personality = npc.personality;
                        npcBrain.walletBalance = npc.walletBalance;
                    }
                }
            }
            
            Debug.Log($"Generated content near {playerPosition}");
        }

        /// <summary>
        /// Determine zone type based on position (simplified example)
        /// </summary>
        private ZoneType DetermineZoneType(Vector3 position)
        {
            // Simple logic - in real implementation, use zone boundaries
            float x = position.x;
            float z = position.z;
            
            if (x > 0 && z > 0) return ZoneType.Business;
            if (x < 0 && z > 0) return ZoneType.Commercial;
            if (x > 0 && z < 0) return ZoneType.Recreation;
            if (x < 0 && z < 0) return ZoneType.Industrial;
            return ZoneType.Residential;
        }

        /// <summary>
        /// Get appropriate NPC role for zone type
        /// </summary>
        private NPCRole GetRoleForZone(ZoneType zone)
        {
            return zone switch
            {
                ZoneType.Business => NPCRole.Banker,
                ZoneType.Commercial => NPCRole.Merchant,
                ZoneType.Recreation => NPCRole.Entertainer,
                ZoneType.Industrial => NPCRole.Citizen,
                ZoneType.Residential => NPCRole.Citizen,
                _ => NPCRole.Citizen
            };
        }
    }
}
