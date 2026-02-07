# BSC Integration & Contract Deployment Guide

## Overview

This document provides the complete configuration and deployment specifications for integrating the OMNI ecosystem with Binance Smart Chain (BSC).

## Network Configuration

### Binance Smart Chain Mainnet
- **RPC URL**: `https://bsc-dataseed.binance.org/`
- **Chain ID**: `56`
- **Currency**: BNB
- **Block Explorer**: https://bscscan.com/

### BSC Testnet
- **RPC URL**: `https://data-seed-prebsc-1-s1.binance.org:8545/`
- **Chain ID**: `97`
- **Block Explorer**: https://testnet.bscscan.com/

## Key Addresses

### Administrative Wallets

| Purpose | Address | Permissions |
|---------|---------|-------------|
| Developer Address | `0xCbBf46e4BFbcd099601D63482866EEC68Ebd8992` | Full admin control, contract deployment |
| Recovery Address | `0x81f5cfdD2851362E5986b26614517638Af89E514` | Equal permissions to Developer (backup/failover) |

### Treasury & Revenue

| Purpose | Address | Type |
|---------|---------|------|
| Treasury Wallet | `0x94140Fdcf420ce32E24c55B91a425fa71d80427B` | Wallet (NOT a contract) - Main revenue vault |
| Omni Revenue Wallet | `0xD6490ADA82710c4a43D71E9f6D7E4bF8CD1282CF` | Wallet - Platform operational fees |

### Token Contracts

| Purpose | Address | Type |
|---------|---------|------|
| OMNICoin Token | `0x8979878229e2e55b80e116283DF22d8203919f27` | ERC-20 Token Contract (Deployed) |

## Smart Contracts to Deploy

### Required Contracts

The following contracts need to be deployed to complete the OMNI system:

#### 1. BusinessLicenseNFT.sol
**Purpose**: Mint ownership NFTs for businesses and real estate properties

**Key Features**:
- ERC-721 compliant NFT minting
- Represents ownership of real estate and business licenses
- Auto-split payment routing on minting
- Integration with Treasury and Revenue wallets

**Revenue Flow**:
- Mint fees collected in $OMNICOIN
- Automatic split: 90% Creator, 5% Treasury, 5% Revenue Wallet

#### 2. MathGodEvaluator.sol
**Purpose**: Sellback, appraisal, and NFT value recalculation logic

**Key Features**:
- Property and asset valuation algorithms
- Market-based pricing calculations
- Sellback mechanism for NFTs
- Dynamic pricing based on market conditions

**Revenue Flow**:
- Appraisal fees route to Treasury Wallet
- Sellback transactions follow revenue split model

#### 3. OmniTransactionVerifier.sol (Optional)
**Purpose**: Transaction validation and security guardrails

**Key Features**:
- Validates authenticity of sales and purchases
- Prevents fraudulent transactions
- Additional security layer for high-value transfers
- Whitelist/blacklist functionality

**Revenue Flow**:
- No direct revenue handling (security only)

#### 4. OmniUGCRoyaltyV2.sol (Optional Upgrade)
**Purpose**: Advanced NFT marketplace with automated royalty distribution

**Key Features**:
- 3-way royalty splitter (Creator/Treasury/Revenue)
- Configurable split ratios (90/5/5 or 85/5/10)
- Perpetual creator royalties
- Secondary market transaction support

**Revenue Flow**:
- Primary sales: 90% Creator, 5% Treasury, 5% Revenue
- Secondary sales: Creator royalties + platform fees

## Revenue Split Configuration

### Default Split Model

| Recipient | Percentage | Address |
|-----------|-----------|---------|
| Content Creator/Seller | 90% | Dynamic (transaction sender) |
| Treasury Wallet | 5% | `0x94140Fdcf420ce32E24c55B91a425fa71d80427B` |
| Omni Revenue Wallet | 5% | `0xD6490ADA82710c4a43D71E9f6D7E4bF8CD1282CF` |

### Optional Enhanced Split (Phase 2)

| Recipient | Percentage | Use Case |
|-----------|-----------|----------|
| Content Creator/Seller | 85% | Higher platform utility |
| Treasury Wallet | 5% | Core reserves |
| Omni Revenue Wallet | 10% | Operational scaling |

## Payment Methods

### Configuration Options

Three payment acceptance modes are under consideration:

#### Option 1: Strict OMNICOIN Only
- **Accepted**: $OMNICOIN only
- **Benefits**: Strengthens token utility, simpler accounting
- **Drawbacks**: May limit user adoption initially

#### Option 2: Flexible Multi-Token
- **Accepted**: $OMNICOIN, BNB, USDC
- **Benefits**: Maximum accessibility, faster onboarding
- **Drawbacks**: Mixed-asset treasury, requires conversion strategy

#### Option 3: User Selectable
- **Accepted**: User chooses at purchase time
- **Benefits**: User preference, flexible adoption
- **Drawbacks**: Requires wallet picker UI implementation

### Recommendation

**Flexible Multi-Token** approach is recommended for initial launch:
- Lower barrier to entry for new users
- Higher conversion and sales volume
- Can transition to OMNICOIN-only as ecosystem matures
- Implement optional auto-conversion to OMNICOIN in Treasury

## Treasury Model

### Treasury Wallet Characteristics

- **Type**: Standard wallet (not a smart contract)
- **Purpose**: Gross revenue vault
- **Ownership**: Controlled by Developer Address with Recovery backup
- **Revenue Sources**:
  - NFT minting fees
  - Property sales (5% platform share)
  - Marketplace transaction fees
  - Service fees

### Fund Flow

```
User Payment (OMNICOIN/BNB/USDC)
        ↓
    Contract
        ↓
   Split Logic
    ↙    ↓    ↘
Creator Treasury Revenue
 (90%)   (5%)    (5%)
```

### Expense Management

- All funds accumulate until manually withdrawn
- No automatic payouts or recurring expenses
- Manual distribution for:
  - Development team payments
  - Marketing and operations
  - Staking rewards
  - Buybacks and burns
  - Partner distributions

## Security Features

### Access Control

- **Developer Address**: Full admin permissions
- **Recovery Address**: Backup admin access (equal permissions)
- **Multi-sig**: Consider upgrading to multi-sig for production

### Contract Security

- **ReentrancyGuard**: All payment functions protected
- **Access Control**: OpenZeppelin AccessControl patterns
- **Pausable**: Emergency stop functionality
- **Upgradeable**: Optional upgradeability for future enhancements

### Audit Requirements

Before mainnet deployment:
- Complete smart contract audit by reputable firm
- Security review of payment flows
- Gas optimization analysis
- Edge case testing (zero values, overflow, etc.)

## Deployment Checklist

### Pre-Deployment

- [ ] Audit all contract code
- [ ] Test on BSC Testnet thoroughly
- [ ] Verify all addresses are correct
- [ ] Configure revenue split percentages
- [ ] Set up block explorer verification scripts
- [ ] Prepare deployment documentation

### Deployment Steps

1. **Deploy BusinessLicenseNFT.sol**
   - Constructor: Developer Address, Recovery Address, Treasury, Revenue, OMNICoin
   - Verify on BSCScan
   - Test mint functionality
   
2. **Deploy MathGodEvaluator.sol**
   - Constructor: Admin addresses, pricing oracle
   - Configure initial valuation parameters
   - Test appraisal calculations

3. **Deploy OmniTransactionVerifier.sol** (Optional)
   - Constructor: Admin addresses
   - Configure verification rules
   - Test validation logic

4. **Deploy OmniUGCRoyaltyV2.sol** (Optional)
   - Constructor: All system addresses, split configuration
   - Test primary and secondary sales
   - Verify royalty calculations

### Post-Deployment

- [ ] Update .env with deployed contract addresses
- [ ] Verify all contracts on BSCScan
- [ ] Test full payment flow end-to-end
- [ ] Update frontend configuration
- [ ] Document all transaction hashes
- [ ] Announce contract addresses publicly

## Integration with Existing Systems

### Unity Integration

The deployed contracts will integrate with existing Unity systems:

```csharp
// Assets/Scripts/Web3/OmniContractManager.cs
public class OmniContractManager : MonoBehaviour
{
    public string omniCoinAddress = "0x8979878229e2e55b80e116283DF22d8203919f27";
    public string businessLicenseNFT;
    public string mathGodEvaluator;
    // ... other addresses
}
```

### Backend API Integration

FastAPI backend will expose endpoints for:
- Contract interaction
- Transaction monitoring
- Balance queries
- Revenue analytics

```python
# Backend/api/contracts.py
OMNICOIN_ADDRESS = "0x8979878229e2e55b80e116283DF22d8203919f27"
TREASURY_WALLET = "0x94140Fdcf420ce32E24c55B91a425fa71d80427B"
REVENUE_WALLET = "0xD6490ADA82710c4a43D71E9f6D7E4bF8CD1282CF"
```

## Monitoring & Analytics

### Recommended Tracking

- Treasury balance (real-time)
- Revenue wallet balance (real-time)
- Transaction volume (daily/weekly/monthly)
- Revenue splits (verify correct percentages)
- Gas usage and optimization opportunities
- Contract interaction success rates

### Tools

- BSCScan API for transaction history
- Dune Analytics for ecosystem dashboards
- Custom backend analytics service
- Alert system for large transactions

## Support & Resources

### BSC Resources
- BSC Documentation: https://docs.bnbchain.org/
- BSC Developer Portal: https://www.bnbchain.org/en/developers
- BSC Forum: https://forum.bnbchain.org/

### Contract Development
- OpenZeppelin Contracts: https://docs.openzeppelin.com/contracts/
- Hardhat Documentation: https://hardhat.org/
- Remix IDE: https://remix.ethereum.org/

## Version History

- **v1.0** (2026-01-17): Initial BSC integration specification
  - Extracted configuration from expansion files
  - Documented key addresses and contracts
  - Defined revenue split model
  - Outlined deployment process
