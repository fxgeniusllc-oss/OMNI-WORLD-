// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";
import "@openzeppelin/contracts/security/Pausable.sol";
import "@openzeppelin/contracts/token/ERC721/IERC721.sol";
import "@openzeppelin/contracts/token/ERC20/IERC20.sol";

/**
 * @title MathGodEvaluator
 * @dev Contract for NFT valuation, appraisal, and sellback logic
 * 
 * Features:
 * - Dynamic property and asset valuation algorithms
 * - Market-based pricing calculations
 * - Sellback mechanism for NFTs
 * - Appraisal service with configurable fees
 * 
 * Revenue Flow:
 * - Appraisal fees route to Treasury Wallet
 * - Sellback transactions follow revenue split model (90/5/5)
 * 
 * Configuration:
 * - Treasury: 0x94140Fdcf420ce32E24c55B91a425fa71d80427B
 * - Revenue: 0xD6490ADA82710c4a43D71E9f6D7E4bF8CD1282CF
 * - Developer: 0xCbBf46e4BFbcd099601D63482866EEC68Ebd8992
 * - Recovery: 0x81f5cfdD2851362E5986b26614517638Af89E514
 */
contract MathGodEvaluator is AccessControl, ReentrancyGuard, Pausable {
    // Roles
    bytes32 public constant ADMIN_ROLE = keccak256("ADMIN_ROLE");
    bytes32 public constant EVALUATOR_ROLE = keccak256("EVALUATOR_ROLE");

    // System addresses
    address public treasuryWallet;
    address public revenueWallet;
    address public omniCoinToken;

    // Fee configuration (in basis points)
    uint256 public appraisalFee = 10 * 10**18; // 10 OMNICOIN default
    uint256 public sellbackFeePercent = 300; // 3% (300 basis points)

    // Valuation data structures
    struct AssetValuation {
        uint256 baseValue;
        uint256 marketMultiplier; // in basis points (10000 = 1.0x)
        uint256 lastUpdated;
        bool isActive;
    }

    // NFT contract => TokenID => Valuation
    mapping(address => mapping(uint256 => AssetValuation)) public valuations;
    
    // Market conditions
    uint256 public globalMarketMultiplier = 10000; // 1.0x default
    uint256 public lastMarketUpdate;

    // Events
    event AssetAppraised(
        address indexed nftContract,
        uint256 indexed tokenId,
        uint256 baseValue,
        uint256 marketValue,
        uint256 timestamp
    );

    event SellbackExecuted(
        address indexed nftContract,
        uint256 indexed tokenId,
        address indexed seller,
        uint256 amount,
        uint256 fee
    );

    event MarketMultiplierUpdated(
        uint256 oldMultiplier,
        uint256 newMultiplier,
        uint256 timestamp
    );

    /**
     * @dev Constructor
     */
    constructor(
        address _treasuryWallet,
        address _revenueWallet,
        address _omniCoinToken,
        address _developerAddress,
        address _recoveryAddress
    ) {
        require(_treasuryWallet != address(0), "Invalid treasury address");
        require(_revenueWallet != address(0), "Invalid revenue address");
        require(_omniCoinToken != address(0), "Invalid OMNICOIN address");
        require(_developerAddress != address(0), "Invalid developer address");
        require(_recoveryAddress != address(0), "Invalid recovery address");

        treasuryWallet = _treasuryWallet;
        revenueWallet = _revenueWallet;
        omniCoinToken = _omniCoinToken;

        // Grant roles
        _grantRole(DEFAULT_ADMIN_ROLE, _developerAddress);
        _grantRole(ADMIN_ROLE, _developerAddress);
        _grantRole(ADMIN_ROLE, _recoveryAddress);
        _grantRole(EVALUATOR_ROLE, _developerAddress);

        lastMarketUpdate = block.timestamp;
    }

    /**
     * @dev Request an appraisal for an NFT
     * @param nftContract Address of the NFT contract
     * @param tokenId Token ID to appraise
     * @return currentValue The current market value
     */
    function requestAppraisal(
        address nftContract,
        uint256 tokenId
    ) external nonReentrant whenNotPaused returns (uint256 currentValue) {
        require(nftContract != address(0), "Invalid NFT contract");

        // Collect appraisal fee
        require(
            IERC20(omniCoinToken).transferFrom(msg.sender, treasuryWallet, appraisalFee),
            "Appraisal fee payment failed"
        );

        // Get or calculate valuation
        AssetValuation storage valuation = valuations[nftContract][tokenId];
        
        if (!valuation.isActive) {
            // First appraisal - set base value
            valuation.baseValue = _calculateBaseValue(nftContract, tokenId);
            valuation.marketMultiplier = globalMarketMultiplier;
            valuation.isActive = true;
        }

        valuation.lastUpdated = block.timestamp;
        
        currentValue = _calculateMarketValue(valuation);

        emit AssetAppraised(
            nftContract,
            tokenId,
            valuation.baseValue,
            currentValue,
            block.timestamp
        );

        return currentValue;
    }

    /**
     * @dev Execute a sellback transaction
     * @param nftContract Address of the NFT contract
     * @param tokenId Token ID to sell back
     */
    function executeSellback(
        address nftContract,
        uint256 tokenId
    ) external nonReentrant whenNotPaused {
        require(nftContract != address(0), "Invalid NFT contract");
        
        // Verify caller owns the NFT
        require(
            IERC721(nftContract).ownerOf(tokenId) == msg.sender,
            "Not NFT owner"
        );

        AssetValuation storage valuation = valuations[nftContract][tokenId];
        require(valuation.isActive, "Asset not appraised");

        // Calculate sellback value
        uint256 marketValue = _calculateMarketValue(valuation);
        uint256 sellbackFee = (marketValue * sellbackFeePercent) / 10000;
        uint256 sellerAmount = marketValue - sellbackFee;

        // Transfer NFT to contract (or burn)
        IERC721(nftContract).transferFrom(msg.sender, address(this), tokenId);

        // Pay seller
        require(
            IERC20(omniCoinToken).transfer(msg.sender, sellerAmount),
            "Seller payment failed"
        );

        // Split fee between treasury and revenue
        uint256 treasuryFee = (sellbackFee * 50) / 100;
        uint256 revenueFee = sellbackFee - treasuryFee;

        IERC20(omniCoinToken).transfer(treasuryWallet, treasuryFee);
        IERC20(omniCoinToken).transfer(revenueWallet, revenueFee);

        emit SellbackExecuted(
            nftContract,
            tokenId,
            msg.sender,
            sellerAmount,
            sellbackFee
        );

        // Deactivate valuation
        valuation.isActive = false;
    }

    /**
     * @dev Get current market value for an NFT
     * @param nftContract Address of the NFT contract
     * @param tokenId Token ID
     * @return Current market value in OMNICOIN
     */
    function getCurrentValue(
        address nftContract,
        uint256 tokenId
    ) external view returns (uint256) {
        AssetValuation storage valuation = valuations[nftContract][tokenId];
        require(valuation.isActive, "Asset not appraised");
        return _calculateMarketValue(valuation);
    }

    /**
     * @dev Calculate market value based on base value and multipliers
     */
    function _calculateMarketValue(
        AssetValuation storage valuation
    ) private view returns (uint256) {
        uint256 baseValue = valuation.baseValue;
        uint256 assetMultiplier = valuation.marketMultiplier;
        
        // Apply both asset and global market multipliers
        uint256 marketValue = (baseValue * assetMultiplier * globalMarketMultiplier) / (10000 * 10000);
        
        return marketValue;
    }

    /**
     * @dev Calculate base value for an NFT (simplified algorithm)
     * In production, this would integrate with oracles, historical data, etc.
     */
    function _calculateBaseValue(
        address nftContract,
        uint256 tokenId
    ) private view returns (uint256) {
        // Placeholder: base calculation
        // In production, integrate with:
        // - Historical sale prices
        // - Property attributes (size, location, etc.)
        // - Market trends
        // - Comparable sales
        
        // For now, return a default value
        return 1000 * 10**18; // 1000 OMNICOIN base
    }

    /**
     * @dev Set base valuation for an asset (evaluator only)
     */
    function setBaseValuation(
        address nftContract,
        uint256 tokenId,
        uint256 baseValue,
        uint256 marketMultiplier
    ) external onlyRole(EVALUATOR_ROLE) {
        AssetValuation storage valuation = valuations[nftContract][tokenId];
        valuation.baseValue = baseValue;
        valuation.marketMultiplier = marketMultiplier;
        valuation.lastUpdated = block.timestamp;
        valuation.isActive = true;
    }

    /**
     * @dev Update global market multiplier (admin only)
     */
    function setGlobalMarketMultiplier(
        uint256 newMultiplier
    ) external onlyRole(ADMIN_ROLE) {
        require(newMultiplier > 0, "Multiplier must be positive");
        
        uint256 oldMultiplier = globalMarketMultiplier;
        globalMarketMultiplier = newMultiplier;
        lastMarketUpdate = block.timestamp;

        emit MarketMultiplierUpdated(oldMultiplier, newMultiplier, block.timestamp);
    }

    /**
     * @dev Update appraisal fee (admin only)
     */
    function setAppraisalFee(uint256 newFee) external onlyRole(ADMIN_ROLE) {
        appraisalFee = newFee;
    }

    /**
     * @dev Update sellback fee percentage (admin only)
     */
    function setSellbackFeePercent(uint256 newFeePercent) external onlyRole(ADMIN_ROLE) {
        require(newFeePercent <= 1000, "Fee too high (max 10%)");
        sellbackFeePercent = newFeePercent;
    }

    /**
     * @dev Emergency withdraw function (admin only)
     * For recovering NFTs or tokens sent to contract
     */
    function emergencyWithdrawNFT(
        address nftContract,
        uint256 tokenId,
        address recipient
    ) external onlyRole(ADMIN_ROLE) {
        IERC721(nftContract).transferFrom(address(this), recipient, tokenId);
    }

    function emergencyWithdrawToken(
        address token,
        uint256 amount,
        address recipient
    ) external onlyRole(ADMIN_ROLE) {
        IERC20(token).transfer(recipient, amount);
    }

    /**
     * @dev Pause contract (emergency)
     */
    function pause() external onlyRole(ADMIN_ROLE) {
        _pause();
    }

    /**
     * @dev Unpause contract
     */
    function unpause() external onlyRole(ADMIN_ROLE) {
        _unpause();
    }

    /**
     * @dev Receive function to accept NFTs
     */
    function onERC721Received(
        address,
        address,
        uint256,
        bytes memory
    ) public pure returns (bytes4) {
        return this.onERC721Received.selector;
    }
}
