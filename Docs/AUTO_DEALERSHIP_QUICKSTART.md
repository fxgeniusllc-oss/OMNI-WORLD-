# OmniWorld Auto Dealership - Quick Start Guide

## Installation & Setup

The Auto Dealership system is fully integrated into OmniWorld. No additional installation required.

## Quick Start (5 Minutes)

### 1. Run the Demo

```csharp
// In Unity Editor, create a new GameObject and attach AutoDealershipDemo
var demo = gameObject.AddComponent<AutoDealershipDemo>();
demo.runDemoOnStart = true;
demo.testPlayerBalance = 50000000f; // 50M OMNI for testing
demo.testPrestigeScore = 0.95f;

// Press Play to see full system demonstration
```

### 2. Basic Usage

#### Mint a Vehicle

```csharp
using OmniWorld.World;

// Get dealership instance
var dealership = AutoDealership.Instance;

// Mint Ultra-Legendary 1-of-1
var vehicle = dealership.MintVehicle(
    vehicleName: "My Dream Car",
    tier: RarityTier.UltraLegendary,
    edition: 1,
    total: 1,
    basePrice: 10000000f,
    minterAddress: "0x123..."
);

Debug.Log($"Minted: {vehicle.vehicleName} - {vehicle.nftId}");
```

#### Purchase a Vehicle

```csharp
// Get economy integration
var integration = DealershipEconomyIntegration.Instance;

// Process transaction
bool success = integration.ProcessPurchaseTransaction(
    vehicle,
    buyerAddress: "0x456...",
    playerBalance: 15000000f
);

if (success) {
    dealership.PurchaseVehicle(
        vehicle.nftId,
        "0x456...",
        buyerPrestige: 0.85f
    );
}
```

#### Create an Auction

```csharp
// Get auction system
var auctions = AuctionSystem.Instance;

// Create auction for ultra-rare vehicle
var auction = auctions.CreateAuction(
    vehicle: myVehicle,
    startingBid: 8000000f,
    eliteOnly: true
);

Debug.Log($"Auction created: {auction.auctionId}");
```

#### Place a Bid

```csharp
// Place bid on active auction
bool bidPlaced = auctions.PlaceBid(
    auctionId: "AUCTION-12345",
    bidderAddress: "0x789...",
    bidAmount: 8500000f,
    bidderPrestige: 0.95f
);
```

#### Display in Showroom

```csharp
// Get showroom
var showroom = VehicleShowroom.Instance;

// Display vehicle on platform 5
showroom.DisplayVehicle(myVehicle, platformIndex: 5);
```

## Common Patterns

### Loading NFT Vehicles from JSON

```csharp
// Load vehicle configuration
string jsonPath = "Assets/Prefabs/Vehicles/Cars/NFT_ApexOne_1of1.json";
string jsonData = File.ReadAllText(jsonPath);
var vehicleData = JsonUtility.FromJson<VehicleConfig>(jsonData);

// Create NFT from configuration
var vehicle = new VehicleNFT(
    vehicleData.branding.model,
    ParseRarityTier(vehicleData.metadata.rarityTier),
    vehicleData.metadata.edition,
    vehicleData.metadata.totalEditions
);

// Set properties from JSON
vehicle.horsepower = vehicleData.specifications.horsepower;
vehicle.topSpeed = vehicleData.specifications.topSpeed;
vehicle.mintingPrice = vehicleData.nftProperties.mintingPrice;
```

### Checking Elite Status

```csharp
// Check if vehicle qualifies for elite auctions
if (vehicle.IsEliteStatus()) {
    Debug.Log("Vehicle is Ultra-Legendary or 1-of-1 Legendary");
    // Can participate in monthly elite auctions
}
```

### Resale Policy Enforcement

```csharp
// List vehicle for resale
bool canList = dealership.ListVehicleForResale(
    vehicle,
    askingPrice: 12000000f
);

if (!canList) {
    // Check reasons:
    // - Minimum ownership period not met
    // - Price below floor or above ceiling
    // - Resale not allowed for this tier
}
```

### Dynamic Pricing

```csharp
// Get current market price with all modifiers
float currentPrice = integration.CalculateVehiclePrice(vehicle);

// Price includes:
// - Base minting price
// - Location premium (1.5x for prime areas)
// - Demand multiplier
// - Appreciation over time (rare vehicles)
// - Dominion Economy factors
```

### Get Statistics

```csharp
// Dealership statistics
var stats = dealership.GetStatistics();
Debug.Log($"Total Sales: {stats.totalSales}");
Debug.Log($"Revenue: {stats.totalRevenue:N0} OMNI");
Debug.Log($"Average Price: {stats.averageSalePrice:N0} OMNI");

// Integration statistics
var integStats = integration.GetStatistics();
Debug.Log($"Token Price: ${integStats.currentTokenPrice:F4} USD");
Debug.Log($"Transactions: {integStats.transactionsProcessed}");
Debug.Log($"Burned: {integStats.totalBurned:N0} OMNI");
```

## Key Classes

### VehicleNFT
**Properties:**
- `nftId` - Unique identifier
- `rarityTier` - Rarity level (Common → Ultra-Legendary)
- `mintingPrice` - Original mint price
- `currentValue` - Current market value
- `currentOwner` - Wallet address
- `prestigePoints` - Status symbol value

**Methods:**
- `CalculateTotalPurchasePrice()` - Get full cost with fees
- `CalculateRoyalty(salePrice)` - Calculate resale royalty
- `UpdateMarketValue(demandMultiplier)` - Adjust value
- `TransferOwnership(newOwner, salePrice)` - Transfer vehicle
- `IsEliteStatus()` - Check if elite-tier

### AutoDealership
**Methods:**
- `MintVehicle(...)` - Create new NFT
- `PurchaseVehicle(nftId, buyer, prestige)` - Buy vehicle
- `ListVehicleForResale(vehicle, price)` - Secondary market
- `OfferBuyback(vehicle)` - Get buyback offer
- `ExecuteBuyback(vehicle, seller)` - Complete buyback
- `AddToShowroom(vehicle)` - Display in showroom
- `GetVehiclesByRarity(tier)` - Filter by rarity
- `GetUltraRareVehicles()` - Get elite vehicles
- `GetStatistics()` - Dealership analytics

### AuctionSystem
**Methods:**
- `CreateAuction(vehicle, startingBid, eliteOnly)` - Start auction
- `PlaceBid(auctionId, bidder, amount, prestige)` - Submit bid
- `EndAuction(auction)` - Close and transfer
- `StartMonthlyAuction(vehicles)` - Monthly event
- `GetActiveEliteAuctions()` - List current auctions
- `GetVehicleAuctionHistory(nftId)` - Past auctions

### VehicleShowroom
**Methods:**
- `DisplayVehicle(vehicle, platformIndex)` - Show vehicle
- `RemoveVehicle(platformIndex)` - Clear platform
- `GetDisplayedVehicle(platformIndex)` - Query display
- `SetWindowShoppingMode(enabled)` - Toggle 24/7 viewing
- `PlayEngineSoundPreview(vehicle, sound)` - Audio preview

### DealershipEconomyIntegration
**Methods:**
- `CalculateVehiclePrice(vehicle)` - Dynamic pricing
- `ProcessPurchaseTransaction(...)` - Handle purchase
- `ProcessResaleTransaction(...)` - Handle resale
- `ProcessAuctionTransaction(...)` - Handle auction
- `UpdateDemandMultiplier(viewers, sales)` - Market demand
- `GetCurrentTokenPrice()` - OMNI price
- `ConvertToUSD(omniAmount)` - Currency conversion
- `ConvertToOMNI(usdAmount)` - Currency conversion
- `GetStatistics()` - Integration analytics

## Events

Subscribe to events for real-time updates:

```csharp
// Token price updates
DominionEconomy.Instance.OnTokenPriceUpdated += (newPrice) => {
    Debug.Log($"Token price: ${newPrice}");
};

// Transactions
DominionEconomy.Instance.OnTransactionProcessed += (type, amount) => {
    Debug.Log($"Transaction: {type} - {amount} OMNI");
};

// Auctions
AuctionSystem.Instance.OnAuctionStarted += (auction) => {
    Debug.Log($"Auction started: {auction.vehicle.vehicleName}");
};

AuctionSystem.Instance.OnBidPlaced += (auction, bidder, amount) => {
    Debug.Log($"New bid: {amount} OMNI");
};

AuctionSystem.Instance.OnAuctionEnded += (auction) => {
    Debug.Log($"Winner: {auction.winner}");
};
```

## Configuration

### Economic Settings
Edit in Unity Inspector on AutoDealership component:
- `baseMintingFee` - Default 5%
- `salesTaxRate` - Default 8%
- `buybackPercentage` - Default 75%
- `selectiveResalePolicy` - Enable/disable restrictions

### Auction Settings
Edit on AuctionSystem component:
- `minimumPrestigeScore` - Default 0.8
- `auctionDurationHours` - Default 48
- `minimumBidIncrement` - Default 5%
- `auctionFeePercent` - Default 10%

### Showroom Settings
Edit on VehicleShowroom component:
- `dynamicLighting` - Enable/disable
- `spotlightIntensity` - 0-10 range
- `rotatingPlatforms` - Enable/disable
- `windowShoppingEnabled` - 24/7 viewing

## Troubleshooting

### "Vehicle not found in inventory"
Make sure the vehicle was minted and is in `availableVehicles` list.

### "Insufficient balance"
Check player balance is greater than total cost including fees and burn.

### "Prestige score too low"
Elite auctions require minimum prestige (0.8-0.9 depending on vehicle).

### "Minimum ownership period not met"
Ultra-Legendary: 90 days, Legendary: 60 days, others: 30 days.

### "Integration not available"
Ensure DominionEconomy and AutoDealership are initialized.

## Best Practices

1. **Always use DealershipEconomyIntegration** for transactions
2. **Check prestige scores** before auction participation
3. **Validate balances** before processing purchases
4. **Update market values** regularly for rare vehicles
5. **Monitor demand multipliers** to adjust pricing
6. **Subscribe to events** for real-time updates
7. **Use showroom** to feature rare vehicles
8. **Schedule monthly auctions** for ultra-rare vehicles

## Resources

- **Full Documentation**: `/Docs/AUTO_DEALERSHIP.md`
- **Vehicle Catalog**: `/Assets/Prefabs/Vehicles/NFT_CATALOG.md`
- **Demo Script**: `/Assets/Scripts/Examples/AutoDealershipDemo.cs`
- **API Reference**: See AUTO_DEALERSHIP.md

## Support

- **GitHub Issues**: Bug reports and feature requests
- **Discord**: #auto-dealership channel
- **Email**: autodealership@omniworld.io

---

**Version**: 1.0.0
**Last Updated**: 2025-01-23
