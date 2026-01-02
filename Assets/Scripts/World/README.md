# OmniWorld - World Scripts

This directory contains all world-related systems for OmniWorld, including the OmniSound Global Grid implementation.

## 📁 Directory Structure

```
World/
├── AirportData.cs              (14KB) - Airport terminal data structures
├── AirportManager.cs           (15KB) - OmniGate Travel Network controller
├── CityReputationSystem.cs     (16KB) - Per-city progression tracking
├── MusicBiomeController.cs     (15KB) - Dynamic sound environment controller
├── MusicBiomeData.cs           (16KB) - City sound profile configurations
├── TransitSystem.cs            (9KB)  - City travel and transit system
└── ZoneController.cs           (8KB)  - District and zone management
```

## 🎵 OmniSound Global Grid

The **OmniSound Global Grid** is a comprehensive system that transforms OmniWorld cities into playable music biomes with unique sounds, missions, and cultural identities.

### Core Components

#### Music Biome System
- **MusicBiomeController.cs** - Manages dynamic music environments per city
- **MusicBiomeData.cs** - Contains presets for 9+ cities with genre-specific configurations

#### Airport System
- **AirportManager.cs** - Controls the OmniGate Travel Network
- **AirportData.cs** - Defines 7 airport terminals with unlock requirements

#### Reputation System
- **CityReputationSystem.cs** - Tracks player reputation across cities with 5 levels

#### Supporting Systems
- **TransitSystem.cs** - Handles city travel and integrates with music biomes
- **ZoneController.cs** - Manages districts and triggers audio variations

## 🚀 Quick Start

### Load a Music Biome

```csharp
MusicBiomeController.Instance.LoadBiomeForCity("OmniNYC");
```

### Book a Flight

```csharp
AirportManager.Instance.BookFlight("OmniTokyo", playerWalletAddress);
```

### Check Reputation

```csharp
CityReputationData rep = CityReputationSystem.Instance.GetCityReputation("OmniNYC");
Debug.Log($"Level: {rep.level}, Points: {rep.reputationPoints}");
```

## 📚 Documentation

Full documentation is available in `Docs/`:
- `OMNISOUND_GLOBAL_GRID.md` - Complete system architecture
- `OMNISOUND_QUICKSTART.md` - Developer guide with examples
- `IMPLEMENTATION_SUMMARY.md` - High-level overview

## 🎯 Features

### 9 Music Biomes
- OmniNYC (Boom Bap Hip-Hop, 90 BPM)
- OmniLanta (Trap, 140 BPM)
- OmniTokyo (J-Pop, 128 BPM)
- OmniVegas (EDM, 128 BPM)
- OmniLA (West Coast Hip-Hop, 95 BPM)
- OmniParis (French House, 120 BPM)
- OmniDubai (Arabic Pop, 115 BPM)
- Berlin (Techno, 130 BPM)
- Lagos (Afrobeats, 110 BPM)

### 7 Airport Terminals
- ATL (OmniLanta) - Unlocked ✅
- LAS (OmniVegas) - Unlocked ✅
- JFK (OmniNYC) - 250 $OMNI, 25 reputation
- LAX (OmniLA) - 300 $OMNI, 30 reputation
- TYO (OmniTokyo) - 500 $OMNI, 50 reputation
- CDG (OmniParis) - 750 $OMNI, 60 reputation
- DXB (OmniDubai) - 1000 $OMNI, 75 reputation

### 5 Reputation Levels per City
- Unknown (0) - Tourist
- Novice (1-24) - Basic access
- Local (25-49) - Local missions, discounts
- Respected (50-74) - Mentor unlock
- Influencer (75-99) - Exclusive content
- Legend (100+) - Max benefits

## 🔗 Integration

All systems are fully integrated:
- Airport travel triggers music biome loading
- Reputation gates airport unlocks
- Quests award reputation and music mastery
- Districts trigger audio variations

## 📊 Configuration

Configuration files are in `Assets/Config/`:
- `MusicBiomes/*.json` - City sound profiles
- `Airports/*.json` - Airport terminal configurations

## 🛠️ Development

When adding new cities:
1. Add preset to `MusicBiomePresets.GetBiomeForCity()`
2. Add airport config to `AirportPresets.GetAirportForCity()`
3. Add music quests to `ProceduralGeneration`
4. Create JSON configs
5. Test integration

## ✨ Status

**Implementation Status: 100% Complete**

All core systems are production-ready with comprehensive documentation.

---

*Part of the OmniSound Global Grid - The first open-world metaverse where real-world music cities are playable biomes.*
