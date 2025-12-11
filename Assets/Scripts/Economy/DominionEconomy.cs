using UnityEngine;
using System;

namespace OmniWorld.Economy
{
    /// <summary>
    /// Core implementation of the Dominion Economy
    /// Quantum-calibrated financial physics engine
    /// Price calculation: P_OMNI = (U_p × H_r × C_x) / (D_r × Z_i × T_s)
    /// </summary>
    public class DominionEconomy : MonoBehaviour
    {
        private static DominionEconomy _instance;
        public static DominionEconomy Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<DominionEconomy>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("DominionEconomy");
                        _instance = go.AddComponent<DominionEconomy>();
                    }
                }
                return _instance;
            }
        }

        [Header("Economic Parameters")]
        [Tooltip("Demand Rate - Active wallet interactions/sec per zone")]
        public float demandRate = 1.0f;
        
        [Tooltip("Zone Inflation Index - Oracle feed linked to real-world CPI")]
        public float zoneInflationIndex = 1.0f;
        
        [Tooltip("Tier Scale - Multiplier (1-5) based on Asset Rarity")]
        public int tierScale = 1;
        
        [Tooltip("User Prestige - Governance score (0.1 - 1.0)")]
        public float userPrestige = 0.5f;
        
        [Tooltip("Housing Rarity - Inverse supply curve metric")]
        public float housingRarity = 1.0f;
        
        [Tooltip("Circulation Coefficient - Velocity of money")]
        public float circulationCoefficient = 1.0f;

        [Header("Token Info")]
        public float omniTokenPrice = 0.01f;
        public long totalSupply = 2000000000; // 2 billion
        public float circulatingSupply = 0f;

        [Header("Inactivity Tax")]
        [Tooltip("Tax rate applied to inactive wallets (progressive)")]
        public float inactivityTaxRate = 0.05f;
        public int inactivityThresholdDays = 30;

        public event Action<float> OnTokenPriceUpdated;
        public event Action<string, float> OnTransactionProcessed;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeEconomy();
        }

        private void InitializeEconomy()
        {
            Debug.Log("Dominion Economy Initialized");
            Debug.Log($"Total $OMNI Supply: {totalSupply:N0}");
            CalculateTokenPrice();
        }

        /// <summary>
        /// Calculate token price using the Quantum Algorithm
        /// P_OMNI = (U_p × H_r × C_x) / (D_r × Z_i × T_s)
        /// </summary>
        public float CalculateTokenPrice()
        {
            // Prevent division by zero
            float denominator = Mathf.Max(demandRate * zoneInflationIndex * tierScale, 0.001f);
            float numerator = userPrestige * housingRarity * circulationCoefficient;
            
            omniTokenPrice = numerator / denominator;
            
            // Apply bounds to prevent extreme values
            omniTokenPrice = Mathf.Clamp(omniTokenPrice, 0.001f, 100f);
            
            OnTokenPriceUpdated?.Invoke(omniTokenPrice);
            
            Debug.Log($"$OMNI Price Calculated: ${omniTokenPrice:F4}");
            return omniTokenPrice;
        }

        /// <summary>
        /// Process a transaction and validate against economic constraints
        /// </summary>
        public bool ProcessTransaction(string walletAddress, float amount, string transactionType)
        {
            if (amount <= 0)
            {
                Debug.LogWarning("Transaction amount must be positive");
                return false;
            }

            Debug.Log($"Processing {transactionType}: {amount} $OMNI for wallet {walletAddress}");
            
            // TODO: Implement actual transaction validation
            // - Check wallet balance
            // - Validate against anti-fraud rules
            // - Apply inactivity tax if applicable
            // - Update circulation metrics
            
            OnTransactionProcessed?.Invoke(walletAddress, amount);
            
            // Update circulation coefficient based on transaction velocity
            UpdateCirculationMetrics(amount);
            
            return true;
        }

        /// <summary>
        /// Calculate inactivity tax for dormant wallets
        /// Progressive rates over time
        /// </summary>
        public float CalculateInactivityTax(int daysInactive, float balance)
        {
            if (daysInactive < inactivityThresholdDays)
                return 0f;

            // Progressive tax: increases with inactivity duration
            float taxMultiplier = Mathf.Min((daysInactive - inactivityThresholdDays) / 30f, 5f);
            float tax = balance * inactivityTaxRate * taxMultiplier;
            
            return tax;
        }

        /// <summary>
        /// Update circulation metrics based on transaction activity
        /// </summary>
        private void UpdateCirculationMetrics(float transactionAmount)
        {
            // Increase circulation coefficient with transaction activity
            circulationCoefficient = Mathf.Min(circulationCoefficient + (transactionAmount * 0.0001f), 2.0f);
            
            // Recalculate token price with updated metrics
            CalculateTokenPrice();
        }

        /// <summary>
        /// Update zone inflation index from oracle feed
        /// </summary>
        public void UpdateInflationIndex(float newIndex)
        {
            zoneInflationIndex = Mathf.Max(newIndex, 0.1f);
            Debug.Log($"Inflation Index Updated: {zoneInflationIndex:F4}");
            CalculateTokenPrice();
        }

        /// <summary>
        /// Calculate ROI for property investment
        /// </summary>
        public float CalculatePropertyROI(float purchasePrice, float currentValue, int daysHeld)
        {
            if (purchasePrice <= 0 || daysHeld <= 0)
                return 0f;

            float profit = currentValue - purchasePrice;
            float roi = (profit / purchasePrice) * 100f;
            
            return roi;
        }

        private void Update()
        {
            // Periodic economic updates (every 60 seconds in real-time)
            if (Time.frameCount % (60 * 60) == 0) // Assuming 60 FPS
            {
                // Gradually decay circulation coefficient if no activity
                circulationCoefficient = Mathf.Max(circulationCoefficient * 0.99f, 0.1f);
                CalculateTokenPrice();
            }
        }
    }
}
