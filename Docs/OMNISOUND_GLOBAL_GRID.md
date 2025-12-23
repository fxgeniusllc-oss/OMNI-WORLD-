# 🎵 OmniSound Global Grid System

## Overview

The **OmniSound Global Grid** transforms OmniWorld into the first open-world metaverse where **real-world music cities are playable biomes**. Each city isn't just a backdrop—it's a living, breathing musical ecosystem with unique sounds, missions, visuals, and cultural identity.

## 🌐 System Architecture

### Core Components

1. **Music Biome System** (`MusicBiomeController.cs`, `MusicBiomeData.cs`)
2. **Airport Transit Network** (`AirportManager.cs`, `AirportData.cs`)
3. **City Reputation System** (`CityReputationSystem.cs`)
4. **Integration Layer** (Enhanced `TransitSystem.cs`, `ProceduralGeneration.cs`)

---

## 🎧 Music Biome System

### Purpose
Dynamically switches sound environments based on the player's current city, creating immersive cultural audio experiences.

### Features

#### City-Specific Sound Profiles
Each of the 9+ cities has a unique musical identity:

| City | Genre | BPM | Cultural Identity |
|------|-------|-----|-------------------|
| **OmniNYC** | Boom Bap Hip-Hop | 90 | Classic Hip-Hop, Financial Capital |
| **Berlin** | Techno | 130 | Cold Concrete, Underground Culture |
| **Lagos** | Afrobeats | 110 | Street Market Energy, Polyrhythm |
| **OmniTokyo** | J-Pop/City Pop | 128 | Cyber-Tech, Minimalist Tonal |
| **OmniLanta** | Trap | 140 | Creator Culture, 808 Legacy |
| **OmniVegas** | EDM | 128 | Neon Capital, High Stakes |
| **OmniDubai** | Arabic Pop/Fusion | 115 | Luxury, Innovation |
| **OmniLA** | West Coast Hip-Hop | 95 | Beach Culture, G-Funk |
| **OmniParis** | French House | 120 | Art, Fashion, Romance |

#### Layered Audio System

1. **Ambient Soundtrack Layer**
   - Continuous background music loop
   - Genre-specific to each city
   - Seamless transitions between cities

2. **Environmental SFX Layer**
   - NYC: Subway rumble, taxi horns, street chatter
   - Berlin: Concrete reverb, tram pass, warehouse echo
   - Lagos: Market chatter, okada bikes, generators
   - Tokyo: Train announcements, vending machines, pachinko
   - And more...

3. **Cultural Sound Layer**
   - One-shot sounds tied to city culture
   - Triggered by player actions or events
   - Examples: Talking drums (Lagos), Koto (Tokyo), Accordion (Paris)

#### Daypart System

Music dynamically shifts based on time of day:

- **Morning** (6am-12pm): Awakening, hustle, energy building
- **Afternoon** (12pm-6pm): Peak activity, grind
- **Evening** (6pm-12am): Wind down, nightlife prep
- **Night** (12am-6am): Late night, club hours, underground

Example for NYC:
```
Morning: "morning_hustle"
Afternoon: "afternoon_grind"
Evening: "evening_cool"
Night: "night_pulse"
```

#### Dynamic Transitions

- **Crossfade Duration**: Configurable per city (1.5s - 4.0s)
- **Smooth Volume Curves**: Prevents jarring audio changes
- **District Variations**: Sub-zones can have unique audio profiles

### Implementation Example

```csharp
// Load biome when entering a city
MusicBiomeController.Instance.LoadBiomeForCity("OmniTokyo");

// Get current biome info
string info = MusicBiomeController.Instance.GetBiomeInfo();

// Play cultural sound effect
MusicBiomeController.Instance.PlayCulturalSound("koto_strum");

// Enter a specific district
MusicBiomeController.Instance.EnterDistrict("Shibuya_Tech_District");
```

---

## ✈️ Airport System - OmniGate Travel Network

### Purpose
Realistic global airport terminals that function as travel hubs, mission boards, and cultural gateways between cities.

### Airport Features

#### Terminal Components

1. **Check-in Terminals**
   - Flight booking UI
   - Destination selection
   - Cost calculation display

2. **Customs NPCs**
   - First-time city introduction
   - Cultural briefings
   - Reputation checks

3. **Mission Boards**
   - City-specific quests
   - Reputation-gated content
   - Music biome missions

4. **Lounges** (Unlockable)
   - Premium waiting areas
   - Networking with other players
   - Special merchants

5. **Services**
   - Vehicle/property rentals
   - Travel insurance
   - Currency exchange
   - NFT marketplace access

#### Airport Network

| Airport | Code | City | Unlock Cost | Reputation Required | Status |
|---------|------|------|-------------|---------------------|--------|
| OmniGate Atlanta | ATL | OmniLanta | 0 $OMNI | 0 | ✅ Unlocked (Starting) |
| OmniGate Las Vegas | LAS | OmniVegas | 0 $OMNI | 0 | ✅ Unlocked |
| OmniGate JFK | JFK | OmniNYC | 250 $OMNI | 25 | 🔒 Locked |
| OmniGate LAX | LAX | OmniLA | 300 $OMNI | 30 | 🔒 Locked |
| OmniGate Narita | TYO | OmniTokyo | 500 $OMNI | 50 | 🔒 Locked |
| OmniGate Charles de Gaulle | CDG | OmniParis | 750 $OMNI | 60 | 🔒 Locked |
| OmniGate Dubai | DXB | OmniDubai | 1000 $OMNI | 75 | 🔒 Locked |

#### Flight System

**Cost Calculation:**
```
Total Flight Cost = Base Travel Cost + Landing Fee
Base Travel Cost = Base Rate + (Distance × Rate Per KM)
Landing Fee = Airport Base Fee × Multiplier
```

**Cinematic Transitions:**
- 5-second default flight duration
- Loading screen with city preview
- Arrival animation at destination
- Skippable for frequent travelers

### Implementation Example

```csharp
// Enter airport terminal
AirportManager.Instance.EnterAirport("OmniTokyo");

// Check available destinations
List<string> destinations = AirportManager.Instance.GetAvailableDestinations();

// Book a flight
bool success = AirportManager.Instance.BookFlight("OmniParis", playerWalletAddress);

// Unlock new airport
AirportManager.Instance.UnlockAirport("OmniDubai", playerWallet, playerReputation);

// Leave airport and enter city
AirportManager.Instance.LeaveAirport(); // Triggers music biome load
```

---

## 🏆 City Reputation System

### Purpose
Track cultural reputation per city, tied to local sound, style, and progression. Players build unique reputations in each city independently.

### Reputation Levels

| Level | Points | Benefits |
|-------|--------|----------|
| **Unknown** | 0 | Tourist status, limited access |
| **Novice** | 1-24 | Basic city access, standard pricing |
| **Local** | 25-49 | Local-only missions, 10% discounts |
| **Respected** | 50-74 | Mentor system unlocked, special gear |
| **Influencer** | 75-99 | Exclusive events, signature items, 50% discounts |
| **Legend** | 100+ | Maximum benefits, legendary items, city-wide recognition |

### Reputation Sources

1. **Quest Completion** (+5-20 points)
2. **Event Attendance** (+2-5 points)
3. **Property Ownership** (+5 points per property)
4. **Cultural Learning** (+1 point per 10% knowledge)
5. **Music Mastery** (+1 point per 5% mastery)
6. **Mentor Progression** (Variable)

### Progression Metrics

Each city tracks:
- **Quests Completed**: Total missions done
- **Events Attended**: Cultural events participated in
- **Properties Owned**: Real estate in the city
- **Mentor Relationship**: 0-100% progress with city mentor
- **Cultural Knowledge**: 0-100% understanding of city culture
- **Music Style Mastery**: 0-100% proficiency in city's music genre

### City-Specific Mentors

When reaching **Respected** level, players unlock a city mentor:

| City | Mentor | Specialty |
|------|--------|-----------|
| OmniNYC | DJ Premier | Boom Bap Production |
| Berlin | Richie Hawtin | Techno Mastery |
| Lagos | Fela Kuti Legacy | Afrobeats Rhythm |
| OmniTokyo | Yoko Kanno | J-Pop Innovation |
| OmniLanta | Metro Boomin | Trap Architecture |
| OmniVegas | Calvin Harris | EDM Production |
| OmniDubai | Amr Diab | Arabic Pop Fusion |
| OmniLA | Dr. Dre | West Coast Sound |
| OmniParis | Daft Punk Legacy | French House |

### Economic Benefits

Reputation provides economic multipliers:

```
Legend Status: 2.0x multiplier
Influencer Status: 1.5x multiplier
Respected Status: 1.25x multiplier
Local Status: 1.1x multiplier
```

Applied to:
- Property rental income
- Quest rewards
- Trading profits
- Event payouts

### Implementation Example

```csharp
// Get city reputation
CityReputationData rep = CityReputationSystem.Instance.GetCityReputation("OmniNYC");

// Add reputation points
CityReputationSystem.Instance.AddReputation("OmniNYC", 10, "Completed music quest");

// Track quest completion
CityReputationSystem.Instance.OnQuestCompleted("OmniNYC", "boom_bap_master", 15);

// Update music mastery
CityReputationSystem.Instance.IncreaseMusicMastery("OmniNYC", 10);

// Get economic multiplier
float multiplier = CityReputationSystem.Instance.GetReputationMultiplier("OmniNYC");

// Check content access
bool canAccess = CityReputationSystem.Instance.CanAccessContent("OmniNYC", ReputationLevel.Respected);
```

---

## 🎮 Music-Based Quest System

### Quest Types

The procedural generation system now includes **Music Biome Quests** specific to each city's culture.

### Examples by City

#### OmniNYC (Boom Bap)
- Master the 808 at The Bronx Studio
- Attend Underground Cipher in Brooklyn
- Sample Rare Vinyl at Queens Record Shop
- Freestyle Battle at Times Square

#### Berlin (Techno)
- DJ Set at Berghain
- Master Modular Synthesis Workshop
- Warehouse Techno Marathon
- Cold Concrete Echo Session

#### Lagos (Afrobeats)
- Play Talking Drums at Street Festival
- Afrobeats Dance Battle
- Market Energy Recording Session
- Master Polyrhythm Patterns

#### OmniTokyo (J-Pop)
- Koto Sampling at Shibuya Studio
- Anime OP Recording Session
- Cyber Cafe Music Production
- Future Bass at Akihabara Club

### Quest Rewards

Music biome quests offer:
- **Higher $OMNI Rewards**: 100-400 $OMNI per quest
- **Reputation Boost**: +5-20 reputation points
- **Cultural Items**: City-specific gear and collectibles
- **Music Mastery**: Progress toward genre mastery
- **Mentor Progress**: Advance relationship with city mentor

### Implementation Example

```csharp
// Generate music biome quest for current city
Quest musicQuest = ProceduralGeneration.Instance.GenerateMusicBiomeQuest("OmniNYC");

// Quest properties
Debug.Log($"Title: {musicQuest.title}");
Debug.Log($"Reward: {musicQuest.reward} $OMNI");
Debug.Log($"Description: {musicQuest.description}");
```

---

## 🔗 System Integration

### How It All Works Together

1. **Player arrives at airport**
   ```
   AirportManager → Displays terminal UI
   ```

2. **Player books flight to new city**
   ```
   AirportManager → Calculates cost
   TransitSystem → Updates current city
   MusicBiomeController → Loads new biome
   CityReputationSystem → Tracks visit
   ```

3. **Player enters city**
   ```
   MusicBiomeController → Starts ambient soundtrack
   MusicBiomeController → Plays environmental SFX
   AirportManager → Shows customs/mission board
   ```

4. **Player completes music quest**
   ```
   ProceduralGeneration → Generates quest
   CityReputationSystem → Awards reputation
   CityReputationSystem → Updates music mastery
   ```

5. **Player gains reputation level**
   ```
   CityReputationSystem → Checks thresholds
   CityReputationSystem → Unlocks mentor (if Respected)
   CityReputationSystem → Applies economic multiplier
   ```

### Data Flow Diagram

```
┌─────────────────┐
│  AirportManager │
└────────┬────────┘
         │ Flight Booking
         ▼
┌─────────────────┐
│  TransitSystem  │◄────── City Unlock Logic
└────────┬────────┘
         │ City Change
         ▼
┌─────────────────┐
│ MusicBiome      │◄────── Audio Asset Loading
│ Controller      │
└────────┬────────┘
         │ Biome Active
         ▼
┌─────────────────┐
│ CityReputation  │◄────── Quest/Event Tracking
│ System          │
└────────┬────────┘
         │ Reputation Change
         ▼
┌─────────────────┐
│ Procedural      │◄────── Quest Generation
│ Generation      │
└─────────────────┘
```

---

## 🎨 Future Enhancements

### Planned Features

1. **District Audio Variations**
   - Sub-zones with unique soundscapes
   - Residential vs. commercial vs. nightlife districts
   - Dynamic mixing based on player location

2. **Player-Created Music**
   - In-game music production tools
   - Upload custom tracks as NFTs
   - City-specific music challenges

3. **Live Events**
   - Real-time concerts in city venues
   - DJ battles and competitions
   - Music festival takeovers

4. **Cross-City Collaborations**
   - Multi-city tour quests
   - Cultural exchange missions
   - International music festivals

5. **Advanced Mentor System**
   - Skill trees per music genre
   - Masterclass sessions
   - Legendary equipment unlocks

6. **Music NFT Integration**
   - Own rare tracks as NFTs
   - Trade music collectibles
   - Revenue sharing for creators

---

## 🛠️ Technical Notes

### Audio Asset Organization

```
Resources/
  Audio/
    Ambient/
      nyc_ambient_loop.mp3
      berlin_ambient_loop.mp3
      lagos_ambient_loop.mp3
      ...
    Environmental/
      NYC/
        subway_rumble.wav
        taxi_horns.wav
        ...
      Berlin/
        concrete_reverb.wav
        tram_pass.wav
        ...
    Cultural/
      OmniTokyo/
        koto_strum.wav
        shamisen_pluck.wav
        ...
```

### Performance Considerations

- **Audio Source Pooling**: Reuse audio sources to reduce overhead
- **Streaming**: Large ambient tracks loaded asynchronously
- **LOD System**: Reduce audio fidelity for distant sounds
- **Compression**: Use appropriate audio codecs (Vorbis for music, ADPCM for SFX)

### Memory Management

- **Lazy Loading**: Load city audio only when needed
- **Unload on Exit**: Clear previous city audio when changing cities
- **Caching**: Keep frequently used clips in memory
- **Asset Bundles**: Package city audio separately for on-demand loading

---

## 📊 Configuration

### Music Biome Data

Each city's music biome is configured in `MusicBiomePresets.GetBiomeForCity()`:

```csharp
new MusicBiomeData
{
    cityName = "OmniNYC",
    biomeName = "Boom Bap Metropolitan",
    primaryGenre = "Hip-Hop",
    baseBPM = 90f,
    ambientSoundtrack = "nyc_ambient_loop",
    environmentalSounds = new List<string> { "subway_rumble", "taxi_horns" },
    culturalInstruments = new List<string> { "808_drums", "vinyl_scratches" },
    // ... more settings
}
```

### Airport Data

Airport configurations in `AirportPresets.GetAirportForCity()`:

```csharp
new AirportData
{
    airportCode = "ATL",
    airportName = "OmniGate Atlanta International",
    unlockCost = 0f,
    requiredReputation = 0,
    availableDestinations = new List<string> { "OmniVegas", "OmniNYC" },
    // ... more settings
}
```

---

## 🎯 Design Philosophy

The OmniSound Global Grid embodies three core principles:

1. **Cultural Authenticity**: Each city's music biome reflects real-world musical heritage
2. **Progressive Discovery**: Cities unlock gradually, encouraging exploration
3. **Economic Integration**: Reputation and music mastery tie directly to earning potential

This creates a system where **music isn't just background—it's gameplay**.

---

## 📞 API Reference

### MusicBiomeController

```csharp
// Load biome for city
void LoadBiomeForCity(string cityName)

// Get current biome info
string GetBiomeInfo()

// Play cultural sound
void PlayCulturalSound(string soundName)

// Set master volume
void SetMasterVolume(float volume)

// Enter district variation
void EnterDistrict(string districtName)
```

### AirportManager

```csharp
// Enter airport terminal
void EnterAirport(string cityName)

// Leave airport
void LeaveAirport()

// Book flight
bool BookFlight(string destinationCity, string walletAddress)

// Unlock airport
bool UnlockAirport(string cityName, string walletAddress, int playerReputation)

// Get unlocked airports
List<AirportData> GetUnlockedAirports()

// Get airport info
string GetAirportInfo()
```

### CityReputationSystem

```csharp
// Get city reputation
CityReputationData GetCityReputation(string cityName)

// Add reputation
void AddReputation(string cityName, int points, string reason)

// Track quest completion
void OnQuestCompleted(string cityName, string questId, int reputationReward)

// Update music mastery
void IncreaseMusicMastery(string cityName, int mastery)

// Get economic multiplier
float GetReputationMultiplier(string cityName)

// Get reputation summary
string GetReputationSummary(string cityName)
```

### ProceduralGeneration

```csharp
// Generate music biome quest
Quest GenerateMusicBiomeQuest(string cityName)

// Generate city quest
Quest GenerateCityQuest(string cityName, NPCRole npcRole)

// Generate city event
CityEvent GenerateCityEvent(string cityName)
```

---

## 🎊 Conclusion

The OmniSound Global Grid transforms OmniWorld from a generic metaverse into a **living, breathing musical universe** where every city has its own soul, sound, and story.

Players don't just visit cities—they **live their music**, **build their reputation**, and **master their craft** in each unique biome.

This is the future of music-driven gaming.

**Welcome to the OmniSound Global Grid.**

---

*Documentation Version: 1.0*  
*Last Updated: December 23, 2025*  
*System Status: ✅ Core Implementation Complete*
