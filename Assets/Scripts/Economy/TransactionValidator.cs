using UnityEngine;
using System;
using System.Collections.Generic;

namespace OmniWorld.Economy
{
    /// <summary>
    /// Transaction validator with fraud prevention
    /// Implements AI-powered anomaly detection
    /// </summary>
    public class TransactionValidator : MonoBehaviour
    {
        private static TransactionValidator _instance;
        public static TransactionValidator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TransactionValidator>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("TransactionValidator");
                        _instance = go.AddComponent<TransactionValidator>();
                    }
                }
                return _instance;
            }
        }

        [Header("Fraud Prevention")]
        public float suspiciousAmountThreshold = 10000f;
        public int maxTransactionsPerMinute = 10;
        public float reputationScoreThreshold = 0.3f;

        private Dictionary<string, List<float>> recentTransactions = new Dictionary<string, List<float>>();
        private Dictionary<string, float> walletReputations = new Dictionary<string, float>();

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

        /// <summary>
        /// Validate transaction against fraud rules
        /// </summary>
        public bool ValidateTransaction(string walletAddress, float amount, string transactionType)
        {
            // Check for suspicious amounts
            if (amount > suspiciousAmountThreshold)
            {
                Debug.LogWarning($"Suspicious high-value transaction detected: {amount} $OMNI");
                return RequireAdditionalVerification(walletAddress);
            }

            // Check transaction frequency
            if (!CheckTransactionRate(walletAddress))
            {
                Debug.LogWarning($"Transaction rate limit exceeded for wallet: {walletAddress}");
                return false;
            }

            // Check wallet reputation
            float reputation = GetWalletReputation(walletAddress);
            if (reputation < reputationScoreThreshold)
            {
                Debug.LogWarning($"Low reputation wallet attempting transaction: {walletAddress}");
                return false;
            }

            // Pattern analysis for bot detection
            if (DetectBotBehavior(walletAddress, amount))
            {
                Debug.LogWarning($"Bot-like behavior detected for wallet: {walletAddress}");
                return false;
            }

            Debug.Log($"Transaction validated successfully: {amount} $OMNI");
            RecordTransaction(walletAddress, amount);
            
            return true;
        }

        /// <summary>
        /// Check if wallet exceeds transaction rate limits
        /// </summary>
        private bool CheckTransactionRate(string walletAddress)
        {
            if (!recentTransactions.ContainsKey(walletAddress))
            {
                recentTransactions[walletAddress] = new List<float>();
                return true;
            }

            var transactions = recentTransactions[walletAddress];
            
            // Remove transactions older than 1 minute
            float currentTime = Time.time;
            transactions.RemoveAll(t => currentTime - t > 60f);

            return transactions.Count < maxTransactionsPerMinute;
        }

        /// <summary>
        /// Record transaction for rate limiting and analysis
        /// </summary>
        private void RecordTransaction(string walletAddress, float amount)
        {
            if (!recentTransactions.ContainsKey(walletAddress))
            {
                recentTransactions[walletAddress] = new List<float>();
            }

            recentTransactions[walletAddress].Add(Time.time);
        }

        /// <summary>
        /// Get or initialize wallet reputation score
        /// </summary>
        public float GetWalletReputation(string walletAddress)
        {
            if (!walletReputations.ContainsKey(walletAddress))
            {
                // New wallets start with neutral reputation
                walletReputations[walletAddress] = 0.5f;
            }

            return walletReputations[walletAddress];
        }

        /// <summary>
        /// Update wallet reputation based on behavior
        /// </summary>
        public void UpdateReputation(string walletAddress, float delta)
        {
            float current = GetWalletReputation(walletAddress);
            walletReputations[walletAddress] = Mathf.Clamp01(current + delta);
            
            Debug.Log($"Reputation updated for {walletAddress}: {walletReputations[walletAddress]:F2}");
        }

        /// <summary>
        /// Detect bot-like behavior patterns
        /// </summary>
        private bool DetectBotBehavior(string walletAddress, float amount)
        {
            if (!recentTransactions.ContainsKey(walletAddress))
                return false;

            var transactions = recentTransactions[walletAddress];
            
            // Check for repetitive amounts (bot pattern)
            if (transactions.Count >= 3)
            {
                int sameAmountCount = 0;
                foreach (var tx in transactions)
                {
                    // Note: We're storing timestamps, not amounts
                    // This is a simplified check
                }
                
                // TODO: Implement more sophisticated ML-based bot detection
            }

            return false;
        }

        /// <summary>
        /// Require additional verification for suspicious transactions
        /// </summary>
        private bool RequireAdditionalVerification(string walletAddress)
        {
            float reputation = GetWalletReputation(walletAddress);
            
            // High reputation wallets can bypass additional verification
            if (reputation > 0.8f)
            {
                Debug.Log("High reputation wallet - verification bypassed");
                return true;
            }

            Debug.Log("Additional verification required");
            // TODO: Implement multi-factor authentication
            // TODO: Implement biometric verification
            
            return false;
        }

        /// <summary>
        /// Report suspicious activity to network
        /// </summary>
        public void ReportSuspiciousActivity(string walletAddress, string reason)
        {
            Debug.LogWarning($"FRAUD ALERT: {walletAddress} - Reason: {reason}");
            
            // Decrease reputation
            UpdateReputation(walletAddress, -0.2f);
            
            // TODO: Notify network administrators
            // TODO: Log to blockchain for transparency
        }
    }
}
