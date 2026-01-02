# Asset Definition System - Quick Reference

## Overview

The Asset Definition System provides a centralized, JSON-based approach to managing all prefabs and assets in OmniWorld.

## Quick Links

- 📖 **[Complete Schema Documentation](ASSET_DEFINITION_SCHEMA.md)** - Full specification
- 📝 **[Adding New Assets Guide](ADDING_NEW_ASSETS.md)** - Step-by-step tutorial
- 📊 **[Master Asset Registry](../Assets/AssetRegistry.json)** - Central index
- 💻 **[AssetDefinitionManager.cs](../Assets/Scripts/Core/AssetDefinitionManager.cs)** - C# implementation

## Quick Start

### 1. Using the Asset Manager

```csharp
using OmniWorld.Core;

// Get the singleton instance
var assetManager = AssetDefinitionManager.Instance;

// Load all assets in a category
var housingAssets = assetManager.GetAssetsByCategory("housing");

// Filter by economic tier
var luxuryAssets = assetManager.GetAssetsByEconomicTier("Luxury");

// Filter by price range
var affordableAssets = assetManager.GetAssetsByPriceRange(10000, 100000);

// Get NFT-compatible assets only
var nftAssets = assetManager.GetNFTAssets();

// Load a specific asset
var asset = assetManager.LoadAssetDefinition("Assets/Prefabs/Housing/Apartments/StudioApartment.json");

// Get registry statistics
var stats = assetManager.GetStatistics();
Debug.Log($"Total assets: {stats.totalAssets}");
```

### 2. Running Examples

See `Assets/Scripts/Examples/AssetSystemExample.cs` for working examples:

1. Add the script to a GameObject in your scene
2. Press Play in Unity Editor
3. Check the Console for output

### 3. Adding a New Asset

1. Create a JSON file following the schema (see examples in `Assets/Prefabs/`)
2. Place it in the appropriate category folder
3. Update `Assets/AssetRegistry.json` to include it
4. Test loading with `AssetDefinitionManager`

## Asset Categories

| Category | Count | Path | Description |
|----------|-------|------|-------------|
| Housing | 35 | `Assets/Prefabs/Housing/` | Apartments, condos, mansions, penthouses |
| Vehicles | 41 | `Assets/Prefabs/Vehicles/` | Cars, bikes, aircraft |
| Avatars | 25 | `Assets/Prefabs/Avatars/` | Base models, customization, animations |
| Gyms | 12 | `Assets/Prefabs/Gyms/` | Fighting facilities and equipment |
| Buildings | 2 | `Assets/Prefabs/Buildings/` | Commercial structures |

**Total: 100+ assets**

## Economic Tiers

| Tier | Price Range | Description |
|------|-------------|-------------|
| Entry | $9.5K - $85K | Affordable for new players |
| Standard | $85K - $250K | Mid-tier for established players |
| Premium | $250K - $1M | High-quality assets |
| Luxury | $1M - $5M | Luxury assets for wealthy players |
| Ultra-Luxury | $5M - $50M | Ultra-rare, exclusive assets |

## File Structure

```
Assets/
├── AssetRegistry.json                 # Master registry (index of all assets)
└── Prefabs/
    ├── Housing/
    │   ├── Apartments/
    │   │   ├── StudioApartment.json
    │   │   └── ...
    │   ├── Condos/
    │   ├── Mansions/
    │   └── Penthouses/
    ├── Vehicles/
    │   ├── Cars/
    │   ├── Bikes/
    │   └── Aircraft/
    ├── Avatars/
    │   ├── Base/
    │   ├── Customization/
    │   └── Animations/
    ├── Gyms/
    └── Buildings/

Docs/
├── ASSET_DEFINITION_SCHEMA.md         # Complete schema
├── ADDING_NEW_ASSETS.md              # Tutorial
└── ASSET_SYSTEM_QUICKREF.md          # This file

Scripts/
└── Core/
    └── AssetDefinitionManager.cs      # Runtime manager
```

## JSON Schema Structure

Every asset definition follows this structure:

```json
{
  "prefabName": "Category_Subcategory_Type",
  "category": "Housing|Vehicles|Avatars|Gyms|Buildings",
  "subCategory": "Specific subcategory",
  "type": "Specific type",
  "metadata": {
    "nftCompatible": true,
    "ownershipType": "Individual",
    "economicTier": "Entry|Standard|Premium|Luxury|Ultra-Luxury",
    "dominionZone": "Zone name (optional)"
  },
  "specifications": { /* Category-specific */ },
  "graphics": {
    "lodLevels": 3,
    "polyCount": { "lod0": 25000, "lod1": 12000, "lod2": 5000 },
    "textureResolution": { "diffuse": 2048, "normal": 2048, "roughness": 1024, "metallic": 1024 },
    "materials": ["Material names"],
    "renderPipeline": "URP"
  },
  "components": ["UnityComponent1", "UnityComponent2"],
  "price": {
    "purchasePrice": 50000,
    "monthlyRent": 1200,
    "currency": "OMNI"
  }
}
```

## Key Features

✅ **Centralized Management** - Single source of truth  
✅ **NFT Integration** - Built-in NFT metadata  
✅ **Economic Balance** - Dominion Economy integration  
✅ **Dynamic Loading** - Runtime loading with caching  
✅ **Filtering** - Query by multiple criteria  
✅ **Validation** - Schema ensures consistency  
✅ **Performance** - Cached loading up to 50MB  

## Validation

Validate JSON files before committing:

```bash
# Validate a single file
python3 -m json.tool Assets/Prefabs/Housing/Apartments/StudioApartment.json

# Validate the registry
python3 -m json.tool Assets/AssetRegistry.json
```

## Integration

### With Dominion Economy

Assets automatically integrate with the Dominion Economy:
- Prices denominated in OMNI tokens
- Economic tier classification
- Dynamic value calculation based on demand

### With NFT System

All NFT-compatible assets include:
- `nftCompatible: true` in metadata
- Blockchain standards (ERC-721, ERC-1155)
- Royalty information
- Creator share percentages

### With Unity

- C# classes for deserialization
- Singleton manager pattern
- Caching for performance
- Editor-friendly Inspector integration

## Performance

- **Caching:** Enabled by default (configurable)
- **Cache Size:** Max 50MB (configurable)
- **Loading:** Lazy loading on-demand
- **Memory:** Efficient dictionary-based lookups

## Troubleshooting

### Asset not loading?
1. Check file path is correct
2. Validate JSON syntax
3. Ensure asset is in AssetRegistry.json
4. Check Unity Console for errors

### Registry not found?
- Ensure `AssetRegistry.json` exists at `Assets/AssetRegistry.json`
- Check `registryPath` in AssetDefinitionManager Inspector

### Cache issues?
- Call `assetManager.ClearCache()` to clear
- Call `assetManager.ReloadRegistry()` to refresh

## Examples

See working examples in:
- `Assets/Scripts/Examples/AssetSystemExample.cs`
- `Assets/Scripts/Examples/AutoDealershipDemo.cs`
- `Assets/Scripts/Examples/CityGenerator.cs`

## Support

For questions or issues:
1. Review the complete [schema documentation](ASSET_DEFINITION_SCHEMA.md)
2. Check the [step-by-step guide](ADDING_NEW_ASSETS.md)
3. Examine existing asset definitions
4. Test with AssetSystemExample.cs

## Version

**Schema Version:** 1.0.0  
**Last Updated:** January 2, 2025  
**Total Assets:** 100+  
**Compatible With:** Unity 2022.3 LTS+

---

**Built for OmniWorld** - The AI-Powered, Creator-First Metaverse
