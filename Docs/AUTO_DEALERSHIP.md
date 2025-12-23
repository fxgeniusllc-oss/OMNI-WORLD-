# OmniWorld Auto Dealership System

## Overview

The OmniWorld Auto Dealership is an **exclusive NFT-based luxury automotive experience** featuring ultra-rare vehicles as tradeable digital assets. This system implements a sophisticated economy with 1-of-1 and 10-of-10 limited edition vehicles, dynamic pricing, auction mechanics, and selective resale policies.

## Core Concept

### Exclusive NFT-Based Luxury Auto Dealership
- **1-of-1 Ultra-Legendary Vehicles**: Singular masterpieces that exist nowhere else in the metaverse
- **10-of-10 Legendary Editions**: Limited collectible series with numbered editions
- **Glass Showroom**: Ultra-modern architectural design with 24/7 window shopping
- **Status Symbol**: Vehicles provide prestige points and elite social standing
- **In-Game Usability**: All vehicles are fully functional within OmniWorld gameplay
- **Selective Resale Policies**: Strategic controls to maintain value and exclusivity

## Visual & Experiential Design

### Showroom Architecture
- **Location**: Prime area in OmniVegas Luxury Strip (777 Prestige Boulevard)
- **Design**: Ultra-modern glass construction with titanium frames
- **Size**: 25,000 sq ft total (12,000 sq ft showroom)
- **Capacity**: 20 display platforms with rotating pedestals
- **Features**:
  - Floor-to-ceiling glass panels
  - Dynamic LED lighting system
  - Holographic displays
  - 24/7 window shopping enabled
  - Valet parking with 50 spaces

### Lounge Zones
1. **VIP Platinum Lounge** (Floor 2)
   - Elite members only access
   - Luxury seating and private bar
   - Panoramic city views
   - Exclusive art collection

2. **Collectors Corner** (Floor 2)
   - Racing simulators
   - Vehicle history displays
   - Networking area for owners

3. **Consultation Suite** (Floor 1)
   - Private meeting rooms
   - NFT minting station
   - Smart contracts terminal
   - Financial advisory services

### Dynamic Lighting
- **Showroom**: Color-shifting dynamic lights with pulse animation
- **Display Platforms**: Focused spotlights (8.0 intensity) with rotating highlights
- **Exterior**: Accent lighting with wave patterns (sunset to sunrise)
- **Lounges**: Ambient mood lighting with subtle gradients

## Revenue Logic

### Primary Sales (Minting)
```
Base Price: Vehicle value in OMNI tokens
Minting Fee: 5% of base price
Sales Tax: 8% of base price
Total Cost: Base Price × 1.13
```

**Revenue Split**:
- Dealership: 85%
- Platform Treasury: 15%

### Secondary Sales (Resale)
```
Sale Price: Market-determined value
Royalty: 20% to original creator
Platform Fee: 5%
Seller Proceeds: 75%
```

### Strategic Asset Buybacks
- **Enabled**: Yes (configurable per vehicle tier)
- **Buyback Rate**: 75-85% of current market value
- **Purpose**: Maintain market stability and provide liquidity

### Monthly Auctions
- **Frequency**: First day of each month
- **Duration**: 48-72 hours depending on vehicle rarity
- **Featured Vehicles**: Ultra-rare and elite-status vehicles only
- **Access**: Restricted to players with minimum prestige score (0.85-0.9)
- **Bidding**:
  - Minimum increment: 5% of current bid
  - Auction fee: 10% of winning bid
  - Royalties: 20% to original creator

## System Architecture

### Core Components

#### 1. VehicleNFT.cs
Represents individual NFT vehicles with:
- **Identity**: NFT ID, token ID, contract address
- **Rarity**: Six-tier system (Common → Ultra-Legendary)
- **Economics**: Pricing, fees, royalties, appreciation
- **Ownership**: Current owner, transfer history, provenance
- **Status**: Listed, auction, resale eligibility
- **Gameplay**: In-game stats, prefab references

#### 2. AutoDealership.cs
Main dealership controller managing:
- **Inventory**: Available, sold, and display vehicles
- **Minting**: New vehicle NFT creation
- **Sales**: Purchase processing with fee calculation
- **Resale**: Secondary market listing with policy enforcement
- **Buyback**: Strategic asset repurchase program
- **Statistics**: Revenue tracking and analytics

#### 3. AuctionSystem.cs
Handles monthly elite auctions:
- **Auction Creation**: Start new auctions for ultra-rare vehicles
- **Bid Management**: Place, validate, and track bids
- **Access Control**: Prestige-based eligibility filtering
- **Auction Ending**: Winner determination and transfer
- **History**: Complete auction records and analytics

#### 4. VehicleShowroom.cs
Manages visual presentation:
- **Display Management**: 20 rotating platforms
- **Lighting Control**: Dynamic spotlight system
- **Audio System**: Ambient music and engine previews
- **Lounge Zones**: VIP area management
- **Window Shopping**: 24/7 exterior viewing

## Rarity Tiers

| Tier | Description | Production | Appreciation | Prestige | Examples |
|------|-------------|------------|--------------|----------|----------|
| **Ultra-Legendary** | 1-of-1 masterpieces | 1 unit | 1% per day | 1000 pts | Apex One, Phoenix Eternal |
| **Legendary** | Limited 10-of-10 editions | 10 units | 0.5% per day | 500 pts | Dominion GT |
| **Epic** | High-performance exclusives | 50 units | 0.2% per day | 300 pts | - |
| **Rare** | Special editions | 100 units | 0% | 200 pts | - |
| **Uncommon** | Limited production | 500 units | 0% | 150 pts | - |
| **Common** | Mass production | Unlimited | 0% | 100 pts | - |

## Selective Resale Policies

### Resale Eligibility
- **Minimum Ownership Period**: 30-90 days depending on rarity
- **Price Floor**: Minimum 80% of original purchase price
- **Price Ceiling**: Maximum 300% of original purchase price
- **Transfer Cooldown**: Prevents rapid flipping

### Policy Enforcement
```csharp
// Example: Legendary tier vehicle
MinimumOwnershipDays: 60
MinimumResalePrice: 0.8 × Purchase Price
MaximumResalePrice: 3.0 × Purchase Price
CanResell: true (configurable)
```

## Featured Vehicles

### 1-of-1 Ultra-Legendary

#### Apex One
- **Price**: 10,000,000 OMNI ($350,000 USD at $0.035/OMNI)
- **Engine**: 9.0L Twin-Turbo Hybrid V16
- **Power**: 2000 HP
- **Top Speed**: 280 mph
- **0-60**: 1.9 seconds
- **Features**:
  - AI-integrated systems
  - Quantum-tuned aerodynamics
  - Holographic displays
  - Diamond-infused carbon fiber
  - Self-optimizing performance

#### Phoenix Eternal
- **Price**: 8,500,000 OMNI ($297,500 USD)
- **Engine**: 7.0L Quad-Turbo W12 Hybrid
- **Power**: 1600 HP
- **Top Speed**: 255 mph
- **0-60**: 2.3 seconds
- **Features**:
  - Morphing body panels
  - Self-healing paint
  - Bio-sync driver interface
  - Adaptive comfort system
  - Phoenix wing spoiler

### 10-of-10 Legendary

#### Dominion GT (Editions 1-10)
- **Price**: 3,500,000 OMNI ($122,500 USD)
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

## Integration with Dominion Economy

### Price Calculation
Vehicle values integrate with the Dominion Economy quantum algorithm:

```
P_VEHICLE = Base_Price × Location_Premium × (1 + Appreciation_Rate × Days)
Total_Cost = P_VEHICLE × (1 + Minting_Fee + Sales_Tax)
```

### Dynamic Pricing Factors
- **Location Premium**: 1.5x for prime OmniVegas location
- **Demand Multiplier**: Increases with player interest
- **Appreciation**: Daily value increase for rare tiers
- **Market Conditions**: Influenced by global economy state

### Social Currency
- **Prestige Points**: Awarded based on vehicle rarity
- **Status Bonuses**: Enhanced gameplay rewards for owners
- **Elite Access**: Unlocks exclusive events and missions
- **Community Recognition**: Visible ownership badges

## Gameplay Integration

### In-Game Usage
All vehicles are fully functional with:
- **Driving Physics**: Realistic handling and performance
- **Customization**: Paint, wheels, interior, upgrades
- **Missions**: Vehicle-specific quest lines
- **Showcase Events**: Content creation and revenue-generating exhibitions
- **Fast Travel**: Elite vehicles enable special transport

### Owner Benefits

#### Ultra-Legendary (1-of-1)
- +50% race rewards
- Exclusive missions unlocked
- Fast travel enabled
- Reduced fuel consumption
- Enhanced durability
- VIP lounge access
- Priority event invitations

#### Legendary (10-of-10)
- +30% race rewards
- Dominion missions unlocked
- Enhanced performance
- Reduced maintenance costs
- Elite parking access
- Club membership
- Annual gatherings

## Usage Examples

### Minting a New Vehicle
```csharp
// Create Ultra-Legendary vehicle
VehicleNFT apexOne = AutoDealership.Instance.MintVehicle(
    vehicleName: "Apex One",
    tier: RarityTier.UltraLegendary,
    edition: 1,
    total: 1,
    basePrice: 10000000f,
    minterAddress: "0x123..."
);
```

### Purchasing a Vehicle
```csharp
// Buy vehicle from dealership
bool success = AutoDealership.Instance.PurchaseVehicle(
    nftId: "OMNI-AUTO-ULTRALEGENDARY-ABC123",
    buyerAddress: "0x456...",
    buyerPrestige: 0.95f
);
```

### Creating an Auction
```csharp
// Start auction for ultra-rare vehicle
VehicleAuction auction = AuctionSystem.Instance.CreateAuction(
    vehicle: phoenixEternal,
    startingBid: 7000000f,
    eliteOnly: true
);
```

### Placing a Bid
```csharp
// Bid on active auction
bool bidPlaced = AuctionSystem.Instance.PlaceBid(
    auctionId: "AUCTION-12345",
    bidderAddress: "0x789...",
    bidAmount: 7500000f,
    bidderPrestige: 0.92f
);
```

### Displaying in Showroom
```csharp
// Add vehicle to showroom display
VehicleShowroom.Instance.DisplayVehicle(
    vehicle: dominionGT,
    platformIndex: 5
);
```

## File Structure

```
Assets/
├── Scripts/
│   └── World/
│       ├── AutoDealership.cs       # Main dealership controller
│       ├── VehicleNFT.cs           # NFT vehicle data model
│       ├── AuctionSystem.cs        # Auction management
│       └── VehicleShowroom.cs      # Showroom visualization
│
├── Prefabs/
│   ├── Buildings/
│   │   └── AutoDealership_Showroom.json   # Dealership building
│   │
│   └── Vehicles/
│       └── Cars/
│           ├── NFT_ApexOne_1of1.json         # Ultra-Legendary
│           ├── NFT_PhoenixEternal_1of1.json  # Ultra-Legendary
│           └── NFT_DominionGT_10of10.json    # Legendary
│
└── Docs/
    └── AUTO_DEALERSHIP.md          # This documentation
```

## Future Enhancements

### 3D Modeling Phase
- [ ] Create .obj files for showroom building
- [ ] Create .obj files for each vehicle model
- [ ] High-poly models for LOD0 (close-up viewing)
- [ ] Optimized models for LOD1-4 (distance viewing)

### Unity Prefabs
- [ ] Create .prefab files for showroom with proper components
- [ ] Create .prefab files for each vehicle with physics
- [ ] Setup vehicle controller scripts
- [ ] Configure audio sources and particle systems
- [ ] Setup collision meshes and wheel colliders

### Additional Content
- [ ] Create more 1-of-1 vehicles (target: 10 total)
- [ ] Create more 10-of-10 editions (target: 10 series)
- [ ] Add Epic tier vehicles (50 editions each)
- [ ] Interior apartment/home configurations
- [ ] Garage and storage systems

### Blockchain Integration
- [ ] Deploy NFT smart contracts to Polygon
- [ ] Implement on-chain minting
- [ ] Setup royalty payment automation
- [ ] Create auction smart contracts
- [ ] Integrate with WalletConnect

### Enhanced Features
- [ ] Virtual test drive system
- [ ] Vehicle customization studio
- [ ] Trade-in evaluation system
- [ ] Insurance and warranty programs
- [ ] Collector achievement system
- [ ] Social sharing and galleries

## API Reference

### AutoDealership Methods

- `MintVehicle()` - Create new NFT vehicle
- `PurchaseVehicle()` - Buy vehicle from dealership
- `ListVehicleForResale()` - List on secondary market
- `OfferBuyback()` - Get buyback offer
- `ExecuteBuyback()` - Complete buyback transaction
- `AddToShowroom()` - Display vehicle in showroom
- `GetVehiclesByRarity()` - Filter vehicles by tier
- `GetUltraRareVehicles()` - Get 1-of-1 and Legendary vehicles
- `GetStatistics()` - Dealership analytics

### AuctionSystem Methods

- `CreateAuction()` - Start new auction
- `PlaceBid()` - Submit bid on auction
- `EndAuction()` - Close auction and transfer vehicle
- `StartMonthlyAuction()` - Begin monthly event
- `GetActiveEliteAuctions()` - List current auctions
- `GetVehicleAuctionHistory()` - Past auction records

### VehicleShowroom Methods

- `DisplayVehicle()` - Show vehicle on platform
- `RemoveVehicle()` - Clear platform
- `GetDisplayedVehicle()` - Query displayed vehicle
- `SetWindowShoppingMode()` - Toggle 24/7 viewing
- `PlayEngineSoundPreview()` - Audio preview

## Configuration

### Economic Settings
```csharp
baseMintingFee = 0.05f;          // 5%
salesTaxRate = 0.08f;            // 8%
royaltyPercent = 0.20f;          // 20%
buybackPercentage = 0.75f;       // 75%
```

### Auction Settings
```csharp
minimumPrestigeScore = 0.8f;     // Elite access
auctionDurationHours = 48;       // 2 days
minimumBidIncrement = 0.05f;     // 5%
auctionFeePercent = 0.10f;       // 10%
```

### Resale Policy
```csharp
minimumOwnershipDays = 30-90;           // Tier-based
maximumResalePriceMultiplier = 3.0f;   // 300% max
```

## Support & Contact

- **Documentation**: `/Docs/AUTO_DEALERSHIP.md`
- **Issues**: GitHub Issues
- **Community**: Discord #auto-dealership channel
- **Email**: autodealership@omniworld.io

---

**Version**: 1.0.0  
**Last Updated**: 2025-01-23  
**Author**: Omega Prime  
**Status**: Phase 1 - Core Implementation Complete
