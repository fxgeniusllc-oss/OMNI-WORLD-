using UnityEngine;
using System;

namespace OmniWorld.World
{
    /// <summary>
    /// TrophyNFT - Represents a Tournament Trophy NFT with rarity tiers
    /// Awards for tournament victories granting VIP access and prestige
    /// </summary>
    [System.Serializable]
    public class TrophyNFT
    {
        [Header("NFT Identity")]
        public string nftId;
        public string tokenId;
        public string contractAddress;
        
        [Header("Trophy Details")]
        public string trophyName;
        public TrophyRank rank;
        public string tournamentName;
        public string tournamentType;
        public DateTime tournamentDate;
        
        [Header("Tournament Information")]
        public int participantCount;
        public float prizePool;              // Prize pool in $OMNI
        public string difficulty;            // "Beginner", "Intermediate", "Elite", "Championship"
        public int playerRanking;            // Player's final ranking (1st, 2nd, 3rd, etc.)
        
        [Header("Ownership")]
        public string currentOwner;          // Wallet address
        public string originalWinner;        // Original tournament winner
        public DateTime mintedDate;
        public DateTime lastTransferDate;
        public int transferCount = 0;
        
        [Header("VIP Access & Perks")]
        public bool canAccessGoldTournaments;
        public bool canAccessSilverTournaments;
        public bool canAccessBronzeTournaments;
        public float xpBoostMultiplier = 1.0f;  // 1.25x, 1.5x, 2.0x based on rank
        public int prestigePoints;
        
        [Header("Smart Contract Trading Bot")]
        public bool hasSmartContract = false;
        public string tradingBotAddress;
        public TradingStrategy tradingStrategy;
        public float totalEarningsUSDC = 0f;     // Total earnings in USDC (not $OMNI)
        public DateTime botActivationDate;
        public DateTime botExpirationDate;       // Bot expires after 6-12 months
        public bool isBotActive = false;
        
        [Header("Marketplace")]
        public bool isListed = false;
        public float listingPrice = 0f;          // Price in $OMNI
        public bool canResell = true;
        public float minimumResalePrice = 0f;
        
        [Header("Display & Metadata")]
        public string metadataURI;
        public Sprite trophyIcon;
        public Color trophyColor;
        public string description;

        public TrophyNFT(string name, TrophyRank trophyRank, string tournament, string type)
        {
            trophyName = name;
            rank = trophyRank;
            tournamentName = tournament;
            tournamentType = type;
            nftId = GenerateNFTId();
            mintedDate = DateTime.UtcNow;
            lastTransferDate = DateTime.UtcNow;
            tournamentDate = DateTime.UtcNow;
            
            // Set perks based on rank
            ConfigureRankPerks();
            
            // Set display properties
            ConfigureDisplay();
        }
        
        private string GenerateNFTId()
        {
            return $"OMNI-TROPHY-{rank}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }
        
        /// <summary>
        /// Configure VIP access and perks based on trophy rank
        /// </summary>
        private void ConfigureRankPerks()
        {
            switch (rank)
            {
                case TrophyRank.Gold:
                    canAccessGoldTournaments = true;
                    canAccessSilverTournaments = true;
                    canAccessBronzeTournaments = true;
                    xpBoostMultiplier = 2.0f;          // 2x XP boost
                    prestigePoints = 1000;
                    description = "Elite championship trophy - massive XP boost, VIP tournament access, permanent leaderboard prestige";
                    break;
                    
                case TrophyRank.Silver:
                    canAccessSilverTournaments = true;
                    canAccessBronzeTournaments = true;
                    xpBoostMultiplier = 1.5f;          // 1.5x XP boost
                    prestigePoints = 500;
                    description = "Mid-tier competitive trophy - moderate XP boost, discounts on tournament fees";
                    break;
                    
                case TrophyRank.Bronze:
                    canAccessBronzeTournaments = true;
                    xpBoostMultiplier = 1.25f;         // 1.25x XP boost
                    prestigePoints = 250;
                    description = "Entry-level trophy - small XP boost, allows entry into beginner-level exclusive events";
                    break;
            }
        }
        
        /// <summary>
        /// Configure visual display based on rank
        /// </summary>
        private void ConfigureDisplay()
        {
            switch (rank)
            {
                case TrophyRank.Gold:
                    trophyColor = new Color(1f, 0.84f, 0f);  // Gold color
                    break;
                case TrophyRank.Silver:
                    trophyColor = new Color(0.75f, 0.75f, 0.75f);  // Silver color
                    break;
                case TrophyRank.Bronze:
                    trophyColor = new Color(0.8f, 0.5f, 0.2f);  // Bronze color
                    break;
            }
        }
        
        /// <summary>
        /// Attach smart contract trading bot to trophy
        /// </summary>
        public void AttachTradingBot(string botAddress, TradingStrategy strategy, int durationMonths)
        {
            // Only Gold and Silver trophies can have trading bots
            if (rank == TrophyRank.Bronze)
            {
                Debug.LogWarning("Bronze trophies cannot have trading bots attached");
                return;
            }
            
            hasSmartContract = true;
            tradingBotAddress = botAddress;
            tradingStrategy = strategy;
            botActivationDate = DateTime.UtcNow;
            botExpirationDate = DateTime.UtcNow.AddMonths(durationMonths);
            isBotActive = true;
            
            Debug.Log($"Trading bot attached to {trophyName}. Expires in {durationMonths} months.");
        }
        
        /// <summary>
        /// Check if trading bot is still active
        /// </summary>
        public bool IsBotActive()
        {
            return hasSmartContract && isBotActive && DateTime.UtcNow < botExpirationDate;
        }
        
        /// <summary>
        /// Get estimated monthly earnings from trading bot
        /// </summary>
        public float GetEstimatedMonthlyEarnings()
        {
            if (!IsBotActive()) return 0f;
            
            // Estimated monthly earnings based on rank
            switch (rank)
            {
                case TrophyRank.Gold:
                    return UnityEngine.Random.Range(500f, 2500f);  // 500-2500 USDC/month
                case TrophyRank.Silver:
                    return UnityEngine.Random.Range(100f, 1000f);  // 100-1000 USDC/month
                default:
                    return 0f;
            }
        }
        
        /// <summary>
        /// Update earnings from trading bot
        /// </summary>
        public void UpdateBotEarnings(float earningsUSDC)
        {
            if (IsBotActive())
            {
                totalEarningsUSDC += earningsUSDC;
                Debug.Log($"Trading bot earned {earningsUSDC} USDC. Total: {totalEarningsUSDC} USDC");
            }
        }
        
        /// <summary>
        /// Deactivate trading bot
        /// </summary>
        public void DeactivateBot(string reason = "Manual deactivation")
        {
            isBotActive = false;
            Debug.Log($"Trading bot deactivated: {reason}");
        }
        
        /// <summary>
        /// Transfer ownership to new owner
        /// </summary>
        public bool TransferOwnership(string newOwner, float salePrice)
        {
            if (!canResell)
            {
                Debug.LogWarning($"Trophy {trophyName} cannot be resold due to policy restrictions");
                return false;
            }
            
            if (salePrice < minimumResalePrice && minimumResalePrice > 0)
            {
                Debug.LogWarning($"Sale price {salePrice} is below minimum {minimumResalePrice}");
                return false;
            }
            
            currentOwner = newOwner;
            lastTransferDate = DateTime.UtcNow;
            transferCount++;
            
            Debug.Log($"Trophy {trophyName} transferred to {newOwner} for {salePrice} OMNI");
            return true;
        }
        
        /// <summary>
        /// Check if holder has VIP access to tournament tier
        /// </summary>
        public bool HasVIPAccessToTournament(TrophyRank requiredRank)
        {
            switch (requiredRank)
            {
                case TrophyRank.Gold:
                    return canAccessGoldTournaments;
                case TrophyRank.Silver:
                    return canAccessSilverTournaments;
                case TrophyRank.Bronze:
                    return canAccessBronzeTournaments;
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Get days remaining until bot expires
        /// </summary>
        public int GetBotDaysRemaining()
        {
            if (!hasSmartContract || !isBotActive) return 0;
            
            TimeSpan remaining = botExpirationDate - DateTime.UtcNow;
            return remaining.Days > 0 ? remaining.Days : 0;
        }
        
        /// <summary>
        /// Get trophy value estimate based on rank, rarity, and bot
        /// </summary>
        public float GetEstimatedValue()
        {
            float baseValue = 0f;
            
            // Base value by rank
            switch (rank)
            {
                case TrophyRank.Gold:
                    baseValue = 5000f;  // Base 5000 OMNI
                    break;
                case TrophyRank.Silver:
                    baseValue = 2000f;  // Base 2000 OMNI
                    break;
                case TrophyRank.Bronze:
                    baseValue = 500f;   // Base 500 OMNI
                    break;
            }
            
            // Add value if it has active trading bot
            if (IsBotActive())
            {
                float botValue = GetEstimatedMonthlyEarnings() * GetBotDaysRemaining() / 30f;
                baseValue += botValue * 0.7f; // Bot value at 70% of potential earnings
            }
            
            // Add historical prestige value
            int daysSinceMint = (int)(DateTime.UtcNow - mintedDate).TotalDays;
            float ageMultiplier = 1f + (daysSinceMint * 0.001f); // Slight appreciation over time
            
            return baseValue * ageMultiplier;
        }
    }
    
    /// <summary>
    /// Trophy rank enum
    /// </summary>
    public enum TrophyRank
    {
        Bronze = 0,   // Entry-level tournaments
        Silver = 1,   // Mid-tier tournaments
        Gold = 2      // Elite championship events
    }
    
    /// <summary>
    /// Trading strategy for smart contract bots
    /// </summary>
    public enum TradingStrategy
    {
        Conservative,  // Low risk, stable returns with USDC
        Balanced,      // Medium risk, moderate returns
        Aggressive     // Higher risk, higher potential returns with WBTC/ETH
    }
}
