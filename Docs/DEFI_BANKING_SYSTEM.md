# DeFi Banking System - Technical Documentation

## Overview

The DeFi Banking System is OmniWorld's SaaS-based arbitrage and flash loan platform that enables users to generate passive income through automated trading strategies. The system contributes 30% of all profits to the Omni Treasury, creating a sustainable revenue stream for the ecosystem.

## Architecture

### Core Components

1. **DefiBankingSystem.cs** - Main system controller
2. **Flash Loan Engine** - Executes flash loans across DeFi protocols
3. **Arbitrage Scanner** - Continuously scans for profitable opportunities
4. **Treasury Integration** - Routes profits to Omni Treasury
5. **Account Management** - Handles user account linking and API keys

## Features

### 1. Account Linking (SaaS Model)

Users can link their exchange accounts to enable automated trading:

```csharp
// Link user account
DefiBankingSystem.Instance.LinkAccount(
    userAddress: "0x123...",
    exchangeApiKey: "user_api_key",
    exchangeApiSecret: "user_api_secret"
);
```

**Account Benefits:**
- Automated arbitrage execution
- Flash loan access
- Real-time profit tracking
- 70% profit share
- No manual trading required

### 2. Flash Loans

Flash loans allow users to borrow large amounts without collateral, execute profitable trades, and repay within the same transaction.

**Configuration:**
- Minimum loan: 1,000 $OMNI
- Maximum loan: 1,000,000 $OMNI
- Flash loan fee: 0.09%
- Auto-execution available

**Example:**
```csharp
// Execute flash loan with arbitrage path
DEXPlatform[] path = new DEXPlatform[] 
{ 
    DEXPlatform.Uniswap, 
    DEXPlatform.Sushiswap, 
    DEXPlatform.Quickswap 
};

FlashLoanTransaction tx = DefiBankingSystem.Instance.ExecuteFlashLoan(
    userAddress: "0x123...",
    loanAmount: 50000f,
    targetToken: "ETH",
    arbitragePath: path
);

Debug.Log($"Net profit: {tx.netProfit} $OMNI");
```

### 3. Automated Arbitrage

The system automatically scans for arbitrage opportunities every 30 seconds across major DEX platforms.

**Supported DEX Platforms:**
- Uniswap
- Sushiswap
- Quickswap
- Pancakeswap
- Curve
- Balancer

**Arbitrage Process:**
1. System detects price differences across DEXs
2. Validates profitability (minimum 0.5% profit after fees)
3. Auto-executes trade for linked accounts
4. Distributes profits (70% user, 30% treasury)

**Configuration:**
```csharp
// Minimum profit threshold
minArbitrageProfitPercent = 0.5f; // 0.5%

// Maximum slippage tolerance
maxSlippagePercent = 1.0f; // 1%

// Execution fee
arbitrageExecutionFee = 10f; // $OMNI
```

### 4. Profit Distribution

All profits are automatically split:
- **70% to User** - Direct deposit to linked wallet
- **30% to Omni Treasury** - Ecosystem revenue generation

**Example Transaction:**
```
Flash Loan: 50,000 $OMNI
Flash Loan Fee: 45 $OMNI (0.09%)
Arbitrage Profit: 500 $OMNI
Net Profit: 455 $OMNI

Distribution:
- User (70%): 318.5 $OMNI
- Treasury (30%): 136.5 $OMNI
```

### 5. Treasury Integration

The DeFi Banking system is a key revenue generator for the Omni Treasury.

**Treasury Contributions:**
- 30% of all arbitrage profits
- 30% of all flash loan profits
- Automatic contribution on each successful trade
- Real-time treasury balance tracking

```csharp
// Get treasury statistics
TreasuryStats stats = DefiBankingSystem.Instance.GetTreasuryStats();

Debug.Log($"Total contributions: {stats.totalContributions} $OMNI");
Debug.Log($"Total flash loans: {stats.totalFlashLoans}");
Debug.Log($"Success rate: {stats.successfulFlashLoans}/{stats.totalFlashLoans}");
```

## API Reference

### DefiBankingSystem

#### LinkAccount
```csharp
public bool LinkAccount(string userAddress, string exchangeApiKey, string exchangeApiSecret)
```
Link user's exchange account for automated trading.

**Parameters:**
- `userAddress` - User's wallet address
- `exchangeApiKey` - Exchange API key
- `exchangeApiSecret` - Exchange API secret

**Returns:** `true` if successful

#### ExecuteFlashLoan
```csharp
public FlashLoanTransaction ExecuteFlashLoan(
    string userAddress, 
    float loanAmount, 
    string targetToken, 
    DEXPlatform[] arbitragePath
)
```
Execute a flash loan with specified arbitrage path.

**Parameters:**
- `userAddress` - User's wallet address
- `loanAmount` - Loan amount in $OMNI
- `targetToken` - Target token symbol (e.g., "ETH", "BTC")
- `arbitragePath` - Array of DEX platforms to execute trades

**Returns:** `FlashLoanTransaction` object with results

#### GetUserStats
```csharp
public UserDeFiStats GetUserStats(string userAddress)
```
Get DeFi statistics for a user.

**Returns:** User statistics including total profit, trades, and flash loans

#### GetTreasuryStats
```csharp
public TreasuryStats GetTreasuryStats()
```
Get overall treasury statistics from DeFi operations.

**Returns:** Treasury contribution data

## Data Structures

### LinkedAccount
```csharp
public class LinkedAccount
{
    public string userAddress;
    public string exchangeApiKey;
    public string exchangeApiSecret;
    public bool isActive;
    public bool autoExecuteArbitrage;
    public System.DateTime linkedDate;
    public float totalProfit;
    public int totalArbitrageTrades;
    public int totalFlashLoans;
}
```

### FlashLoanTransaction
```csharp
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
```

### ArbitrageTransaction
```csharp
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
```

## Integration Guide

### Step 1: Setup
Add DefiBankingSystem component to a GameObject in your scene:

```csharp
GameObject defiBanking = new GameObject("DefiBankingSystem");
defiBanking.AddComponent<DefiBankingSystem>();
```

### Step 2: Link User Account
When user wants to enable DeFi features:

```csharp
string userAddress = WalletConnect.Instance.connectedAddress;
DefiBankingSystem.Instance.LinkAccount(userAddress, apiKey, apiSecret);
```

### Step 3: Execute Trades
Execute flash loans manually or let the system auto-execute arbitrage:

```csharp
// Manual flash loan
DEXPlatform[] path = new DEXPlatform[] { DEXPlatform.Uniswap, DEXPlatform.Sushiswap };
DefiBankingSystem.Instance.ExecuteFlashLoan(userAddress, 10000f, "ETH", path);

// Auto-arbitrage is enabled by default for linked accounts
```

### Step 4: Monitor Performance
Track user performance and treasury contributions:

```csharp
// User stats
UserDeFiStats stats = DefiBankingSystem.Instance.GetUserStats(userAddress);
Debug.Log($"Total profit: {stats.totalProfit} $OMNI");

// Treasury stats
TreasuryStats treasury = DefiBankingSystem.Instance.GetTreasuryStats();
Debug.Log($"Treasury contributions: {treasury.totalContributions} $OMNI");
```

## Configuration

### Default Settings
```csharp
[Header("Flash Loan Configuration")]
public float minFlashLoanAmount = 1000f;
public float maxFlashLoanAmount = 1000000f;
public float flashLoanFeePercent = 0.09f;

[Header("Arbitrage Configuration")]
public float minArbitrageProfitPercent = 0.5f;
public float maxSlippagePercent = 1.0f;
public float arbitrageExecutionFee = 10f;

[Header("Profit Sharing")]
public float userProfitSharePercent = 70f;
public float treasuryProfitSharePercent = 30f;
```

### Adjusting Parameters
Modify these values in Unity Inspector or via code:

```csharp
DefiBankingSystem.Instance.minArbitrageProfitPercent = 0.3f; // Lower threshold
DefiBankingSystem.Instance.userProfitSharePercent = 80f; // Higher user share
```

## Security Considerations

1. **API Key Storage**: In production, use secure key management (HSM, AWS KMS)
2. **Rate Limiting**: Implement per-user transaction limits
3. **Slippage Protection**: Maximum slippage prevents large losses
4. **Flash Loan Safety**: All loans must be repaid in same transaction
5. **Audit Trail**: Complete transaction history maintained

## Performance

- **Arbitrage Scan Frequency**: Every 30 seconds
- **Trade Execution**: < 1 second average
- **Profit Distribution**: Instant
- **Treasury Updates**: Real-time

## Future Enhancements

1. **Multi-chain Support**: Expand beyond Polygon to Ethereum, Solana, BSC
2. **Advanced Strategies**: MEV protection, sandwich attack detection
3. **AI-Powered Routing**: ML models for optimal path finding
4. **Lending Pools**: Create liquidity pools for flash loans
5. **DAO Governance**: Community voting on profit share percentages

## Economic Impact

The DeFi Banking system provides multiple benefits to the OmniWorld ecosystem:

1. **Passive Income for Users**: 70% profit share with zero manual effort
2. **Treasury Revenue**: Sustainable 30% contribution stream
3. **Token Utility**: Increased $OMNI demand for flash loans
4. **Ecosystem Growth**: Attracts DeFi traders and arbitrageurs
5. **Real Yield**: Actual profit generation, not just token emissions

## Example: Complete User Journey

```csharp
// 1. User connects wallet
string userAddress = "0xABC123...";

// 2. Link exchange account
bool linked = DefiBankingSystem.Instance.LinkAccount(
    userAddress, 
    "api_key_here", 
    "api_secret_here"
);

// 3. System auto-scans and detects opportunity
// (happens automatically every 30 seconds)

// 4. Auto-execute arbitrage
// User receives notification: "Arbitrage executed! Profit: 250 $OMNI"

// 5. Check earnings
UserDeFiStats stats = DefiBankingSystem.Instance.GetUserStats(userAddress);
// Output: Total profit: 175 $OMNI (70% of 250)

// 6. Treasury receives 75 $OMNI (30% of 250)
TreasuryStats treasury = DefiBankingSystem.Instance.GetTreasuryStats();
// Output: Total contributions: 75 $OMNI
```

## Support

For questions or issues:
- Discord: #defi-banking channel
- Email: defi@omniworld.io
- Documentation: https://docs.omniworld.io/defi-banking

---

**Built with ❤️ for the OmniWorld ecosystem**
