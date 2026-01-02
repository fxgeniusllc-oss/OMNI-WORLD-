using UnityEngine;
using System;
using System.Collections.Generic;

namespace OmniWorld.Security
{
    /// <summary>
    /// Fraud Prevention System - AI-powered security and anti-fraud measures
    /// Detects and prevents wash trading, bot activity, and suspicious patterns
    /// </summary>
    public class FraudPrevention : MonoBehaviour
    {
        private static FraudPrevention _instance;
        private static readonly object _lock = new object();
        
        public static FraudPrevention Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = FindObjectOfType<FraudPrevention>();
                            if (_instance == null)
                            {
                                GameObject go = new GameObject("FraudPrevention");
                                _instance = go.AddComponent<FraudPrevention>();
                                DontDestroyOnLoad(go);
                            }
                        }
                    }
                }
                return _instance;
            }
        }

        [Header("Detection Thresholds")]
        [Tooltip("Maximum transactions per minute before flagging")]
        public int maxTransactionsPerMinute = 10;
        
        [Tooltip("Maximum same-buyer transactions before flagging")]
        public int maxSameBuyerTransactions = 3;
        
        [Tooltip("Minimum time between transactions (seconds)")]
        public float minTimeBetweenTransactions = 5f;
        
        [Tooltip("Suspicious price variation threshold (%)")]
        public float suspiciousPriceVariation = 50f;

        [Header("Reputation Impact")]
        [Tooltip("Reputation penalty for suspicious activity")]
        public float suspiciousActivityPenalty = -5f;
        
        [Tooltip("Reputation penalty for confirmed fraud")]
        public float confirmedFraudPenalty = -25f;

        // Transaction tracking
        private Dictionary<string, List<Transaction>> walletTransactions = new Dictionary<string, List<Transaction>>();
        private Dictionary<string, List<string>> buyerSellerPairs = new Dictionary<string, List<string>>();
        private Dictionary<string, float> walletRiskScores = new Dictionary<string, float>();
        private HashSet<string> flaggedWallets = new HashSet<string>();
        private HashSet<string> bannedWallets = new HashSet<string>();

        public event Action<string, string> OnSuspiciousActivityDetected;
        public event Action<string> OnWalletFlagged;
        public event Action<string> OnWalletBanned;

        [Serializable]
        private class Transaction
        {
            public string transactionId;
            public string seller;
            public string buyer;
            public float price;
            public DateTime timestamp;
            public string assetId;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Core.LogManager.Info("=== Fraud Prevention System Initialized ===", new {
                message = "AI-powered security and anti-fraud protection active"
            });
        }

        /// <summary>
        /// Validate transaction for fraud patterns
        /// </summary>
        public bool ValidateTransaction(string seller, string buyer, string assetId, float price)
        {
            // Check if either wallet is banned
            if (bannedWallets.Contains(seller) || bannedWallets.Contains(buyer))
            {
                Core.LogManager.Warn("Transaction blocked - banned wallet", new { seller, buyer });
                return false;
            }

            // Cannot sell to self
            if (seller == buyer)
            {
                Core.LogManager.Warn("Transaction blocked - self-trading", new { seller });
                FlagSuspiciousActivity(seller, "Attempted self-trading");
                return false;
            }

            // Check for wash trading pattern
            if (IsWashTrading(seller, buyer))
            {
                Core.LogManager.Warn("Transaction blocked - wash trading detected", new { seller, buyer });
                FlagSuspiciousActivity(seller, "Wash trading pattern detected");
                FlagSuspiciousActivity(buyer, "Wash trading pattern detected");
                return false;
            }

            // Check transaction frequency
            if (IsExcessiveFrequency(seller))
            {
                Core.LogManager.Warn("Transaction blocked - excessive frequency", new { seller });
                FlagSuspiciousActivity(seller, "Excessive transaction frequency");
                return false;
            }

            // Check for price manipulation
            if (IsPriceManipulation(assetId, price))
            {
                Core.LogManager.Warn("Transaction flagged - suspicious price", new { assetId, price });
                FlagSuspiciousActivity(seller, "Suspicious pricing pattern");
            }

            // Record transaction
            RecordTransaction(seller, buyer, assetId, price);

            // Update risk scores
            UpdateRiskScore(seller);
            UpdateRiskScore(buyer);

            return true;
        }

        /// <summary>
        /// Detect wash trading (circular trading between same parties)
        /// </summary>
        private bool IsWashTrading(string seller, string buyer)
        {
            string pairKey = $"{seller}:{buyer}";
            string reversePairKey = $"{buyer}:{seller}";

            // Check if these wallets have traded before
            if (!buyerSellerPairs.ContainsKey(pairKey))
            {
                buyerSellerPairs[pairKey] = new List<string>();
            }

            // Count current direction trades
            int currentPairCount = buyerSellerPairs[pairKey].Count;
            
            // Check reverse pair - if they trade back and forth repeatedly, it's wash trading
            if (buyerSellerPairs.ContainsKey(reversePairKey))
            {
                int reversePairCount = buyerSellerPairs[reversePairKey].Count;
                
                // Both directions have excessive trades = wash trading pattern
                if (currentPairCount >= maxSameBuyerTransactions && 
                    reversePairCount >= maxSameBuyerTransactions)
                {
                    return true; // Wash trading detected
                }
            }

            return false;
        }

        /// <summary>
        /// Check for excessive transaction frequency
        /// </summary>
        private bool IsExcessiveFrequency(string wallet)
        {
            if (!walletTransactions.ContainsKey(wallet))
                return false;

            var transactions = walletTransactions[wallet];
            DateTime now = DateTime.UtcNow;
            DateTime oneMinuteAgo = now.AddMinutes(-1);

            // Count transactions in last minute
            int recentTransactions = 0;
            foreach (var tx in transactions)
            {
                if (tx.timestamp > oneMinuteAgo)
                {
                    recentTransactions++;
                }
            }

            return recentTransactions >= maxTransactionsPerMinute;
        }

        /// <summary>
        /// Detect price manipulation patterns
        /// TODO: Integrate with market data for comprehensive price analysis
        /// Current implementation: Basic threshold check for extremely suspicious prices
        /// </summary>
        private bool IsPriceManipulation(string assetId, float price)
        {
            // Basic sanity check: Flag extremely high prices (> $100,000)
            // or suspiciously low prices (< $0.01) as potentially manipulative
            const float maxReasonablePrice = 100000f;
            const float minReasonablePrice = 0.01f;
            
            if (price > maxReasonablePrice || price < minReasonablePrice)
            {
                return true; // Suspicious price range
            }
            
            // TODO: Implement advanced checks:
            // - Compare with average market price for category
            // - Detect sudden price spikes (>50% change)
            // - Analyze historical pricing patterns
            // - Cross-reference with similar asset prices
            
            return false;
        }

        /// <summary>
        /// Record transaction for pattern analysis
        /// </summary>
        private void RecordTransaction(string seller, string buyer, string assetId, float price)
        {
            Transaction tx = new Transaction
            {
                transactionId = Guid.NewGuid().ToString(),
                seller = seller,
                buyer = buyer,
                price = price,
                timestamp = DateTime.UtcNow,
                assetId = assetId
            };

            // Record for seller
            if (!walletTransactions.ContainsKey(seller))
            {
                walletTransactions[seller] = new List<Transaction>();
            }
            walletTransactions[seller].Add(tx);

            // Record for buyer
            if (!walletTransactions.ContainsKey(buyer))
            {
                walletTransactions[buyer] = new List<Transaction>();
            }
            walletTransactions[buyer].Add(tx);

            // Record buyer-seller pair
            string pairKey = $"{seller}:{buyer}";
            if (!buyerSellerPairs.ContainsKey(pairKey))
            {
                buyerSellerPairs[pairKey] = new List<string>();
            }
            buyerSellerPairs[pairKey].Add(assetId);

            // Clean old transactions (keep last 24 hours)
            CleanOldTransactions(seller);
            CleanOldTransactions(buyer);
        }

        /// <summary>
        /// Clean transactions older than 24 hours
        /// </summary>
        private void CleanOldTransactions(string wallet)
        {
            if (!walletTransactions.ContainsKey(wallet))
                return;

            DateTime cutoff = DateTime.UtcNow.AddHours(-24);
            walletTransactions[wallet].RemoveAll(tx => tx.timestamp < cutoff);
        }

        /// <summary>
        /// Update risk score for wallet
        /// </summary>
        private void UpdateRiskScore(string wallet)
        {
            if (!walletRiskScores.ContainsKey(wallet))
            {
                walletRiskScores[wallet] = 0f;
            }

            float riskScore = 0f;

            // Check transaction frequency
            if (walletTransactions.ContainsKey(wallet))
            {
                int txCount = walletTransactions[wallet].Count;
                if (txCount > 50) riskScore += 20f;
                else if (txCount > 20) riskScore += 10f;
            }

            // Check flagged status
            if (flaggedWallets.Contains(wallet))
            {
                riskScore += 30f;
            }

            walletRiskScores[wallet] = Mathf.Clamp(riskScore, 0f, 100f);

            // Auto-flag if risk score is too high
            if (riskScore >= 70f && !flaggedWallets.Contains(wallet))
            {
                FlagWallet(wallet, "High risk score");
            }

            // Auto-ban if risk score is critical
            if (riskScore >= 95f && !bannedWallets.Contains(wallet))
            {
                BanWallet(wallet, "Critical risk score");
            }
        }

        /// <summary>
        /// Flag suspicious activity
        /// </summary>
        private void FlagSuspiciousActivity(string wallet, string reason)
        {
            Core.LogManager.Warn("Suspicious Activity Detected", new { wallet, reason });
            
            OnSuspiciousActivityDetected?.Invoke(wallet, reason);

            // Update reputation if Creator Economy is available
            if (Economy.CreatorEconomy.Instance != null)
            {
                Economy.CreatorEconomy.Instance.UpdateReputation(wallet, suspiciousActivityPenalty, reason);
            }

            // Increase risk score
            if (!walletRiskScores.ContainsKey(wallet))
            {
                walletRiskScores[wallet] = 0f;
            }
            walletRiskScores[wallet] = Mathf.Min(walletRiskScores[wallet] + 15f, 100f);
        }

        /// <summary>
        /// Flag wallet for review
        /// </summary>
        public void FlagWallet(string wallet, string reason)
        {
            if (flaggedWallets.Contains(wallet))
                return;

            flaggedWallets.Add(wallet);
            
            Core.LogManager.Warn("Wallet Flagged", new { wallet, reason });
            
            OnWalletFlagged?.Invoke(wallet);

            // Update reputation
            if (Economy.CreatorEconomy.Instance != null)
            {
                Economy.CreatorEconomy.Instance.UpdateReputation(wallet, suspiciousActivityPenalty, $"Wallet flagged: {reason}");
            }
        }

        /// <summary>
        /// Ban wallet from platform
        /// </summary>
        public void BanWallet(string wallet, string reason)
        {
            if (bannedWallets.Contains(wallet))
                return;

            bannedWallets.Add(wallet);
            flaggedWallets.Remove(wallet); // Remove from flagged since now banned
            
            Core.LogManager.Error("Wallet Banned", new { wallet, reason });
            
            OnWalletBanned?.Invoke(wallet);

            // Severe reputation penalty
            if (Economy.CreatorEconomy.Instance != null)
            {
                Economy.CreatorEconomy.Instance.UpdateReputation(wallet, confirmedFraudPenalty, $"Banned: {reason}");
            }
        }

        /// <summary>
        /// Unban wallet (admin function)
        /// </summary>
        public void UnbanWallet(string wallet, string reason)
        {
            if (!bannedWallets.Contains(wallet))
                return;

            bannedWallets.Remove(wallet);
            walletRiskScores[wallet] = 50f; // Reset to neutral
            
            Core.LogManager.Info("Wallet Unbanned", new { wallet, reason });
        }

        /// <summary>
        /// Get wallet risk score
        /// </summary>
        public float GetRiskScore(string wallet)
        {
            return walletRiskScores.ContainsKey(wallet) ? walletRiskScores[wallet] : 0f;
        }

        /// <summary>
        /// Check if wallet is flagged
        /// </summary>
        public bool IsWalletFlagged(string wallet)
        {
            return flaggedWallets.Contains(wallet);
        }

        /// <summary>
        /// Check if wallet is banned
        /// </summary>
        public bool IsWalletBanned(string wallet)
        {
            return bannedWallets.Contains(wallet);
        }

        /// <summary>
        /// Get fraud prevention statistics
        /// </summary>
        public Dictionary<string, object> GetStats()
        {
            return new Dictionary<string, object>
            {
                { "totalWalletsMonitored", walletTransactions.Count },
                { "flaggedWallets", flaggedWallets.Count },
                { "bannedWallets", bannedWallets.Count },
                { "totalTransactionsTracked", GetTotalTransactionCount() },
                { "highRiskWallets", CountHighRiskWallets() }
            };
        }

        private int GetTotalTransactionCount()
        {
            int total = 0;
            foreach (var txList in walletTransactions.Values)
            {
                total += txList.Count;
            }
            return total;
        }

        private int CountHighRiskWallets()
        {
            int count = 0;
            foreach (var score in walletRiskScores.Values)
            {
                if (score >= 70f) count++;
            }
            return count;
        }
    }
}
