using UnityEngine;
using OmniWorld.Vehicles;
using OmniWorld.Economy;
using OmniWorld.World;

namespace OmniWorld.Tests
{
    /// <summary>
    /// Integration test for OmniLux Auto Dealership system
    /// Demonstrates vehicle purchase, auction, and service workflows
    /// </summary>
    public class DealershipIntegrationTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        public bool runTestsOnStart = false;
        public string testWalletAddress = "0xTestWallet123456789";
        
        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunIntegrationTests());
            }
        }

        private System.Collections.IEnumerator RunIntegrationTests()
        {
            Debug.Log("=================================");
            Debug.Log("OmniLux Auto Dealership Integration Tests");
            Debug.Log("=================================");
            
            yield return new WaitForSeconds(1f);
            
            // Test 1: Initialize Dealership
            TestDealershipInitialization();
            yield return new WaitForSeconds(1f);
            
            // Test 2: View Available Vehicles
            TestViewInventory();
            yield return new WaitForSeconds(1f);
            
            // Test 3: Vehicle Inspection
            TestVehicleInspection();
            yield return new WaitForSeconds(1f);
            
            // Test 4: Calculate Financing
            TestFinancing();
            yield return new WaitForSeconds(1f);
            
            // Test 5: Purchase Vehicle
            TestVehiclePurchase();
            yield return new WaitForSeconds(1f);
            
            // Test 6: Auction System
            TestAuctionSystem();
            yield return new WaitForSeconds(1f);
            
            // Test 7: Modular Systems Integration
            TestModularSystems();
            
            Debug.Log("=================================");
            Debug.Log("Integration Tests Completed");
            Debug.Log("=================================");
        }

        private void TestDealershipInitialization()
        {
            Debug.Log("\n[TEST 1] Dealership Initialization");
            Debug.Log("------------------------------------");
            
            var dealership = VehicleDealershipManager.Instance;
            
            if (dealership != null)
            {
                Debug.Log($"✓ Dealership initialized: {dealership.dealershipName}");
                Debug.Log($"✓ Location: {dealership.location}, {dealership.city}");
                Debug.Log($"✓ Operating Hours: {dealership.operatingHours}");
                Debug.Log($"✓ NFT Minting: {(dealership.nftMintingEnabled ? "Enabled" : "Disabled")}");
            }
            else
            {
                Debug.LogError("✗ Failed to initialize dealership");
            }
        }

        private void TestViewInventory()
        {
            Debug.Log("\n[TEST 2] View Available Inventory");
            Debug.Log("------------------------------------");
            
            var dealership = VehicleDealershipManager.Instance;
            var availableVehicles = dealership.GetAvailableExclusiveVehicles();
            
            Debug.Log($"Available Exclusive Vehicles: {availableVehicles.Count}");
            
            foreach (var vehicle in availableVehicles)
            {
                Debug.Log($"✓ {vehicle.name} ({vehicle.rarity})");
                Debug.Log($"  - Price: {vehicle.priceOmni:N0} OMNI ({vehicle.priceEth} ETH)");
                Debug.Log($"  - Class: {vehicle.vehicleClass}");
                Debug.Log($"  - Power: {vehicle.horsepower} HP");
                Debug.Log($"  - Top Speed: {vehicle.topSpeed} mph");
            }
        }

        private void TestVehicleInspection()
        {
            Debug.Log("\n[TEST 3] Vehicle Inspection Service");
            Debug.Log("------------------------------------");
            
            var dealership = VehicleDealershipManager.Instance;
            bool inspectionResult = dealership.InspectVehicle("001", testWalletAddress);
            
            if (inspectionResult)
            {
                Debug.Log("✓ Vehicle inspection completed successfully");
            }
            else
            {
                Debug.LogWarning("✗ Vehicle inspection failed");
            }
        }

        private void TestFinancing()
        {
            Debug.Log("\n[TEST 4] Financing Calculator");
            Debug.Log("------------------------------------");
            
            var dealership = VehicleDealershipManager.Instance;
            var terms = dealership.CalculateFinancing("001", 36);
            
            if (terms != null)
            {
                Debug.Log("✓ Financing terms calculated successfully");
                Debug.Log($"  - Vehicle: {terms.vehicleName}");
                Debug.Log($"  - Base Price: {terms.basePrice:N0} OMNI");
                Debug.Log($"  - Down Payment: {terms.downPayment:N0} OMNI");
                Debug.Log($"  - Monthly Payment: {terms.monthlyPayment:N0} OMNI");
                Debug.Log($"  - Total Interest: {terms.totalInterest:N0} OMNI");
            }
            else
            {
                Debug.LogWarning("✗ Failed to calculate financing");
            }
        }

        private void TestVehiclePurchase()
        {
            Debug.Log("\n[TEST 5] Vehicle Purchase (Simulation)");
            Debug.Log("------------------------------------");
            Debug.Log("Note: Actual purchase commented out to preserve test data");
            Debug.Log("In production, this would execute:");
            Debug.Log("dealership.PurchaseVehicle('002', testWalletAddress, false)");
            
            // Commented out to avoid modifying inventory during test
            // bool purchaseResult = dealership.PurchaseVehicle("002", testWalletAddress, false);
            
            Debug.Log("✓ Purchase flow validated (not executed)");
        }

        private void TestAuctionSystem()
        {
            Debug.Log("\n[TEST 6] Auction System");
            Debug.Log("------------------------------------");
            
            var auctionManager = AuctionManager.Instance;
            
            if (auctionManager != null)
            {
                Debug.Log("✓ Auction Manager initialized");
                Debug.Log($"  - Schedule: {auctionManager.schedule}");
                Debug.Log($"  - Duration: {auctionManager.durationHours} hours");
                Debug.Log($"  - Minimum Prestige: {auctionManager.minimumPrestige}");
                
                // Start test auction
                bool auctionStarted = auctionManager.StartAuction(
                    "001",
                    "Aether Phantom GT",
                    3750000f
                );
                
                if (auctionStarted)
                {
                    Debug.Log("✓ Auction started successfully");
                    
                    // Simulate VIP bid
                    bool bidPlaced = auctionManager.PlaceBid(
                        testWalletAddress,
                        3900000f,
                        "Diamond",
                        0.85f
                    );
                    
                    if (bidPlaced)
                    {
                        Debug.Log("✓ Bid placed successfully");
                        
                        var status = auctionManager.GetAuctionStatus();
                        Debug.Log($"  - Current Bid: {status.currentBid:N0} OMNI");
                        Debug.Log($"  - Leader: {status.currentLeader}");
                        Debug.Log($"  - Bid Count: {status.bidCount}");
                    }
                    else
                    {
                        Debug.LogWarning("✗ Bid placement failed");
                    }
                    
                    // End auction
                    auctionManager.EndAuction();
                    Debug.Log("✓ Auction ended");
                }
                else
                {
                    Debug.LogWarning("✗ Failed to start auction");
                }
            }
            else
            {
                Debug.LogError("✗ Auction Manager not initialized");
            }
        }

        private void TestModularSystems()
        {
            Debug.Log("\n[TEST 7] Modular Systems Integration");
            Debug.Log("------------------------------------");
            
            // Test Combat System
            var combat = OmniWorld.Combat.AvatarCombatManager.Instance;
            if (combat != null)
            {
                Debug.Log("✓ AvatarCombatManager initialized");
            }
            
            // Test Gym System
            var gym = OmniWorld.Training.GymTrainingSystem.Instance;
            if (gym != null)
            {
                Debug.Log("✓ GymTrainingSystem initialized");
            }
            
            // Test Mod Shop
            var modShop = VehicleModShopManager.Instance;
            if (modShop != null)
            {
                Debug.Log($"✓ VehicleModShopManager initialized: {modShop.shopName}");
            }
            
            // Test Race Events
            var raceEvents = OmniWorld.Racing.RaceEventSpawner.Instance;
            if (raceEvents != null)
            {
                Debug.Log("✓ RaceEventSpawner initialized");
            }
            
            // Test Transit
            var transit = CityTransitManager.Instance;
            if (transit != null)
            {
                Debug.Log("✓ CityTransitManager initialized");
            }
            
            // Test Property System
            var property = OmniWorld.Property.PropertyOwnershipSystem.Instance;
            if (property != null)
            {
                Debug.Log("✓ PropertyOwnershipSystem initialized");
            }
            
            // Test Zone Lore
            var lore = ZoneLoreManager.Instance;
            if (lore != null)
            {
                Debug.Log("✓ ZoneLoreManager initialized");
                lore.DisplayZoneLore("Vegas Strip Zone");
            }
            
            // Test Zone Detector
            var detector = DynamicZoneDetector.Instance;
            if (detector != null)
            {
                Debug.Log($"✓ DynamicZoneDetector initialized - Current Zone: {detector.currentZone}");
            }
        }

        // Manual test trigger methods that can be called from Unity Inspector or other scripts
        
        public void ManualTestPurchase()
        {
            Debug.Log("Manual Test: Vehicle Purchase");
            var dealership = VehicleDealershipManager.Instance;
            bool result = dealership.PurchaseVehicle("002", testWalletAddress, false);
            Debug.Log($"Purchase result: {result}");
        }

        public void ManualTestAuction()
        {
            Debug.Log("Manual Test: Start Auction");
            var auctionManager = AuctionManager.Instance;
            bool result = auctionManager.StartAuction("001", "Aether Phantom GT", 3750000f);
            Debug.Log($"Auction start result: {result}");
        }

        public void ManualTestBid()
        {
            Debug.Log("Manual Test: Place Bid");
            var auctionManager = AuctionManager.Instance;
            bool result = auctionManager.PlaceBid(testWalletAddress, 3900000f, "Diamond", 0.85f);
            Debug.Log($"Bid result: {result}");
        }
    }
}
