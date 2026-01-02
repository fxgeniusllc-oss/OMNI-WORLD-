# OmniWorld Prefabs

This directory contains reusable prefabs for the OmniWorld game.

## 📋 Asset Definition System

OmniWorld uses a comprehensive JSON-based asset definition system. For complete documentation:

- **[Asset Definition Schema](../../Docs/ASSET_DEFINITION_SCHEMA.md)** - Complete schema documentation
- **[Adding New Assets Guide](../../Docs/ADDING_NEW_ASSETS.md)** - Step-by-step guide for creating new assets
- **[Master Asset Registry](../AssetRegistry.json)** - Central registry of all assets

### Key Features
- ✅ Centralized asset management via JSON definitions
- ✅ NFT compatibility built-in
- ✅ Dominion Economy integration
- ✅ Automatic loading and caching via `AssetDefinitionManager.cs`
- ✅ Economic tier classification
- ✅ Graphics optimization with LOD specifications

## Structure

```
Prefabs/
├── Housing/
│   ├── Apartments/
│   ├── Condos/
│   ├── Mansions/
│   └── Penthouses/
├── Vehicles/
│   ├── Cars/
│   ├── Bikes/
│   └── Aircraft/
└── Avatars/
    ├── Base/
    ├── Customization/
    └── Animations/
```

## Housing Prefabs

### Residential Properties
- Studio Apartment
- 1-Bedroom Apartment
- 2-Bedroom Condo
- Luxury Penthouse
- Suburban House
- Mansion

Each housing prefab should include:
- Colliders for physics
- Interior/exterior models
- Door interaction points
- Ownership data component
- NFT metadata component

## Vehicle Prefabs

### Ground Vehicles
- Compact Car
- Sports Car
- Luxury Sedan
- SUV
- Motorcycle

### Air Vehicles
- Helicopter
- Private Jet

Each vehicle prefab should include:
- Rigidbody for physics
- Vehicle controller script
- Audio sources
- Particle effects (exhaust, etc.)

## Avatar Prefabs

### Base Character
- Humanoid rig (compatible with Unity's Mecanim)
- Animator controller
- Character controller
- IK targets for animations

### Customization
- Hair styles
- Clothing items
- Accessories
- Facial features

## Creating New Prefabs

**Important:** All new prefabs must have a corresponding JSON definition file. Follow the complete guide: [Adding New Assets](../../Docs/ADDING_NEW_ASSETS.md)

### Quick Steps:
1. Create JSON definition file following the [schema](../../Docs/ASSET_DEFINITION_SCHEMA.md)
2. Create the 3D model or import from Asset Store
3. Add required components (listed in JSON definition)
4. Configure settings according to JSON specs
5. Save as prefab in appropriate folder
6. Update [AssetRegistry.json](../AssetRegistry.json)
7. Tag appropriately
8. Test in scene using `AssetDefinitionManager`

### Using AssetDefinitionManager

```csharp
// Load asset definitions programmatically
var assetManager = AssetDefinitionManager.Instance;

// Get all housing assets
var housingAssets = assetManager.GetAssetsByCategory("housing");

// Filter by economic tier
var luxuryAssets = assetManager.GetAssetsByEconomicTier("Luxury");

// Filter by price range
var affordableAssets = assetManager.GetAssetsByPriceRange(10000, 100000);

// Get NFT-compatible assets only
var nftAssets = assetManager.GetNFTAssets();
```

## Naming Convention

Use the format: `Category_Name_Variant`

Examples:
- `Housing_Apartment_Studio`
- `Vehicle_Car_Sports`
- `Avatar_Base_Male`

## Optimization

- Use LOD groups for complex models
- Optimize polygon count
- Use texture atlases where possible
- Implement object pooling for frequently spawned prefabs

## Notes

- All prefabs must have a unique identifier
- Include metadata for blockchain integration
- Ensure collision layers are correct
- Test prefabs in all target platforms
