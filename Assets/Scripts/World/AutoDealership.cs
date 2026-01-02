using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using OmniWorld.Economy;

namespace OmniWorld.World
{
    /// <summary>
    /// AutoDealership - Manages the exclusive NFT-based luxury auto dealership
    /// Features 1-of-1 and 10-of-10 NFT cars with selective resale policies
    /// Ultra-modern glass showroom for 24/7 window shopping experience
    /// </summary>
    public class AutoDealership : MonoBehaviour
    {
        private static AutoDealership _instance;
        public static AutoDealership Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AutoDealership>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AutoDealership");
                        _instance = go.AddComponent<AutoDealership>();
                    }
                }
                return _instance;
            }
        }
        
        [Header("Dealership Configuration")]
        [Tooltip("Dealership name")]
        public string dealershipName = "OmniWorld Auto Gallery";
        
        [Tooltip("City location")]
        public string cityLocation = "OmniVegas";
        
        [Tooltip("Prime location bonus multiplier")]
        public float locationPremiumMultiplier = 1.5f;
        
        [Header("Showroom Settings")]
        [Tooltip("Enable 24/7 window shopping")]
        public bool alwaysOpen = true;
        
        [Tooltip("Maximum vehicles in showroom")]
        public int showroomCapacity = 20;
        
        [Tooltip("Enable dynamic lighting")]
        public bool dynamicLighting = true;
        
        [Tooltip("Include VIP lounge zones")]
        public bool hasLoungeZones = true;
        
        [Header("Vehicle Inventory")]
        public List<VehicleNFT> availableVehicles = new List<VehicleNFT>();
        public List<VehicleNFT> soldVehicles = new List<VehicleNFT>();
        public List<VehicleNFT> displayVehicles = new List<VehicleNFT>();
        
        [Header("Economic Settings")]
        [Tooltip("Base minting fee for new vehicles")]
        public float baseMintingFee = 0.05f; // 5%
        
        [Tooltip("Sales tax rate")]
        public float salesTaxRate = 0.08f; // 8%
        
        [Tooltip("Enable strategic buyback program")]
        public bool buybackEnabled = true;
        
        [Tooltip("Buyback price percentage of current value")]
        public float buybackPercentage = 0.75f; // 75%
        
        [Header("Resale Policy")]
        [Tooltip("Enable selective resale restrictions")]
        public bool selectiveResalePolicy = true;
        
        [Tooltip("Minimum ownership days before resale")]
        public int minimumOwnershipDays = 30;
        
        [Tooltip("Maximum resale price multiplier")]
        public float maximumResalePriceMultiplier = 3.0f; // 300% of purchase price
        
        [Header("Status & Prestige")]
        [Tooltip("Base prestige points for owning any vehicle")]
        public int basePrestigePoints = 100;
        
        [Tooltip("Prestige multiplier for rare vehicles")]
        public Dictionary<RarityTier, float> prestigeMultipliers = new Dictionary<RarityTier, float>();
        
        [Header("Statistics")]
        public int totalVehiclesMinted = 0;
        public int totalSales = 0;
        public float totalRevenue = 0f;
        public float totalRoyaltiesPaid = 0f;
        
        private DominionEconomy economySystem;
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeDealership();
        }
        
        private void Start()
        {
            economySystem = DominionEconomy.Instance;
        }
        
        private void InitializeDealership()
        {
            Debug.Log($"=== {dealershipName} Initialized ===");
            Debug.Log($"Location: {cityLocation} (Prime Location)");
            Debug.Log($"Showroom Capacity: {showroomCapacity} vehicles");
            Debug.Log($"24/7 Window Shopping: {(alwaysOpen ? "ENABLED" : "DISABLED")}");
            
            InitializePrestigeMultipliers();
            LoadVehicleInventory();
        }
        
        private void InitializePrestigeMultipliers()
        {
            prestigeMultipliers[RarityTier.Common] = 1.0f;
            prestigeMultipliers[RarityTier.Uncommon] = 1.5f;
            prestigeMultipliers[RarityTier.Rare] = 2.0f;
            prestigeMultipliers[RarityTier.Epic] = 3.0f;
            prestigeMultipliers[RarityTier.Legendary] = 5.0f;
            prestigeMultipliers[RarityTier.UltraLegendary] = 10.0f;
        }
        
        /// <summary>
        /// Load vehicle inventory from JSON configurations
        /// </summary>
        private void LoadVehicleInventory()
        {
            Debug.Log("Loading vehicle inventory from configurations...");
            // This will be populated from JSON files in the Prefabs/Vehicles directory
            // For now, we initialize an empty inventory that can be populated
        }
        
        /// <summary>
        /// Mint new NFT vehicle
        /// </summary>
        public VehicleNFT MintVehicle(string vehicleName, RarityTier tier, int edition, int total, 
                                      float basePrice, string minterAddress)
        {
            VehicleNFT vehicle = new VehicleNFT(vehicleName, tier, edition, total)
            {
                mintingPrice = basePrice,
                currentValue = basePrice * locationPremiumMultiplier,
                currentOwner = minterAddress,
                originalMinter = minterAddress,
                mintingFeePercent = baseMintingFee,
                salesTaxPercent = salesTaxRate,
                appreciationRate = CalculateAppreciationRate(tier)
            };
            
            // Apply resale policy
            if (selectiveResalePolicy)
            {
                vehicle.canResell = true;
                vehicle.minimumResalePrice = basePrice * 0.8f; // Minimum 80% of original price
            }
            
            // Calculate prestige points
            vehicle.prestigePoints = CalculatePrestigePoints(tier);
            
            availableVehicles.Add(vehicle);
            totalVehiclesMinted++;
            
            Debug.Log($"✨ NEW VEHICLE MINTED ✨");
            Debug.Log($"Name: {vehicleName}");
            Debug.Log($"Tier: {tier} ({edition}/{total})");
            Debug.Log($"Price: {vehicle.currentValue} OMNI");
            Debug.Log($"NFT ID: {vehicle.nftId}");
            Debug.Log($"Prestige: {vehicle.prestigePoints} points");
            
            return vehicle;
        }
        
        /// <summary>
        /// Purchase vehicle from dealership
        /// </summary>
        public bool PurchaseVehicle(string nftId, string buyerAddress, float buyerPrestige)
        {
            VehicleNFT vehicle = availableVehicles.Find(v => v.nftId == nftId);
            
            if (vehicle == null)
            {
                Debug.LogWarning($"Vehicle {nftId} not found in inventory");
                return false;
            }
            
            // Check if vehicle is in auction
            if (vehicle.isInAuction)
            {
                Debug.LogWarning($"Vehicle {vehicle.vehicleName} is currently in auction");
                return false;
            }
            
            float totalPrice = vehicle.CalculateTotalPurchasePrice();
            
            Debug.Log($"=== PURCHASE INITIATED ===");
            Debug.Log($"Vehicle: {vehicle.vehicleName}");
            Debug.Log($"Base Price: {vehicle.currentValue} OMNI");
            Debug.Log($"Minting Fee ({vehicle.mintingFeePercent * 100}%): {vehicle.currentValue * vehicle.mintingFeePercent} OMNI");
            Debug.Log($"Sales Tax ({vehicle.salesTaxPercent * 100}%): {vehicle.currentValue * vehicle.salesTaxPercent} OMNI");
            Debug.Log($"Total Price: {totalPrice} OMNI");
            
            // Transfer ownership
            vehicle.currentOwner = buyerAddress;
            vehicle.lastTransferDate = System.DateTime.UtcNow;
            
            // Update lists
            availableVehicles.Remove(vehicle);
            soldVehicles.Add(vehicle);
            
            // Update statistics
            totalSales++;
            totalRevenue += totalPrice;
            
            Debug.Log($"✅ PURCHASE COMPLETE");
            Debug.Log($"New Owner: {buyerAddress}");
            Debug.Log($"Prestige Awarded: {vehicle.prestigePoints} points");
            
            return true;
        }
        
        /// <summary>
        /// List vehicle for resale (secondary market)
        /// </summary>
        public bool ListVehicleForResale(VehicleNFT vehicle, float askingPrice)
        {
            if (!vehicle.canResell)
            {
                Debug.LogWarning($"Vehicle {vehicle.vehicleName} cannot be resold due to policy");
                return false;
            }
            
            int ownershipDays = (int)(System.DateTime.UtcNow - vehicle.lastTransferDate).TotalDays;
            if (ownershipDays < minimumOwnershipDays)
            {
                Debug.LogWarning($"Minimum ownership period not met ({ownershipDays}/{minimumOwnershipDays} days)");
                return false;
            }
            
            if (askingPrice < vehicle.minimumResalePrice)
            {
                Debug.LogWarning($"Asking price below minimum: {askingPrice} < {vehicle.minimumResalePrice}");
                return false;
            }
            
            float maxPrice = vehicle.currentValue * maximumResalePriceMultiplier;
            if (askingPrice > maxPrice)
            {
                Debug.LogWarning($"Asking price exceeds maximum: {askingPrice} > {maxPrice}");
                return false;
            }
            
            vehicle.isListed = true;
            vehicle.currentValue = askingPrice;
            availableVehicles.Add(vehicle);
            
            Debug.Log($"Vehicle {vehicle.vehicleName} listed for resale at {askingPrice} OMNI");
            return true;
        }
        
        /// <summary>
        /// Strategic buyback program
        /// </summary>
        public float OfferBuyback(VehicleNFT vehicle)
        {
            if (!buybackEnabled)
                return 0f;
            
            // Calculate buyback offer
            float buybackOffer = vehicle.currentValue * buybackPercentage;
            
            Debug.Log($"Buyback offer for {vehicle.vehicleName}: {buybackOffer} OMNI");
            return buybackOffer;
        }
        
        /// <summary>
        /// Execute buyback transaction
        /// </summary>
        public bool ExecuteBuyback(VehicleNFT vehicle, string sellerAddress)
        {
            if (!buybackEnabled)
                return false;
            
            float buybackPrice = OfferBuyback(vehicle);
            
            vehicle.currentOwner = "OmniWorld_Dealership";
            vehicle.lastTransferDate = System.DateTime.UtcNow;
            
            soldVehicles.Remove(vehicle);
            availableVehicles.Add(vehicle);
            vehicle.isListed = false;
            
            Debug.Log($"Buyback executed: {vehicle.vehicleName} for {buybackPrice} OMNI");
            return true;
        }
        
        /// <summary>
        /// Add vehicle to showroom display
        /// </summary>
        public bool AddToShowroom(VehicleNFT vehicle)
        {
            if (displayVehicles.Count >= showroomCapacity)
            {
                Debug.LogWarning("Showroom at capacity");
                return false;
            }
            
            if (!displayVehicles.Contains(vehicle))
            {
                displayVehicles.Add(vehicle);
                Debug.Log($"{vehicle.vehicleName} added to showroom display");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Get vehicles by rarity tier
        /// </summary>
        public List<VehicleNFT> GetVehiclesByRarity(RarityTier tier)
        {
            return availableVehicles.Where(v => v.rarityTier == tier).ToList();
        }
        
        /// <summary>
        /// Get ultra-rare vehicles (1-of-1 and Legendary)
        /// </summary>
        public List<VehicleNFT> GetUltraRareVehicles()
        {
            return availableVehicles.Where(v => v.IsEliteStatus()).ToList();
        }
        
        /// <summary>
        /// Calculate appreciation rate based on rarity
        /// </summary>
        private float CalculateAppreciationRate(RarityTier tier)
        {
            switch (tier)
            {
                case RarityTier.UltraLegendary:
                    return 0.01f; // 1% per day
                case RarityTier.Legendary:
                    return 0.005f; // 0.5% per day
                case RarityTier.Epic:
                    return 0.002f; // 0.2% per day
                default:
                    return 0.0f;
            }
        }
        
        /// <summary>
        /// Calculate prestige points for vehicle
        /// </summary>
        private int CalculatePrestigePoints(RarityTier tier)
        {
            if (prestigeMultipliers.ContainsKey(tier))
            {
                return (int)(basePrestigePoints * prestigeMultipliers[tier]);
            }
            return basePrestigePoints;
        }
        
        /// <summary>
        /// Get dealership statistics
        /// </summary>
        public DealershipStats GetStatistics()
        {
            return new DealershipStats
            {
                totalVehicles = totalVehiclesMinted,
                availableVehicles = availableVehicles.Count,
                soldVehicles = soldVehicles.Count,
                showroomVehicles = displayVehicles.Count,
                totalSales = totalSales,
                totalRevenue = totalRevenue,
                totalRoyalties = totalRoyaltiesPaid,
                averageSalePrice = totalSales > 0 ? totalRevenue / totalSales : 0f
            };
        }
    }
    
    [System.Serializable]
    public class DealershipStats
    {
        public int totalVehicles;
        public int availableVehicles;
        public int soldVehicles;
        public int showroomVehicles;
        public int totalSales;
        public float totalRevenue;
        public float totalRoyalties;
        public float averageSalePrice;
    }
}
