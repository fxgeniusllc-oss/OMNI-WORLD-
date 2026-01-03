# Procedural Generation System

## Overview

The ProceduralGeneration system is the core content creation engine for OmniWorld, responsible for dynamically generating cities, buildings, NPCs, quests, and events. It integrates seamlessly with all major systems including ZoneController, DominionEconomy, NPCBrain, and GameManager to create cohesive, city-specific content.

## Architecture

### Core Components Integration

```
ProceduralGeneration
    ├── ZoneController Integration
    │   └── Zone-aware building generation
    ├── DominionEconomy Integration
    │   └── Dynamic asset pricing with quantum algorithm
    ├── NPCBrain Integration
    │   └── NPC spawning and role assignment
    └── GameManager Integration
        └── City-specific content generation
```

## Features

### 1. Building Generation

Buildings are generated based on zone types with economic value calculated through DominionEconomy:

```csharp
// Generate a district with buildings
ProceduralGeneration.Instance.GenerateDistrict(
    World.ZoneType.Business, 
    centerPoint, 
    radius
);
```

**Building Properties:**
- Position (Vector3)
- Zone Type (Residential, Business, Commercial, Recreation, Industrial)
- Dimensions (height, width, depth)
- Architectural Style (city-specific)
- Economic Value (DominionEconomy-integrated)
- Landmark Status (for signature properties)

### 2. NPC Generation

NPCs are generated with roles, personalities, and city-themed names:

```csharp
// Generate random NPC
NPCData npc = ProceduralGeneration.Instance.GenerateNPC();

// Generate NPC with specific role for a city
NPCData merchant = ProceduralGeneration.Instance.GenerateNPCWithRole(
    NPCRole.Merchant, 
    "OmniTokyo"
);
```

**NPC Attributes:**
- City-themed names (e.g., "Yuki Tanaka" for OmniTokyo)
- Role-based wallet balance and reputation
- Personality traits
- Economic participation

### 3. Quest Generation

Quests are generated with city-specific themes aligned with each metropolis's culture:

```csharp
// Generate city-themed quest
Quest quest = ProceduralGeneration.Instance.GenerateCityQuest(
    "OmniLanta", 
    NPCRole.Merchant
);
```

**City-Specific Quest Themes:**

| City | Quest Examples |
|------|----------------|
| **OmniLanta** | Record at the Studio, Tech Startup Pitch, Mercedes-Benz Stadium Event |
| **OmniVegas** | Casino Floor Challenge, High Roller Suite Service, Neon District Promotion |
| **OmniTokyo** | Shibuya Tech Demo, Anime Cafe Event, Billboard Ad Campaign |
| **OmniNYC** | Wall Street Trading, Art Gallery Opening, Broadway Show Tickets |
| **OmniDubai** | Luxury Shopping Spree, Burj Tower Event, Gold Souk Trading |
| **OmniLA** | Hollywood Studio Tour, Influencer Photoshoot, Film Premiere Event |
| **OmniParis** | Fashion Show Coordination, Louvre Art Exhibition, Café Culture Experience |

### 4. Event Generation

City events are generated with economic impact and cultural relevance:

```csharp
// Generate city-specific event
CityEvent event = ProceduralGeneration.Instance.GenerateCityEvent("OmniVegas");
```

**Event Types:**
- Cultural Events
- Economic Events
- Entertainment Events
- Sports Events
- Technology Events

**City-Specific Event Examples:**

| City | Event Examples |
|------|----------------|
| **OmniLanta** | Trap Music Festival, Tech Startup Summit, Atlanta Film Festival |
| **OmniVegas** | Casino Grand Opening, Neon Night Spectacular, High Roller Championship |
| **OmniTokyo** | Anime Convention, Shibuya Tech Expo, Tokyo Game Show |
| **OmniNYC** | Wall Street Summit, Broadway Gala, NYC Art Week |
| **OmniDubai** | Dubai Luxury Expo, Gold Souk Festival, Marina Yacht Show |
| **OmniLA** | Hollywood Film Premiere, Beach Music Festival, Venice Art Walk |
| **OmniParis** | Paris Fashion Week, Louvre Night, Seine River Festival |

### 5. Complete City Generation

Generate an entire city with all features in one call:

```csharp
// Generate complete city with all zones, NPCs, quests, and events
ProceduralGeneration.Instance.GenerateCompleteCity("OmniTokyo");
```

This generates:
- 5 district zones (Residential, Business, Commercial, Recreation, Industrial)
- Buildings appropriate for each zone
- NPCs distributed across districts
- City-themed quests
- Cultural events
- Signature landmarks

## City-Specific Features

### Landmark Generation

Each city has unique landmarks that reflect its culture:

```csharp
// Landmarks are automatically generated per city
// Examples:
// OmniLanta: Mercedes-Benz Stadium
// OmniVegas: Maevn "Saint Drip" Private Penthouse, Maevn Mansion
// OmniTokyo: Shibuya Crossing Tower
// OmniNYC: Wall Street Financial Tower
// OmniDubai: Burj OmniWorld
// OmniLA: Hollywood Studios Complex
// OmniParis: Tour OmniWorld
```

### Architectural Styles

Building styles are city-specific:

| City | Architectural Style |
|------|-------------------|
| OmniTokyo | Cyberpunk |
| OmniVegas | Neon |
| OmniParis | Classical |
| OmniDubai | Modern |
| Others | Contemporary |

## Integration with Other Systems

### ZoneController Integration

```csharp
// ProceduralGeneration uses ZoneController for property values
var zoneData = World.ZoneController.Instance?.GetZoneData(zoneType);
float baseValue = zoneData?.basePropertyValue ?? 1000f;
```

**Benefits:**
- Zone-appropriate building generation
- Economic multipliers per zone type
- Occupancy tracking

### DominionEconomy Integration

```csharp
// Building values are calculated with current token price
float tokenPrice = Economy.DominionEconomy.Instance?.omniTokenPrice ?? 0.01f;
float economicMultiplier = Mathf.Max(tokenPrice / 0.01f, 0.5f);
return baseValue * variation * economicMultiplier;
```

**Benefits:**
- Dynamic asset pricing
- Real-time economic integration
- Market-responsive values

### NPCBrain Integration

Generated NPCs can be instantiated with NPCBrain components:

```csharp
// NPCData from ProceduralGeneration can be used to initialize NPCBrain
NPCData npcData = ProceduralGeneration.Instance.GenerateNPC();
// Apply to NPCBrain component
npcBrain.npcName = npcData.name;
npcBrain.role = npcData.role;
npcBrain.walletBalance = npcData.walletBalance;
```

### GameManager Integration

```csharp
// Current city context is used for generation
string city = Core.GameManager.Instance?.currentCity ?? "OmniLanta";
BuildingStyle style = GetBuildingStyle(zoneType); // City-specific
```

## Extensibility

The system is designed for easy extension:

### Adding a New City

1. Add city-specific quest titles:
```csharp
private string GetNewCityQuestTitle(NPCRole role)
{
    string[] quests = { "Quest 1", "Quest 2", "Quest 3" };
    return quests[random.Next(quests.Length)];
}
```

2. Add city-specific event names:
```csharp
private string GetNewCityEventName()
{
    string[] events = { "Event 1", "Event 2", "Event 3" };
    return events[random.Next(events.Length)];
}
```

3. Add landmarks in `GenerateCityLandmarks()`:
```csharp
case "NewCity":
    GenerateLandmark("Signature Building", position, BuildingStyle.Modern, 500000f);
    break;
```

4. Update name generation in `GenerateCityThemedName()`:
```csharp
case "NewCity":
    string[] firstNames = { "Name1", "Name2", "Name3" };
    string[] lastNames = { "LastName1", "LastName2" };
    // ... generate name
    break;
```

### Adding New Features

The system supports easy addition of new procedurally generated features:

```csharp
// Example: Add vehicle generation
public VehicleData GenerateVehicle()
{
    // Implementation
}

// Example: Add music track generation
public MusicTrack GenerateMusicTrack(string cityName)
{
    // City-themed music generation
}
```

## Usage Examples

### Scenario 1: Loading a City

```csharp
void LoadCity(string cityName)
{
    // Generate the complete city
    ProceduralGeneration.Instance.GenerateCompleteCity(cityName);
    
    // Get generated content
    var buildings = ProceduralGeneration.Instance.GetGeneratedBuildings();
    var npcs = ProceduralGeneration.Instance.GetGeneratedNPCs();
    var quests = ProceduralGeneration.Instance.GetGeneratedQuests();
    var events = ProceduralGeneration.Instance.GetGeneratedEvents();
    
    // Instantiate content in the scene
    InstantiateBuildings(buildings);
    SpawnNPCs(npcs);
    RegisterQuests(quests);
    ScheduleEvents(events);
}
```

### Scenario 2: Dynamic Content Updates

```csharp
void GenerateNewDistrict(Vector3 location)
{
    // Generate a new district at runtime
    ProceduralGeneration.Instance.GenerateDistrict(
        World.ZoneType.Commercial,
        location,
        200f
    );
    
    // Get newly generated buildings
    var newBuildings = ProceduralGeneration.Instance.GetGeneratedBuildings();
    
    // Instantiate only new buildings
    InstantiateNewContent(newBuildings);
}
```

### Scenario 3: Event-Driven Generation

```csharp
void OnPlayerEntersZone(World.ZoneType zone)
{
    // Generate zone-specific content
    string currentCity = Core.GameManager.Instance.currentCity;
    
    // Generate NPCs for this zone
    for (int i = 0; i < 5; i++)
    {
        var npc = ProceduralGeneration.Instance.GenerateNPCWithRole(
            GetAppropriateRole(zone),
            currentCity
        );
        SpawnNPC(npc);
    }
    
    // Generate quests
    var quest = ProceduralGeneration.Instance.GenerateCityQuest(
        currentCity,
        NPCRole.QuestGiver
    );
    RegisterQuest(quest);
}
```

## Performance Considerations

- **Batch Generation**: Use `GenerateCompleteCity()` during loading screens
- **Incremental Generation**: Generate districts on-demand as players explore
- **Caching**: Store generated content to avoid regeneration
- **Cleanup**: Use `ClearGenerated()` when switching cities

## Configuration

Key settings in the ProceduralGeneration component:

```csharp
[Header("Generation Settings")]
public int seed = 12345;              // Random seed for reproducibility
public bool useRandomSeed = false;    // Use random seed each time

[Header("Building Generation")]
public int minBuildingsPerZone = 10;  // Minimum buildings per district
public int maxBuildingsPerZone = 50;  // Maximum buildings per district
public float buildingSpacing = 10f;   // Spacing between buildings

[Header("Asset Variety")]
public int buildingVariations = 20;   // Number of building variations
public int npcVariations = 50;        // Number of NPC variations
public int questVariations = 100;     // Number of quest variations

[Header("City-Specific Features")]
public bool generateCityLandmarks = true;      // Generate landmarks
public bool generateSignatureProperties = true; // Generate signature properties
public bool generateCityEvents = true;         // Generate city events
```

## Best Practices

1. **Always specify a seed** for reproducible generation in testing
2. **Use city-specific methods** for themed content (quests, events, NPCs)
3. **Integrate with DominionEconomy** for realistic asset pricing
4. **Clear generated content** when switching cities to prevent memory leaks
5. **Generate incrementally** for large cities to maintain performance
6. **Test landmark generation** for each city to ensure uniqueness

## Future Enhancements

Planned additions to the procedural generation system:

- [ ] Vehicle generation with city-specific models
- [ ] Music track generation for OmniTunes
- [ ] Business generation with revenue models
- [ ] Weather and atmosphere generation
- [ ] Transit system generation
- [ ] Billboard and advertising content generation
- [ ] AI-driven content evolution based on player behavior
- [ ] Cross-city trade route generation
- [ ] Festival and seasonal event generation
- [ ] Dynamic quest chains and storylines

## API Reference

### Main Methods

| Method | Parameters | Returns | Description |
|--------|-----------|---------|-------------|
| `GenerateDistrict` | `ZoneType, Vector3, float` | `void` | Generate a complete district |
| `GenerateNPC` | - | `NPCData` | Generate random NPC |
| `GenerateNPCWithRole` | `NPCRole, string` | `NPCData` | Generate NPC with specific role |
| `GenerateQuest` | `NPCRole` | `Quest` | Generate random quest |
| `GenerateCityQuest` | `string, NPCRole` | `Quest` | Generate city-themed quest |
| `GenerateEvent` | - | `CityEvent` | Generate random event |
| `GenerateCityEvent` | `string` | `CityEvent` | Generate city-themed event |
| `GenerateCompleteCity` | `string` | `void` | Generate entire city |
| `ClearGenerated` | - | `void` | Clear all generated content |

### Getter Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `GetGeneratedBuildings` | `List<GeneratedBuilding>` | All generated buildings |
| `GetGeneratedNPCs` | `List<NPCData>` | All generated NPCs |
| `GetGeneratedQuests` | `List<Quest>` | All generated quests |
| `GetGeneratedEvents` | `List<CityEvent>` | All generated events |

## Conclusion

The ProceduralGeneration system is a comprehensive, extensible, and fully integrated content creation engine that brings OmniWorld's seven metropolises to life. By leveraging city-specific themes, economic integration, and modular architecture, it enables infinite scalability while maintaining the unique character of each city.
