# Architecture Career Path - Technical Documentation

## Overview

The Architecture Career Path enables players to design 3D blueprints, mint them as NFTs, and certify them for real-world construction. This system bridges digital creativity with physical world opportunities, creating a unique value proposition for architects in OmniWorld.

## Core Concept

Architects can:
1. **Design** - Create detailed 3D blueprints with dimensions and specifications
2. **Mint** - Convert blueprints into tradeable NFTs
3. **Certify** - Get real-world construction certification for physical builds
4. **Monetize** - Earn from both digital NFT sales and real-world building licenses

## System Architecture

### Components

- **ArchitectureSystem.cs** - Main system controller
- **BlueprintData** - Data structure for architectural designs
- **NFT Integration** - Web3 minting through ContractBridge
- **Certification System** - Real-world build validation
- **Economic Integration** - DominionEconomy pricing

## Features

### 1. Blueprint Creation

Architects can create detailed 3D blueprints with full specifications.

**Blueprint Types:**
- Residential
- Commercial
- Industrial
- Luxury
- Infrastructure

**Example:**
```csharp
// Create a residential blueprint
BlueprintData blueprint = ArchitectureSystem.Instance.CreateBlueprint(
    architectAddress: "0x123...",
    type: BlueprintType.Residential,
    structureName: "Modern Villa",
    dimensions: new Vector3(20f, 8f, 15f) // Width, Height, Depth in meters
);

Debug.Log($"Blueprint created: {blueprint.structureName}");
Debug.Log($"Estimated build cost: ${blueprint.estimatedBuildCost:N2}");
Debug.Log($"Blueprint value: {blueprint.valueInOMNI} $OMNI");
```

### 2. NFT Minting

Convert blueprints into tradeable NFTs on the blockchain.

**Process:**
1. Blueprint must be created first
2. Pay minting fee (100 $OMNI default)
3. NFT is minted with metadata
4. Blueprint becomes tradeable

**Example:**
```csharp
// Mint blueprint as NFT
bool success = ArchitectureSystem.Instance.MintBlueprintNFT(
    blueprintId: blueprint.id,
    architectAddress: "0x123..."
);

if (success)
{
    Debug.Log($"NFT minted! Token ID: {blueprint.nftTokenId}");
}
```

**NFT Metadata:**
- Blueprint name
- Blueprint type
- Dimensions
- Architect address
- Creation date
- Real-world certification status
- Estimated build cost

### 3. Real-World Certification

High-reputation architects can certify blueprints for physical construction.

**Requirements:**
- Blueprint must be minted as NFT
- Architect reputation ≥ 0.8 (80%)
- Blueprint meets building standards
- Valid dimensions and specifications

**Benefits:**
- Real-world building code assignment
- 50% value increase
- Eligibility for construction licensing
- Physical construction permits

**Example:**
```csharp
// Certify for real-world construction
bool certified = ArchitectureSystem.Instance.CertifyForRealWorld(
    blueprintId: blueprint.id,
    architectReputation: 0.9f // 90% reputation
);

if (certified)
{
    Debug.Log($"Certified! Building code: {blueprint.realWorldBuildingCode}");
    Debug.Log($"New value: {blueprint.valueInOMNI} $OMNI (50% increase)");
}
```

### 4. Economic Model

#### Creation Costs
- **Blueprint Creation**: 50 $OMNI (base cost)
- **NFT Minting**: 100 $OMNI
- **Total Entry**: 150 $OMNI

#### Blueprint Valuation
Blueprint value is calculated based on:
1. **Volume**: (width × height × depth) / 10
2. **Type Multiplier**:
   - Residential: 1.0×
   - Commercial: 1.5×
   - Industrial: 1.2×
   - Luxury: 3.0×
   - Infrastructure: 2.0×

**Example Calculation:**
```
Modern Villa: 20m × 8m × 15m = 2,400 m³
Base value: 2,400 / 10 = 240 $OMNI
Type: Residential (1.0×)
Final value: 240 $OMNI

After certification: 240 × 1.5 = 360 $OMNI
```

#### Real-World Build Costs
Estimated construction costs (USD):
- **Residential**: $1,500 per m³
- **Commercial**: $2,000 per m³
- **Industrial**: $1,200 per m³
- **Luxury**: $5,000 per m³
- **Infrastructure**: $3,000 per m³

**Example:**
```
Modern Villa: 2,400 m³ × $1,500 = $3,600,000
```

## API Reference

### ArchitectureSystem

#### CreateBlueprint
```csharp
public BlueprintData CreateBlueprint(
    string architectAddress,
    BlueprintType type,
    string structureName,
    Vector3 dimensions
)
```
Create a new architectural blueprint.

**Parameters:**
- `architectAddress` - Architect's wallet address
- `type` - Blueprint type (Residential, Commercial, etc.)
- `structureName` - Name of the structure
- `dimensions` - Vector3 (width, height, depth) in meters

**Returns:** `BlueprintData` object or `null` if failed

**Limits:**
- Maximum 100 blueprints per architect
- Minimum dimensions: > 0 for all axes
- Maximum height: 300 meters

#### MintBlueprintNFT
```csharp
public bool MintBlueprintNFT(int blueprintId, string architectAddress)
```
Mint a blueprint as an NFT.

**Parameters:**
- `blueprintId` - ID of the blueprint to mint
- `architectAddress` - Architect's wallet address (must be owner)

**Returns:** `true` if successful

**Requirements:**
- Blueprint must exist
- Caller must be the blueprint creator
- Blueprint not already minted
- Sufficient $OMNI for minting fee

#### CertifyForRealWorld
```csharp
public bool CertifyForRealWorld(int blueprintId, float architectReputation)
```
Certify a blueprint for real-world construction.

**Parameters:**
- `blueprintId` - ID of the blueprint
- `architectReputation` - Architect's reputation (0.0-1.0)

**Returns:** `true` if certified

**Requirements:**
- Blueprint must be minted as NFT
- Architect reputation ≥ 0.8
- Blueprint meets building standards
- Valid dimensions

**Effects:**
- Assigns real-world building code
- Increases value by 50%
- Enables construction licensing

#### GetBlueprintById
```csharp
public BlueprintData GetBlueprintById(int id)
```
Retrieve a blueprint by its ID.

**Returns:** `BlueprintData` or `null` if not found

#### GetCertifiedBlueprints
```csharp
public List<BlueprintData> GetCertifiedBlueprints()
```
Get all blueprints certified for real-world construction.

**Returns:** List of certified blueprints

## Data Structures

### BlueprintData
```csharp
public class BlueprintData
{
    public int id;                           // Unique blueprint ID
    public string architectAddress;          // Creator's wallet
    public string structureName;             // Building name
    public BlueprintType blueprintType;      // Type classification
    public Vector3 dimensions;               // Width × Height × Depth (meters)
    public System.DateTime creationDate;     // When created
    public bool isNFTMinted;                 // NFT status
    public string nftTokenId;                // NFT token ID
    public System.DateTime mintDate;         // When minted
    public bool isCertifiedForRealWorld;     // Certification status
    public string realWorldBuildingCode;     // Building code
    public System.DateTime certificationDate; // When certified
    public float estimatedBuildCost;         // USD estimate
    public float valueInOMNI;                // Blueprint value
}
```

### BlueprintType Enum
```csharp
public enum BlueprintType
{
    Residential,      // Houses, apartments, condos
    Commercial,       // Offices, retail, restaurants
    Industrial,       // Factories, warehouses
    Luxury,          // High-end properties
    Infrastructure   // Roads, bridges, public works
}
```

## Building Standards Validation

The system validates blueprints against basic building standards:

### Validation Checks
1. **Dimension Validity**: All dimensions > 0
2. **Height Limit**: Maximum 300 meters
3. **Structural Feasibility**: Basic ratio checks
4. **Code Compliance**: Future integration with real building codes

### Real-World Certification Process
1. Blueprint submitted for certification
2. System validates dimensions and type
3. Checks architect reputation (≥ 0.8)
4. Generates unique building code
5. Records certification date
6. Increases blueprint value by 50%

## Integration Guide

### Step 1: Setup
Add ArchitectureSystem to your scene:

```csharp
GameObject archSystem = new GameObject("ArchitectureSystem");
archSystem.AddComponent<ArchitectureSystem>();
```

### Step 2: Create Blueprint
When architect designs a building:

```csharp
string architectAddress = WalletConnect.Instance.connectedAddress;

BlueprintData blueprint = ArchitectureSystem.Instance.CreateBlueprint(
    architectAddress,
    BlueprintType.Residential,
    "Sunset Villa",
    new Vector3(15f, 10f, 12f)
);
```

### Step 3: Mint as NFT
Convert to tradeable NFT:

```csharp
bool minted = ArchitectureSystem.Instance.MintBlueprintNFT(
    blueprint.id,
    architectAddress
);
```

### Step 4: Certify for Real-World
If architect has high reputation:

```csharp
float reputation = 0.85f; // From reputation system

bool certified = ArchitectureSystem.Instance.CertifyForRealWorld(
    blueprint.id,
    reputation
);
```

### Step 5: Marketplace Integration
List certified blueprints for sale:

```csharp
List<BlueprintData> certified = ArchitectureSystem.Instance.GetCertifiedBlueprints();

foreach (var bp in certified)
{
    Debug.Log($"{bp.structureName}: {bp.valueInOMNI} $OMNI");
    Debug.Log($"Building Code: {bp.realWorldBuildingCode}");
    Debug.Log($"Estimated Build: ${bp.estimatedBuildCost:N2}");
}
```

## Career Progression

### Reputation Levels
- **0.0-0.3**: Novice - Can create basic blueprints
- **0.3-0.5**: Intermediate - Larger projects
- **0.5-0.8**: Professional - Complex designs
- **0.8-1.0**: Master - Real-world certification

### Earning Potential

**Digital NFT Sales:**
- Basic blueprint: 100-500 $OMNI
- Professional blueprint: 500-2,000 $OMNI
- Certified blueprint: 2,000-10,000 $OMNI

**Real-World Licensing:**
- Construction license: 5-10% of build cost
- Consultation fees: 50-200 $OMNI per hour
- Royalties: 1-3% of property value

### Example Career Path
```
Month 1: Create 10 basic blueprints → 2,400 $OMNI
Month 2: 5 professional blueprints → 7,500 $OMNI
Month 3: 2 certified blueprints → 12,000 $OMNI
Month 4: 1 real-world build license → 180,000 $OMNI (6M build @ 3%)
```

## Real-World Integration

### Use Cases

1. **Homebuyers**: Purchase certified blueprints for dream homes
2. **Developers**: License designs for real construction projects
3. **Cities**: Commission infrastructure blueprints
4. **Investors**: Collect valuable architectural NFTs

### Legal Framework
- Digital blueprints are NFTs with full ownership rights
- Certified blueprints include construction licensing
- Real-world builds require local permits
- Architect retains design copyright

### Partnership Opportunities
- Construction companies
- Real estate developers
- Architecture firms
- City planning departments

## Economic Impact

### Valuation Increase
The Architecture career path significantly increases ecosystem valuation:

1. **Digital Asset Value**: Blueprints as tradeable NFTs
2. **Real-World Revenue**: Construction licensing fees
3. **Professional Services**: Architecture consultation
4. **Property Value**: Certified designs increase land value
5. **Economic Activity**: Stimulates construction industry

### Revenue Streams
- Blueprint sales (85% to architect, 15% to treasury)
- NFT royalties (20% perpetual)
- Certification fees
- Real-world licensing (architect sets terms)

## Configuration

### Default Settings
```csharp
[Header("Blueprint Configuration")]
public int maxBlueprintsPerArchitect = 100;
public float blueprintCreationCost = 50f;
public float nftMintingFee = 100f;

[Header("Real World Integration")]
public bool enableRealWorldBuilds = true;
public float minReputationForCertification = 0.8f;
```

### Adjusting Parameters
```csharp
// Increase blueprint limit for VIP architects
ArchitectureSystem.Instance.maxBlueprintsPerArchitect = 500;

// Reduce certification requirements for events
ArchitectureSystem.Instance.minReputationForCertification = 0.6f;
```

## Future Enhancements

1. **3D Visualization**: In-world blueprint preview
2. **Collaboration**: Multi-architect projects
3. **Material Library**: Detailed construction materials
4. **BIM Integration**: Building Information Modeling
5. **AR Preview**: Augmented reality on-site visualization
6. **Smart Contracts**: Automated licensing and royalties
7. **DAO Governance**: Community blueprint approval

## Support

For questions or issues:
- Discord: #architecture channel
- Email: architects@omniworld.io
- Documentation: https://docs.omniworld.io/architecture

---

**Build your dreams, from digital to reality™**
