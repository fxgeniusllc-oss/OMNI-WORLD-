using UnityEngine;

namespace OmniWorld.Examples
{
    /// <summary>
    /// AutoDealershipDemo - Example usage of the Auto Dealership system
    /// Demonstrates minting, purchasing, auctions, and showroom features
    /// </summary>
    public class AutoDealershipDemo : MonoBehaviour
    {
        [Header("Demo Configuration")]
        public bool runDemoOnStart = false;
        public bool enableDebugLogs = true;
        
        [Header("Test Data")]
        public string testWalletAddress = "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb1";
        public float testPlayerBalance = 50000000f; // 50M OMNI for testing
        public float testPrestigeScore = 0.95f;
        
        private void Start()
        {
            if (runDemoOnStart)
            {
                RunFullDemo();
            }
        }
        
        /// <summary>
        /// Run complete Auto Dealership demonstration
        /// </summary>
        public void RunFullDemo()
        {
            Debug.Log("╔═══════════════════════════════════════════════════════╗");
            Debug.Log("║     OmniWorld Auto Dealership - System Demo          ║");
            Debug.Log("╚═══════════════════════════════════════════════════════╝");
            Debug.Log("");
            
            DemoMintingVehicles();
            Debug.Log("");
            
            DemoPurchaseVehicle();
            Debug.Log("");
            
            DemoAuctionSystem();
            Debug.Log("");
            
            DemoShowroomDisplay();
            Debug.Log("");
            
            DemoEconomyIntegration();
            Debug.Log("");
            
            DemoStatistics();
            
            Debug.Log("");
            Debug.Log("╔═══════════════════════════════════════════════════════╗");
            Debug.Log("║              Demo Complete!                           ║");
            Debug.Log("╚═══════════════════════════════════════════════════════╝");
        }
        
        /// <summary>
        /// Demonstrate vehicle minting
        /// </summary>
        public void DemoMintingVehicles()
        {
            Debug.Log(">>> DEMO 1: Minting NFT Vehicles");
            Debug.Log("─────────────────────────────────────────────────────");
            
            var dealership = World.AutoDealership.Instance;
            
            // Mint Ultra-Legendary 1-of-1
            Debug.Log("\n[1] Minting Ultra-Legendary 1-of-1 Vehicle...");
            var apexOne = dealership.MintVehicle(
                vehicleName: "Apex One",
                tier: World.RarityTier.UltraLegendary,
                edition: 1,
                total: 1,
                basePrice: 10000000f,
                minterAddress: testWalletAddress
            );
            
            // Mint Legendary 10-of-10
            Debug.Log("\n[2] Minting Legendary 10-of-10 Vehicle (Edition 1)...");
            var dominionGT = dealership.MintVehicle(
                vehicleName: "Dominion GT Edition 1",
                tier: World.RarityTier.Legendary,
                edition: 1,
                total: 10,
                basePrice: 3500000f,
                minterAddress: testWalletAddress
            );
            
            // Mint another Legendary edition
            Debug.Log("\n[3] Minting Legendary 10-of-10 Vehicle (Edition 2)...");
            var dominionGT2 = dealership.MintVehicle(
                vehicleName: "Dominion GT Edition 2",
                tier: World.RarityTier.Legendary,
                edition: 2,
                total: 10,
                basePrice: 3500000f,
                minterAddress: testWalletAddress
            );
            
            Debug.Log($"\n✓ Minted {dealership.totalVehiclesMinted} vehicles");
            Debug.Log($"✓ Available inventory: {dealership.availableVehicles.Count}");
        }
        
        /// <summary>
        /// Demonstrate vehicle purchase
        /// </summary>
        public void DemoPurchaseVehicle()
        {
            Debug.Log(">>> DEMO 2: Purchasing a Vehicle");
            Debug.Log("─────────────────────────────────────────────────────");
            
            var dealership = World.AutoDealership.Instance;
            var integration = World.DealershipEconomyIntegration.Instance;
            
            if (dealership.availableVehicles.Count == 0)
            {
                Debug.LogWarning("No vehicles available. Run DemoMintingVehicles first.");
                return;
            }
            
            // Get first available vehicle
            var vehicle = dealership.availableVehicles[0];
            
            Debug.Log($"\n[1] Selected Vehicle: {vehicle.vehicleName}");
            Debug.Log($"    Rarity: {vehicle.rarityTier}");
            Debug.Log($"    Edition: {vehicle.editionNumber} of {vehicle.totalEditions}");
            
            // Calculate pricing
            float dynamicPrice = integration.CalculateVehiclePrice(vehicle);
            Debug.Log($"\n[2] Price Calculation:");
            Debug.Log($"    Base Price: {vehicle.mintingPrice:N0} OMNI");
            Debug.Log($"    Dynamic Price: {dynamicPrice:N0} OMNI");
            Debug.Log($"    Total Cost: {vehicle.CalculateTotalPurchasePrice():N0} OMNI");
            
            // Process transaction
            Debug.Log($"\n[3] Processing Purchase...");
            Debug.Log($"    Buyer: {testWalletAddress}");
            Debug.Log($"    Balance: {testPlayerBalance:N0} OMNI");
            
            bool transactionSuccess = integration.ProcessPurchaseTransaction(
                vehicle, 
                testWalletAddress, 
                testPlayerBalance
            );
            
            if (transactionSuccess)
            {
                bool purchaseSuccess = dealership.PurchaseVehicle(
                    vehicle.nftId,
                    testWalletAddress,
                    testPrestigeScore
                );
                
                if (purchaseSuccess)
                {
                    Debug.Log("\n✓ Purchase completed successfully!");
                }
            }
        }
        
        /// <summary>
        /// Demonstrate auction system
        /// </summary>
        public void DemoAuctionSystem()
        {
            Debug.Log(">>> DEMO 3: Monthly Auction System");
            Debug.Log("─────────────────────────────────────────────────────");
            
            var dealership = World.AutoDealership.Instance;
            var auctionSystem = World.AuctionSystem.Instance;
            
            // Get ultra-rare vehicles
            var ultraRareVehicles = dealership.GetUltraRareVehicles();
            
            if (ultraRareVehicles.Count == 0)
            {
                Debug.LogWarning("No ultra-rare vehicles available for auction.");
                return;
            }
            
            Debug.Log($"\n[1] Found {ultraRareVehicles.Count} ultra-rare vehicles");
            
            var vehicle = ultraRareVehicles[0];
            float startingBid = vehicle.currentValue * 0.8f;
            
            Debug.Log($"\n[2] Creating Auction:");
            Debug.Log($"    Vehicle: {vehicle.vehicleName}");
            Debug.Log($"    Starting Bid: {startingBid:N0} OMNI");
            Debug.Log($"    Elite Only: Yes");
            
            var auction = auctionSystem.CreateAuction(vehicle, startingBid, eliteOnly: true);
            
            if (auction != null)
            {
                Debug.Log($"\n[3] Auction Created: {auction.auctionId}");
                Debug.Log($"    Duration: {auctionSystem.auctionDurationHours} hours");
                Debug.Log($"    End Time: {auction.endTime}");
                
                // Simulate bids
                Debug.Log("\n[4] Simulating Bids...");
                
                string bidder1 = "0x111111111111111111111111111111111111111";
                float bid1 = startingBid * 1.1f;
                bool bid1Success = auctionSystem.PlaceBid(auction.auctionId, bidder1, bid1, 0.95f);
                string bidder1Short = bidder1.Length >= 10 ? bidder1.Substring(0, 10) : bidder1;
                Debug.Log($"    Bid 1: {bid1:N0} OMNI from {bidder1Short}... - {(bid1Success ? "✓" : "✗")}");
                
                string bidder2 = "0x222222222222222222222222222222222222222";
                float bid2 = bid1 * 1.1f;
                bool bid2Success = auctionSystem.PlaceBid(auction.auctionId, bidder2, bid2, 0.92f);
                string bidder2Short = bidder2.Length >= 10 ? bidder2.Substring(0, 10) : bidder2;
                Debug.Log($"    Bid 2: {bid2:N0} OMNI from {bidder2Short}... - {(bid2Success ? "✓" : "✗")}");
                
                Debug.Log($"\n✓ Auction active with {auction.bidCount} bids");
                Debug.Log($"✓ Leading bid: {auction.currentBid:N0} OMNI");
            }
        }
        
        /// <summary>
        /// Demonstrate showroom display
        /// </summary>
        public void DemoShowroomDisplay()
        {
            Debug.Log(">>> DEMO 4: Showroom Display System");
            Debug.Log("─────────────────────────────────────────────────────");
            
            var dealership = World.AutoDealership.Instance;
            var showroom = World.VehicleShowroom.Instance;
            
            if (showroom == null)
            {
                Debug.LogWarning("Showroom not initialized. Creating instance...");
                GameObject showroomObj = new GameObject("VehicleShowroom");
                showroom = showroomObj.AddComponent<World.VehicleShowroom>();
            }
            
            Debug.Log("\n[1] Showroom Configuration:");
            Debug.Log($"    Design: {(showroom.glassShowroomDesign ? "Ultra-Modern Glass" : "Standard")}");
            Debug.Log($"    Location: {(showroom.primeLocation ? "Prime Area" : "Standard")}");
            Debug.Log($"    Dynamic Lighting: {(showroom.dynamicLighting ? "ENABLED" : "DISABLED")}");
            Debug.Log($"    24/7 Window Shopping: {(showroom.windowShoppingEnabled ? "ENABLED" : "DISABLED")}");
            Debug.Log($"    VIP Lounges: {(showroom.hasLoungeZones ? showroom.loungeCount + " zones" : "None")}");
            
            // Add vehicles to showroom
            Debug.Log("\n[2] Adding Vehicles to Display:");
            int displayCount = Mathf.Min(dealership.availableVehicles.Count, 3);
            for (int i = 0; i < displayCount; i++)
            {
                var vehicle = dealership.availableVehicles[i];
                bool added = dealership.AddToShowroom(vehicle);
                Debug.Log($"    Platform {i + 1}: {vehicle.vehicleName} - {(added ? "✓" : "✗")}");
            }
            
            Debug.Log($"\n✓ Showroom displaying {dealership.displayVehicles.Count} vehicles");
        }
        
        /// <summary>
        /// Demonstrate economy integration
        /// </summary>
        public void DemoEconomyIntegration()
        {
            Debug.Log(">>> DEMO 5: Dominion Economy Integration");
            Debug.Log("─────────────────────────────────────────────────────");
            
            var integration = World.DealershipEconomyIntegration.Instance;
            
            Debug.Log("\n[1] Integration Status:");
            Debug.Log($"    Connected: {(integration.isIntegrated ? "YES" : "NO")}");
            Debug.Log($"    Dynamic Pricing: {(integration.useDynamicPricing ? "ENABLED" : "DISABLED")}");
            Debug.Log($"    Location Premium: {integration.locationPremium}x");
            Debug.Log($"    Demand Multiplier: {integration.demandMultiplier}x");
            
            var stats = integration.GetStatistics();
            Debug.Log("\n[2] Economic Statistics:");
            Debug.Log($"    Current Token Price: ${stats.currentTokenPrice:F4} USD");
            Debug.Log($"    Transactions Processed: {stats.transactionsProcessed}");
            Debug.Log($"    Total Burned: {stats.totalBurned:N0} OMNI");
            Debug.Log($"    Burn Rate: {stats.burnRate * 100:F2}%");
            
            // Test price conversions
            Debug.Log("\n[3] Price Conversions:");
            float testOmni = 1000000f;
            float usdValue = integration.ConvertToUSD(testOmni);
            Debug.Log($"    {testOmni:N0} OMNI = ${usdValue:N2} USD");
            
            float testUsd = 10000f;
            float omniValue = integration.ConvertToOMNI(testUsd);
            Debug.Log($"    ${testUsd:N2} USD = {omniValue:N0} OMNI");
        }
        
        /// <summary>
        /// Demonstrate statistics and analytics
        /// </summary>
        public void DemoStatistics()
        {
            Debug.Log(">>> DEMO 6: Dealership Statistics");
            Debug.Log("─────────────────────────────────────────────────────");
            
            var dealership = World.AutoDealership.Instance;
            var stats = dealership.GetStatistics();
            
            Debug.Log("\n[1] Inventory Statistics:");
            Debug.Log($"    Total Vehicles Minted: {stats.totalVehicles}");
            Debug.Log($"    Available for Sale: {stats.availableVehicles}");
            Debug.Log($"    Sold Vehicles: {stats.soldVehicles}");
            Debug.Log($"    Showroom Display: {stats.showroomVehicles}");
            
            Debug.Log("\n[2] Sales Statistics:");
            Debug.Log($"    Total Sales: {stats.totalSales}");
            Debug.Log($"    Total Revenue: {stats.totalRevenue:N0} OMNI");
            Debug.Log($"    Average Sale Price: {stats.averageSalePrice:N0} OMNI");
            Debug.Log($"    Total Royalties Paid: {stats.totalRoyalties:N0} OMNI");
            
            // Rarity breakdown
            Debug.Log("\n[3] Inventory by Rarity:");
            var ultraLegendary = dealership.GetVehiclesByRarity(World.RarityTier.UltraLegendary);
            var legendary = dealership.GetVehiclesByRarity(World.RarityTier.Legendary);
            var epic = dealership.GetVehiclesByRarity(World.RarityTier.Epic);
            
            Debug.Log($"    Ultra-Legendary: {ultraLegendary.Count} vehicles");
            Debug.Log($"    Legendary: {legendary.Count} vehicles");
            Debug.Log($"    Epic: {epic.Count} vehicles");
        }
    }
}
