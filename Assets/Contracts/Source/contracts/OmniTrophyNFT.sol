// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/token/ERC721/extensions/ERC721URIStorage.sol";
import "@openzeppelin/contracts/access/Ownable.sol";


/**
 * @title OmniTrophyNFT
 * @dev ERC-721 contract for OmniWorld Tournament Trophy NFTs
 * Trophies are awarded to tournament winners and grant VIP access and prestige
 */
contract OmniTrophyNFT is ERC721, ERC721URIStorage, Ownable {
    
    uint256 private _tokenIdCounter;

    // Trophy rarity tiers
    enum TrophyRank {
        Bronze,  // Entry-level tournaments
        Silver,  // Mid-tier tournaments
        Gold     // Elite championship events
    }

    // Trophy metadata structure
    struct TrophyData {
        string trophyName;
        TrophyRank rank;
        string tournamentName;
        string tournamentType;
        uint256 tournamentDate;
        address winner;
        uint256 prizePool;
        bool hasSmartContract;     // True if trophy includes trading bot
        address tradingBotAddress; // Address of embedded trading bot contract
        uint256 mintedDate;
    }

    // VIP Access permissions
    struct VIPAccess {
        bool canAccessGoldTournaments;
        bool canAccessSilverTournaments;
        bool canAccessBronzeTournaments;
        uint256 xpBoostMultiplier; // 100 = 1x, 150 = 1.5x, etc.
    }

    // Mappings
    mapping(uint256 => TrophyData) public trophies;
    mapping(address => VIPAccess) public vipAccess;
    mapping(TrophyRank => uint256) public trophyCount; // Track trophies per rank
    
    // Tournament organizers who can mint trophies
    mapping(address => bool) public authorizedMinters;

    // Events
    event TrophyMinted(
        uint256 indexed tokenId,
        address indexed winner,
        TrophyRank rank,
        string tournamentName,
        bool hasSmartContract
    );
    
    event VIPAccessGranted(
        address indexed holder,
        TrophyRank rank
    );

    event TradingBotAttached(
        uint256 indexed tokenId,
        address tradingBotAddress
    );

    constructor() ERC721("OmniWorld Trophy NFT", "OMNI-TROPHY") {}

    /**
     * @dev Mint a new Trophy NFT to tournament winner
     */
    function mintTrophy(
        address winner,
        TrophyRank rank,
        string memory trophyName,
        string memory tournamentName,
        string memory tournamentType,
        uint256 prizePool,
        string memory tokenURI,
        bool hasSmartContract,
        address tradingBotAddress
    ) public returns (uint256) {
        require(
            authorizedMinters[msg.sender] || msg.sender == owner(),
            "Not authorized to mint trophies"
        );
        require(winner != address(0), "Invalid winner address");

        uint256 tokenId = _tokenIdCounter;
        _tokenIdCounter++;

        // Mint the NFT
        _safeMint(winner, tokenId);
        _setTokenURI(tokenId, tokenURI);

        // Store trophy data
        trophies[tokenId] = TrophyData({
            trophyName: trophyName,
            rank: rank,
            tournamentName: tournamentName,
            tournamentType: tournamentType,
            tournamentDate: block.timestamp,
            winner: winner,
            prizePool: prizePool,
            hasSmartContract: hasSmartContract,
            tradingBotAddress: tradingBotAddress,
            mintedDate: block.timestamp
        });

        // Update trophy count
        trophyCount[rank]++;

        // Grant VIP access
        _grantVIPAccess(winner, rank);

        emit TrophyMinted(tokenId, winner, rank, tournamentName, hasSmartContract);
        
        if (hasSmartContract && tradingBotAddress != address(0)) {
            emit TradingBotAttached(tokenId, tradingBotAddress);
        }

        return tokenId;
    }

    /**
     * @dev Grant VIP access based on trophy rank
     */
    function _grantVIPAccess(address holder, TrophyRank rank) internal {
        VIPAccess storage access = vipAccess[holder];

        if (rank == TrophyRank.Gold) {
            access.canAccessGoldTournaments = true;
            access.canAccessSilverTournaments = true;
            access.canAccessBronzeTournaments = true;
            access.xpBoostMultiplier = 200; // 2x XP boost
        } else if (rank == TrophyRank.Silver) {
            access.canAccessSilverTournaments = true;
            access.canAccessBronzeTournaments = true;
            access.xpBoostMultiplier = 150; // 1.5x XP boost
        } else if (rank == TrophyRank.Bronze) {
            access.canAccessBronzeTournaments = true;
            access.xpBoostMultiplier = 125; // 1.25x XP boost
        }

        emit VIPAccessGranted(holder, rank);
    }

    /**
     * @dev Update VIP access when trophy is transferred
     */
    function _afterTokenTransfer(
        address from,
        address to,
        uint256 tokenId,
        uint256 batchSize
    ) internal override {
        super._afterTokenTransfer(from, to, tokenId, batchSize);

        // Grant VIP access to new owner (if not minting)
        if (from != address(0) && to != address(0)) {
            TrophyData memory trophy = trophies[tokenId];
            _grantVIPAccess(to, trophy.rank);
        }
    }

    /**
     * @dev Check if address has VIP access to tournament tier
     */
    function hasVIPAccess(address holder, TrophyRank minRank) public view returns (bool) {
        VIPAccess memory access = vipAccess[holder];
        
        if (minRank == TrophyRank.Gold) {
            return access.canAccessGoldTournaments;
        } else if (minRank == TrophyRank.Silver) {
            return access.canAccessSilverTournaments;
        } else if (minRank == TrophyRank.Bronze) {
            return access.canAccessBronzeTournaments;
        }
        
        return false;
    }

    /**
     * @dev Get XP boost multiplier for holder
     */
    function getXPBoostMultiplier(address holder) public view returns (uint256) {
        return vipAccess[holder].xpBoostMultiplier;
    }

    /**
     * @dev Get trophy data
     */
    function getTrophyData(uint256 tokenId) public view returns (TrophyData memory) {
        require(_exists(tokenId), "Trophy does not exist");
        return trophies[tokenId];
    }

    /**
     * @dev Get all trophies owned by address
     */
    function getTrophiesByOwner(address owner) public view returns (uint256[] memory) {
        uint256 balance = balanceOf(owner);
        uint256[] memory ownedTrophies = new uint256[](balance);
        
        uint256 currentIndex = 0;
        uint256 totalSupply = _tokenIdCounter;
        
        for (uint256 i = 0; i < totalSupply && currentIndex < balance; i++) {
            if (_exists(i) && ownerOf(i) == owner) {
                ownedTrophies[currentIndex] = i;
                currentIndex++;
            }
        }
        
        return ownedTrophies;
    }

    /**
     * @dev Get trophy statistics
     */
    function getTrophyStats() public view returns (
        uint256 totalTrophies,
        uint256 goldCount,
        uint256 silverCount,
        uint256 bronzeCount
    ) {
        return (
            _tokenIdCounter,
            trophyCount[TrophyRank.Gold],
            trophyCount[TrophyRank.Silver],
            trophyCount[TrophyRank.Bronze]
        );
    }

    /**
     * @dev Authorize address to mint trophies
     */
    function authorizeMinter(address minter) public onlyOwner {
        authorizedMinters[minter] = true;
    }

    /**
     * @dev Revoke minting authorization
     */
    function revokeMinter(address minter) public onlyOwner {
        authorizedMinters[minter] = false;
    }

    // Override required functions
    function tokenURI(uint256 tokenId)
        public
        view
        override(ERC721, ERC721URIStorage)
        returns (string memory)
    {
        return super.tokenURI(tokenId);
    }

    function supportsInterface(bytes4 interfaceId)
        public
        view
        override(ERC721, ERC721URIStorage)
        returns (bool)
    {
        return super.supportsInterface(interfaceId);
    }

    function _burn(uint256 tokenId) internal override(ERC721, ERC721URIStorage) {
        super._burn(tokenId);
    }
}
