# Contract Deployment Guide

## Overview

This guide provides step-by-step instructions for deploying the OMNI ecosystem smart contracts to Binance Smart Chain (BSC).

## Prerequisites

- Node.js and npm installed
- Hardhat or Remix IDE
- BSC Mainnet/Testnet RPC access
- Private keys for deployment (NEVER commit these to git)
- BNB for gas fees
- All contract addresses from BSC_INTEGRATION.md

## Environment Setup

### 1. Install Dependencies

```bash
cd Assets/Contracts/Source
npm install
```

### 2. Configure Environment Variables

Create a `.env` file in `Assets/Contracts/Source/`:

```bash
# Network Configuration
BSC_RPC_URL=https://bsc-dataseed.binance.org/
BSC_TESTNET_RPC=https://data-seed-prebsc-1-s1.binance.org:8545/

# Deployment Account (DO NOT COMMIT ACTUAL PRIVATE KEY)
DEPLOYER_PRIVATE_KEY=your_private_key_here

# System Addresses
DEVELOPER_ADDRESS=0xCbBf46e4BFbcd099601D63482866EEC68Ebd8992
RECOVERY_ADDRESS=0x81f5cfdD2851362E5986b26614517638Af89E514
TREASURY_WALLET=0x94140Fdcf420ce32E24c55B91a425fa71d80427B
OMNI_REVENUE_WALLET=0xD6490ADA82710c4a43D71E9f6D7E4bF8CD1282CF

# Token Addresses
OMNICOIN_TOKEN=0x8979878229e2e55b80e116283DF22d8203919f27
USDC_TOKEN=0x8AC76a51cc950d9822D68b83fE1Ad97B32Cd580d

# BSC Scan API (for verification)
BSCSCAN_API_KEY=your_bscscan_api_key
```

## Deployment Order

Deploy contracts in this specific order to handle dependencies:

### 1. BusinessLicenseNFT
### 2. MathGodEvaluator
### 3. OmniTransactionVerifier (Optional)

## Deployment Scripts

### Script 1: Deploy BusinessLicenseNFT

Create `scripts/deploy-business-license.js`:

```javascript
const hre = require("hardhat");

async function main() {
    console.log("Deploying BusinessLicenseNFT...");

    // Get deployment account
    const [deployer] = await hre.ethers.getSigners();
    console.log("Deploying with account:", deployer.address);
    console.log("Account balance:", (await deployer.getBalance()).toString());

    // Get addresses from environment
    const TREASURY_WALLET = process.env.TREASURY_WALLET;
    const OMNI_REVENUE_WALLET = process.env.OMNI_REVENUE_WALLET;
    const OMNICOIN_TOKEN = process.env.OMNICOIN_TOKEN;
    const USDC_TOKEN = process.env.USDC_TOKEN;
    const DEVELOPER_ADDRESS = process.env.DEVELOPER_ADDRESS;
    const RECOVERY_ADDRESS = process.env.RECOVERY_ADDRESS;

    // Deploy contract
    const BusinessLicenseNFT = await hre.ethers.getContractFactory("BusinessLicenseNFT");
    const businessLicense = await BusinessLicenseNFT.deploy(
        TREASURY_WALLET,
        OMNI_REVENUE_WALLET,
        OMNICOIN_TOKEN,
        USDC_TOKEN,
        DEVELOPER_ADDRESS,
        RECOVERY_ADDRESS
    );

    await businessLicense.deployed();

    console.log("BusinessLicenseNFT deployed to:", businessLicense.address);
    console.log("Transaction hash:", businessLicense.deployTransaction.hash);

    // Wait for block confirmations
    console.log("Waiting for block confirmations...");
    await businessLicense.deployTransaction.wait(5);

    // Verify on BSCScan
    console.log("Verifying contract on BSCScan...");
    try {
        await hre.run("verify:verify", {
            address: businessLicense.address,
            constructorArguments: [
                TREASURY_WALLET,
                OMNI_REVENUE_WALLET,
                OMNICOIN_TOKEN,
                USDC_TOKEN,
                DEVELOPER_ADDRESS,
                RECOVERY_ADDRESS
            ],
        });
        console.log("Contract verified successfully");
    } catch (error) {
        console.log("Verification error:", error.message);
    }

    console.log("\n=== Deployment Summary ===");
    console.log("Contract: BusinessLicenseNFT");
    console.log("Address:", businessLicense.address);
    console.log("Network:", hre.network.name);
    console.log("Treasury:", TREASURY_WALLET);
    console.log("Revenue:", OMNI_REVENUE_WALLET);
    console.log("========================\n");
}

main()
    .then(() => process.exit(0))
    .catch((error) => {
        console.error(error);
        process.exit(1);
    });
```

### Script 2: Deploy MathGodEvaluator

Create `scripts/deploy-math-god-evaluator.js`:

```javascript
const hre = require("hardhat");

async function main() {
    console.log("Deploying MathGodEvaluator...");

    const [deployer] = await hre.ethers.getSigners();
    console.log("Deploying with account:", deployer.address);

    const TREASURY_WALLET = process.env.TREASURY_WALLET;
    const OMNI_REVENUE_WALLET = process.env.OMNI_REVENUE_WALLET;
    const OMNICOIN_TOKEN = process.env.OMNICOIN_TOKEN;
    const DEVELOPER_ADDRESS = process.env.DEVELOPER_ADDRESS;
    const RECOVERY_ADDRESS = process.env.RECOVERY_ADDRESS;

    const MathGodEvaluator = await hre.ethers.getContractFactory("MathGodEvaluator");
    const evaluator = await MathGodEvaluator.deploy(
        TREASURY_WALLET,
        OMNI_REVENUE_WALLET,
        OMNICOIN_TOKEN,
        DEVELOPER_ADDRESS,
        RECOVERY_ADDRESS
    );

    await evaluator.deployed();

    console.log("MathGodEvaluator deployed to:", evaluator.address);
    console.log("Transaction hash:", evaluator.deployTransaction.hash);

    await evaluator.deployTransaction.wait(5);

    try {
        await hre.run("verify:verify", {
            address: evaluator.address,
            constructorArguments: [
                TREASURY_WALLET,
                OMNI_REVENUE_WALLET,
                OMNICOIN_TOKEN,
                DEVELOPER_ADDRESS,
                RECOVERY_ADDRESS
            ],
        });
        console.log("Contract verified successfully");
    } catch (error) {
        console.log("Verification error:", error.message);
    }

    console.log("\n=== Deployment Summary ===");
    console.log("Contract: MathGodEvaluator");
    console.log("Address:", evaluator.address);
    console.log("========================\n");
}

main()
    .then(() => process.exit(0))
    .catch((error) => {
        console.error(error);
        process.exit(1);
    });
```

### Script 3: Deploy OmniTransactionVerifier

Create `scripts/deploy-transaction-verifier.js`:

```javascript
const hre = require("hardhat");

async function main() {
    console.log("Deploying OmniTransactionVerifier...");

    const [deployer] = await hre.ethers.getSigners();
    console.log("Deploying with account:", deployer.address);

    const DEVELOPER_ADDRESS = process.env.DEVELOPER_ADDRESS;
    const RECOVERY_ADDRESS = process.env.RECOVERY_ADDRESS;

    const OmniTransactionVerifier = await hre.ethers.getContractFactory("OmniTransactionVerifier");
    const verifier = await OmniTransactionVerifier.deploy(
        DEVELOPER_ADDRESS,
        RECOVERY_ADDRESS
    );

    await verifier.deployed();

    console.log("OmniTransactionVerifier deployed to:", verifier.address);
    console.log("Transaction hash:", verifier.deployTransaction.hash);

    await verifier.deployTransaction.wait(5);

    try {
        await hre.run("verify:verify", {
            address: verifier.address,
            constructorArguments: [
                DEVELOPER_ADDRESS,
                RECOVERY_ADDRESS
            ],
        });
        console.log("Contract verified successfully");
    } catch (error) {
        console.log("Verification error:", error.message);
    }

    console.log("\n=== Deployment Summary ===");
    console.log("Contract: OmniTransactionVerifier");
    console.log("Address:", verifier.address);
    console.log("========================\n");
}

main()
    .then(() => process.exit(0))
    .catch((error) => {
        console.error(error);
        process.exit(1);
    });
```

## Running Deployments

### Deploy to BSC Testnet (Recommended First)

```bash
# 1. Deploy BusinessLicenseNFT
npx hardhat run scripts/deploy-business-license.js --network bscTestnet

# 2. Deploy MathGodEvaluator
npx hardhat run scripts/deploy-math-god-evaluator.js --network bscTestnet

# 3. Deploy OmniTransactionVerifier (optional)
npx hardhat run scripts/deploy-transaction-verifier.js --network bscTestnet
```

### Deploy to BSC Mainnet

⚠️ **WARNING**: Ensure all contracts are thoroughly tested on testnet first!

```bash
# 1. Deploy BusinessLicenseNFT
npx hardhat run scripts/deploy-business-license.js --network bscMainnet

# 2. Deploy MathGodEvaluator
npx hardhat run scripts/deploy-math-god-evaluator.js --network bscMainnet

# 3. Deploy OmniTransactionVerifier (optional)
npx hardhat run scripts/deploy-transaction-verifier.js --network bscMainnet
```

## Post-Deployment Checklist

After deploying each contract:

- [ ] Save contract address immediately
- [ ] Verify contract on BSCScan
- [ ] Test basic functionality
- [ ] Update `.env` with deployed address
- [ ] Update documentation with address
- [ ] Announce address if public

## Testing Deployed Contracts

### Test BusinessLicenseNFT

```javascript
// scripts/test-business-license.js
const hre = require("hardhat");

async function main() {
    const contractAddress = "DEPLOYED_ADDRESS_HERE";
    const contract = await hre.ethers.getContractAt("BusinessLicenseNFT", contractAddress);
    
    // Check configuration
    console.log("Treasury:", await contract.treasuryWallet());
    console.log("Revenue:", await contract.revenueWallet());
    console.log("Default Price:", (await contract.defaultMintPrice()).toString());
    
    // Test mint (requires OMNICOIN approval first)
    // const tx = await contract.mintWithOMNICoin(yourAddress, "ipfs://metadata", 0);
    // await tx.wait();
    // console.log("Mint successful!");
}

main().catch(console.error);
```

## Troubleshooting

### Common Issues

**Issue**: "Insufficient funds for gas"
- **Solution**: Ensure deployer account has enough BNB

**Issue**: "Nonce too low"
- **Solution**: Reset nonce or wait for pending transactions

**Issue**: "Contract verification failed"
- **Solution**: Ensure constructor arguments match exactly, check BSCScan API key

**Issue**: "Transaction underpriced"
- **Solution**: Increase gas price in hardhat.config.js

### Gas Optimization Tips

- Deploy during low network usage
- Use `gasPrice` parameter to control costs
- Batch operations when possible
- Consider deploying to testnet first to estimate costs

## Security Reminders

1. **NEVER** commit private keys to git
2. Use hardware wallets for production deployments
3. Test thoroughly on testnet first
4. Conduct security audits before mainnet
5. Have emergency pause procedures ready
6. Monitor contracts after deployment

## Support

For deployment issues:
- Check Hardhat documentation
- Review BSC developer docs
- Consult OpenZeppelin guides
- Reach out to the development team

## Appendix: Hardhat Configuration

Ensure your `hardhat.config.js` includes:

```javascript
require("@nomiclabs/hardhat-ethers");
require("@nomiclabs/hardhat-etherscan");
require("dotenv").config();

module.exports = {
  solidity: {
    version: "0.8.20",
    settings: {
      optimizer: {
        enabled: true,
        runs: 200
      }
    }
  },
  networks: {
    bscTestnet: {
      url: process.env.BSC_TESTNET_RPC,
      chainId: 97,
      accounts: [process.env.DEPLOYER_PRIVATE_KEY]
    },
    bscMainnet: {
      url: process.env.BSC_RPC_URL,
      chainId: 56,
      accounts: [process.env.DEPLOYER_PRIVATE_KEY]
    }
  },
  etherscan: {
    apiKey: {
      bsc: process.env.BSCSCAN_API_KEY,
      bscTestnet: process.env.BSCSCAN_API_KEY
    }
  }
};
```
