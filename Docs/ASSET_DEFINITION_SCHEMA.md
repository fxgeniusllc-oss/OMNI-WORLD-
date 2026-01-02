# OmniWorld Asset Definition Schema

## Overview

This document defines the standardized JSON schema for all assets and prefabs in OmniWorld. Every asset must conform to this schema to ensure consistency, proper integration with the Dominion Economy, and NFT compatibility.

## Version

**Schema Version:** 1.0.0  
**Last Updated:** January 2, 2025

---

## Table of Contents

1. [Master Registry](#master-registry)
2. [Asset Definition Structure](#asset-definition-structure)
3. [Category-Specific Schemas](#category-specific-schemas)
4. [Common Properties](#common-properties)
5. [Graphics Specifications](#graphics-specifications)
6. [NFT Integration](#nft-integration)
7. [Validation Rules](#validation-rules)

---

## Master Registry

The master asset registry is located at: `/Assets/AssetRegistry.json`

### Registry Structure

```json
{
  "version": "1.0.0",
  "lastUpdated": "YYYY-MM-DD",
  "description": "Master registry description",
  "categories": { ... },
  "statistics": { ... },
  "economicTiers": [ ... ],
  "nftCategories": { ... },
  "integrations": { ... }
}
```

### Purpose

- **Central Index:** Single source of truth for all game assets
- **Asset Discovery:** Enables programmatic asset loading
- **Economic Planning:** Tracks pricing tiers and distribution
- **NFT Management:** Defines rarity and royalty structures

---

## Asset Definition Structure

All individual asset definition files follow this base structure:

```json
{
  "prefabName": "string",           // Unique identifier (Category_Subcategory_Type)
  "category": "string",              // Housing | Vehicles | Avatars | Gyms | Buildings
  "subCategory": "string",           // Specific subcategory
  "type": "string",                  // Specific type
  "metadata": { ... },               // Asset metadata
  "specifications": { ... },         // Category-specific specs
  "graphics": { ... },               // Graphics and rendering
  "components": [ ... ],             // Unity components
  "price": { ... }                   // Economic data
}
```

---

## Common Properties

### 1. Metadata Object

Required for all assets:

```json
"metadata": {
  "nftCompatible": true,             // Boolean: Can be minted as NFT
  "ownershipType": "string",         // Individual | Shared | Corporate
  "economicTier": "string",          // Entry | Standard | Premium | Luxury | Ultra-Luxury
  "dominionZone": "string"           // Zone where asset is available (optional)
}
```

### 2. Graphics Object

Standard graphics configuration:

```json
"graphics": {
  "lodLevels": 3,                    // Number of LOD levels (2-4)
  "polyCount": {
    "lod0": 25000,                   // Highest detail
    "lod1": 12000,                   // Medium detail
    "lod2": 5000                     // Lowest detail
  },
  "textureResolution": {
    "diffuse": 2048,                 // Base color map resolution
    "normal": 2048,                  // Normal map resolution
    "roughness": 1024,               // Roughness map resolution
    "metallic": 1024                 // Metallic map resolution
  },
  "materials": [                     // Array of material names
    "Material_Name_PBR"
  ],
  "lighting": {                      // Lighting configuration
    "realtimeLights": 2,
    "lightProbes": true,
    "reflectionProbes": true,
    "ambientOcclusion": true
  },
  "renderPipeline": "URP",           // Unity Render Pipeline
  "realisticFeatures": [             // Array of visual features
    "PBR Materials",
    "Real-time Shadows",
    "Global Illumination"
  ]
}
```

### 3. Price Object

Economic information:

```json
"price": {
  "purchasePrice": 50000,            // Full purchase price
  "monthlyRent": 1200,               // Rental cost (for housing)
  "dailyRent": 450,                  // Daily rental (for vehicles)
  "currency": "OMNI"                 // Currency type
}
```

### 4. Components Array

Unity components to attach:

```json
"components": [
  "Rigidbody",                       // Physics body
  "BoxCollider",                     // Collision detection
  "PropertyOwnership",               // Ownership tracking
  "NFTMetadata",                     // NFT data
  "InteriorController"               // Custom controllers
]
```

---

## Category-Specific Schemas

### Housing Assets

Required specifications for housing:

```json
"specifications": {
  "squareFootage": 450,              // Total area in sq ft
  "bedrooms": 0,                     // Number of bedrooms
  "bathrooms": 1,                    // Number of bathrooms
  "floors": 1,                       // Number of floors
  "parkingSpaces": 0                 // Parking availability
},
"layout": {
  "dimensions": {
    "width": 20,                     // Width in meters
    "length": 22.5,                  // Length in meters
    "height": 10                     // Height in meters
  },
  "rooms": [                         // Array of room definitions
    {
      "name": "Main Living Area",
      "type": "Combination",
      "dimensions": { "width": 15, "length": 18, "height": 10 },
      "position": { "x": 0, "y": 0, "z": 0 },
      "features": ["Murphy Bed", "Kitchenette"]
    }
  ],
  "entryPoints": [                   // Array of doors/entries
    {
      "name": "Main Door",
      "position": { "x": 0, "y": 0, "z": 11 },
      "type": "Standard",
      "interactable": true
    }
  ],
  "windows": [                       // Array of windows
    {
      "position": { "x": 20, "y": 5, "z": 5 },
      "size": "Large",
      "type": "Fixed"
    }
  ]
}
```

### Vehicle Assets

Required specifications for vehicles:

```json
"specifications": {
  "class": "Sports",                 // Vehicle class
  "seats": 2,                        // Number of seats
  "engine": "3.0L Twin-Turbo V6",   // Engine description
  "horsepower": 400,                 // Power output
  "topSpeed": 180,                   // Top speed in mph
  "acceleration": 4.2,               // 0-60 mph time
  "handling": 9.5,                   // Handling rating (1-10)
  "fuelType": "Premium Gasoline"     // Fuel type
},
"physics": {
  "mass": 1450,                      // Mass in kg
  "centerOfMass": { "x": 0, "y": 0.35, "z": -0.2 },
  "wheelBase": 2.55,                 // Wheelbase in meters
  "trackWidth": 1.6,                 // Track width in meters
  "suspensionTravel": 0.1,           // Suspension travel
  "springRate": 55000,               // Spring stiffness
  "damperRate": 4500,                // Damper rate
  "downforce": 200                   // Aerodynamic downforce
},
"model": {
  "dimensions": {
    "length": 4.5,                   // Length in meters
    "width": 1.85,                   // Width in meters
    "height": 1.25,                  // Height in meters
    "wheelbase": 2.55                // Wheelbase in meters
  },
  "doors": 2,                        // Number of doors
  "trunk": "Compact",                // Trunk size
  "bodyStyle": "Coupe"               // Body style
},
"audio": {
  "engineSound": "V6_Twin_Turbo_Sport",
  "exhaustNote": "Aggressive",
  "turboWhistle": true,
  "hornSound": "Sport",
  "doorSound": "Premium",
  "brakeSound": "Performance"
},
"effects": {
  "exhaust": {
    "particleSystem": true,
    "dualExhaust": true,
    "position": [
      { "x": -0.5, "y": 0.2, "z": -2.3 }
    ]
  },
  "tireSmokeOnBurnout": true,
  "brakelightGlow": true,
  "headlightBeams": true
},
"customization": {
  "paintColors": ["Racing Red", "Phantom Black"],
  "wheels": ["Sport 18in", "Racing 19in"],
  "interior": ["Alcantara", "Full Leather"],
  "upgrades": ["Stage 1 Tune", "Exhaust System"]
}
```

### Avatar Assets

#### Base Avatar

```json
"specifications": {
  "ageGroup": "Adult",               // Child | Adult | Elderly
  "gender": "Male",                  // Male | Female | Neutral
  "boneCount": 78,                   // Skeleton bones
  "blendShapes": 52,                 // Facial blend shapes
  "height": 180,                     // Height in cm
  "weight": 75                       // Weight in kg
},
"rigging": {
  "armatureType": "Humanoid",
  "ikSupport": true,
  "fingerBones": true,
  "facialRig": true,
  "twistBones": true
}
```

#### Customization Options

```json
"options": {
  "category": "HairStyles",          // Customization category
  "items": [
    {
      "id": "long_wavy",
      "name": "Long Wavy",
      "description": "Flowing wavy hair",
      "rarity": "Common",
      "price": 15,
      "attachmentPoint": "Head",
      "physicsBased": true
    }
  ]
}
```

#### Animation Sets

```json
"animations": {
  "category": "Locomotion",
  "clips": [
    {
      "name": "Walk_Forward",
      "duration": 1.2,
      "looping": true,
      "blendable": true,
      "rootMotion": true
    }
  ]
}
```

### Gym & Equipment Assets

```json
"specifications": {
  "gymType": "Boxing",               // Boxing | MMA | StreetFight
  "capacity": 50,                    // Player capacity
  "equipmentCount": 12,              // Included equipment
  "area": 5000                       // Area in sq ft
},
"features": {
  "lighting": "Overhead Industrial",
  "flooring": "Canvas Covered",
  "spectatorArea": true,
  "changingRooms": true,
  "medicalStation": true
},
"equipment": [                       // Array of equipment
  "Boxing_Ring_Professional",
  "Heavy_Bags_Classic"
]
```

### Building Assets

```json
"specifications": {
  "buildingType": "Dealership",      // Building type
  "businessType": "Auto Sales",      // Business type
  "floors": 2,                       // Number of floors
  "area": 15000,                     // Total area in sq ft
  "capacity": 100                    // Customer capacity
},
"features": {
  "showroomFloor": true,
  "officeSpace": true,
  "serviceArea": true,
  "parkingLot": true,
  "signage": "LED Display"
},
"inventory": {
  "vehicleSlots": 20,                // Vehicle display capacity
  "storageSlots": 10                 // Storage capacity
}
```

---

## Graphics Specifications

### LOD (Level of Detail) Guidelines

| LOD Level | Distance | Poly Count | Usage |
|-----------|----------|------------|-------|
| LOD0 | 0-15m | High (50K-100K) | Close inspection |
| LOD1 | 15-50m | Medium (15K-50K) | Normal viewing |
| LOD2 | 50-100m | Low (5K-15K) | Mid distance |
| LOD3 | 100m+ | Minimal (1K-5K) | Far distance |

### Texture Resolution Standards

| Asset Type | Diffuse | Normal | Roughness | Metallic |
|------------|---------|--------|-----------|----------|
| Housing | 2048-4096 | 2048-4096 | 1024-2048 | 1024-2048 |
| Vehicles | 4096 | 4096 | 2048 | 2048 |
| Avatars | 2048-4096 | 2048-4096 | 1024-2048 | 1024-2048 |
| Props | 1024-2048 | 1024-2048 | 512-1024 | 512-1024 |

### PBR Materials

All assets must use Physically Based Rendering (PBR) materials with:

- **Albedo/Diffuse Map:** Base color information
- **Normal Map:** Surface detail without geometry
- **Roughness Map:** Surface smoothness/roughness
- **Metallic Map:** Metallic vs non-metallic surfaces
- **Ambient Occlusion:** Contact shadows (optional)
- **Emission Map:** Self-illuminating surfaces (optional)

### Rendering Features

Required for all assets:
- Unity URP (Universal Render Pipeline)
- Real-time shadows
- Global illumination support
- Screen space reflections
- Contact shadows (for high-quality assets)

---

## NFT Integration

### NFT Metadata Fields

All NFT-compatible assets must include:

```json
"nftMetadata": {
  "tokenStandard": "ERC-721",        // ERC-721 | ERC-1155
  "rarity": "Common",                // Common | Rare | Epic | Legendary | Ultra-Legendary
  "edition": "Limited",              // Standard | Limited | OneOfOne
  "editionSize": 10,                 // Total editions (if limited)
  "royalty": 0.20,                   // 20% royalty rate
  "creatorShare": 0.85,              // 85% to creator
  "attributes": [                    // On-chain attributes
    {
      "trait_type": "Category",
      "value": "Housing"
    },
    {
      "trait_type": "Economic Tier",
      "value": "Premium"
    }
  ],
  "blockchain": "Polygon",
  "ipfsCompatible": true
}
```

### Rarity Tiers

| Tier | Description | Supply | Royalty |
|------|-------------|--------|---------|
| Common | Standard assets | Unlimited | 20% |
| Rare | Limited availability | 1,000-10,000 | 20% |
| Epic | Very limited | 100-1,000 | 25% |
| Legendary | Extremely rare | 10-100 | 25% |
| Ultra-Legendary | 1/1 or very few | 1-10 | 30% |

---

## Validation Rules

### Required Fields

All assets **must** include:
- ✅ `prefabName` (unique identifier)
- ✅ `category` (valid category name)
- ✅ `subCategory` (valid subcategory)
- ✅ `metadata.nftCompatible` (boolean)
- ✅ `metadata.economicTier` (valid tier)
- ✅ `graphics` object with LOD and materials
- ✅ `components` array (minimum 3 components)
- ✅ `price` object with valid currency

### Naming Conventions

**Prefab Names:** `Category_Subcategory_Type`
- Examples: `Housing_Apartment_Studio`, `Vehicle_Car_SportsCar`

**File Names:** Match prefab name with `.json` extension
- Example: `Housing_Apartment_Studio` → `Housing_Apartment_Studio.json`

### Economic Validation

- Prices must align with defined economic tiers
- Housing assets must include both purchase and rental prices
- Vehicle assets should include purchase price and daily/monthly rental
- All prices in OMNI token denomination

### Graphics Validation

- LOD levels must be 2-4
- Each LOD must have decreasing polygon count
- Texture resolutions must be power of 2 (512, 1024, 2048, 4096)
- All materials must be PBR-compliant
- Render pipeline must be "URP"

### Component Validation

Required components by category:

**Housing:**
- `BoxCollider` or `MeshCollider`
- `PropertyOwnership`
- `NFTMetadata`
- `InteriorController`

**Vehicles:**
- `Rigidbody`
- `WheelCollider` (for ground vehicles)
- `VehicleController`
- `VehicleOwnership`
- `NFTMetadata`

**Avatars:**
- `Animator`
- `CharacterController`
- `AvatarCustomization`
- `IKController` (if IK supported)

---

## Integration with Unity

### Loading Asset Definitions

```csharp
// Load a single asset definition
string jsonPath = "Assets/Prefabs/Housing/Apartments/StudioApartment.json";
string jsonData = File.ReadAllText(jsonPath);
AssetDefinition asset = JsonUtility.FromJson<AssetDefinition>(jsonData);

// Load the master registry
string registryPath = "Assets/AssetRegistry.json";
string registryData = File.ReadAllText(registryPath);
AssetRegistry registry = JsonUtility.FromJson<AssetRegistry>(registryData);
```

### Creating Prefabs from Definitions

See `AssetDefinitionManager.cs` for implementation details.

### Dynamic Asset Loading

Assets can be loaded at runtime based on:
- Economic tier filtering
- Category/subcategory browsing
- Price range queries
- NFT rarity filters
- Zone availability

---

## Schema Versioning

**Current Version:** 1.0.0

### Version History

- **1.0.0** (2025-01-02): Initial schema definition

### Future Enhancements

Planned for future versions:
- Weather-responsive material properties
- Seasonal variation definitions
- Damage/wear system integration
- Procedural detail parameters
- AI interaction metadata
- Cross-platform compatibility flags

---

## Best Practices

1. **Consistency:** Always follow the schema exactly
2. **Validation:** Validate JSON before committing
3. **Documentation:** Include clear descriptions
4. **Testing:** Test assets in-game before finalizing
5. **Optimization:** Optimize polygon counts and textures
6. **Naming:** Use clear, descriptive names
7. **Economics:** Ensure prices align with game balance
8. **NFTs:** Properly configure royalty and ownership data

---

## Tools & Resources

### JSON Validators
- [JSONLint](https://jsonlint.com/) - Validate JSON syntax
- [JSON Schema Validator](https://www.jsonschemavalidator.net/) - Schema validation

### Unity Integration
- AssetDefinitionManager.cs - Asset loading and management
- PrefabGenerator.cs - Generate Unity prefabs from definitions
- NFTMetadataBuilder.cs - Build NFT metadata

### Documentation
- [AssetRegistry.json](../Assets/AssetRegistry.json) - Master registry
- [ADDING_NEW_ASSETS.md](ADDING_NEW_ASSETS.md) - Guide for adding assets
- [PREFAB_INTEGRATION.md](PREFAB_INTEGRATION.md) - Unity integration guide

---

## Support

For questions or issues with asset definitions:
- Review existing asset files for examples
- Check the ADDING_NEW_ASSETS.md guide
- Contact the development team

**Last Updated:** January 2, 2025  
**Schema Version:** 1.0.0
