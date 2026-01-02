# Adding New Assets to OmniWorld

## Complete Guide for Adding Prefabs and Assets

This guide walks you through the complete process of adding new assets and prefabs to the OmniWorld game, from initial concept to final integration.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Asset Planning](#asset-planning)
3. [Creating the Definition File](#creating-the-definition-file)
4. [Step-by-Step Process](#step-by-step-process)
5. [Category-Specific Guidelines](#category-specific-guidelines)
6. [Testing Your Asset](#testing-your-asset)
7. [Updating the Registry](#updating-the-registry)
8. [Common Issues](#common-issues)
9. [Examples](#examples)

---

## Prerequisites

Before adding a new asset, ensure you have:

- ✅ Unity 2022.3 LTS or later installed
- ✅ Access to the OmniWorld project repository
- ✅ Understanding of the [Asset Definition Schema](ASSET_DEFINITION_SCHEMA.md)
- ✅ JSON editing tools (VS Code recommended)
- ✅ 3D modeling software (if creating custom models)
- ✅ Basic understanding of Unity prefabs and components

---

## Asset Planning

### 1. Define the Asset Purpose

Ask yourself:
- What category does this asset belong to? (Housing, Vehicles, Avatars, etc.)
- What is its purpose in the game?
- What economic tier should it belong to?
- Will it be NFT-compatible?
- What is the target price range?

### 2. Determine Economic Impact

Consider the Dominion Economy:
- How does this asset fit into the existing economy?
- What should the purchase price be?
- Should it have rental options?
- What is the appropriate rarity tier?

### 3. Plan Technical Requirements

Define technical specifications:
- Polygon count targets for each LOD
- Texture resolution requirements
- Required Unity components
- Physics requirements
- Special effects needed

---

## Creating the Definition File

### Step 1: Choose the Correct Location

Place your JSON file in the appropriate directory:

```
Assets/Prefabs/
├── Housing/
│   ├── Apartments/         → For apartments
│   ├── Condos/            → For condos
│   ├── Mansions/          → For mansions
│   └── Penthouses/        → For penthouses
├── Vehicles/
│   ├── Cars/              → For cars
│   ├── Bikes/             → For motorcycles
│   └── Aircraft/          → For aircraft
├── Avatars/
│   ├── Base/              → For base character models
│   ├── Customization/     → For customization options
│   └── Animations/        → For animation sets
├── Gyms/
│   └── Equipment/         → For gym equipment
└── Buildings/             → For commercial buildings
```

### Step 2: Create the JSON File

**Naming Convention:** Use descriptive names that match the asset type.

Examples:
- `ExecutivePenthouse.json`
- `ElectricSupercar.json`
- `FashionDistrict_Boutique.json`

### Step 3: Use the Template

Start with this base template and customize for your asset type:

```json
{
  "prefabName": "Category_Subcategory_Type",
  "category": "Category",
  "subCategory": "Subcategory",
  "type": "Type",
  "metadata": {
    "nftCompatible": true,
    "ownershipType": "Individual",
    "economicTier": "Premium",
    "dominionZone": "Downtown"
  },
  "specifications": {
    // Category-specific specifications here
  },
  "graphics": {
    "lodLevels": 3,
    "polyCount": {
      "lod0": 50000,
      "lod1": 25000,
      "lod2": 10000
    },
    "textureResolution": {
      "diffuse": 2048,
      "normal": 2048,
      "roughness": 1024,
      "metallic": 1024
    },
    "materials": [
      "Material_Name_PBR"
    ],
    "lighting": {
      "realtimeLights": 2,
      "lightProbes": true,
      "reflectionProbes": true,
      "ambientOcclusion": true
    },
    "renderPipeline": "URP",
    "realisticFeatures": [
      "PBR Materials",
      "Real-time Shadows",
      "Global Illumination"
    ]
  },
  "components": [
    "RequiredComponent1",
    "RequiredComponent2"
  ],
  "price": {
    "purchasePrice": 0,
    "currency": "OMNI"
  }
}
```

---

## Step-by-Step Process

### Phase 1: Definition Creation

#### Step 1: Create JSON File

1. Navigate to the appropriate folder
2. Create a new `.json` file with a descriptive name
3. Copy the base template

#### Step 2: Fill in Basic Information

```json
{
  "prefabName": "Housing_Penthouse_Executive",
  "category": "Housing",
  "subCategory": "Penthouses",
  "type": "Executive",
```

#### Step 3: Configure Metadata

```json
  "metadata": {
    "nftCompatible": true,           // Can be minted as NFT?
    "ownershipType": "Individual",   // Individual | Shared | Corporate
    "economicTier": "Luxury",        // Entry | Standard | Premium | Luxury | Ultra-Luxury
    "dominionZone": "Financial"      // Optional: specific zone
  },
```

#### Step 4: Add Specifications

Add category-specific specifications (see Category-Specific Guidelines below).

#### Step 5: Configure Graphics

```json
  "graphics": {
    "lodLevels": 4,
    "polyCount": {
      "lod0": 75000,    // High detail for close viewing
      "lod1": 35000,    // Medium detail
      "lod2": 15000,    // Low detail
      "lod3": 5000      // Minimal detail for distance
    },
    "textureResolution": {
      "diffuse": 4096,
      "normal": 4096,
      "roughness": 2048,
      "metallic": 2048
    },
    "materials": [
      "Marble_Floor_Italian_PBR",
      "Glass_Window_FloorToCeiling",
      "Wood_Mahogany_Polished",
      "Leather_Premium_Italian",
      "Metal_Brushed_Steel"
    ],
    "lighting": {
      "realtimeLights": 4,
      "lightProbes": true,
      "reflectionProbes": true,
      "ambientOcclusion": true
    },
    "renderPipeline": "URP",
    "realisticFeatures": [
      "PBR Materials",
      "Real-time Shadows",
      "Global Illumination",
      "Screen Space Reflections",
      "Volumetric Lighting",
      "Contact Shadows"
    ]
  },
```

#### Step 6: Define Components

```json
  "components": [
    "BoxCollider",
    "PropertyOwnership",
    "NFTMetadata",
    "InteriorController",
    "DoorInteraction",
    "LightingController",
    "SecuritySystem"
  ],
```

#### Step 7: Set Pricing

```json
  "price": {
    "purchasePrice": 8500000,        // Full purchase price
    "monthlyRent": 45000,            // Monthly rental (if applicable)
    "currency": "OMNI"
  }
}
```

### Phase 2: Validation

#### Step 1: Validate JSON Syntax

Use [JSONLint](https://jsonlint.com/) or VS Code's built-in validator to ensure proper JSON syntax.

#### Step 2: Check Schema Compliance

Verify your definition includes all required fields:
- ✅ prefabName
- ✅ category
- ✅ subCategory
- ✅ metadata
- ✅ specifications
- ✅ graphics
- ✅ components
- ✅ price

#### Step 3: Verify Economic Balance

Check that:
- Price aligns with economicTier
- Price is reasonable compared to similar assets
- Rental prices (if any) are proportional to purchase price

### Phase 3: Unity Integration

#### Step 1: Create the Unity Prefab

1. Open Unity Editor
2. Import or create the 3D model
3. Add required components from your JSON definition
4. Configure materials and textures
5. Set up LOD group if multiple LOD levels
6. Save as prefab in appropriate folder

#### Step 2: Link Definition to Prefab

Ensure the prefab name matches the JSON `prefabName` field.

#### Step 3: Test in Scene

1. Drag prefab into a test scene
2. Verify all components work correctly
3. Check visual quality at different distances
4. Test interactions (doors, lights, etc.)

### Phase 4: Registry Update

#### Update AssetRegistry.json

Add your new asset to the master registry:

1. Open `/Assets/AssetRegistry.json`
2. Find the appropriate category and subcategory
3. Add your asset filename to the `assets` array
4. Increment the `count` field
5. Update `statistics` section if needed

Example:

```json
{
  "name": "Penthouses",
  "path": "Assets/Prefabs/Housing/Penthouses",
  "count": 10,  // Increment this
  "economicTier": "Ultra-Luxury",
  "priceRange": { "min": 3800000, "max": 45000000, "currency": "OMNI" },
  "assets": [
    "SkyVilla.json",
    "CornerTower.json",
    "Duplex.json",
    "Triplex.json",
    "Contemporary.json",
    "ExecutivePenthouse.json"  // Add your new asset here
  ]
}
```

---

## Category-Specific Guidelines

### Housing Assets

**Required Specifications:**
```json
"specifications": {
  "squareFootage": 3200,
  "bedrooms": 3,
  "bathrooms": 2.5,
  "floors": 2,
  "parkingSpaces": 2
},
"layout": {
  "dimensions": {
    "width": 40,
    "length": 50,
    "height": 24
  },
  "rooms": [
    {
      "name": "Master Bedroom",
      "type": "Bedroom",
      "dimensions": { "width": 18, "length": 20, "height": 12 },
      "position": { "x": 0, "y": 12, "z": 0 },
      "features": ["Walk-in Closet", "En-suite Bathroom", "Private Balcony"]
    }
  ],
  "entryPoints": [
    {
      "name": "Main Entrance",
      "position": { "x": 20, "y": 0, "z": 0 },
      "type": "Double Door",
      "interactable": true
    }
  ],
  "windows": [
    {
      "position": { "x": 40, "y": 6, "z": 10 },
      "size": "Floor-to-Ceiling",
      "type": "Fixed"
    }
  ]
}
```

**Required Components:**
- `BoxCollider` or `MeshCollider`
- `PropertyOwnership`
- `NFTMetadata`
- `InteriorController`
- `DoorInteraction`

**Pricing Guidelines:**
- Entry Apartments: $18K-$85K OMNI
- Standard Condos: $145K-$385K OMNI
- Premium Houses: $500K-$2.8M OMNI
- Luxury Mansions: $2.8M-$8.5M OMNI
- Ultra-Luxury Penthouses: $8.5M-$45M OMNI

### Vehicle Assets

**Required Specifications:**
```json
"specifications": {
  "class": "Luxury",
  "seats": 4,
  "engine": "4.0L V8 Twin-Turbo",
  "horsepower": 550,
  "topSpeed": 190,
  "acceleration": 3.8,
  "handling": 9.2,
  "fuelType": "Premium Gasoline"
},
"physics": {
  "mass": 1850,
  "centerOfMass": { "x": 0, "y": 0.4, "z": -0.1 },
  "wheelBase": 2.95,
  "trackWidth": 1.65,
  "suspensionTravel": 0.12,
  "springRate": 45000,
  "damperRate": 3800,
  "downforce": 150
},
"model": {
  "dimensions": {
    "length": 5.1,
    "width": 1.92,
    "height": 1.45,
    "wheelbase": 2.95
  },
  "doors": 4,
  "trunk": "Large",
  "bodyStyle": "Sedan"
}
```

**Required Components:**
- `Rigidbody`
- `WheelCollider` (4x for cars)
- `VehicleController`
- `VehicleOwnership`
- `NFTMetadata`
- `AudioSource`
- `ParticleSystem` (for effects)

**Optional but Recommended:**
- `DamageSystem`
- `AerodynamicsController`
- `CustomizationController`

**Pricing Guidelines:**
- Entry Vehicles: $9.5K-$32K OMNI
- Standard Vehicles: $45K-$95K OMNI
- Premium Vehicles: $125K-$450K OMNI
- Luxury Vehicles: $1.8M-$5.5M OMNI
- Ultra-Luxury Aircraft: $5.5M-$12M OMNI

### Avatar Assets

#### Base Models

**Required Specifications:**
```json
"specifications": {
  "ageGroup": "Adult",
  "gender": "Female",
  "boneCount": 78,
  "blendShapes": 58,
  "height": 168,
  "weight": 58
},
"rigging": {
  "armatureType": "Humanoid",
  "ikSupport": true,
  "fingerBones": true,
  "facialRig": true,
  "twistBones": true
}
```

**Required Components:**
- `Animator`
- `CharacterController`
- `AvatarCustomization`
- `IKController`

#### Customization Options

**Structure:**
```json
"options": {
  "category": "Clothing",
  "items": [
    {
      "id": "formal_suit_001",
      "name": "Executive Suit",
      "description": "Professional business attire",
      "rarity": "Rare",
      "price": 250,
      "attachmentPoints": ["Torso", "Arms", "Legs"],
      "physicsBased": false,
      "colorVariants": ["Black", "Navy", "Charcoal", "Brown"]
    }
  ]
}
```

### Gym & Equipment Assets

**Required Specifications:**
```json
"specifications": {
  "equipmentType": "Training Equipment",
  "sportType": "Boxing",
  "weight": 45,
  "dimensions": {
    "width": 0.4,
    "length": 0.4,
    "height": 1.8
  },
  "durability": 1000,
  "material": "Leather and Canvas"
}
```

**Required Components:**
- `MeshCollider`
- `EquipmentController`
- `DurabilitySystem`
- `InteractionZone`

### Building Assets

**Required Specifications:**
```json
"specifications": {
  "buildingType": "Retail",
  "businessType": "Fashion Boutique",
  "floors": 3,
  "area": 8500,
  "capacity": 75
},
"features": {
  "retailFloor": true,
  "officeSpace": true,
  "storageArea": true,
  "parkingLot": true,
  "signage": "LED Display"
}
```

---

## Testing Your Asset

### Visual Testing

1. **LOD Verification**
   - Test asset at various distances
   - Ensure smooth transitions between LOD levels
   - Verify polygon counts match definition

2. **Material Quality**
   - Check PBR materials render correctly
   - Verify textures are sharp and properly mapped
   - Test lighting response

3. **Performance**
   - Monitor frame rate with asset in scene
   - Check draw calls
   - Verify memory usage

### Functional Testing

1. **Component Testing**
   - Test all interactive components
   - Verify ownership system works
   - Check NFT metadata is correct

2. **Physics Testing**
   - For vehicles: test driving physics
   - For housing: test collision detection
   - Verify physics behaves realistically

3. **Economic Testing**
   - Verify pricing is correct
   - Test purchase/rental transactions
   - Check integration with Dominion Economy

### Integration Testing

1. **Scene Integration**
   - Place asset in appropriate city scene
   - Verify it fits the environment
   - Check for clipping or placement issues

2. **System Integration**
   - Test with GameManager
   - Verify ZoneController integration
   - Check DominionEconomy integration

---

## Updating the Registry

After creating and testing your asset, update the master registry:

### Step 1: Open AssetRegistry.json

```bash
# File location
/Assets/AssetRegistry.json
```

### Step 2: Find Correct Category

Navigate to the appropriate category and subcategory.

### Step 3: Add Asset Entry

Add your asset filename to the `assets` array:

```json
"assets": [
  "ExistingAsset1.json",
  "ExistingAsset2.json",
  "YourNewAsset.json"  // Add here
]
```

### Step 4: Update Counts

Increment the `count` field:

```json
"count": 10,  // Was 9, now 10
```

### Step 5: Update Statistics

Update the statistics section at the bottom of the registry:

```json
"statistics": {
  "totalCategories": 5,
  "totalSubcategories": 14,
  "totalAssets": 101,  // Increment this
  "breakdown": {
    "housing": 36,  // Increment category count if adding housing
    "vehicles": 41,
    "avatars": 25,
    "gyms": 12,
    "buildings": 2
  }
}
```

### Step 6: Commit Changes

```bash
git add Assets/AssetRegistry.json
git add Assets/Prefabs/YourCategory/YourNewAsset.json
git commit -m "Add new asset: YourNewAsset"
git push
```

---

## Common Issues

### Issue 1: JSON Validation Errors

**Problem:** JSON file has syntax errors

**Solution:**
- Use JSONLint.com to validate
- Check for missing commas
- Verify all brackets and braces match
- Remove trailing commas

### Issue 2: Asset Not Loading in Unity

**Problem:** Asset definition file is not being recognized

**Solution:**
- Verify file is in correct directory
- Check that filename matches prefabName
- Ensure JSON is valid
- Check AssetRegistry.json includes the asset

### Issue 3: Price Doesn't Match Economic Tier

**Problem:** Asset price doesn't align with its economicTier

**Solution:**
- Review economic tier price ranges in schema
- Adjust price to match tier
- Or adjust tier to match price

### Issue 4: Missing Required Components

**Problem:** Unity prefab doesn't have all required components

**Solution:**
- Review category-specific component requirements
- Add missing components in Unity
- Update components array in JSON

### Issue 5: LOD Not Working Properly

**Problem:** Asset doesn't switch LOD levels correctly

**Solution:**
- Verify LOD Group is configured in Unity
- Check polygon counts are descending (LOD0 > LOD1 > LOD2)
- Ensure LOD distances are set appropriately

---

## Examples

### Example 1: Adding a New Apartment

**File:** `Assets/Prefabs/Housing/Apartments/LuxuryStudio.json`

```json
{
  "prefabName": "Housing_Apartment_LuxuryStudio",
  "category": "Housing",
  "subCategory": "Apartments",
  "type": "Luxury Studio",
  "metadata": {
    "nftCompatible": true,
    "ownershipType": "Individual",
    "economicTier": "Premium",
    "dominionZone": "Downtown"
  },
  "specifications": {
    "squareFootage": 650,
    "bedrooms": 0,
    "bathrooms": 1,
    "floors": 1,
    "parkingSpaces": 1
  },
  "layout": {
    "dimensions": {
      "width": 25,
      "length": 26,
      "height": 12
    },
    "rooms": [
      {
        "name": "Main Living Area",
        "type": "Open Concept",
        "dimensions": { "width": 20, "length": 22, "height": 12 },
        "position": { "x": 0, "y": 0, "z": 0 },
        "features": ["Designer Kitchen", "Smart Home Tech", "Floor-to-Ceiling Windows"]
      }
    ],
    "entryPoints": [
      {
        "name": "Main Door",
        "position": { "x": 12.5, "y": 0, "z": 0 },
        "type": "Premium",
        "interactable": true
      }
    ],
    "windows": [
      {
        "position": { "x": 25, "y": 6, "z": 13 },
        "size": "Floor-to-Ceiling",
        "type": "Fixed"
      }
    ]
  },
  "graphics": {
    "lodLevels": 3,
    "polyCount": {
      "lod0": 35000,
      "lod1": 18000,
      "lod2": 8000
    },
    "textureResolution": {
      "diffuse": 2048,
      "normal": 2048,
      "roughness": 1024,
      "metallic": 1024
    },
    "materials": [
      "Hardwood_Floor_Premium",
      "Marble_Countertop_Italian",
      "Glass_Window_FloorToCeiling",
      "Steel_Appliances_Brushed"
    ],
    "lighting": {
      "realtimeLights": 3,
      "lightProbes": true,
      "reflectionProbes": true,
      "ambientOcclusion": true
    },
    "renderPipeline": "URP",
    "realisticFeatures": [
      "PBR Materials",
      "Real-time Shadows",
      "Global Illumination",
      "Screen Space Reflections"
    ]
  },
  "components": [
    "BoxCollider",
    "PropertyOwnership",
    "NFTMetadata",
    "InteriorController",
    "DoorInteraction",
    "SmartHomeController"
  ],
  "price": {
    "purchasePrice": 285000,
    "monthlyRent": 3500,
    "currency": "OMNI"
  }
}
```

### Example 2: Adding a New Vehicle

**File:** `Assets/Prefabs/Vehicles/Cars/FamilySUV.json`

```json
{
  "prefabName": "Vehicle_Car_FamilySUV",
  "category": "Vehicles",
  "subCategory": "Cars",
  "type": "Family SUV",
  "metadata": {
    "nftCompatible": true,
    "ownershipType": "Individual",
    "economicTier": "Standard",
    "performanceClass": "Utility"
  },
  "specifications": {
    "class": "SUV",
    "seats": 7,
    "engine": "3.5L V6",
    "horsepower": 290,
    "topSpeed": 130,
    "acceleration": 7.5,
    "handling": 7.0,
    "fuelType": "Regular Gasoline"
  },
  "physics": {
    "mass": 2100,
    "centerOfMass": { "x": 0, "y": 0.6, "z": 0 },
    "wheelBase": 2.85,
    "trackWidth": 1.7,
    "suspensionTravel": 0.15,
    "springRate": 35000,
    "damperRate": 3000,
    "downforce": 50
  },
  "model": {
    "dimensions": {
      "length": 5.0,
      "width": 2.0,
      "height": 1.8,
      "wheelbase": 2.85
    },
    "doors": 4,
    "trunk": "Extra Large",
    "bodyStyle": "SUV"
  },
  "graphics": {
    "lodLevels": 4,
    "polyCount": {
      "lod0": 55000,
      "lod1": 28000,
      "lod2": 12000,
      "lod3": 5000
    },
    "textureResolution": {
      "diffuse": 4096,
      "normal": 4096,
      "roughness": 2048,
      "metallic": 2048
    },
    "materials": [
      "Car_Paint_Metallic_Standard",
      "Plastic_Interior_Textured",
      "Glass_Window_Tinted",
      "Rubber_Tire_AllSeason",
      "Chrome_Standard",
      "Fabric_Seats_Durable"
    ],
    "lighting": {
      "headlights": true,
      "taillights": true,
      "brakelights": true,
      "turnsignals": true,
      "interiorLights": true,
      "ledAccents": false
    },
    "renderPipeline": "URP",
    "realisticFeatures": [
      "PBR Materials",
      "Real-time Reflections",
      "Dynamic Shadows",
      "Suspension Animation",
      "Working Lights",
      "Dashboard Instruments"
    ]
  },
  "audio": {
    "engineSound": "V6_NA_Utility",
    "exhaustNote": "Subdued",
    "turboWhistle": false,
    "hornSound": "Standard",
    "doorSound": "Heavy",
    "brakeSound": "Standard"
  },
  "effects": {
    "exhaust": {
      "particleSystem": true,
      "dualExhaust": false,
      "position": [
        { "x": 0, "y": 0.3, "z": -2.5 }
      ]
    },
    "tireSmokeOnBurnout": false,
    "brakelightGlow": true,
    "headlightBeams": true
  },
  "components": [
    "Rigidbody",
    "VehicleController",
    "WheelCollider",
    "AudioSource",
    "ParticleSystem",
    "VehicleOwnership",
    "NFTMetadata"
  ],
  "customization": {
    "paintColors": ["Pearl White", "Graphite Gray", "Ocean Blue", "Ruby Red"],
    "wheels": ["Standard 18in", "Alloy 19in"],
    "interior": ["Fabric", "Leather"],
    "upgrades": ["Roof Rack", "Tow Package", "Entertainment System"]
  },
  "price": {
    "purchasePrice": 48000,
    "dailyRent": 125,
    "currency": "OMNI"
  }
}
```

### Example 3: Adding Avatar Customization

**File:** `Assets/Prefabs/Avatars/Customization/Eyewear.json`

```json
{
  "prefabName": "Avatar_Customization_Eyewear",
  "category": "Avatars",
  "subCategory": "Customization",
  "type": "Eyewear",
  "metadata": {
    "nftCompatible": true,
    "ownershipType": "Individual",
    "economicTier": "Entry"
  },
  "options": {
    "category": "Eyewear",
    "items": [
      {
        "id": "glasses_classic_001",
        "name": "Classic Frames",
        "description": "Timeless eyeglass style",
        "rarity": "Common",
        "price": 25,
        "attachmentPoint": "Head",
        "physicsBased": false,
        "colorVariants": ["Black", "Brown", "Silver", "Gold"]
      },
      {
        "id": "sunglasses_aviator_001",
        "name": "Aviator Sunglasses",
        "description": "Iconic pilot-style sunglasses",
        "rarity": "Common",
        "price": 35,
        "attachmentPoint": "Head",
        "physicsBased": false,
        "colorVariants": ["Gold", "Silver", "Black"]
      },
      {
        "id": "sunglasses_designer_001",
        "name": "Designer Shades",
        "description": "High-fashion sunglasses",
        "rarity": "Rare",
        "price": 150,
        "attachmentPoint": "Head",
        "physicsBased": false,
        "colorVariants": ["Black", "Tortoise", "White"]
      }
    ]
  },
  "graphics": {
    "lodLevels": 2,
    "polyCount": {
      "lod0": 2000,
      "lod1": 800
    },
    "textureResolution": {
      "diffuse": 1024,
      "normal": 1024,
      "roughness": 512,
      "metallic": 512
    },
    "materials": [
      "Plastic_Eyewear_PBR",
      "Glass_Lens_Clear",
      "Glass_Lens_Tinted",
      "Metal_Frame_Polished"
    ],
    "renderPipeline": "URP",
    "realisticFeatures": [
      "PBR Materials",
      "Transparent Glass",
      "Metal Reflections"
    ]
  },
  "components": [
    "SkinnedMeshRenderer",
    "CustomizationItem",
    "NFTMetadata"
  ],
  "price": {
    "purchasePrice": 25,
    "currency": "OMNI"
  }
}
```

---

## Checklist

Use this checklist when adding a new asset:

### Planning Phase
- [ ] Asset purpose defined
- [ ] Category and subcategory determined
- [ ] Economic tier selected
- [ ] Price range established
- [ ] Technical requirements planned

### Creation Phase
- [ ] JSON file created in correct location
- [ ] Base template copied
- [ ] All required fields filled
- [ ] Category-specific specifications added
- [ ] Graphics configuration complete
- [ ] Components list defined
- [ ] Pricing set

### Validation Phase
- [ ] JSON syntax validated
- [ ] Schema compliance checked
- [ ] Economic balance verified
- [ ] All required fields present

### Unity Integration Phase
- [ ] 3D model imported/created
- [ ] Components added
- [ ] Materials configured
- [ ] LOD group set up (if applicable)
- [ ] Prefab saved
- [ ] Prefab tested in scene

### Registry Update Phase
- [ ] AssetRegistry.json updated
- [ ] Asset added to correct subcategory
- [ ] Count incremented
- [ ] Statistics updated
- [ ] Changes committed to git

### Testing Phase
- [ ] Visual quality verified
- [ ] LOD transitions smooth
- [ ] Components functional
- [ ] Physics working correctly
- [ ] Economic integration working
- [ ] NFT metadata correct

---

## Additional Resources

- **Schema Documentation:** [ASSET_DEFINITION_SCHEMA.md](ASSET_DEFINITION_SCHEMA.md)
- **Master Registry:** [AssetRegistry.json](../Assets/AssetRegistry.json)
- **Prefab Examples:** `/Assets/Prefabs/` (any existing category)
- **Unity Scripts:** `/Assets/Scripts/Core/AssetDefinitionManager.cs`

---

## Support

For questions or issues:
1. Review existing asset definitions for examples
2. Check the schema documentation
3. Test with similar existing assets
4. Contact the development team

---

**Last Updated:** January 2, 2025  
**Document Version:** 1.0.0  
**Compatible with Schema:** 1.0.0
