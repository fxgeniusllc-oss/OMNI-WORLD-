# OmniWorld Development Guide

## Getting Started

This guide will help you set up the OmniWorld development environment and understand the project structure.

## Prerequisites

- **Unity 2022.3 LTS** or later
- **Node.js 16+** and npm
- **Python 3.9+**
- **Git** and Git LFS
- **MetaMask** or compatible Web3 wallet

## Project Structure

```
OMNI-WORLD-/
├── Assets/
│   ├── _Core/              # Singleton Managers
│   │   ├── GameManager.cs
│   │   └── NetworkManager.cs
│   ├── Scenes/             # City Scenes
│   ├── Prefabs/            # Reusable Assets
│   ├── Scripts/
│   │   ├── Economy/        # Dominion Economy Core
│   │   ├── World/          # Environment Systems
│   │   ├── Web3/           # Blockchain Integration
│   │   └── AI/             # Intelligence Systems
│   └── Contracts/          # Smart Contracts
├── Backend/                # Python Services
│   ├── api/
│   └── omni_agent_mcp.py
└── Docs/                   # Documentation
```

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/fxgeniusllc-oss/OMNI-WORLD-.git
cd OMNI-WORLD-
```

### 2. Install Dependencies

```bash
# Install all dependencies
npm run install

# Or install individually:
npm run contracts:install
pip install -r Backend/requirements.txt
```

### 3. Configure Environment

```bash
# Copy environment template
cp .env.example .env

# Edit .env with your configuration
nano .env
```

### 4. Initialize MCP Agent

```bash
npm run agent:init
```

## Running the Project

### Backend API

```bash
# Development mode with auto-reload
npm run backend:dev

# Production mode
npm run backend
```

API Documentation: http://localhost:8000/docs

### Unity Client

1. Open Unity Hub
2. Click "Open Project"
3. Navigate to the `OMNI-WORLD-` directory
4. Open the project
5. Open `Assets/Scenes/MainMenu.unity`
6. Press Play

### Smart Contracts

```bash
# Compile contracts
npm run contracts:compile

# Run tests
npm run contracts:test

# Deploy to testnet
npm run contracts:deploy:testnet
```

## Development Workflow

### 1. Creating New Features

Use the MCP Agent to generate features with economic validation:

```bash
python Backend/omni_agent_mcp.py --task "Create player inventory system" --priority HIGH
```

### 2. Economic System

The Dominion Economy is the core of OmniWorld. All features must respect economic constraints:

- Token Price Formula: `P_OMNI = (U_p × H_r × C_x) / (D_r × Z_i × T_s)`
- Creator Revenue: 85% first sale, 20% perpetual royalties
- Inactivity Tax: Progressive rates for inactive wallets

### 3. Testing

Always test changes in the Unity Editor before committing:

1. Test in Play Mode
2. Verify economic calculations
3. Check Web3 integration
4. Test multiplayer sync

### 4. Code Style

- Follow C# naming conventions
- Use XML documentation comments
- Namespace: `OmniWorld.<Category>`
- Keep files focused and modular

## Key Systems

### GameManager
Singleton managing core game state and initialization.

### NetworkManager
Handles multiplayer synchronization and real-time events.

### DominionEconomy
Implements the quantum-calibrated economic engine.

### WalletConnect
Manages Web3 wallet connections (MetaMask, WalletConnect, OmniID).

### ContractBridge
Bridges Unity with smart contracts for NFT operations.

### NPCBrain
AI-powered NPC with dialogue and economic decision-making.

### ProceduralGeneration
Generates cities, buildings, NPCs, and quests dynamically.

## API Endpoints

### Player Management
- `POST /api/players/register` - Register new player
- `GET /api/players/{wallet}` - Get player info
- `PUT /api/players/{wallet}/balance` - Update balance

### Economy
- `GET /api/economy/token-price` - Current $OMNI price
- `GET /api/economy/stats` - Economic statistics

### Properties
- `POST /api/properties` - Register property NFT
- `GET /api/properties/{wallet}` - Get owned properties

### WebSocket
- `ws://localhost:8000/ws/{wallet}` - Real-time updates

## Smart Contracts

### OmniLandNFT (ERC-721)
Land and property NFTs with:
- Perpetual 20% royalties (EIP-2981)
- City and zone metadata
- Value tracking

### OmniItemsNFT (ERC-1155)
Consumables and items with:
- Max supply limits
- Batch minting
- Consumable burning

## Troubleshooting

### Unity Won't Open Project
- Ensure Unity 2022.3 LTS is installed
- Check Unity Hub for errors
- Verify Git LFS is installed

### Backend Won't Start
- Check Python version: `python --version`
- Install dependencies: `pip install -r Backend/requirements.txt`
- Verify port 8000 is available

### Contract Deployment Fails
- Check network configuration in `hardhat.config.js`
- Verify `PRIVATE_KEY` in `.env`
- Ensure wallet has test MATIC

## Resources

- [Unity Documentation](https://docs.unity3d.com/)
- [FastAPI Documentation](https://fastapi.tiangolo.com/)
- [Hardhat Documentation](https://hardhat.org/docs)
- [Web3.js Documentation](https://web3js.readthedocs.io/)

## Support

For questions or issues:
- GitHub Issues: https://github.com/fxgeniusllc-oss/OMNI-WORLD-/issues
- Email: dev@omniworld.io
- Discord: https://discord.gg/omniworld

## License

Proprietary - See LICENSE file for details.
