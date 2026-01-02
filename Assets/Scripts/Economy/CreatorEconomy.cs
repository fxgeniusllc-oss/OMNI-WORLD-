using UnityEngine;
using System;
using System.Collections.Generic;

namespace OmniWorld.Economy
{
    /// <summary>
    /// Creator Economy System - 85% first sale + 20% perpetual royalties
    /// Manages creator revenue distribution, tier progression, and royalty payments
    /// </summary>
    public class CreatorEconomy : MonoBehaviour
    {
        private static CreatorEconomy _instance;
        private static readonly object _lock = new object();
        
        public static CreatorEconomy Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = FindObjectOfType<CreatorEconomy>();
                            if (_instance == null)
                            {
                                GameObject go = new GameObject("CreatorEconomy");
                                _instance = go.AddComponent<CreatorEconomy>();
                                DontDestroyOnLoad(go);
                            }
                        }
                    }
                }
                return _instance;
            }
        }

        [Header("Revenue Split Configuration")]
        [Tooltip("Base creator share on first sale (8500 = 85%)")]
        public int baseCreatorShare = 8500;
        
        [Tooltip("Base perpetual royalty percentage (2000 = 20%)")]
        public int baseRoyaltyPercentage = 2000;
        
        [Tooltip("Platform fee on secondary sales (1000 = 10%)")]
        public int secondaryPlatformFee = 1000;

        [Header("Tier Bonuses")]
        [Tooltip("Tier 4 (Elite) creator share bonus (200 = 2%)")]
        public int tier4Bonus = 200;
        
        [Tooltip("Tier 5 (Legendary) creator share bonus (500 = 5%)")]
        public int tier5Bonus = 500;
        
        [Tooltip("Tier 4 royalty bonus (200 = 2%)")]
        public int tier4RoyaltyBonus = 200;
        
        [Tooltip("Tier 5 royalty bonus (500 = 5%)")]
        public int tier5RoyaltyBonus = 500;

        [Header("Treasury Configuration")]
        [Tooltip("Treasury wallet address for platform fees")]
        public string treasuryAddress = "0x0000000000000000000000000000000000000000";

        // Events
        public event Action<string, float, float> OnPrimarySaleCompleted;
        public event Action<string, float, float, float> OnSecondarySaleCompleted;
        public event Action<string, float> OnRoyaltyPaid;
        public event Action<string, int, int> OnCreatorTierChanged;

        // Statistics
        private Dictionary<string, CreatorStats> creatorStats = new Dictionary<string, CreatorStats>();
        
        [Serializable]
        public class CreatorStats
        {
            public string creatorAddress;
            public int tier = 1;
            public float totalSales = 0f;
            public float totalRoyalties = 0f;
            public int assetsCreated = 0;
            public float reputationScore = 50f;
            public int followerCount = 0;
        }

        [Serializable]
        public class TierRequirements
        {
            public float minSales;
            public float minReputation;
            public int minFollowers;
        }

        private Dictionary<int, TierRequirements> tierRequirements = new Dictionary<int, TierRequirements>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeTierRequirements();
            
            Core.LogManager.Info("=== Creator Economy Initialized ===", new {
                baseCreatorShare = baseCreatorShare / 100f + "%",
                baseRoyalty = baseRoyaltyPercentage / 100f + "%",
                message = "Creators First, Always"
            });
        }

        private void InitializeTierRequirements()
        {
            // Tier 1: New Creator (default)
            tierRequirements[1] = new TierRequirements { 
                minSales = 0f, 
                minReputation = 0f, 
                minFollowers = 0 
            };
            
            // Tier 2: Emerging Creator ($1K+)
            tierRequirements[2] = new TierRequirements { 
                minSales = 1000f, 
                minReputation = 60f, 
                minFollowers = 25 
            };
            
            // Tier 3: Established Creator ($10K+)
            tierRequirements[3] = new TierRequirements { 
                minSales = 10000f, 
                minReputation = 75f, 
                minFollowers = 100 
            };
            
            // Tier 4: Elite Creator ($100K+)
            tierRequirements[4] = new TierRequirements { 
                minSales = 100000f, 
                minReputation = 85f, 
                minFollowers = 500 
            };
            
            // Tier 5: Legendary Creator ($1M+)
            tierRequirements[5] = new TierRequirements { 
                minSales = 1000000f, 
                minReputation = 95f, 
                minFollowers = 1000 
            };
        }

        /// <summary>
        /// Process primary (first) sale with 85% creator revenue
        /// </summary>
        public bool ProcessPrimarySale(string creatorAddress, string buyerAddress, float salePrice, string assetId)
        {
            try
            {
                if (salePrice <= 0)
                {
                    Core.LogManager.Warn("Sale price must be positive", new { salePrice });
                    return false;
                }

                // Get or create creator stats
                if (!creatorStats.ContainsKey(creatorAddress))
                {
                    creatorStats[creatorAddress] = new CreatorStats { creatorAddress = creatorAddress };
                }

                CreatorStats stats = creatorStats[creatorAddress];
                
                // Calculate revenue split based on tier
                int creatorShare = CalculateCreatorShare(stats.tier);
                float creatorAmount = salePrice * (creatorShare / 10000f);
                float platformAmount = salePrice - creatorAmount;

                // Update statistics
                stats.totalSales += salePrice;
                stats.assetsCreated++;

                // Check for tier upgrade
                CheckAndUpgradeTier(creatorAddress);

                // Log transaction
                Core.LogManager.Info($"Primary Sale Completed", new {
                    creator = creatorAddress,
                    buyer = buyerAddress,
                    salePrice,
                    creatorAmount,
                    platformAmount,
                    creatorShare = creatorShare / 100f + "%",
                    tier = stats.tier
                });

                // Trigger event
                OnPrimarySaleCompleted?.Invoke(creatorAddress, salePrice, creatorAmount);

                return true;
            }
            catch (Exception ex)
            {
                Core.LogManager.Exception(ex, "Failed to process primary sale");
                return false;
            }
        }

        /// <summary>
        /// Process secondary sale with 20% perpetual royalty
        /// </summary>
        public bool ProcessSecondarySale(
            string creatorAddress, 
            string sellerAddress, 
            string buyerAddress, 
            float salePrice, 
            string assetId)
        {
            try
            {
                if (salePrice <= 0)
                {
                    Core.LogManager.Warn("Sale price must be positive", new { salePrice });
                    return false;
                }

                // Get creator stats
                if (!creatorStats.ContainsKey(creatorAddress))
                {
                    Core.LogManager.Warn("Creator not found in registry", new { creatorAddress });
                    return false;
                }

                CreatorStats stats = creatorStats[creatorAddress];
                
                // Calculate splits based on tier
                int royaltyPercentage = CalculateRoyaltyPercentage(stats.tier);
                float royaltyAmount = salePrice * (royaltyPercentage / 10000f);
                float platformAmount = salePrice * (secondaryPlatformFee / 10000f);
                float sellerAmount = salePrice - royaltyAmount - platformAmount;

                // Update statistics
                stats.totalRoyalties += royaltyAmount;

                // Check for tier upgrade
                CheckAndUpgradeTier(creatorAddress);

                // Log transaction
                Core.LogManager.Info($"Secondary Sale Completed", new {
                    creator = creatorAddress,
                    seller = sellerAddress,
                    buyer = buyerAddress,
                    salePrice,
                    royaltyAmount,
                    sellerAmount,
                    platformAmount,
                    royaltyPercentage = royaltyPercentage / 100f + "%",
                    tier = stats.tier
                });

                // Trigger events
                OnSecondarySaleCompleted?.Invoke(creatorAddress, salePrice, royaltyAmount, sellerAmount);
                OnRoyaltyPaid?.Invoke(creatorAddress, royaltyAmount);

                return true;
            }
            catch (Exception ex)
            {
                Core.LogManager.Exception(ex, "Failed to process secondary sale");
                return false;
            }
        }

        /// <summary>
        /// Calculate creator share based on tier
        /// Tier 1-3: 85% (8500)
        /// Tier 4: 87% (8700)
        /// Tier 5: 90% (9000)
        /// </summary>
        private int CalculateCreatorShare(int tier)
        {
            if (tier >= 5) return baseCreatorShare + tier5Bonus; // 90%
            if (tier == 4) return baseCreatorShare + tier4Bonus; // 87%
            return baseCreatorShare; // 85%
        }

        /// <summary>
        /// Calculate royalty percentage based on tier
        /// Tier 1-3: 20% (2000)
        /// Tier 4: 22% (2200)
        /// Tier 5: 25% (2500)
        /// </summary>
        private int CalculateRoyaltyPercentage(int tier)
        {
            if (tier >= 5) return baseRoyaltyPercentage + tier5RoyaltyBonus; // 25%
            if (tier == 4) return baseRoyaltyPercentage + tier4RoyaltyBonus; // 22%
            return baseRoyaltyPercentage; // 20%
        }

        /// <summary>
        /// Check if creator qualifies for tier upgrade
        /// </summary>
        private void CheckAndUpgradeTier(string creatorAddress)
        {
            if (!creatorStats.ContainsKey(creatorAddress))
                return;

            CreatorStats stats = creatorStats[creatorAddress];
            int currentTier = stats.tier;
            int newTier = CalculateTier(stats);

            if (newTier > currentTier)
            {
                stats.tier = newTier;
                
                Core.LogManager.Info($"Creator Tier Upgraded!", new {
                    creator = creatorAddress,
                    oldTier = currentTier,
                    newTier = newTier,
                    totalRevenue = stats.totalSales + stats.totalRoyalties,
                    reputation = stats.reputationScore,
                    followers = stats.followerCount
                });

                OnCreatorTierChanged?.Invoke(creatorAddress, currentTier, newTier);
            }
        }

        /// <summary>
        /// Calculate appropriate tier based on stats
        /// </summary>
        private int CalculateTier(CreatorStats stats)
        {
            float totalRevenue = stats.totalSales + stats.totalRoyalties;

            // Check from highest tier down
            for (int tier = 5; tier >= 1; tier--)
            {
                if (tierRequirements.ContainsKey(tier))
                {
                    TierRequirements req = tierRequirements[tier];
                    
                    if (totalRevenue >= req.minSales &&
                        stats.reputationScore >= req.minReputation &&
                        stats.followerCount >= req.minFollowers)
                    {
                        return tier;
                    }
                }
            }

            return 1; // Default to Tier 1
        }

        /// <summary>
        /// Update creator reputation score
        /// </summary>
        public void UpdateReputation(string creatorAddress, float change, string reason)
        {
            if (!creatorStats.ContainsKey(creatorAddress))
            {
                creatorStats[creatorAddress] = new CreatorStats { creatorAddress = creatorAddress };
            }

            CreatorStats stats = creatorStats[creatorAddress];
            float oldScore = stats.reputationScore;
            
            stats.reputationScore = Mathf.Clamp(stats.reputationScore + change, 0f, 100f);

            Core.LogManager.Info($"Reputation Updated", new {
                creator = creatorAddress,
                oldScore,
                newScore = stats.reputationScore,
                change,
                reason
            });

            // Check for tier changes
            CheckAndUpgradeTier(creatorAddress);
        }

        /// <summary>
        /// Update creator follower count
        /// </summary>
        public void UpdateFollowerCount(string creatorAddress, int newCount)
        {
            if (!creatorStats.ContainsKey(creatorAddress))
            {
                creatorStats[creatorAddress] = new CreatorStats { creatorAddress = creatorAddress };
            }

            creatorStats[creatorAddress].followerCount = newCount;
            CheckAndUpgradeTier(creatorAddress);
        }

        /// <summary>
        /// Get creator statistics
        /// </summary>
        public CreatorStats GetCreatorStats(string creatorAddress)
        {
            if (!creatorStats.ContainsKey(creatorAddress))
            {
                creatorStats[creatorAddress] = new CreatorStats { creatorAddress = creatorAddress };
            }
            
            return creatorStats[creatorAddress];
        }

        /// <summary>
        /// Calculate projected royalties for an asset
        /// </summary>
        public float CalculateProjectedRoyalties(string creatorAddress, float assetPrice, int estimatedResales)
        {
            if (!creatorStats.ContainsKey(creatorAddress))
                return 0f;

            CreatorStats stats = creatorStats[creatorAddress];
            int royaltyPercentage = CalculateRoyaltyPercentage(stats.tier);
            
            float royaltyPerSale = assetPrice * (royaltyPercentage / 10000f);
            float projectedTotal = royaltyPerSale * estimatedResales;

            return projectedTotal;
        }

        /// <summary>
        /// Get creator tier benefits description
        /// </summary>
        public string GetTierBenefits(int tier)
        {
            switch (tier)
            {
                case 1:
                    return "85% first sale, 20% perpetual royalties";
                case 2:
                    return "85% first sale, 20% perpetual royalties, Enhanced AI tools";
                case 3:
                    return "85% first sale, 20% perpetual royalties, Advanced AI tools, Analytics";
                case 4:
                    return "87% first sale, 22% perpetual royalties, Premium tools, Marketing support";
                case 5:
                    return "90% first sale, 25% perpetual royalties, Zero fees, Featured placement, Governance";
                default:
                    return "Unknown tier";
            }
        }

        /// <summary>
        /// Check if creator qualifies for specific tier
        /// </summary>
        public bool QualifiesForTier(string creatorAddress, int tier)
        {
            if (!creatorStats.ContainsKey(creatorAddress) || !tierRequirements.ContainsKey(tier))
                return false;

            CreatorStats stats = creatorStats[creatorAddress];
            TierRequirements req = tierRequirements[tier];
            float totalRevenue = stats.totalSales + stats.totalRoyalties;

            return (totalRevenue >= req.minSales &&
                    stats.reputationScore >= req.minReputation &&
                    stats.followerCount >= req.minFollowers);
        }

        /// <summary>
        /// Get global creator economy statistics
        /// </summary>
        public Dictionary<string, object> GetGlobalStats()
        {
            float totalPaidToCreators = 0f;
            float totalRoyaltiesPaid = 0f;
            int totalCreators = creatorStats.Count;
            int tier5Creators = 0;
            int tier4Creators = 0;
            int tier3Creators = 0;

            foreach (var stats in creatorStats.Values)
            {
                totalPaidToCreators += stats.totalSales + stats.totalRoyalties;
                totalRoyaltiesPaid += stats.totalRoyalties;
                
                if (stats.tier >= 5) tier5Creators++;
                else if (stats.tier == 4) tier4Creators++;
                else if (stats.tier == 3) tier3Creators++;
            }

            return new Dictionary<string, object>
            {
                { "totalCreators", totalCreators },
                { "totalPaidToCreators", totalPaidToCreators },
                { "totalRoyaltiesPaid", totalRoyaltiesPaid },
                { "legendaryCreators", tier5Creators },
                { "eliteCreators", tier4Creators },
                { "establishedCreators", tier3Creators },
                { "averagePerCreator", totalCreators > 0 ? totalPaidToCreators / totalCreators : 0f }
            };
        }
    }
}
