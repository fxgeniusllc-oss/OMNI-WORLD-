// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";

/**
 * @title TrophyTradingBot
 * @dev Smart contract embedded in Trophy NFTs for automated trading and passive income
 * Generates income using USDC, WBTC, or other stablecoins (NOT $OMNI)
 * This protects the $OMNI ecosystem from inflation
 */
contract TrophyTradingBot is Ownable, ReentrancyGuard {
    
    // Trading bot configuration
    struct BotConfig {
        string botName;
        TrophyRank trophyRank;
        address trophyNFTContract;
        uint256 trophyTokenId;
        address beneficiary;       // Trophy NFT holder
        uint256 activationDate;
        uint256 expirationDate;    // Bot stops after 6-12 months
        bool isActive;
        uint256 totalEarned;       // Total earnings in USDC
    }

    // Trophy ranks (matching OmniTrophyNFT)
    enum TrophyRank {
        Bronze,
        Silver,
        Gold
    }

    // Trading strategy
    enum TradingStrategy {
        Conservative,    // Low risk, stable returns
        Balanced,        // Medium risk, moderate returns
        Aggressive       // Higher risk, higher potential returns
    }

    // Supported alternative tokens (NOT OMNI)
    address public usdcToken;      // USDC stablecoin
    address public wbtcToken;      // Wrapped Bitcoin
    address public usdtToken;      // Tether stablecoin
    address public daiToken;       // DAI stablecoin

    // Bot configuration
    BotConfig public config;
    TradingStrategy public strategy;

    // Performance tracking
    uint256 public totalTrades;
    uint256 public successfulTrades;
    uint256 public failedTrades;
    uint256 public lastTradeTimestamp;

    // Daily earnings limits (to prevent abuse)
    uint256 public constant MAX_DAILY_EARNINGS_GOLD = 100 * 10**6;    // 100 USDC (6 decimals)
    uint256 public constant MAX_DAILY_EARNINGS_SILVER = 50 * 10**6;   // 50 USDC
    uint256 public constant MAX_DAILY_EARNINGS_BRONZE = 20 * 10**6;   // 20 USDC

    uint256 public dailyEarnings;
    uint256 public lastResetTimestamp;

    // Events
    event BotActivated(address indexed beneficiary, TrophyRank rank);
    event BotDeactivated(string reason);
    event TradeExecuted(string tradeType, uint256 amount, uint256 profit);
    event EarningsWithdrawn(address indexed beneficiary, uint256 amount);
    event TradingStrategyUpdated(TradingStrategy newStrategy);

    constructor(
        string memory botName,
        TrophyRank rank,
        address nftContract,
        uint256 tokenId,
        address beneficiaryAddress,
        uint256 durationMonths,
        address _usdcToken,
        address _wbtcToken
    ) {
        config.botName = botName;
        config.trophyRank = rank;
        config.trophyNFTContract = nftContract;
        config.trophyTokenId = tokenId;
        config.beneficiary = beneficiaryAddress;
        config.activationDate = block.timestamp;
        config.expirationDate = block.timestamp + (durationMonths * 30 days);
        config.isActive = true;
        config.totalEarned = 0;

        usdcToken = _usdcToken;
        wbtcToken = _wbtcToken;

        lastResetTimestamp = block.timestamp;

        // Set default strategy based on trophy rank
        if (rank == TrophyRank.Gold) {
            strategy = TradingStrategy.Aggressive;
        } else if (rank == TrophyRank.Silver) {
            strategy = TradingStrategy.Balanced;
        } else {
            strategy = TradingStrategy.Conservative;
        }

        emit BotActivated(beneficiaryAddress, rank);
    }

    /**
     * @dev Check if bot is still active
     */
    modifier onlyActive() {
        require(config.isActive, "Bot is not active");
        require(block.timestamp < config.expirationDate, "Bot has expired");
        _;
    }

    /**
     * @dev Execute automated trade (simulated)
     * In production, this would integrate with DEX protocols like Uniswap
     */
    function executeTrade(
        address tokenIn,
        address tokenOut,
        uint256 amountIn
    ) public onlyOwner onlyActive nonReentrant returns (uint256) {
        require(tokenIn != address(0) && tokenOut != address(0), "Invalid token addresses");
        require(amountIn > 0, "Amount must be greater than 0");

        // Reset daily earnings if 24 hours have passed
        if (block.timestamp >= lastResetTimestamp + 1 days) {
            dailyEarnings = 0;
            lastResetTimestamp = block.timestamp;
        }

        // Check daily earnings limit based on trophy rank
        uint256 maxDailyEarnings = _getMaxDailyEarnings();
        require(dailyEarnings < maxDailyEarnings, "Daily earnings limit reached");

        // Simulate trade execution and profit calculation
        uint256 profit = _calculateProfit(amountIn);

        // Ensure we don't exceed daily limit
        if (dailyEarnings + profit > maxDailyEarnings) {
            profit = maxDailyEarnings - dailyEarnings;
        }

        // Update tracking
        totalTrades++;
        successfulTrades++;
        lastTradeTimestamp = block.timestamp;
        config.totalEarned += profit;
        dailyEarnings += profit;

        emit TradeExecuted("AUTO_SWAP", amountIn, profit);

        return profit;
    }

    /**
     * @dev Calculate profit based on strategy and trophy rank
     */
    function _calculateProfit(uint256 amountIn) internal view returns (uint256) {
        uint256 baseReturn;

        // Different return rates based on strategy
        if (strategy == TradingStrategy.Aggressive) {
            baseReturn = (amountIn * 3) / 100; // 3% return
        } else if (strategy == TradingStrategy.Balanced) {
            baseReturn = (amountIn * 2) / 100; // 2% return
        } else {
            baseReturn = (amountIn * 1) / 100; // 1% return
        }

        // Trophy rank multiplier
        if (config.trophyRank == TrophyRank.Gold) {
            return (baseReturn * 150) / 100; // 1.5x multiplier
        } else if (config.trophyRank == TrophyRank.Silver) {
            return (baseReturn * 125) / 100; // 1.25x multiplier
        } else {
            return baseReturn; // 1x multiplier
        }
    }

    /**
     * @dev Get max daily earnings based on trophy rank
     */
    function _getMaxDailyEarnings() internal view returns (uint256) {
        if (config.trophyRank == TrophyRank.Gold) {
            return MAX_DAILY_EARNINGS_GOLD;
        } else if (config.trophyRank == TrophyRank.Silver) {
            return MAX_DAILY_EARNINGS_SILVER;
        } else {
            return MAX_DAILY_EARNINGS_BRONZE;
        }
    }

    /**
     * @dev Withdraw accumulated earnings
     */
    function withdrawEarnings() public nonReentrant {
        require(msg.sender == config.beneficiary, "Only beneficiary can withdraw");
        require(config.totalEarned > 0, "No earnings to withdraw");

        uint256 amount = config.totalEarned;
        config.totalEarned = 0;

        // Transfer USDC to beneficiary
        IERC20(usdcToken).transfer(config.beneficiary, amount);

        emit EarningsWithdrawn(config.beneficiary, amount);
    }

    /**
     * @dev Simulate yield farming (for Gold/Silver trophies)
     */
    function executeYieldFarming(uint256 amount) public onlyOwner onlyActive returns (uint256) {
        require(
            config.trophyRank == TrophyRank.Gold || config.trophyRank == TrophyRank.Silver,
            "Only Gold/Silver trophies can yield farm"
        );
        require(amount > 0, "Amount must be greater than 0");

        // Simulate yield farming returns (typically 5-10% APY)
        uint256 dailyYield = (amount * 8) / (365 * 100); // ~8% APY
        
        config.totalEarned += dailyYield;
        
        emit TradeExecuted("YIELD_FARM", amount, dailyYield);
        
        return dailyYield;
    }

    /**
     * @dev Update trading strategy
     */
    function updateStrategy(TradingStrategy newStrategy) public {
        require(msg.sender == config.beneficiary || msg.sender == owner(), "Not authorized");
        strategy = newStrategy;
        emit TradingStrategyUpdated(newStrategy);
    }

    /**
     * @dev Deactivate bot (can be called by owner or when expired)
     */
    function deactivateBot() public {
        require(
            msg.sender == owner() || 
            msg.sender == config.beneficiary || 
            block.timestamp >= config.expirationDate,
            "Not authorized"
        );

        config.isActive = false;
        
        string memory reason = block.timestamp >= config.expirationDate 
            ? "Expiration reached" 
            : "Manual deactivation";
            
        emit BotDeactivated(reason);
    }

    /**
     * @dev Get bot status and statistics
     */
    function getBotStatus() public view returns (
        bool isActive,
        uint256 daysRemaining,
        uint256 totalEarned,
        uint256 totalTrades_,
        uint256 successRate
    ) {
        uint256 remaining = 0;
        if (block.timestamp < config.expirationDate) {
            remaining = (config.expirationDate - block.timestamp) / 1 days;
        }

        uint256 rate = 0;
        if (totalTrades > 0) {
            rate = (successfulTrades * 100) / totalTrades;
        }

        return (
            config.isActive && block.timestamp < config.expirationDate,
            remaining,
            config.totalEarned,
            totalTrades,
            rate
        );
    }

    /**
     * @dev Get estimated monthly earnings
     */
    function getEstimatedMonthlyEarnings() public view returns (uint256) {
        uint256 maxDaily = _getMaxDailyEarnings();
        
        // Assume 50% efficiency rate (bot doesn't hit max every day)
        uint256 avgDaily = (maxDaily * 50) / 100;
        
        return avgDaily * 30; // Monthly estimate
    }

    /**
     * @dev Emergency withdraw (owner only, for stuck funds)
     */
    function emergencyWithdraw(address token) public onlyOwner {
        uint256 balance = IERC20(token).balanceOf(address(this));
        require(balance > 0, "No balance to withdraw");
        
        IERC20(token).transfer(owner(), balance);
    }

    /**
     * @dev Update beneficiary (when NFT is transferred)
     */
    function updateBeneficiary(address newBeneficiary) public onlyOwner {
        require(newBeneficiary != address(0), "Invalid beneficiary address");
        config.beneficiary = newBeneficiary;
    }

    /**
     * @dev Get time remaining until expiration
     */
    function getTimeRemaining() public view returns (uint256) {
        if (block.timestamp >= config.expirationDate) {
            return 0;
        }
        return config.expirationDate - block.timestamp;
    }
}
