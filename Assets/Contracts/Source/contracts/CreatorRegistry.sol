// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";

/**
 * @title CreatorRegistry
 * @dev Manages creator profiles, tiers, and reputation scores
 * 
 * Tier System:
 * - Tier 1: New Creator ($0+, Rep 0+)
 * - Tier 2: Emerging Creator ($1K+, Rep 60+, 25+ followers)
 * - Tier 3: Established Creator ($10K+, Rep 75+, 100+ followers)
 * - Tier 4: Elite Creator ($100K+, Rep 85+, 500+ followers)
 * - Tier 5: Legendary Creator ($1M+, Rep 95+, 1000+ followers)
 */
contract CreatorRegistry is AccessControl, ReentrancyGuard {
    
    // ========== ROLES ==========
    
    bytes32 public constant VERIFIER_ROLE = keccak256("VERIFIER_ROLE");
    bytes32 public constant REPUTATION_UPDATER_ROLE = keccak256("REPUTATION_UPDATER_ROLE");
    
    // ========== STRUCTS ==========
    
    struct Creator {
        address walletAddress;
        string username;
        string profileURI; // IPFS hash for profile data
        uint256 tier;
        uint256 reputationScore; // 0-100
        uint256 totalSales;
        uint256 totalRoyalties;
        uint256 followerCount;
        uint256 creationCount;
        uint256 registrationTimestamp;
        bool isVerified;
        bool isActive;
        CreatorCategory primaryCategory;
    }
    
    enum CreatorCategory {
        Music,
        VisualArt,
        Architecture,
        Education,
        Entertainment,
        Business,
        MultiCategory
    }
    
    struct TierRequirements {
        uint256 minSales;
        uint256 minReputation;
        uint256 minFollowers;
    }
    
    // ========== STATE VARIABLES ==========
    
    mapping(address => Creator) public creators;
    mapping(string => address) public usernameToAddress;
    mapping(uint256 => TierRequirements) public tierRequirements;
    
    uint256 public totalCreators;
    uint256 public verifiedCreators;
    
    // ========== EVENTS ==========
    
    event CreatorRegistered(
        address indexed creator,
        string username,
        CreatorCategory category
    );
    
    event CreatorTierUpdated(
        address indexed creator,
        uint256 oldTier,
        uint256 newTier
    );
    
    event ReputationUpdated(
        address indexed creator,
        uint256 oldScore,
        uint256 newScore,
        string reason
    );
    
    event CreatorVerified(address indexed creator, address indexed verifier);
    
    event CreatorDeactivated(address indexed creator, string reason);
    
    event CreatorReactivated(address indexed creator);
    
    event SalesUpdated(
        address indexed creator,
        uint256 amount,
        bool isRoyalty
    );
    
    // ========== CONSTRUCTOR ==========
    
    constructor() {
        _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
        _grantRole(VERIFIER_ROLE, msg.sender);
        _grantRole(REPUTATION_UPDATER_ROLE, msg.sender);
        
        _initializeTierRequirements();
    }
    
    // ========== INITIALIZATION ==========
    
    function _initializeTierRequirements() internal {
        // Tier 1: New Creator (default)
        tierRequirements[1] = TierRequirements({
            minSales: 0,
            minReputation: 0,
            minFollowers: 0
        });
        
        // Tier 2: Emerging Creator
        tierRequirements[2] = TierRequirements({
            minSales: 1000 ether,  // $1,000 in sales
            minReputation: 60,
            minFollowers: 25
        });
        
        // Tier 3: Established Creator
        tierRequirements[3] = TierRequirements({
            minSales: 10000 ether,  // $10,000 in sales
            minReputation: 75,
            minFollowers: 100
        });
        
        // Tier 4: Elite Creator
        tierRequirements[4] = TierRequirements({
            minSales: 100000 ether,  // $100,000 in sales
            minReputation: 85,
            minFollowers: 500
        });
        
        // Tier 5: Legendary Creator
        tierRequirements[5] = TierRequirements({
            minSales: 1000000 ether,  // $1,000,000 in sales
            minReputation: 95,
            minFollowers: 1000
        });
    }
    
    // ========== REGISTRATION ==========
    
    /**
     * @dev Register as a new creator
     */
    function registerCreator(
        string memory username,
        string memory profileURI,
        CreatorCategory category
    ) external nonReentrant {
        require(creators[msg.sender].walletAddress == address(0), "Already registered");
        require(bytes(username).length >= 3 && bytes(username).length <= 20, "Invalid username length");
        require(usernameToAddress[username] == address(0), "Username taken");
        
        creators[msg.sender] = Creator({
            walletAddress: msg.sender,
            username: username,
            profileURI: profileURI,
            tier: 1,  // Start at Tier 1
            reputationScore: 50,  // Start at neutral reputation
            totalSales: 0,
            totalRoyalties: 0,
            followerCount: 0,
            creationCount: 0,
            registrationTimestamp: block.timestamp,
            isVerified: false,
            isActive: true,
            primaryCategory: category
        });
        
        usernameToAddress[username] = msg.sender;
        totalCreators++;
        
        emit CreatorRegistered(msg.sender, username, category);
    }
    
    // ========== SALES TRACKING ==========
    
    /**
     * @dev Update creator sales (called by UGC contract)
     */
    function updateSales(
        address creator,
        uint256 amount,
        bool isRoyalty
    ) external onlyRole(REPUTATION_UPDATER_ROLE) {
        require(creators[creator].isActive, "Creator not active");
        
        if (isRoyalty) {
            creators[creator].totalRoyalties += amount;
        } else {
            creators[creator].totalSales += amount;
        }
        
        emit SalesUpdated(creator, amount, isRoyalty);
        
        // Check for tier upgrade
        _checkAndUpgradeTier(creator);
    }
    
    /**
     * @dev Increment creation count
     */
    function incrementCreationCount(address creator) external onlyRole(REPUTATION_UPDATER_ROLE) {
        require(creators[creator].isActive, "Creator not active");
        creators[creator].creationCount++;
    }
    
    // ========== REPUTATION MANAGEMENT ==========
    
    /**
     * @dev Update creator reputation score
     */
    function updateReputation(
        address creator,
        int256 change,
        string memory reason
    ) external onlyRole(REPUTATION_UPDATER_ROLE) {
        require(creators[creator].isActive, "Creator not active");
        
        uint256 oldScore = creators[creator].reputationScore;
        
        // Apply change with bounds checking
        if (change > 0) {
            uint256 newScore = oldScore + uint256(change);
            creators[creator].reputationScore = newScore > 100 ? 100 : newScore;
        } else if (change < 0) {
            uint256 decrease = uint256(-change);
            creators[creator].reputationScore = oldScore > decrease ? oldScore - decrease : 0;
        }
        
        emit ReputationUpdated(creator, oldScore, creators[creator].reputationScore, reason);
        
        // Check for tier changes
        _checkAndUpgradeTier(creator);
    }
    
    /**
     * @dev Batch update reputation for multiple creators
     */
    function batchUpdateReputation(
        address[] memory creatorList,
        int256[] memory changes,
        string memory reason
    ) external onlyRole(REPUTATION_UPDATER_ROLE) {
        require(creatorList.length == changes.length, "Array length mismatch");
        
        for (uint256 i = 0; i < creatorList.length; i++) {
            if (creators[creatorList[i]].isActive) {
                updateReputation(creatorList[i], changes[i], reason);
            }
        }
    }
    
    // ========== FOLLOWER MANAGEMENT ==========
    
    /**
     * @dev Update follower count
     */
    function updateFollowerCount(
        address creator,
        uint256 newCount
    ) external onlyRole(REPUTATION_UPDATER_ROLE) {
        require(creators[creator].isActive, "Creator not active");
        creators[creator].followerCount = newCount;
        
        // Check for tier upgrade
        _checkAndUpgradeTier(creator);
    }
    
    // ========== TIER MANAGEMENT ==========
    
    /**
     * @dev Check if creator qualifies for tier upgrade
     */
    function _checkAndUpgradeTier(address creator) internal {
        Creator storage c = creators[creator];
        uint256 currentTier = c.tier;
        uint256 newTier = _calculateTier(creator);
        
        if (newTier > currentTier) {
            c.tier = newTier;
            emit CreatorTierUpdated(creator, currentTier, newTier);
        }
    }
    
    /**
     * @dev Calculate appropriate tier based on stats
     */
    function _calculateTier(address creator) internal view returns (uint256) {
        Creator memory c = creators[creator];
        uint256 totalRevenue = c.totalSales + c.totalRoyalties;
        
        // Check from highest tier down
        for (uint256 tier = 5; tier >= 1; tier--) {
            TierRequirements memory req = tierRequirements[tier];
            
            if (
                totalRevenue >= req.minSales &&
                c.reputationScore >= req.minReputation &&
                c.followerCount >= req.minFollowers
            ) {
                return tier;
            }
        }
        
        return 1;  // Default to Tier 1
    }
    
    /**
     * @dev Manually set tier (admin only, for special cases)
     */
    function setCreatorTier(
        address creator,
        uint256 newTier
    ) external onlyRole(DEFAULT_ADMIN_ROLE) {
        require(newTier >= 1 && newTier <= 5, "Invalid tier");
        require(creators[creator].isActive, "Creator not active");
        
        uint256 oldTier = creators[creator].tier;
        creators[creator].tier = newTier;
        
        emit CreatorTierUpdated(creator, oldTier, newTier);
    }
    
    // ========== VERIFICATION ==========
    
    /**
     * @dev Verify a creator (KYC, identity proof, etc.)
     */
    function verifyCreator(address creator) external onlyRole(VERIFIER_ROLE) {
        require(creators[creator].isActive, "Creator not active");
        require(!creators[creator].isVerified, "Already verified");
        
        creators[creator].isVerified = true;
        verifiedCreators++;
        
        emit CreatorVerified(creator, msg.sender);
    }
    
    /**
     * @dev Batch verify creators
     */
    function batchVerifyCreators(address[] memory creatorList) external onlyRole(VERIFIER_ROLE) {
        for (uint256 i = 0; i < creatorList.length; i++) {
            if (creators[creatorList[i]].isActive && !creators[creatorList[i]].isVerified) {
                creators[creatorList[i]].isVerified = true;
                verifiedCreators++;
                emit CreatorVerified(creatorList[i], msg.sender);
            }
        }
    }
    
    // ========== DEACTIVATION ==========
    
    /**
     * @dev Deactivate creator account (fraud, violation, etc.)
     */
    function deactivateCreator(
        address creator,
        string memory reason
    ) external onlyRole(DEFAULT_ADMIN_ROLE) {
        require(creators[creator].isActive, "Already deactivated");
        
        creators[creator].isActive = false;
        
        emit CreatorDeactivated(creator, reason);
    }
    
    /**
     * @dev Reactivate creator account
     */
    function reactivateCreator(address creator) external onlyRole(DEFAULT_ADMIN_ROLE) {
        require(!creators[creator].isActive, "Already active");
        
        creators[creator].isActive = true;
        
        emit CreatorReactivated(creator);
    }
    
    // ========== PROFILE UPDATES ==========
    
    /**
     * @dev Update profile URI
     */
    function updateProfileURI(string memory newURI) external {
        require(creators[msg.sender].isActive, "Creator not active");
        creators[msg.sender].profileURI = newURI;
    }
    
    /**
     * @dev Update primary category
     */
    function updatePrimaryCategory(CreatorCategory newCategory) external {
        require(creators[msg.sender].isActive, "Creator not active");
        creators[msg.sender].primaryCategory = newCategory;
    }
    
    // ========== VIEW FUNCTIONS ==========
    
    function getCreator(address creator) external view returns (Creator memory) {
        return creators[creator];
    }
    
    function getCreatorTier(address creator) external view returns (uint256) {
        return creators[creator].tier;
    }
    
    function getCreatorReputation(address creator) external view returns (uint256) {
        return creators[creator].reputationScore;
    }
    
    function isCreatorActive(address creator) external view returns (bool) {
        return creators[creator].isActive;
    }
    
    function isCreatorVerified(address creator) external view returns (bool) {
        return creators[creator].isVerified;
    }
    
    function getCreatorByUsername(string memory username) external view returns (Creator memory) {
        address creatorAddress = usernameToAddress[username];
        require(creatorAddress != address(0), "Creator not found");
        return creators[creatorAddress];
    }
    
    function getTierRequirements(uint256 tier) external view returns (TierRequirements memory) {
        require(tier >= 1 && tier <= 5, "Invalid tier");
        return tierRequirements[tier];
    }
    
    /**
     * @dev Check if creator qualifies for specific tier
     */
    function qualifiesForTier(address creator, uint256 tier) external view returns (bool) {
        require(tier >= 1 && tier <= 5, "Invalid tier");
        
        Creator memory c = creators[creator];
        TierRequirements memory req = tierRequirements[tier];
        uint256 totalRevenue = c.totalSales + c.totalRoyalties;
        
        return (
            totalRevenue >= req.minSales &&
            c.reputationScore >= req.minReputation &&
            c.followerCount >= req.minFollowers
        );
    }
    
    /**
     * @dev Get creator statistics
     */
    function getCreatorStats(address creator) external view returns (
        uint256 tier,
        uint256 reputation,
        uint256 totalRevenue,
        uint256 followers,
        uint256 creations,
        bool verified,
        bool active
    ) {
        Creator memory c = creators[creator];
        return (
            c.tier,
            c.reputationScore,
            c.totalSales + c.totalRoyalties,
            c.followerCount,
            c.creationCount,
            c.isVerified,
            c.isActive
        );
    }
    
    // ========== ADMIN FUNCTIONS ==========
    
    function updateTierRequirements(
        uint256 tier,
        uint256 minSales,
        uint256 minReputation,
        uint256 minFollowers
    ) external onlyRole(DEFAULT_ADMIN_ROLE) {
        require(tier >= 1 && tier <= 5, "Invalid tier");
        
        tierRequirements[tier] = TierRequirements({
            minSales: minSales,
            minReputation: minReputation,
            minFollowers: minFollowers
        });
    }
}
