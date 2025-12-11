# OmniWorld Quick Start Guide

Welcome to OmniWorld! This guide will help you get started quickly.

## For Players

### 1. Get OmniPass NFT (Coming Soon)
Visit [omniworld.io/pass](https://omniworld.io/pass) to mint your citizen pass.

### 2. Set Up Your Wallet
- Install [MetaMask](https://metamask.io/) browser extension
- Create or import a wallet
- Add Polygon network to MetaMask
- Get some MATIC for gas fees

### 3. Download the Game (Coming Soon)
- Download for your platform (PC, Mac, Android, iOS)
- Install and launch
- Connect your wallet
- Choose your starting city

### 4. Start Playing
- Complete the tutorial quest
- Explore your first city (OmniLanta recommended for beginners)
- Earn $OMNI through missions
- Purchase your first property
- Create content and earn royalties

## For Developers

### Prerequisites
```bash
# Check versions
unity --version    # Should be 2022.3 LTS or later
node --version     # Should be 16+
python --version   # Should be 3.9+
```

### Quick Setup
```bash
# Clone repository
git clone https://github.com/fxgeniusllc-oss/OMNI-WORLD-.git
cd OMNI-WORLD-

# Install dependencies
npm run install

# Configure environment
cp .env.example .env
# Edit .env with your settings

# Start backend
npm run backend:dev

# Open Unity project
# Unity Hub → Open Project → Select OMNI-WORLD- folder
```

### First Steps

1. **Explore the Code**
   - Start with `Assets/_Core/GameManager.cs`
   - Review `Assets/Scripts/Economy/DominionEconomy.cs`
   - Check out `Backend/api/main.py`

2. **Run the Backend API**
   ```bash
   npm run backend:dev
   ```
   Visit http://localhost:8000/docs for API documentation

3. **Open Unity**
   - Open `Assets/Scenes/MainMenu.unity`
   - Press Play to test
   - Connect wallet (simulated in development)

4. **Deploy Test Contracts**
   ```bash
   npm run contracts:deploy:testnet
   ```

## For Creators

### Getting Started

1. **Register as Creator**
   - Complete OmniPass verification
   - Submit portfolio for tier assignment
   - Access creator tools dashboard

2. **Create Your First Asset**
   - Use in-world creation tools
   - AI-assisted content generation
   - One-click NFT minting

3. **Earn Revenue**
   - 85% of first sale proceeds
   - 20% perpetual royalties
   - Build your creator brand

### Creator Tools

- **Asset Creator** - 3D modeling and design tools
- **Music Studio** - OmniTunes for music creation
- **Quest Builder** - Create missions and rewards
- **AI Assistant** - GPT-powered creation help

## Quick Commands

### Backend
```bash
npm run backend          # Start backend API
npm run backend:dev      # Start with auto-reload
```

### Smart Contracts
```bash
npm run contracts:compile       # Compile contracts
npm run contracts:test          # Run tests
npm run contracts:deploy:testnet  # Deploy to testnet
```

### MCP Agent
```bash
npm run agent:init              # Initialize agent
python Backend/omni_agent_mcp.py --task "Your task" --priority HIGH
```

## Common Tasks

### Connect Wallet in Unity
```csharp
// In your script
await WalletConnect.Instance.ConnectWallet(WalletType.MetaMask);
```

### Query Token Price
```csharp
float price = DominionEconomy.Instance.CalculateTokenPrice();
Debug.Log($"Current $OMNI price: ${price}");
```

### Travel to Another City
```csharp
TransitSystem.Instance.TravelToCity("OmniVegas", walletAddress);
```

### Create an NPC
```csharp
NPCData npc = ProceduralGeneration.Instance.GenerateNPC();
```

## API Quick Reference

### Get Player Info
```bash
curl http://localhost:8000/api/players/{wallet_address}
```

### Get Token Price
```bash
curl http://localhost:8000/api/economy/token-price
```

### Get Available Quests
```bash
curl http://localhost:8000/api/quests/available
```

## Troubleshooting

### Unity Won't Start
- Ensure Unity 2022.3 LTS is installed
- Check Unity Hub for errors

### Backend Won't Start
```bash
pip install -r Backend/requirements.txt
python Backend/api/main.py
```

### Contract Deployment Fails
- Check `.env` has correct values
- Ensure wallet has test MATIC
- Verify network settings in `hardhat.config.js`

## Next Steps

1. **Read the Documentation**
   - [Development Guide](DEVELOPMENT_GUIDE.md)
   - [Architecture](ARCHITECTURE.md)
   - [Main README](../README.md)

2. **Join the Community**
   - [Discord](https://discord.gg/omniworld)
   - [Twitter](https://twitter.com/omniworldhq)
   - [GitHub Discussions](https://github.com/fxgeniusllc-oss/OMNI-WORLD-/discussions)

3. **Start Building**
   - Follow tutorials
   - Explore example code
   - Contribute to the project

## Resources

- 📖 [Full Documentation](../README.md)
- 💻 [API Docs](http://localhost:8000/docs)
- 🎮 [Unity Tutorials](https://learn.unity.com/)
- ⛓️ [Polygon Docs](https://docs.polygon.technology/)

## Support

- **Email:** dev@omniworld.io
- **Discord:** https://discord.gg/omniworld
- **GitHub Issues:** https://github.com/fxgeniusllc-oss/OMNI-WORLD-/issues

---

**Welcome to OmniWorld! Let's build the future together.** 🌐✨
