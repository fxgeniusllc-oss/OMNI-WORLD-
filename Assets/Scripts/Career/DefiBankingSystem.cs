using UnityEngine;
using System.Collections.Generic;
using OmniWorld.Web3;
using OmniWorld.Economy;

namespace OmniWorld.Career
{
    /// <summary>
    /// DeFi Banking System with Arbitrage and Flash Loans
    /// SaaS model where users link accounts and receive profit sharing
    /// Integrates with Omni Treasury for ecosystem profit generation
    /// </summary>
    public class DefiBankingSystem : MonoBehaviour
    {
        public static DefiBankingSystem Instance { get; private set; }

        [Header("Flash Loan Configuration")]
        [Tooltip("Minimum flash loan amount in $OMNI")]
        public float minFlashLoanAmount = 1000f;

        [Tooltip("Maximum flash loan amount in $OMNI")]
        public float maxFlashLoanAmount = 1000000f;

        [Tooltip("Flash loan fee percentage (e.g., 0.09 = 0.09%)")]
        public float flashLoanFeePercent = 0.09f;

        [Header("Arbitrage Configuration")]
        [Tooltip("Minimum profit percentage to execute arbitrage (e.g., 0.5 = 0.5%)")]
        public float minArbitrageProfitPercent = 0.5f;

        [Tooltip("Maximum slippage tolerance percentage")]
        public float maxSlippagePercent = 1.0f;

        [Tooltip("Arbitrage execution fee in $OMNI")]
        public float arbitrageExecutionFee = 10f;

        [Header("Profit Sharing")]
        [Tooltip("User profit share percentage (e.g., 70 = 70%)")]
        public float userProfitSharePercent = 70f;

        [Tooltip("Treasury profit share percentage (e.g., 30 = 30%)")]
        public float treasuryProfitSharePercent = 30f;

        [Header("Treasury Integration")]
        [Tooltip("Enable automatic treasury contribution")]
        public bool enableTreasuryContribution = true;

        private Dictionary<string, LinkedAccount> linkedAccounts = new Dictionary<string, LinkedAccount>();
        private List<FlashLoanTransaction> flashLoanHistory = new List<FlashLoanTransaction>();
        private List<ArbitrageTransaction> arbitrageHistory = new List<ArbitrageTransaction>();
        private float totalTreasuryContributions = 0f;
        private int nextTransactionId = 10000;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InvokeRepeating(nameof(ScanArbitrageOpportunities), 10f, 30f); // Scan every 30 seconds
        }

        /// <summary>
        /// Link user account to DeFi banking system (SaaS model)
        /// </summary>
        public bool LinkAccount(string userAddress, string exchangeApiKey, string exchangeApiSecret)
        {
            if (linkedAccounts.ContainsKey(userAddress))
            {
                Debug.LogWarning($"Account {userAddress} is already linked");
                return false;
            }

            LinkedAccount account = new LinkedAccount
            {
                userAddress = userAddress,
                exchangeApiKey = exchangeApiKey,
                exchangeApiSecret = exchangeApiSecret,
                isActive = true,
                linkedDate = System.DateTime.Now,
                totalProfit = 0f,
                totalArbitrageTrades = 0,
                totalFlashLoans = 0
            };

            linkedAccounts[userAddress] = account;
            Debug.Log($"Account linked successfully: {userAddress}");
            return true;
        }

        /// <summary>
        /// Execute flash loan with automatic arbitrage
        /// </summary>
        public FlashLoanTransaction ExecuteFlashLoan(string userAddress, float loanAmount, string targetToken, DEXPlatform[] arbitragePath)
        {
            if (!linkedAccounts.ContainsKey(userAddress))
            {
                Debug.LogError("Account not linked. Please link account first.");
                return null;
            }

            if (loanAmount < minFlashLoanAmount || loanAmount > maxFlashLoanAmount)
            {
                Debug.LogError($"Loan amount must be between {minFlashLoanAmount} and {maxFlashLoanAmount} $OMNI");
                return null;
            }

            LinkedAccount account = linkedAccounts[userAddress];

            // Calculate flash loan fee
            float flashLoanFee = loanAmount * (flashLoanFeePercent / 100f);
            
            // Simulate flash loan execution
            Debug.Log($"Executing flash loan: {loanAmount} $OMNI for {userAddress}");
            Debug.Log($"Flash loan fee: {flashLoanFee} $OMNI ({flashLoanFeePercent}%)");

            // Execute arbitrage path
            float arbitrageProfit = ExecuteArbitragePath(loanAmount, targetToken, arbitragePath);
            float netProfit = arbitrageProfit - flashLoanFee;

            // Create transaction record
            FlashLoanTransaction transaction = new FlashLoanTransaction
            {
                id = nextTransactionId++,
                userAddress = userAddress,
                loanAmount = loanAmount,
                flashLoanFee = flashLoanFee,
                targetToken = targetToken,
                arbitragePath = arbitragePath,
                grossProfit = arbitrageProfit,
                netProfit = netProfit,
                timestamp = System.DateTime.Now,
                success = netProfit > 0
            };

            if (transaction.success)
            {
                // Distribute profits
                float userShare = netProfit * (userProfitSharePercent / 100f);
                float treasuryShare = netProfit * (treasuryProfitSharePercent / 100f);

                account.totalProfit += userShare;
                account.totalFlashLoans++;
                
                if (enableTreasuryContribution)
                {
                    ContributeToTreasury(treasuryShare);
                }

                Debug.Log($"Flash loan successful! Net profit: {netProfit} $OMNI");
                Debug.Log($"User share: {userShare} $OMNI ({userProfitSharePercent}%)");
                Debug.Log($"Treasury share: {treasuryShare} $OMNI ({treasuryProfitSharePercent}%)");
            }
            else
            {
                Debug.LogWarning($"Flash loan did not generate profit. Net: {netProfit} $OMNI");
            }

            flashLoanHistory.Add(transaction);
            linkedAccounts[userAddress] = account;

            return transaction;
        }

        /// <summary>
        /// Execute arbitrage path across DEX platforms
        /// </summary>
        private float ExecuteArbitragePath(float amount, string targetToken, DEXPlatform[] path)
        {
            float currentAmount = amount;
            
            Debug.Log($"Executing arbitrage path for {targetToken}...");
            
            foreach (DEXPlatform dex in path)
            {
                // Simulate exchange on each DEX
                float exchangeRate = GetSimulatedExchangeRate(dex, targetToken);
                float slippage = Random.Range(0f, maxSlippagePercent / 100f);
                
                currentAmount = currentAmount * exchangeRate * (1f - slippage);
                
                Debug.Log($"  {dex}: Rate {exchangeRate}, Amount: {currentAmount} $OMNI");
            }

            float profit = currentAmount - amount;
            Debug.Log($"Arbitrage path complete. Profit: {profit} $OMNI");
            
            return profit;
        }

        /// <summary>
        /// Scan for arbitrage opportunities automatically
        /// </summary>
        private void ScanArbitrageOpportunities()
        {
            Debug.Log("Scanning for arbitrage opportunities...");

            // Simulate arbitrage detection across major DEX platforms
            string[] tokens = { "ETH", "BTC", "USDC", "DAI", "MATIC" };
            
            foreach (string token in tokens)
            {
                float potentialProfit = DetectArbitrageOpportunity(token);
                
                if (potentialProfit > 0)
                {
                    Debug.Log($"Arbitrage opportunity detected for {token}: {potentialProfit} $OMNI potential profit");
                    
                    // Auto-execute for linked accounts if profitable
                    foreach (var account in linkedAccounts.Values)
                    {
                        if (account.isActive && account.autoExecuteArbitrage)
                        {
                            AutoExecuteArbitrage(account.userAddress, token, potentialProfit);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Detect arbitrage opportunity for a token
        /// </summary>
        private float DetectArbitrageOpportunity(string token)
        {
            // Simulate price differences across DEX platforms
            float uniswapPrice = GetSimulatedExchangeRate(DEXPlatform.Uniswap, token);
            float sushiswapPrice = GetSimulatedExchangeRate(DEXPlatform.Sushiswap, token);
            float quickswapPrice = GetSimulatedExchangeRate(DEXPlatform.Quickswap, token);

            float maxPrice = Mathf.Max(uniswapPrice, sushiswapPrice, quickswapPrice);
            float minPrice = Mathf.Min(uniswapPrice, sushiswapPrice, quickswapPrice);
            
            float priceDiffPercent = ((maxPrice - minPrice) / minPrice) * 100f;

            if (priceDiffPercent >= minArbitrageProfitPercent)
            {
                // Estimate profit on 10000 $OMNI
                float testAmount = 10000f;
                float estimatedProfit = testAmount * (priceDiffPercent / 100f);
                return estimatedProfit;
            }

            return 0f;
        }

        /// <summary>
        /// Auto-execute arbitrage for linked account
        /// </summary>
        private void AutoExecuteArbitrage(string userAddress, string token, float estimatedProfit)
        {
            LinkedAccount account = linkedAccounts[userAddress];
            
            // Use a portion of user's available balance or flash loan
            float tradeAmount = 10000f; // Default amount
            
            DEXPlatform[] arbitragePath = new DEXPlatform[] 
            { 
                DEXPlatform.Uniswap, 
                DEXPlatform.Sushiswap, 
                DEXPlatform.Quickswap 
            };

            ArbitrageTransaction transaction = new ArbitrageTransaction
            {
                id = nextTransactionId++,
                userAddress = userAddress,
                targetToken = token,
                tradeAmount = tradeAmount,
                arbitragePath = arbitragePath,
                executionFee = arbitrageExecutionFee,
                estimatedProfit = estimatedProfit,
                timestamp = System.DateTime.Now
            };

            // Execute the trade
            float actualProfit = ExecuteArbitragePath(tradeAmount, token, arbitragePath);
            float netProfit = actualProfit - arbitrageExecutionFee;

            transaction.actualProfit = actualProfit;
            transaction.netProfit = netProfit;
            transaction.success = netProfit > 0;

            if (transaction.success)
            {
                float userShare = netProfit * (userProfitSharePercent / 100f);
                float treasuryShare = netProfit * (treasuryProfitSharePercent / 100f);

                account.totalProfit += userShare;
                account.totalArbitrageTrades++;

                if (enableTreasuryContribution)
                {
                    ContributeToTreasury(treasuryShare);
                }

                Debug.Log($"Auto-arbitrage executed for {userAddress}: Net profit {netProfit} $OMNI");
            }

            arbitrageHistory.Add(transaction);
            linkedAccounts[userAddress] = account;
        }

        /// <summary>
        /// Contribute profits to Omni Treasury
        /// </summary>
        private void ContributeToTreasury(float amount)
        {
            totalTreasuryContributions += amount;
            
            // Integrate with DominionEconomy to add to treasury
            if (DominionEconomy.Instance != null)
            {
                Debug.Log($"Contributing {amount} $OMNI to Omni Treasury");
                Debug.Log($"Total treasury contributions: {totalTreasuryContributions} $OMNI");
                // In full implementation: DominionEconomy.Instance.AddToTreasury(amount);
            }
        }

        /// <summary>
        /// Simulate exchange rate (In production, use real DEX APIs)
        /// </summary>
        private float GetSimulatedExchangeRate(DEXPlatform dex, string token)
        {
            // Simulate slight price variations across DEXs
            float baseRate = 1.0f;
            float variation = Random.Range(-0.02f, 0.02f); // ±2% variation
            
            return baseRate + variation;
        }

        /// <summary>
        /// Get user statistics
        /// </summary>
        public UserDeFiStats GetUserStats(string userAddress)
        {
            if (!linkedAccounts.ContainsKey(userAddress))
                return null;

            LinkedAccount account = linkedAccounts[userAddress];
            
            return new UserDeFiStats
            {
                userAddress = userAddress,
                isActive = account.isActive,
                totalProfit = account.totalProfit,
                totalArbitrageTrades = account.totalArbitrageTrades,
                totalFlashLoans = account.totalFlashLoans,
                linkedDate = account.linkedDate
            };
        }

        /// <summary>
        /// Get treasury statistics
        /// </summary>
        public TreasuryStats GetTreasuryStats()
        {
            return new TreasuryStats
            {
                totalContributions = totalTreasuryContributions,
                totalFlashLoans = flashLoanHistory.Count,
                totalArbitrageTrades = arbitrageHistory.Count,
                successfulFlashLoans = flashLoanHistory.FindAll(t => t.success).Count,
                successfulArbitrageTrades = arbitrageHistory.FindAll(t => t.success).Count
            };
        }
    }

    [System.Serializable]
    public class LinkedAccount
    {
        public string userAddress;
        public string exchangeApiKey;
        public string exchangeApiSecret;
        public bool isActive;
        public bool autoExecuteArbitrage = true;
        public System.DateTime linkedDate;
        public float totalProfit;
        public int totalArbitrageTrades;
        public int totalFlashLoans;
    }

    [System.Serializable]
    public class FlashLoanTransaction
    {
        public int id;
        public string userAddress;
        public float loanAmount;
        public float flashLoanFee;
        public string targetToken;
        public DEXPlatform[] arbitragePath;
        public float grossProfit;
        public float netProfit;
        public System.DateTime timestamp;
        public bool success;
    }

    [System.Serializable]
    public class ArbitrageTransaction
    {
        public int id;
        public string userAddress;
        public string targetToken;
        public float tradeAmount;
        public DEXPlatform[] arbitragePath;
        public float executionFee;
        public float estimatedProfit;
        public float actualProfit;
        public float netProfit;
        public System.DateTime timestamp;
        public bool success;
    }

    public enum DEXPlatform
    {
        Uniswap,
        Sushiswap,
        Quickswap,
        Pancakeswap,
        Curve,
        Balancer
    }

    [System.Serializable]
    public struct UserDeFiStats
    {
        public string userAddress;
        public bool isActive;
        public float totalProfit;
        public int totalArbitrageTrades;
        public int totalFlashLoans;
        public System.DateTime linkedDate;
    }

    [System.Serializable]
    public struct TreasuryStats
    {
        public float totalContributions;
        public int totalFlashLoans;
        public int totalArbitrageTrades;
        public int successfulFlashLoans;
        public int successfulArbitrageTrades;
    }
}
