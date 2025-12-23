using UnityEngine;
using System;

namespace OmniWorld.World
{
    /// <summary>
    /// VehicleNFT - Represents an NFT-based luxury vehicle
    /// Handles ownership, rarity, and metadata for exclusive vehicles
    /// </summary>
    [System.Serializable]
    public class VehicleNFT
    {
        [Header("NFT Identity")]
        public string nftId;
        public string tokenId;
        public string contractAddress;
        
        [Header("Vehicle Details")]
        public string vehicleName;
        public string manufacturer;
        public string model;
        public int productionYear;
        
        [Header("Rarity & Production")]
        public RarityTier rarityTier;
        public int editionNumber;        // e.g., 1 of 10
        public int totalEditions;        // e.g., 10 total
        public bool isOneOfOne;          // True for 1-of-1 vehicles
        
        [Header("Performance Specifications")]
        public string engineType;
        public int horsepower;
        public float topSpeed;             // mph
        public float acceleration;         // 0-60 mph in seconds
        public float handling;             // 0-10 scale
        
        [Header("Economic Properties")]
        public float mintingPrice;         // Initial mint price in OMNI
        public float currentValue;         // Current market value in OMNI
        public float mintingFeePercent = 0.05f;    // 5% minting fee
        public float salesTaxPercent = 0.08f;      // 8% sales tax
        public float royaltyPercent = 0.20f;       // 20% perpetual royalty
        
        [Header("Ownership")]
        public string currentOwner;        // Wallet address
        public string originalMinter;      // Original creator/minter
        public DateTime mintedDate;
        public DateTime lastTransferDate;
        public int transferCount = 0;
        
        [Header("Status & Availability")]
        public bool isListed = false;
        public bool isInAuction = false;
        public bool canResell = true;      // Selective resale policy
        public float minimumResalePrice;   // Minimum allowed resale price
        
        [Header("In-Game Properties")]
        public bool isUsableInGame = true;
        public string prefabReference;     // Reference to Unity prefab
        public VehicleStats gameplayStats;
        
        [Header("Social Currency")]
        public int prestigePoints;         // Status symbol value
        public int ownershipDays;          // Days owned
        public float appreciationRate;     // Value appreciation over time
        
        public VehicleNFT(string name, RarityTier tier, int edition, int total)
        {
            vehicleName = name;
            rarityTier = tier;
            editionNumber = edition;
            totalEditions = total;
            isOneOfOne = (total == 1);
            nftId = GenerateNFTId();
            mintedDate = DateTime.UtcNow;
            lastTransferDate = DateTime.UtcNow;
        }
        
        private string GenerateNFTId()
        {
            return $"OMNI-AUTO-{rarityTier}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }
        
        /// <summary>
        /// Calculate total purchase price including fees
        /// </summary>
        public float CalculateTotalPurchasePrice()
        {
            float mintingFee = currentValue * mintingFeePercent;
            float salesTax = currentValue * salesTaxPercent;
            return currentValue + mintingFee + salesTax;
        }
        
        /// <summary>
        /// Calculate royalty on secondary sale
        /// </summary>
        public float CalculateRoyalty(float salePrice)
        {
            return salePrice * royaltyPercent;
        }
        
        /// <summary>
        /// Update value based on appreciation and market demand
        /// </summary>
        public void UpdateMarketValue(float demandMultiplier = 1.0f)
        {
            ownershipDays = (int)(DateTime.UtcNow - lastTransferDate).TotalDays;
            
            // Apply appreciation over time for rare vehicles
            if (rarityTier == RarityTier.UltraLegendary || rarityTier == RarityTier.Legendary)
            {
                float timeMultiplier = 1f + (ownershipDays * appreciationRate);
                currentValue = mintingPrice * timeMultiplier * demandMultiplier;
            }
            else
            {
                currentValue = mintingPrice * demandMultiplier;
            }
        }
        
        /// <summary>
        /// Transfer ownership to new owner
        /// </summary>
        public bool TransferOwnership(string newOwner, float salePrice)
        {
            if (!canResell)
            {
                Debug.LogWarning($"Vehicle {vehicleName} cannot be resold due to policy restrictions");
                return false;
            }
            
            if (salePrice < minimumResalePrice)
            {
                Debug.LogWarning($"Sale price ${salePrice} is below minimum ${minimumResalePrice}");
                return false;
            }
            
            currentOwner = newOwner;
            lastTransferDate = DateTime.UtcNow;
            transferCount++;
            
            Debug.Log($"Vehicle {vehicleName} transferred to {newOwner} for {salePrice} OMNI");
            return true;
        }
        
        /// <summary>
        /// Check if vehicle qualifies for elite status
        /// </summary>
        public bool IsEliteStatus()
        {
            return rarityTier == RarityTier.UltraLegendary || 
                   (rarityTier == RarityTier.Legendary && isOneOfOne);
        }
    }
    
    [System.Serializable]
    public class VehicleStats
    {
        public float speed;
        public float acceleration;
        public float handling;
        public float durability;
        public float fuelEfficiency;
    }
    
    public enum RarityTier
    {
        Common,          // Mass production vehicles
        Uncommon,        // Limited production
        Rare,            // Special editions
        Epic,            // High-performance exclusives
        Legendary,       // 10-of-10 tier
        UltraLegendary   // 1-of-1 tier
    }
}
