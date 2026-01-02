# OmniLux Auto Dealership System

## Overview

OmniLux Auto is a premier vehicle dealership located in the Vegas Strip Zone of OmniVegas. It specializes in ultra-rare, NFT-mintable vehicles with a focus on 1-of-1 and limited edition supercars and hypercars.

## Location

- **Dealership Name**: OmniLux Auto
- **Location**: Vegas Strip Zone
- **City**: OmniVegas
- **Zone Type**: Commercial
- **Operating Hours**: 24/7

## Key Features

### 1. Glass Facade for 24/7 Window Shopping
The dealership features a stunning glass facade allowing potential buyers to view the exclusive inventory at any time, creating a continuous showcase of luxury vehicles.

### 2. NFT Mintable Vehicles
All exclusive vehicles are NFT-compatible using ERC-721 standard:
- Unique 1-of-1 editions
- Limited 10-of-10 series
- 20% perpetual royalties on secondary sales
- On-chain ownership verification

### 3. Monthly Auction Floor
- **Schedule**: Monthly auctions on the 1st of each month
- **Duration**: 72 hours
- **Eligibility**: VIP tier wallets only (Platinum, Diamond, Elite)
- **Minimum Prestige**: 0.8 (80% governance score)
- **Visibility**: Global livestream with real-time bidding
- **Features**:
  - Live bidding system
  - Bid history tracking
  - Automated outbid notifications
  - Escrow smart contracts
  - Post-auction NFT minting

### 4. VIP Showroom Access
Exclusive access for high-prestige wallets with special viewing privileges and priority purchasing.

### 5. Vehicle Inspection and Walkaround Mode
- **Cost**: 50 OMNI
- **Features**:
  - Full 360° walkaround view
  - Interior inspection
  - Engine bay examination
  - Performance metrics display

## Exclusive Vehicle Inventory

### Aether Phantom GT (1-of-1)
- **Rarity**: Ultra-Legendary (1-of-1)
- **Price**: 3,750,000 OMNI (12.5 ETH)
- **Class**: Hypercar
- **Engine**: Hybrid Electric Twin-Turbo V8
- **Horsepower**: 1,800 HP
- **Top Speed**: 280 mph
- **0-60**: 1.8 seconds
- **Special Features**:
  - Quad-Motor AWD system
  - 120 kWh battery capacity
  - Electric stealth mode
  - Holographic dashboard
  - Butterfly doors

### Stratos Lynx V (10-of-10)
- **Rarity**: Legendary (10-of-10)
- **Price**: 1,200,000 OMNI (4.0 ETH)
- **Class**: Supercar
- **Engine**: 6.2L Supercharged V10
- **Horsepower**: 950 HP
- **Top Speed**: 235 mph
- **0-60**: 2.5 seconds
- **Special Features**:
  - Supercharged V10 engine
  - Carbon fiber body
  - Scissor doors
  - Racing suspension
  - Edition numbered (X of 10)

## Standard Vehicle Inventory

OmniLux Auto also carries standard luxury and performance vehicles:
- Hypercar
- Supercar
- ElectricRoadster
- RacingPedigreeCoupe
- UltraLuxCoupe
- PrestigeSedan
- LuxuryElectricSUV
- LuxuryLimoSUV
- GrandTourer

## Services

### Vehicle Inspection
- **Cost**: 50 OMNI
- Full 360° walkaround
- Interior view
- Engine inspection
- Performance metrics

### Test Drive
- **Cost**: 100 OMNI
- **Duration**: 5 minutes
- Track available for performance testing

### Customization
- **Categories**: Paint, Wheels, Interior, Performance, Aero
- NFT-compatible parts
- On-chain upgrade logging

### Financing
- **Down Payment**: 20%
- **Max Term**: 36 months
- **Interest Rate**: 5.5% APR
- Smart contract-based payment system

## Economic Integration

### Currency
- Primary: $OMNI tokens
- Secondary: ETH (accepted for exclusive vehicles)

### Transaction Fees
- **Transaction Fee**: 1.5%
- **Dealer Commission**: 5%

### DominionEconomy Integration
All transactions are processed through the DominionEconomy system:
- Real-time price calculations
- Transaction burn mechanics (0.5%)
- Circulation coefficient updates
- On-chain logging for NFT vehicles

## Urban Zone Coverage

OmniLux Auto serves customers from all OmniVegas zones:
- OmniDowntown
- OmniHollywood
- OmniCoastline
- OmniSuburbs
- OmniSouthside
- OmniDesert

## Modular Systems Integration

The dealership integrates with the following modular systems:

### VehicleModShopManager
Located at OmniSpeedWorks garage for post-purchase modifications:
- Engine upgrades
- Suspension tuning
- Aerodynamic packages
- NFT part compatibility

### RaceEventSpawner
Racing events for dealership customers:
- Street races
- Track competitions
- Drift events
- Time attacks

### CityTransitManager
Vehicle delivery and transit:
- Fast travel to dealership
- Vehicle pickup services
- Cross-zone delivery

### DynamicZoneDetector
Zone-based vehicle availability:
- Zone-specific inventory
- Regional pricing adjustments
- Local demand tracking

## Database Schema

### MongoDB Collections

#### Vehicle Purchase History
```json
{
  "vehicleId": "001",
  "vehicleName": "Aether Phantom GT",
  "buyer": "0x...",
  "price": 3750000,
  "currency": "OMNI",
  "purchaseDate": "2025-12-23T14:00:00Z",
  "nftTokenId": "...",
  "transactionHash": "0x..."
}
```

#### Auction Bid History
```json
{
  "auctionId": "001",
  "vehicleId": "001",
  "bidder": "0x...",
  "bidAmount": 3900000,
  "bidTime": "2025-12-23T14:00:00Z",
  "vipTier": "Diamond",
  "prestige": 0.85
}
```

#### Vehicle Upgrade History
```json
{
  "vehicleId": "001",
  "upgradeType": "Engine",
  "upgradeName": "Stage 2 Tune",
  "cost": 50000,
  "installDate": "2025-12-23T14:00:00Z",
  "onChainLog": true,
  "transactionHash": "0x..."
}
```

## Smart Contract Integration

### NFT Minting
- **Standard**: ERC-721
- **Royalty**: EIP-2981 (20%)
- **Metadata**: IPFS storage
- **Attributes**: Performance stats, rarity, edition number

### Auction System
- Escrow-based bidding
- Automated winner determination
- Instant NFT transfer on completion
- Refund mechanism for outbid participants

### Upgrade Logging
- On-chain modification history
- Verifiable upgrade authenticity
- NFT metadata updates
- Value appreciation tracking

## API Endpoints

### Purchase Vehicle
```csharp
VehicleDealershipManager.Instance.PurchaseVehicle(
    vehicleId: "001",
    walletAddress: "0x...",
    payWithEth: false
)
```

### Inspect Vehicle
```csharp
VehicleDealershipManager.Instance.InspectVehicle(
    vehicleId: "001",
    walletAddress: "0x..."
)
```

### Place Auction Bid
```csharp
AuctionManager.Instance.PlaceBid(
    walletAddress: "0x...",
    bidAmount: 3900000,
    vipTier: "Diamond",
    userPrestige: 0.85f
)
```

### Calculate Financing
```csharp
FinancingTerms terms = VehicleDealershipManager.Instance.CalculateFinancing(
    vehicleId: "001",
    termMonths: 36
)
```

## Future Enhancements

### Planned Features
1. VR showroom for immersive vehicle viewing
2. AI sales assistant with personalized recommendations
3. Trade-in system for used vehicles
4. Collaborative ownership (fractional NFTs)
5. Cross-city dealership network
6. Vehicle leasing with rent-to-own options
7. Insurance marketplace integration
8. Vehicle history and provenance tracking

### Prefabs Needed
- Dealership prefab with glass facade
- Interior showroom with display platforms
- Auction floor with bidding stations
- Test track facility
- Service bay for inspections
- VIP lounge area

## Technical Implementation

### Scripts
- `VehicleDealershipManager.cs` - Main dealership operations
- `AuctionManager.cs` - Auction system management
- `VehicleModShopManager.cs` - Modification shop integration
- `RaceEventSpawner.cs` - Racing event integration

### Namespace Structure
```csharp
namespace OmniWorld.Vehicles
{
    // Vehicle-specific systems
}
```

### Dependencies
- `OmniWorld.Economy.DominionEconomy` - Economic transactions
- `OmniWorld.World.ZoneController` - Zone management
- `OmniWorld.Web3.ContractBridge` - Blockchain integration (future)

## Testing Checklist

- [ ] Vehicle purchase with OMNI
- [ ] Vehicle purchase with ETH
- [ ] NFT minting on purchase
- [ ] Auction start and bid placement
- [ ] VIP eligibility verification
- [ ] Auction winner determination
- [ ] Vehicle inspection service
- [ ] Test drive scheduling
- [ ] Financing calculation
- [ ] Transaction fee calculation
- [ ] DominionEconomy integration
- [ ] Zone-based availability
- [ ] Upgrade logging

## Conclusion

OmniLux Auto represents the pinnacle of luxury vehicle ownership in OmniWorld, combining cutting-edge NFT technology with a premium automotive experience. The 24/7 glass facade dealership on the Vegas Strip offers unparalleled access to the world's most exclusive vehicles, backed by a sophisticated auction system and comprehensive suite of services.
