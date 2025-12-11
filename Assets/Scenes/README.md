# Unity Scene Structure

Each city in OmniWorld has its own Unity scene with the following structure:

## Scene Hierarchy

```
CityName (e.g., OmniLanta)
├── Managers
│   ├── GameManager (Prefab)
│   ├── NetworkManager (Prefab)
│   ├── DominionEconomy (Prefab)
│   └── ProceduralGeneration (Prefab)
├── Environment
│   ├── Lighting
│   │   ├── Directional Light
│   │   ├── Reflection Probe
│   │   └── Light Probe Group
│   ├── Skybox
│   └── Post Processing Volume
├── Zones
│   ├── ResidentialZone
│   ├── BusinessZone
│   ├── CommercialZone
│   ├── RecreationZone
│   └── IndustrialZone
├── NPCs
│   └── (Spawned at runtime)
├── Player
│   └── PlayerSpawnPoint
└── UI
    ├── Canvas
    ├── HUD
    └── Menus
```

## Scene Files

Create these scene files in `Assets/Scenes/`:

1. **MainMenu.unity** - Entry point, wallet connection, city selection
2. **OmniLanta.unity** - Atlanta-themed city (Creator Culture, Tech Hub)
3. **OmniVegas.unity** - Las Vegas-themed city (High Stakes, Neon)
4. **OmniTokyo.unity** - Tokyo-themed city (Cyber-Tech, Anime)
5. **OmniNYC.unity** - New York-themed city (Financial Capital)
6. **OmniDubai.unity** - Dubai-themed city (Luxury, Innovation)
7. **OmniLA.unity** - Los Angeles-themed city (Entertainment)
8. **OmniParis.unity** - Paris-themed city (Art, Fashion, Culture)

## Required Components

Each city scene must have:

1. **Managers** - GameManager, NetworkManager, DominionEconomy
2. **Lighting** - Baked lighting for performance
3. **Navigation** - NavMesh for NPC pathfinding
4. **Spawn Points** - Player entry points
5. **Zone Controllers** - Zone-specific managers

## Build Settings

Add all scenes to Build Settings in this order:
1. MainMenu
2. OmniLanta
3. OmniVegas
4. (Additional cities as they're developed)

## Notes

- Each city scene should be approximately 2km x 2km
- Use occlusion culling for performance
- Implement LOD groups for distant objects
- Use Universal Render Pipeline (URP)
- Target 60 FPS on mid-range hardware
