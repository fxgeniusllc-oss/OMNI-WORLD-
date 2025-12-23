# OmniWorld Auto Dealership - Vehicle NFT Catalog

## Overview

This directory contains JSON configuration files for exclusive NFT-based luxury vehicles available at the OmniWorld Auto Gallery. Each vehicle is a unique digital asset with full in-game functionality and blockchain-backed ownership.

## Vehicle Categories

### 1-of-1 Ultra-Legendary Vehicles

These are singular masterpieces - only ONE will ever exist in the entire OmniWorld metaverse.

#### Apex One
- **File**: `NFT_ApexOne_1of1.json`
- **Price**: 10,000,000 OMNI (~$350,000 USD)
- **Engine**: 9.0L Twin-Turbo Hybrid V16
- **Power**: 2000 HP
- **Top Speed**: 280 mph
- **0-60**: 1.9 seconds
- **Unique Features**:
  - World's first AI-integrated hypercar
  - Quantum-tuned aerodynamics
  - Holographic display system
  - Diamond-infused carbon fiber
  - Self-optimizing performance

#### Phoenix Eternal
- **File**: `NFT_PhoenixEternal_1of1.json`
- **Price**: 8,500,000 OMNI (~$297,500 USD)
- **Engine**: 7.0L Quad-Turbo W12 Hybrid
- **Power**: 1600 HP
- **Top Speed**: 255 mph
- **0-60**: 2.3 seconds
- **Unique Features**:
  - Morphing body panels
  - Self-healing paint technology
  - Bio-sync driver interface
  - Adaptive comfort system
  - Phoenix wing spoiler

#### Sovereign Crown
- **File**: `NFT_SovereignCrown_1of1.json`
- **Price**: 7,500,000 OMNI (~$262,500 USD)
- **Engine**: 6.0L Twin-Turbo V12 Hybrid
- **Power**: 800 HP
- **Top Speed**: 190 mph
- **0-60**: 3.8 seconds
- **Unique Features**:
  - Presidential-level armor
  - AI security system
  - Biometric access control
  - Executive rear lounge
  - Quantum encryption

### 10-of-10 Legendary Editions

Limited series with only 10 numbered editions of each model worldwide.

#### Dominion GT (Editions 1-10)
- **File**: `NFT_DominionGT_10of10.json`
- **Price**: 3,500,000 OMNI (~$122,500 USD)
- **Engine**: 7.2L Twin-Turbo V12
- **Power**: 1400 HP
- **Top Speed**: 265 mph
- **0-60**: 2.2 seconds
- **Edition Colors**:
  1. Obsidian Black
  2. Platinum Silver
  3. Sapphire Blue
  4. Ruby Red
  5. Emerald Green
  6. Diamond White
  7. Gold Sovereign
  8. Titanium Gray
  9. Copper Bronze
  10. Pearl Iridescent
- **Features**:
  - Numbered edition badge
  - Quantum-sync drivetrain
  - Elite club membership
  - Performance telemetry AI

#### Velocity X (Editions 1-10)
- **File**: `NFT_VelocityX_10of10.json`
- **Price**: 2,800,000 OMNI (~$98,000 USD)
- **Engine**: Tri-Motor Electric (1800 HP combined)
- **Power**: 1800 HP
- **Top Speed**: 270 mph
- **0-60**: 1.8 seconds
- **Range**: 1000+ miles
- **Edition Colors**:
  1. Electric Blue
  2. Cyber White
  3. Neon Green
  4. Quantum Silver
  5. Plasma Orange
  6. Voltage Yellow
  7. Laser Red
  8. Digital Purple
  9. Chrome Mirror
  10. Stealth Black
- **Features**:
  - Tri-motor all-wheel drive
  - 1000+ mile range
  - 5-minute ultra-fast charge
  - Autonomous driving Level 5
  - Neural link interface

## Standard Vehicles

The `Cars` directory also contains standard production vehicles available for regular purchase:

- **Hypercar.json** - High-performance exotic
- **Supercar.json** - Luxury sports car
- **MuscleCar.json** - American power
- **LuxurySedan.json** - Premium comfort
- **SUV.json** - Versatile utility
- And 20+ more models...

## JSON Structure

Each NFT vehicle configuration includes:

```json
{
  "prefabName": "Vehicle identifier",
  "metadata": {
    "rarityTier": "UltraLegendary | Legendary | Epic | Rare | Uncommon | Common",
    "edition": "X of Y",
    "isOneOfOne": true/false
  },
  "specifications": {
    "engine": "Engine configuration",
    "horsepower": 0,
    "topSpeed": 0,
    "acceleration": 0.0
  },
  "nftProperties": {
    "mintingPrice": 0,
    "mintingFeePercent": 0.05,
    "salesTaxPercent": 0.08,
    "royaltyPercent": 0.20,
    "appreciationRate": 0.01,
    "prestigePoints": 0
  },
  "auction": {
    "eligibleForAuction": true,
    "minimumBid": 0,
    "eliteOnly": true,
    "minimumPrestige": 0.9
  },
  "exclusiveFeatures": {
    "ownerPerks": [...],
    "inGameBenefits": [...]
  }
}
```

## Pricing Structure

### Primary Sales (Minting)
```
Base Price: Vehicle value in OMNI
Minting Fee: 5%
Sales Tax: 8%
Total: Base × 1.13
```

### Secondary Sales (Resale)
```
Royalty: 20% to original creator
Platform Fee: 5%
Seller Proceeds: 75%
```

### Monthly Auctions
- **Fee**: 10% of winning bid
- **Royalty**: 20% to original creator
- **Access**: Elite players only (prestige 0.85+)

## Rarity Tiers

| Tier | Production | Appreciation | Prestige | Min Ownership |
|------|------------|--------------|----------|---------------|
| Ultra-Legendary | 1 unit | 1% per day | 900-1000 | 90 days |
| Legendary | 10 units | 0.5% per day | 480-500 | 60 days |
| Epic | 50 units | 0.2% per day | 300 | 30 days |
| Rare | 100 units | 0% | 200 | 14 days |
| Uncommon | 500 units | 0% | 150 | 7 days |
| Common | Unlimited | 0% | 100 | 0 days |

## Owner Benefits

### Ultra-Legendary (1-of-1)
- ✓ +50% race rewards
- ✓ Exclusive missions unlocked
- ✓ Fast travel enabled
- ✓ VIP lounge access
- ✓ Priority event invitations
- ✓ Featured showroom placement

### Legendary (10-of-10)
- ✓ +30-35% race rewards
- ✓ Special quest lines
- ✓ Elite parking access
- ✓ Club membership
- ✓ Annual gatherings
- ✓ Track day access

## Integration

These JSON files are loaded by:
- `AutoDealership.cs` - Main dealership controller
- `VehicleNFT.cs` - NFT data model
- `VehicleShowroom.cs` - Display management
- `AuctionSystem.cs` - Monthly auctions

## Creating New Vehicles

To add a new NFT vehicle:

1. Copy an existing JSON template
2. Update all fields with new specifications
3. Ensure `prefabName` is unique
4. Set appropriate `rarityTier` and `edition` numbers
5. Calculate pricing based on performance and rarity
6. Define exclusive features and benefits
7. Save in appropriate category folder

## Future Work

### 3D Assets (.obj files)
- [ ] High-poly models for each vehicle (LOD0)
- [ ] Optimized models (LOD1-4)
- [ ] Interior details
- [ ] Wheel models
- [ ] Component parts

### Unity Prefabs (.prefab files)
- [ ] Complete vehicle prefabs with physics
- [ ] Vehicle controller setup
- [ ] Audio sources configured
- [ ] Particle systems (exhaust, smoke, effects)
- [ ] Collision meshes
- [ ] Wheel colliders

### Additional Content
- [ ] More 1-of-1 vehicles (target: 10 total)
- [ ] More 10-of-10 editions (target: 10 series)
- [ ] Epic tier vehicles (50 editions each)
- [ ] Rare tier vehicles (100 editions)
- [ ] Customization options

## Documentation

- **Full Documentation**: `/Docs/AUTO_DEALERSHIP.md`
- **System Architecture**: Implementation details and API reference
- **Integration Guide**: How to use the dealership system

## Contact

- **Issues**: GitHub Issues
- **Community**: Discord #auto-dealership
- **Email**: autodealership@omniworld.io

---

**Last Updated**: 2025-01-23
**Total NFT Vehicles**: 5 (3 Ultra-Legendary + 2 Legendary series)
**Total Value**: ~45M OMNI (~$1.6M USD)
