using UnityEngine;
using OmniWorld.Economy;

namespace OmniWorld.World
{
    /// <summary>
    /// DealershipEconomyIntegration - Connects Auto Dealership with Dominion Economy
    /// Handles dynamic pricing, transactions, and economic validation
    /// </summary>
    public class DealershipEconomyIntegration : MonoBehaviour
    {
        private static DealershipEconomyIntegration _instance;
        public static DealershipEconomyIntegration Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<DealershipEconomyIntegration>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("DealershipEconomyIntegration");
                        _instance = go.AddComponent<DealershipEconomyIntegration>();
                    }
                }
                return _instance;
            }
        }
        
        [Header("Economy References")]
        private DominionEconomy dominion;
        private AutoDealership dealership;
        
        [Header("Price Modifiers")]
        [Tooltip("Apply Dominion Economy pricing to vehicles")]
        public bool useDynamicPricing = true;
        
        [Tooltip("Location premium for prime dealership placement")]
        public float locationPremium = 1.5f;
        
        [Tooltip("Demand multiplier based on player interest")]
        public float demandMultiplier = 1.0f;
        
        [Header("Transaction Settings")]
        [Tooltip("Minimum OMNI token balance for purchases")]
        public float minimumBalance = 1000f;
        
        [Tooltip("Transaction burn rate (from DominionEconomy)")]
        public float transactionBurnRate = 0.005f;
        
        [Header("Integration Status")]
        public bool isIntegrated = false;
        public int transactionsProcessed = 0;
        public float totalBurned = 0f;
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            InitializeIntegration();
        }
        
        private void InitializeIntegration()
        {
            Debug.Log("=== Dealership Economy Integration ===");
            
            // Get references to economy systems
            dominion = DominionEconomy.Instance;
            dealership = AutoDealership.Instance;
            
            if (dominion != null && dealership != null)
            {
                isIntegrated = true;
                transactionBurnRate = dominion.transactionBurnRate;
                
                Debug.Log("✓ Integration successful");
                Debug.Log($"Dynamic Pricing: {(useDynamicPricing ? "ENABLED" : "DISABLED")}");
                Debug.Log($"Location Premium: {locationPremium}x");
                Debug.Log($"Transaction Burn Rate: {transactionBurnRate * 100}%");
            }
            else
            {
                Debug.LogWarning("⚠ Integration incomplete - missing system references");
            }
        }
        
        /// <summary>
        /// Calculate dynamic vehicle price using Dominion Economy
        /// </summary>
        public float CalculateVehiclePrice(VehicleNFT vehicle)
        {
            if (!useDynamicPricing || dominion == null)
            {
                return vehicle.currentValue;
            }
            
            // Base price from vehicle
            float basePrice = vehicle.mintingPrice;
            
            // Apply location premium
            float locationAdjusted = basePrice * locationPremium;
            
            // Apply demand multiplier
            float demandAdjusted = locationAdjusted * demandMultiplier;
            
            // Apply appreciation for rare vehicles
            if (vehicle.IsEliteStatus())
            {
                int daysOwned = (int)(System.DateTime.UtcNow - vehicle.mintedDate).TotalDays;
                float appreciationMultiplier = 1f + (daysOwned * vehicle.appreciationRate);
                demandAdjusted *= appreciationMultiplier;
            }
            
            // Apply Dominion Economy factors
            // Use circulation coefficient to reward active economy participation
            float economicMultiplier = dominion.circulationCoefficient;
            float finalPrice = demandAdjusted * economicMultiplier;
            
            // Respect price stability controls
            finalPrice = Mathf.Clamp(finalPrice, 
                vehicle.mintingPrice * 0.5f,  // Floor at 50% of mint price
                vehicle.mintingPrice * 5.0f); // Ceiling at 500% of mint price
            
            return finalPrice;
        }
        
        /// <summary>
        /// Process vehicle purchase transaction through Dominion Economy
        /// </summary>
        public bool ProcessPurchaseTransaction(VehicleNFT vehicle, string buyerAddress, float playerBalance)
        {
            if (!isIntegrated)
            {
                Debug.LogWarning("Economy integration not available");
                return false;
            }
            
            // Calculate total cost
            float vehiclePrice = CalculateVehiclePrice(vehicle);
            vehicle.currentValue = vehiclePrice;
            float totalCost = vehicle.CalculateTotalPurchasePrice();
            
            // Validate player balance
            if (playerBalance < totalCost)
            {
                Debug.LogWarning($"Insufficient balance: {playerBalance} < {totalCost}");
                return false;
            }
            
            if (playerBalance < minimumBalance)
            {
                Debug.LogWarning($"Balance below minimum: {playerBalance} < {minimumBalance}");
                return false;
            }
            
            // Calculate transaction burn (deflationary mechanism)
            float burnAmount = totalCost * transactionBurnRate;
            float netCost = totalCost + burnAmount;
            
            // Validate final cost
            if (playerBalance < netCost)
            {
                Debug.LogWarning($"Insufficient balance after burn: {playerBalance} < {netCost}");
                return false;
            }
            
            // Process transaction
            Debug.Log($"=== TRANSACTION PROCESSING ===");
            Debug.Log($"Vehicle: {vehicle.vehicleName}");
            Debug.Log($"Base Price: {vehiclePrice} OMNI");
            Debug.Log($"Total Cost: {totalCost} OMNI");
            Debug.Log($"Burn Amount: {burnAmount} OMNI ({transactionBurnRate * 100}%)");
            Debug.Log($"Net Cost: {netCost} OMNI");
            Debug.Log($"Remaining Balance: {playerBalance - netCost} OMNI");
            
            // Update statistics
            transactionsProcessed++;
            totalBurned += burnAmount;
            
            // Emit transaction event
            if (dominion != null)
            {
                dominion.OnTransactionProcessed?.Invoke("VehiclePurchase", netCost);
            }
            
            Debug.Log("✓ Transaction successful");
            return true;
        }
        
        /// <summary>
        /// Process resale transaction with royalties
        /// </summary>
        public bool ProcessResaleTransaction(VehicleNFT vehicle, string sellerAddress, 
                                             string buyerAddress, float salePrice, 
                                             float buyerBalance)
        {
            if (!isIntegrated)
            {
                Debug.LogWarning("Economy integration not available");
                return false;
            }
            
            // Calculate fees
            float royalty = vehicle.CalculateRoyalty(salePrice);
            float platformFee = salePrice * 0.05f; // 5% platform fee
            float burnAmount = salePrice * transactionBurnRate;
            float sellerProceeds = salePrice - royalty - platformFee;
            float totalCost = salePrice + burnAmount;
            
            // Validate buyer balance
            if (buyerBalance < totalCost)
            {
                Debug.LogWarning($"Insufficient buyer balance: {buyerBalance} < {totalCost}");
                return false;
            }
            
            Debug.Log($"=== RESALE TRANSACTION ===");
            Debug.Log($"Vehicle: {vehicle.vehicleName}");
            Debug.Log($"Sale Price: {salePrice} OMNI");
            Debug.Log($"Royalty (20%): {royalty} OMNI → {vehicle.originalMinter}");
            Debug.Log($"Platform Fee (5%): {platformFee} OMNI → Treasury");
            Debug.Log($"Burn Amount: {burnAmount} OMNI");
            Debug.Log($"Seller Proceeds: {sellerProceeds} OMNI → {sellerAddress}");
            Debug.Log($"Buyer Cost: {totalCost} OMNI");
            
            // Update statistics
            transactionsProcessed++;
            totalBurned += burnAmount;
            
            if (dominion != null)
            {
                dominion.OnTransactionProcessed?.Invoke("VehicleResale", totalCost);
            }
            
            Debug.Log("✓ Resale transaction successful");
            return true;
        }
        
        /// <summary>
        /// Process auction winning bid transaction
        /// </summary>
        public bool ProcessAuctionTransaction(VehicleAuction auction, float winnerBalance)
        {
            if (!isIntegrated)
            {
                Debug.LogWarning("Economy integration not available");
                return false;
            }
            
            float winningBid = auction.winningBid;
            float auctionFee = winningBid * 0.10f; // 10% auction fee
            float royalty = auction.vehicle.CalculateRoyalty(winningBid);
            float burnAmount = winningBid * transactionBurnRate;
            float sellerProceeds = winningBid - auctionFee - royalty;
            float totalCost = winningBid + burnAmount;
            
            if (winnerBalance < totalCost)
            {
                Debug.LogWarning($"Insufficient winner balance: {winnerBalance} < {totalCost}");
                return false;
            }
            
            Debug.Log($"=== AUCTION SETTLEMENT ===");
            Debug.Log($"Vehicle: {auction.vehicle.vehicleName}");
            Debug.Log($"Winning Bid: {winningBid} OMNI");
            Debug.Log($"Auction Fee (10%): {auctionFee} OMNI");
            Debug.Log($"Royalty (20%): {royalty} OMNI");
            Debug.Log($"Burn Amount: {burnAmount} OMNI");
            Debug.Log($"Seller Proceeds: {sellerProceeds} OMNI");
            
            transactionsProcessed++;
            totalBurned += burnAmount;
            
            if (dominion != null)
            {
                dominion.OnTransactionProcessed?.Invoke("AuctionWin", totalCost);
            }
            
            Debug.Log("✓ Auction settlement successful");
            return true;
        }
        
        /// <summary>
        /// Update demand multiplier based on market conditions
        /// </summary>
        public void UpdateDemandMultiplier(int activeViewers, int recentSales)
        {
            // Calculate demand based on activity
            float viewerMultiplier = 1f + (activeViewers * 0.01f); // +1% per viewer
            float salesMultiplier = 1f + (recentSales * 0.05f);    // +5% per recent sale
            
            demandMultiplier = Mathf.Clamp(viewerMultiplier * salesMultiplier, 0.5f, 3.0f);
            
            Debug.Log($"Demand multiplier updated: {demandMultiplier}x (Viewers: {activeViewers}, Sales: {recentSales})");
        }
        
        /// <summary>
        /// Get current OMNI token price from Dominion Economy
        /// </summary>
        public float GetCurrentTokenPrice()
        {
            if (dominion != null)
            {
                return dominion.omniTokenPrice;
            }
            return 0.035f; // Default initial price
        }
        
        /// <summary>
        /// Convert OMNI tokens to USD
        /// </summary>
        public float ConvertToUSD(float omniAmount)
        {
            return omniAmount * GetCurrentTokenPrice();
        }
        
        /// <summary>
        /// Convert USD to OMNI tokens
        /// </summary>
        public float ConvertToOMNI(float usdAmount)
        {
            float tokenPrice = GetCurrentTokenPrice();
            if (tokenPrice > 0)
            {
                return usdAmount / tokenPrice;
            }
            return 0f;
        }
        
        /// <summary>
        /// Get integration statistics
        /// </summary>
        public IntegrationStats GetStatistics()
        {
            return new IntegrationStats
            {
                isActive = isIntegrated,
                transactionsProcessed = transactionsProcessed,
                totalBurned = totalBurned,
                currentTokenPrice = GetCurrentTokenPrice(),
                locationPremium = locationPremium,
                demandMultiplier = demandMultiplier,
                burnRate = transactionBurnRate
            };
        }
    }
    
    [System.Serializable]
    public class IntegrationStats
    {
        public bool isActive;
        public int transactionsProcessed;
        public float totalBurned;
        public float currentTokenPrice;
        public float locationPremium;
        public float demandMultiplier;
        public float burnRate;
    }
}
