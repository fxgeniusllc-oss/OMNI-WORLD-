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

        [Header("Token Configuration")]
        [Tooltip("Initial OMNI token price in USD (based on economic analysis)")]
        public float initialTokenPrice = 0.035f; // $0.035 USD (Base Case Launch Price)
        
        [Tooltip("Current OMNI token price (dynamically calculated)")]
        public float omniTokenPrice = 0.035f;
        
        [Tooltip("Total token supply (2 billion tokens minted)")]
        public long totalSupply = 2000000000; // 2 billion
        
        [Tooltip("Circulating supply at launch (Public Sale: 15%)")]
        public float circulatingSupply = 300000000f; // 300 million tokens

        [Header("Deflationary Mechanics")]
        [Tooltip("Transaction burn rate (0.5% per transaction)")]
        public float transactionBurnRate = 0.005f;
        
        [Tooltip("Tax rate applied to inactive wallets (progressive)")]
        public float inactivityTaxRate = 0.05f; // 5% base rate
        
        [Tooltip("Days of inactivity before tax applies")]
        public int inactivityThresholdDays = 30;
        
        [Header("Price Stability Controls")]
        [Tooltip("Minimum allowed token price (floor)")]
        public float minTokenPrice = 0.001f; // $0.001 floor
        
        [Tooltip("Maximum allowed token price (ceiling)")]
        public float maxTokenPrice = 100f; // $100 ceiling
        
        [Tooltip("Maximum daily price change allowed")]
        public float maxDailyPriceChange = 0.20f; // ±20% max daily movement

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
            Debug.Log("=== Dominion Economy Initialized ===");
            Debug.Log($"Total $OMNI Supply: {totalSupply:N0} tokens");
            Debug.Log($"Circulating Supply: {circulatingSupply:N0} tokens ({(circulatingSupply/totalSupply)*100:F1}%)");
            Debug.Log($"Initial Token Price: ${initialTokenPrice:F4} USD");
            Debug.Log($"Launch Market Cap (Circulating): ${(circulatingSupply * initialTokenPrice):N0} USD");
            Debug.Log($"Fully Diluted Valuation (FDV): ${(totalSupply * initialTokenPrice):N0} USD");
            
            // Set initial price
            omniTokenPrice = initialTokenPrice;
            
            // Calculate dynamic price based on quantum algorithm
            CalculateTokenPrice();
        }

        /// <summary>
        /// Calculate token price using the Quantum Algorithm
        /// P_OMNI = (U_p × H_r × C_x) / (D_r × Z_i × T_s)
        /// 
        /// Economic Analysis:
        /// - Algorithmic baseline: $0.50 (with default parameters)
        /// - Market-adjusted launch: $0.035 (70% discount for growth incentive)
        /// - Target Year 3: $0.25 (7x growth)
        /// </summary>
        public float CalculateTokenPrice()
        {
            // Store previous price for change calculation
            float previousPrice = omniTokenPrice;
            
            // Prevent division by zero
            float denominator = Mathf.Max(demandRate * zoneInflationIndex * tierScale, 0.001f);
            float numerator = userPrestige * housingRarity * circulationCoefficient;
            
            // Calculate algorithmic price
            float calculatedPrice = numerator / denominator;
            
            // Apply price stability controls
            calculatedPrice = Mathf.Clamp(calculatedPrice, minTokenPrice, maxTokenPrice);
            
            // Limit daily price changes to prevent volatility
            if (previousPrice > 0)
            {
                float maxChange = previousPrice * maxDailyPriceChange;
                calculatedPrice = Mathf.Clamp(calculatedPrice, previousPrice - maxChange, previousPrice + maxChange);
            }
            
            omniTokenPrice = calculatedPrice;
            
            OnTokenPriceUpdated?.Invoke(omniTokenPrice);
            
            float changePercent = previousPrice > 0 ? ((omniTokenPrice - previousPrice) / previousPrice) * 100f : 0f;
            Debug.Log($"$OMNI Price: ${omniTokenPrice:F4} ({(changePercent >= 0 ? "+" : "")}{changePercent:F2}%)");
            
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
            
            // Apply transaction burn (deflationary mechanism)
            float burnAmount = amount * transactionBurnRate;
            float netAmount = amount - burnAmount;
            
            Debug.Log($"Transaction Burn: {burnAmount:F4} $OMNI ({transactionBurnRate*100:F2}%)");
            Debug.Log($"Net Transaction: {netAmount:F4} $OMNI");
            
            // Update circulating supply (burned tokens removed)
            circulatingSupply -= burnAmount;
            
            OnTransactionProcessed?.Invoke(walletAddress, netAmount);
            
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
        
        /// <summary>
        /// Get current market capitalization metrics
        /// </summary>
        public void GetMarketCapMetrics(out float circulatingMarketCap, out float fullyDilutedValuation)
        {
            circulatingMarketCap = circulatingSupply * omniTokenPrice;
            fullyDilutedValuation = totalSupply * omniTokenPrice;
        }
        
        /// <summary>
        /// Get total tokens burned (supply reduction)
        /// </summary>
        public float GetTotalBurned()
        {
            // Initial circulating supply was 300M
            float initialCirculating = 300000000f;
            return initialCirculating - circulatingSupply;
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
