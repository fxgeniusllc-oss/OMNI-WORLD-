# 🏆 OmniWorld Trophy NFT Ranking System

## Overview

The **OmniWorld Trophy NFT Ranking System** rewards tournament winners with collectible Trophy NFTs that grant VIP access, prestige, and in some cases, passive income through embedded smart contract trading bots.

Trophy NFTs serve as **digital championship rings** - lasting proof of competitive dominance that can be traded in the OmniWorld NFT marketplace.

---

## 🎖️ Trophy NFT Ranks

Each Trophy NFT has a rarity tier based on the tournament's prestige, difficulty, and prize pool.

| Rank | NFT Name | Tournament Type | Exclusive Perks & Value |
|------|----------|-----------------|-------------------------|
| 🥇 **Gold** | OmniWorld Legend Trophy | Elite Championship Events | - VIP access to all tournament tiers<br>- 2x XP boost<br>- 1,000 prestige points<br>- Eligible for smart contract trading bot<br>- Permanent leaderboard recognition |
| 🥈 **Silver** | OmniWorld Master Medal | Mid-Tier Competitive Tournaments | - VIP access to Silver & Bronze tournaments<br>- 1.5x XP boost<br>- 500 prestige points<br>- Eligible for smart contract trading bot<br>- Tournament fee discounts |
| 🥉 **Bronze** | OmniWorld Challenger Badge | Entry-Level or Weekly Events | - VIP access to Bronze tournaments<br>- 1.25x XP boost<br>- 250 prestige points<br>- Access to beginner exclusive events |

### Rarity & Value

- **Gold Trophy NFTs** are the most valuable - Awarded only for major championship wins with substantial prize pools (10,000+ OMNI)
- **Silver Trophy NFTs** hold mid-level value - Earned from high-stakes tournaments (2,000+ OMNI prize pool)
- **Bronze Trophy NFTs** are more accessible - For casual players and newcomers entering the competition scene

---

## 💰 Smart Contract Trading Bots

### What Are They?

Special **Gold** and **Silver** Trophy NFTs include embedded smart contract trading bots that generate **passive income using alternative tokens** (USDC, WBTC, ETH) - **NOT $OMNI**.

This design protects the $OMNI ecosystem from inflation while rewarding elite players with ongoing financial benefits.

### How It Works

1. **Automatic Minting**: When a player wins a qualifying tournament, their Trophy NFT is automatically minted with a smart contract trading bot embedded inside
2. **Alternative Token Trading**: The bot trades with USDC (stablecoin), WBTC (Wrapped Bitcoin), or other alternative assets
3. **Passive Income**: The bot executes automated trading strategies and yield farming to generate returns
4. **Time-Limited**: Bots remain active for **6-12 months** after trophy minting, then convert to standard collectible status

### Trading Strategies

| Strategy | Risk Level | Assets | Returns |
|----------|-----------|---------|---------|
| **Conservative** | Low | USDC, stablecoins | 1% per trade, ~5-8% APY yield farming |
| **Balanced** | Medium | USDC, DAI, USDT | 2% per trade, moderate risk |
| **Aggressive** | Higher | ETH, WBTC, volatile assets | 3% per trade, higher potential |

### Income Potential

| Trophy Rank | Estimated Monthly Earnings | Max Daily Earnings |
|-------------|---------------------------|-------------------|
| 🥇 **Gold** | 500 - 2,500 USDC | 100 USDC |
| 🥈 **Silver** | 100 - 1,000 USDC | 50 USDC |
| 🥉 **Bronze** | ❌ Not eligible | N/A |

*Actual earnings vary based on market conditions and trading strategy*

### Benefits

✅ **Passive Income** - Earn while not actively competing  
✅ **Protected $OMNI Ecosystem** - Uses alternative tokens to prevent inflation  
✅ **Increased NFT Value** - Trophy NFTs with active bots are more valuable  
✅ **Time-Limited** - Prevents permanent yield farming exploits  
✅ **Tradeable** - Sell the NFT with the bot included  

---

## 🎮 VIP Tournament Access

Trophy NFTs grant exclusive access to VIP tournaments:

### Access Matrix

| Trophy Owned | Bronze Tournaments | Silver Tournaments | Gold Tournaments |
|--------------|-------------------|-------------------|------------------|
| **Gold Trophy** | ✅ Full Access | ✅ Full Access | ✅ Full Access |
| **Silver Trophy** | ✅ Full Access | ✅ Full Access | ❌ No Access |
| **Bronze Trophy** | ✅ Full Access | ❌ No Access | ❌ No Access |
| **No Trophy** | ✅ Open to All | ❌ No Access | ❌ No Access |

### Tournament Entry Requirements

- **Bronze Tournaments**: Open to everyone (no trophy required)
- **Silver Tournaments**: Requires at least one Silver or Gold trophy
- **Gold Tournaments**: Requires at least one Gold trophy

---

## 📊 Trophy NFT Features

### Core Properties

```csharp
public class TrophyNFT {
    // Identity
    string nftId;
    string tokenId;
    string contractAddress;
    
    // Trophy Details
    string trophyName;
    TrophyRank rank;              // Bronze, Silver, Gold
    string tournamentName;
    DateTime tournamentDate;
    
    // VIP Access
    bool canAccessGoldTournaments;
    bool canAccessSilverTournaments;
    bool canAccessBronzeTournaments;
    float xpBoostMultiplier;
    
    // Smart Contract Bot
    bool hasSmartContract;
    string tradingBotAddress;
    float totalEarningsUSDC;
    DateTime botExpirationDate;
    
    // Marketplace
    bool isListed;
    float listingPrice;
    bool canResell;
}
```

### Trophy Value Calculation

Trophy value is determined by:

1. **Base Value** (by rank):
   - Gold: 5,000 OMNI
   - Silver: 2,000 OMNI
   - Bronze: 500 OMNI

2. **Trading Bot Value** (if active):
   - Estimated remaining earnings × 70% (discounted future value)

3. **Age Appreciation**:
   - Slight appreciation over time (0.1% per day)
   - Historical prestige increases value

---

## 🔗 Smart Contract Architecture

### OmniTrophyNFT.sol

ERC-721 contract for Trophy NFTs with:
- Trophy rank tiers (Bronze, Silver, Gold)
- VIP access control
- Tournament metadata storage
- Trading bot integration
- Transfer hooks for VIP access updates

**Key Functions:**
```solidity
function mintTrophy(
    address winner,
    TrophyRank rank,
    string tournamentName,
    bool hasSmartContract,
    address tradingBotAddress
) returns (uint256);

function hasVIPAccess(address holder, TrophyRank minRank) returns (bool);
function getXPBoostMultiplier(address holder) returns (uint256);
```

### TrophyTradingBot.sol

Smart contract for automated trading with:
- Alternative token trading (USDC, WBTC, ETH)
- Daily earning limits (anti-abuse)
- Time-limited operation (6-12 months)
- Multiple trading strategies
- Yield farming for Gold/Silver

**Key Functions:**
```solidity
function executeTrade(address tokenIn, address tokenOut, uint256 amount) returns (uint256);
function withdrawEarnings() external;
function executeYieldFarming(uint256 amount) returns (uint256);
function getBotStatus() returns (bool, uint256, uint256);
```

---

## 🎯 Tournament Manager

### Creating Tournaments

```csharp
TournamentManager.Instance.CreateTournament(
    name: "OmniWorld Championship 2025",
    requiredRank: TrophyRank.Gold,
    entryFee: 100f,
    prizePool: 50000f,
    tournamentType: "PvP Battle Royale",
    difficulty: TournamentDifficulty.Championship
);
```

### Trophy Awards

Trophies are automatically awarded based on:
- **Tournament Difficulty**: Championship → Gold, Elite → Silver, Beginner → Bronze
- **Prize Pool**: Higher pools → Higher rank trophies
- **Final Rankings**: Top 3 players receive trophies

### Player Registration

```csharp
TournamentManager.Instance.RegisterPlayer(
    tournamentId: "TOUR-123",
    playerAddress: "0x...",
    playerTrophies: trophiesArray
);
```

System automatically:
- Checks VIP access requirements
- Validates trophy ownership
- Collects entry fees
- Awards XP boost based on owned trophies

---

## 💎 NFT Marketplace Integration

### Listing Trophy for Sale

```csharp
trophy.isListed = true;
trophy.listingPrice = 5000f; // Price in OMNI
```

### Trophy Transfer

```csharp
bool success = trophy.TransferOwnership(
    newOwner: "0x...",
    salePrice: 5000f
);
```

When transferred:
- VIP access automatically granted to new owner
- Trading bot beneficiary updated
- Transfer count incremented
- Marketplace fees applied (5% platform + 8% sales tax + 20% royalty)

---

## 🔐 Security & Anti-Abuse Measures

### Daily Earning Limits

To prevent exploitation:
- **Gold Bots**: Max 100 USDC/day
- **Silver Bots**: Max 50 USDC/day
- **Bronze**: No bot (N/A)

### Time Limitation

- All trading bots expire after **6-12 months**
- After expiration, trophy remains as collectible
- Prevents permanent yield farming

### Withdrawal Protection

- Only trophy owner (beneficiary) can withdraw earnings
- Reentrancy guards on all financial functions
- Emergency withdrawal for stuck funds (owner only)

---

## 📈 Economic Impact

### Why Alternative Tokens?

Using USDC, WBTC, and other alternative tokens instead of $OMNI:

✅ **Protects $OMNI from inflation** - Bot earnings don't dilute token supply  
✅ **Stable returns** - Stablecoins provide predictable income  
✅ **Liquidity separation** - Keeps tournament economy separate from passive income  
✅ **Real value** - Players earn USD-pegged assets  

### Revenue Flow

```
Tournament Entry Fees (OMNI)
    ↓
Prize Pool Distribution (OMNI)
    ↓
Trophy NFT Minted
    ↓
Trading Bot Activated (uses USDC/ETH reserves)
    ↓
Passive Income (USDC) → Trophy Holder
```

---

## 🚀 Implementation Checklist

- [x] OmniTrophyNFT.sol smart contract
- [x] TrophyTradingBot.sol smart contract
- [x] TrophyNFT.cs Unity data model
- [x] TournamentManager.cs tournament logic
- [x] ContractBridge.cs Web3 integration
- [x] VIP access control system
- [x] Trading bot attachment logic
- [x] Passive income calculation
- [x] Marketplace integration support
- [x] Documentation

---

## 🎮 Usage Examples

### Award Trophy After Tournament

```csharp
// Tournament completes
Tournament tournament = /* ... */;
List<TournamentPlayer> finalRankings = /* ... */;

TournamentManager.Instance.CompleteTournament(
    tournament.tournamentId,
    finalRankings
);
// Trophies automatically minted to top 3 players
```

### Check Player VIP Access

```csharp
TrophyNFT[] playerTrophies = GetPlayerTrophies(playerAddress);
bool canJoin = TournamentManager.Instance.HasRequiredVIPAccess(
    playerTrophies,
    TrophyRank.Gold
);
```

### Withdraw Bot Earnings

```csharp
float earnings = await ContractBridge.Instance.WithdrawBotEarnings(
    trophy.tradingBotAddress
);
trophy.UpdateBotEarnings(earnings);
```

---

## 📚 Additional Resources

- [OmniWorld Whitepaper](../README.md)
- [Smart Contract Documentation](../Assets/Contracts/Source/)
- [Tournament System Guide](./TOURNAMENT_GUIDE.md)
- [NFT Marketplace](./MARKETPLACE.md)

---

## 🤝 Contributing

Trophy system improvements welcome! Please follow OmniWorld development guidelines.

---

**Built with ❤️ for the OmniWorld Community**
