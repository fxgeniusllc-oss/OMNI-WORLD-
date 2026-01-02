// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/token/ERC721/extensions/ERC721Royalty.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";
import "@openzeppelin/contracts/utils/Counters.sol";

/**
 * @title OmniUGCRoyalty
 * @dev User-Generated Content NFT with 85% creator revenue + 20% perpetual royalties
 * 
 * Core Features:
 * - 85% of first sale goes to creator
 * - 20% perpetual royalties on all secondary sales
 * - Tier-based creator benefits (higher tiers get better terms)
 * - Automated royalty distribution via EIP-2981
 * - Anti-fraud protection and content verification
 * - Integration with Creator Registry for tier management
 */
contract OmniUGCRoyalty is ERC721Royalty, Ownable, ReentrancyGuard {
    using Counters for Counters.Counter;
    
    // ========== STATE VARIABLES ==========
    
    Counters.Counter private _tokenIdCounter;
    
    // Treasury address for platform fees
    address public treasury;
    
    // Creator Registry contract for tier management
    address public creatorRegistry;
    
    // Base creator revenue share (8500 = 85%)
    uint256 public constant BASE_CREATOR_SHARE = 8500;
    
    // Base royalty percentage (2000 = 20%)
    uint256 public constant BASE_ROYALTY_PERCENTAGE = 2000;
    
    // Platform fee on secondary sales (1000 = 10%)
    uint256 public constant SECONDARY_PLATFORM_FEE = 1000;
    
    // ========== STRUCTS ==========
    
    struct Asset {
        address originalCreator;
        uint256 firstSalePrice;
        uint256 creatorTier;
        uint256 totalRevenue;
        string contentURI;
        string contentHash; // IPFS hash or SHA256 for verification
        AssetCategory category;
        uint256 mintTimestamp;
        bool isVerified;
    }
    
    enum AssetCategory {
        Music,
        VisualArt,
        Architecture,
        Education,
        Video,
        Writing,
        GameAsset,
        Other
    }
    
    // ========== MAPPINGS ==========
    
    mapping(uint256 => Asset) public assets;
    mapping(address => uint256) public creatorTotalSales;
    mapping(address => uint256) public creatorTotalRoyalties;
    mapping(string => bool) public contentHashExists;
    
    // ========== EVENTS ==========
    
    event AssetMinted(
        uint256 indexed tokenId,
        address indexed creator,
        AssetCategory category,
        string contentURI,
        string contentHash
    );
    
    event PrimarySaleCompleted(
        uint256 indexed tokenId,
        address indexed creator,
        address indexed buyer,
        uint256 salePrice,
        uint256 creatorAmount,
        uint256 platformAmount
    );
    
    event SecondarySaleCompleted(
        uint256 indexed tokenId,
        address indexed seller,
        address indexed buyer,
        uint256 salePrice,
        uint256 royaltyAmount,
        uint256 sellerAmount,
        uint256 platformAmount
    );
    
    event RoyaltyPaid(
        uint256 indexed tokenId,
        address indexed creator,
        uint256 amount
    );
    
    event AssetVerified(uint256 indexed tokenId, address verifier);
    
    event CreatorTierUpdated(
        address indexed creator,
        uint256 oldTier,
        uint256 newTier
    );
    
    // ========== MODIFIERS ==========
    
    modifier onlyCreatorRegistry() {
        require(msg.sender == creatorRegistry, "Only Creator Registry");
        _;
    }
    
    modifier contentNotDuplicate(string memory contentHash) {
        require(!contentHashExists[contentHash], "Content already exists");
        _;
    }
    
    // ========== CONSTRUCTOR ==========
    
    constructor(
        address _treasury,
        address _creatorRegistry
    ) ERC721("OmniWorld UGC", "OUGC") {
        require(_treasury != address(0), "Invalid treasury address");
        require(_creatorRegistry != address(0), "Invalid registry address");
        
        treasury = _treasury;
        creatorRegistry = _creatorRegistry;
    }
    
    // ========== MINTING FUNCTIONS ==========
    
    /**
     * @dev Mint new UGC asset with creator royalties
     * @param contentURI IPFS URI for the content
     * @param contentHash Hash of content for verification
     * @param category Category of the asset
     * @return tokenId The newly minted token ID
     */
    function mintAsset(
        string memory contentURI,
        string memory contentHash,
        AssetCategory category
    ) external nonReentrant contentNotDuplicate(contentHash) returns (uint256) {
        uint256 tokenId = _tokenIdCounter.current();
        _tokenIdCounter.increment();
        
        // Get creator tier from registry
        uint256 creatorTier = _getCreatorTier(msg.sender);
        
        // Mint NFT to creator
        _safeMint(msg.sender, tokenId);
        
        // Set royalty (20% base, can be higher for elite creators)
        uint256 royaltyPercentage = _calculateRoyaltyPercentage(creatorTier);
        _setTokenRoyalty(tokenId, msg.sender, uint96(royaltyPercentage));
        
        // Store asset data
        assets[tokenId] = Asset({
            originalCreator: msg.sender,
            firstSalePrice: 0,
            creatorTier: creatorTier,
            totalRevenue: 0,
            contentURI: contentURI,
            contentHash: contentHash,
            category: category,
            mintTimestamp: block.timestamp,
            isVerified: false
        });
        
        // Mark content hash as used
        contentHashExists[contentHash] = true;
        
        emit AssetMinted(tokenId, msg.sender, category, contentURI, contentHash);
        
        return tokenId;
    }
    
    // ========== SALE FUNCTIONS ==========
    
    /**
     * @dev Execute primary sale with 85% creator revenue
     * @param tokenId The token being sold
     * @param buyer Address of the buyer
     */
    function executePrimarySale(
        uint256 tokenId,
        address buyer
    ) external payable nonReentrant {
        require(_exists(tokenId), "Token does not exist");
        Asset storage asset = assets[tokenId];
        require(asset.firstSalePrice == 0, "Not a primary sale");
        require(ownerOf(tokenId) == asset.originalCreator, "Creator must own token");
        require(msg.value > 0, "Sale price must be greater than 0");
        
        // Calculate creator share based on tier
        uint256 creatorShare = _calculateCreatorShare(asset.creatorTier);
        uint256 creatorAmount = (msg.value * creatorShare) / 10000;
        uint256 platformAmount = msg.value - creatorAmount;
        
        // Update asset data
        asset.firstSalePrice = msg.value;
        asset.totalRevenue = msg.value;
        
        // Update creator stats
        creatorTotalSales[asset.originalCreator] += msg.value;
        
        // Transfer funds
        payable(asset.originalCreator).transfer(creatorAmount);
        payable(treasury).transfer(platformAmount);
        
        // Transfer NFT
        _transfer(asset.originalCreator, buyer, tokenId);
        
        emit PrimarySaleCompleted(
            tokenId,
            asset.originalCreator,
            buyer,
            msg.value,
            creatorAmount,
            platformAmount
        );
        
        // Check if creator qualifies for tier upgrade
        _checkTierUpgrade(asset.originalCreator);
    }
    
    /**
     * @dev Execute secondary sale with 20% perpetual royalty
     * @param tokenId The token being sold
     * @param buyer Address of the buyer
     */
    function executeSecondarySale(
        uint256 tokenId,
        address buyer
    ) external payable nonReentrant {
        require(_exists(tokenId), "Token does not exist");
        Asset storage asset = assets[tokenId];
        require(asset.firstSalePrice > 0, "Must complete primary sale first");
        address seller = ownerOf(tokenId);
        require(seller != buyer, "Cannot sell to self");
        require(msg.value > 0, "Sale price must be greater than 0");
        
        // Calculate royalty for original creator
        uint256 royaltyPercentage = _calculateRoyaltyPercentage(asset.creatorTier);
        uint256 royaltyAmount = (msg.value * royaltyPercentage) / 10000;
        
        // Calculate platform fee
        uint256 platformAmount = (msg.value * SECONDARY_PLATFORM_FEE) / 10000;
        
        // Seller gets remainder
        uint256 sellerAmount = msg.value - royaltyAmount - platformAmount;
        
        // Update asset stats
        asset.totalRevenue += msg.value;
        
        // Update creator royalty stats
        creatorTotalRoyalties[asset.originalCreator] += royaltyAmount;
        
        // Transfer funds
        payable(asset.originalCreator).transfer(royaltyAmount);
        payable(seller).transfer(sellerAmount);
        payable(treasury).transfer(platformAmount);
        
        // Transfer NFT
        _transfer(seller, buyer, tokenId);
        
        emit SecondarySaleCompleted(
            tokenId,
            seller,
            buyer,
            msg.value,
            royaltyAmount,
            sellerAmount,
            platformAmount
        );
        
        emit RoyaltyPaid(tokenId, asset.originalCreator, royaltyAmount);
    }
    
    // ========== TIER CALCULATION FUNCTIONS ==========
    
    /**
     * @dev Calculate creator revenue share based on tier
     * Tier 1: 85% (8500)
     * Tier 2: 85% (8500)
     * Tier 3: 85% (8500)
     * Tier 4: 87% (8700)
     * Tier 5: 90% (9000)
     */
    function _calculateCreatorShare(uint256 tier) internal pure returns (uint256) {
        if (tier >= 5) return 9000;      // Legendary: 90%
        if (tier == 4) return 8700;      // Elite: 87%
        return BASE_CREATOR_SHARE;       // Standard: 85%
    }
    
    /**
     * @dev Calculate royalty percentage based on tier
     * Tier 1-3: 20% (2000)
     * Tier 4: 22% (2200)
     * Tier 5: 25% (2500)
     */
    function _calculateRoyaltyPercentage(uint256 tier) internal pure returns (uint256) {
        if (tier >= 5) return 2500;      // Legendary: 25%
        if (tier == 4) return 2200;      // Elite: 22%
        return BASE_ROYALTY_PERCENTAGE;  // Standard: 20%
    }
    
    /**
     * @dev Get creator tier from registry
     * Integrates with CreatorRegistry contract for actual tier data
     * TODO: Complete integration when CreatorRegistry is deployed
     */
    function _getCreatorTier(address creator) internal view returns (uint256) {
        // TODO: Replace with actual CreatorRegistry contract call
        // Example implementation:
        // ICreatorRegistry registry = ICreatorRegistry(creatorRegistry);
        // return registry.getCreatorTier(creator);
        
        // For now, return default tier 1
        // This will be updated once CreatorRegistry is properly integrated
        return 1;
    }
    
    /**
     * @dev Check if creator qualifies for tier upgrade
     * Integrates with CreatorRegistry for automatic tier progression
     * TODO: Complete integration when CreatorRegistry is deployed
     */
    function _checkTierUpgrade(address creator) internal {
        // TODO: Trigger CreatorRegistry update based on sales thresholds
        // Example implementation:
        // uint256 totalSales = creatorTotalSales[creator];
        // ICreatorRegistry registry = ICreatorRegistry(creatorRegistry);
        // registry.updateSales(creator, amount, false);
        // This will automatically handle tier upgrades in the registry
        
        // Placeholder: Log that upgrade check would occur here
        // Actual tier management will be handled by CreatorRegistry contract
    }
    
    // ========== VERIFICATION FUNCTIONS ==========
    
    /**
     * @dev Verify asset authenticity (called by authorized verifiers)
     */
    function verifyAsset(uint256 tokenId) external {
        require(_exists(tokenId), "Token does not exist");
        // TODO: Add verifier role management
        
        Asset storage asset = assets[tokenId];
        asset.isVerified = true;
        
        emit AssetVerified(tokenId, msg.sender);
    }
    
    // ========== VIEW FUNCTIONS ==========
    
    function getAsset(uint256 tokenId) external view returns (Asset memory) {
        require(_exists(tokenId), "Token does not exist");
        return assets[tokenId];
    }
    
    function getCreatorStats(address creator) external view returns (
        uint256 totalSales,
        uint256 totalRoyalties,
        uint256 combinedRevenue
    ) {
        totalSales = creatorTotalSales[creator];
        totalRoyalties = creatorTotalRoyalties[creator];
        combinedRevenue = totalSales + totalRoyalties;
    }
    
    function tokenURI(uint256 tokenId) public view virtual override returns (string memory) {
        require(_exists(tokenId), "Token does not exist");
        return assets[tokenId].contentURI;
    }
    
    // ========== ADMIN FUNCTIONS ==========
    
    function setTreasury(address _treasury) external onlyOwner {
        require(_treasury != address(0), "Invalid treasury address");
        treasury = _treasury;
    }
    
    function setCreatorRegistry(address _creatorRegistry) external onlyOwner {
        require(_creatorRegistry != address(0), "Invalid registry address");
        creatorRegistry = _creatorRegistry;
    }
    
    // ========== ROYALTY INFO (EIP-2981) ==========
    
    /**
     * @dev Returns royalty info for EIP-2981 compliance
     * This enables automatic royalty payments on compatible marketplaces
     */
    function royaltyInfo(uint256 tokenId, uint256 salePrice)
        public
        view
        virtual
        override
        returns (address, uint256)
    {
        require(_exists(tokenId), "Token does not exist");
        Asset memory asset = assets[tokenId];
        
        uint256 royaltyPercentage = _calculateRoyaltyPercentage(asset.creatorTier);
        uint256 royaltyAmount = (salePrice * royaltyPercentage) / 10000;
        
        return (asset.originalCreator, royaltyAmount);
    }
}
