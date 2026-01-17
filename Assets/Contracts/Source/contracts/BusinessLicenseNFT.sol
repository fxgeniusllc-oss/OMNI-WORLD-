// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";
import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "@openzeppelin/contracts/security/Pausable.sol";

/**
 * @title BusinessLicenseNFT
 * @dev NFT contract for minting business and property ownership licenses
 * 
 * Features:
 * - ERC-721 compliant NFT minting
 * - Represents ownership of real estate and business licenses
 * - Auto-split payment routing (90% Creator, 5% Treasury, 5% Revenue)
 * - Accepts OMNICOIN, BNB, and USDC payments
 * - Admin controls via Developer and Recovery addresses
 * 
 * Revenue Flow:
 * - Mint fees automatically split to Creator/Treasury/Revenue wallets
 * - Configurable pricing per NFT type
 * 
 * Configuration from BSC Integration:
 * - OMNICoin: 0x8979878229e2e55b80e116283DF22d8203919f27
 * - Treasury: 0x94140Fdcf420ce32E24c55B91a425fa71d80427B
 * - Revenue: 0xD6490ADA82710c4a43D71E9f6D7E4bF8CD1282CF
 * - Developer: 0xCbBf46e4BFbcd099601D63482866EEC68Ebd8992
 * - Recovery: 0x81f5cfdD2851362E5986b26614517638Af89E514
 */
contract BusinessLicenseNFT is ERC721, AccessControl, ReentrancyGuard, Pausable {
    // Roles
    bytes32 public constant ADMIN_ROLE = keccak256("ADMIN_ROLE");
    bytes32 public constant MINTER_ROLE = keccak256("MINTER_ROLE");

    // Revenue split configuration (in basis points, 10000 = 100%)
    uint256 public constant CREATOR_SHARE = 9000;  // 90%
    uint256 public constant TREASURY_SHARE = 500;   // 5%
    uint256 public constant REVENUE_SHARE = 500;    // 5%

    // System addresses
    address public treasuryWallet;
    address public revenueWallet;
    address public omniCoinToken;
    address public usdcToken;

    // NFT tracking
    uint256 private _nextTokenId;
    
    // Pricing (in OMNICOIN wei)
    uint256 public defaultMintPrice;
    
    // Metadata
    mapping(uint256 => string) private _tokenURIs;
    mapping(uint256 => address) private _originalCreators;

    // Events
    event NFTMinted(
        uint256 indexed tokenId,
        address indexed creator,
        address indexed owner,
        uint256 price,
        address paymentToken
    );
    
    event RevenueSplit(
        address indexed creator,
        address indexed treasury,
        address indexed revenue,
        uint256 creatorAmount,
        uint256 treasuryAmount,
        uint256 revenueAmount,
        address paymentToken
    );

    /**
     * @dev Constructor initializes the contract with system addresses
     * @param _treasuryWallet Address of the treasury wallet
     * @param _revenueWallet Address of the revenue wallet
     * @param _omniCoinToken Address of the OMNICOIN token contract
     * @param _usdcToken Address of the USDC token contract
     * @param _developerAddress Address of the developer (admin)
     * @param _recoveryAddress Address of the recovery wallet (backup admin)
     */
    constructor(
        address _treasuryWallet,
        address _revenueWallet,
        address _omniCoinToken,
        address _usdcToken,
        address _developerAddress,
        address _recoveryAddress
    ) ERC721("OmniWorld Business License", "OMNI-BIZ") {
        require(_treasuryWallet != address(0), "Invalid treasury address");
        require(_revenueWallet != address(0), "Invalid revenue address");
        require(_omniCoinToken != address(0), "Invalid OMNICOIN address");
        require(_developerAddress != address(0), "Invalid developer address");
        require(_recoveryAddress != address(0), "Invalid recovery address");

        treasuryWallet = _treasuryWallet;
        revenueWallet = _revenueWallet;
        omniCoinToken = _omniCoinToken;
        usdcToken = _usdcToken;
        
        defaultMintPrice = 100 * 10**18; // 100 OMNICOIN default

        // Grant admin roles
        _grantRole(DEFAULT_ADMIN_ROLE, _developerAddress);
        _grantRole(ADMIN_ROLE, _developerAddress);
        _grantRole(ADMIN_ROLE, _recoveryAddress);
        _grantRole(MINTER_ROLE, _developerAddress);
        _grantRole(MINTER_ROLE, _recoveryAddress);
    }

    /**
     * @dev Mint a new business license NFT with OMNICOIN payment
     * @param to Address to receive the NFT
     * @param tokenURI Metadata URI for the NFT
     * @param price Price in OMNICOIN (0 = use default)
     */
    function mintWithOMNICoin(
        address to,
        string memory tokenURI,
        uint256 price
    ) external nonReentrant whenNotPaused returns (uint256) {
        uint256 actualPrice = price > 0 ? price : defaultMintPrice;
        require(actualPrice > 0, "Price must be greater than 0");

        // Transfer OMNICOIN from buyer
        require(
            IERC20(omniCoinToken).transferFrom(msg.sender, address(this), actualPrice),
            "OMNICOIN transfer failed"
        );

        // Execute the mint
        uint256 tokenId = _mintNFT(to, tokenURI, actualPrice, omniCoinToken);

        // Split and distribute revenue
        _splitRevenue(msg.sender, actualPrice, omniCoinToken);

        return tokenId;
    }

    /**
     * @dev Mint a new business license NFT with BNB payment
     * @param to Address to receive the NFT
     * @param tokenURI Metadata URI for the NFT
     */
    function mintWithBNB(
        address to,
        string memory tokenURI
    ) external payable nonReentrant whenNotPaused returns (uint256) {
        require(msg.value > 0, "Must send BNB");

        // Execute the mint
        uint256 tokenId = _mintNFT(to, tokenURI, msg.value, address(0));

        // Split and distribute revenue (BNB)
        _splitRevenueBNB(msg.sender, msg.value);

        return tokenId;
    }

    /**
     * @dev Mint a new business license NFT with USDC payment
     * @param to Address to receive the NFT
     * @param tokenURI Metadata URI for the NFT
     * @param price Price in USDC
     */
    function mintWithUSDC(
        address to,
        string memory tokenURI,
        uint256 price
    ) external nonReentrant whenNotPaused returns (uint256) {
        require(price > 0, "Price must be greater than 0");
        require(usdcToken != address(0), "USDC not configured");

        // Transfer USDC from buyer
        require(
            IERC20(usdcToken).transferFrom(msg.sender, address(this), price),
            "USDC transfer failed"
        );

        // Execute the mint
        uint256 tokenId = _mintNFT(to, tokenURI, price, usdcToken);

        // Split and distribute revenue
        _splitRevenue(msg.sender, price, usdcToken);

        return tokenId;
    }

    /**
     * @dev Internal function to mint NFT
     */
    function _mintNFT(
        address to,
        string memory tokenURI,
        uint256 price,
        address paymentToken
    ) private returns (uint256) {
        uint256 tokenId = _nextTokenId++;
        _safeMint(to, tokenId);
        _tokenURIs[tokenId] = tokenURI;
        _originalCreators[tokenId] = msg.sender;

        emit NFTMinted(tokenId, msg.sender, to, price, paymentToken);

        return tokenId;
    }

    /**
     * @dev Split and distribute ERC20 token revenue
     */
    function _splitRevenue(
        address creator,
        uint256 amount,
        address token
    ) private {
        uint256 creatorAmount = (amount * CREATOR_SHARE) / 10000;
        uint256 treasuryAmount = (amount * TREASURY_SHARE) / 10000;
        uint256 revenueAmount = (amount * REVENUE_SHARE) / 10000;

        IERC20(token).transfer(creator, creatorAmount);
        IERC20(token).transfer(treasuryWallet, treasuryAmount);
        IERC20(token).transfer(revenueWallet, revenueAmount);

        emit RevenueSplit(
            creator,
            treasuryWallet,
            revenueWallet,
            creatorAmount,
            treasuryAmount,
            revenueAmount,
            token
        );
    }

    /**
     * @dev Split and distribute BNB revenue
     */
    function _splitRevenueBNB(address creator, uint256 amount) private {
        uint256 creatorAmount = (amount * CREATOR_SHARE) / 10000;
        uint256 treasuryAmount = (amount * TREASURY_SHARE) / 10000;
        uint256 revenueAmount = (amount * REVENUE_SHARE) / 10000;

        payable(creator).transfer(creatorAmount);
        payable(treasuryWallet).transfer(treasuryAmount);
        payable(revenueWallet).transfer(revenueAmount);

        emit RevenueSplit(
            creator,
            treasuryWallet,
            revenueWallet,
            creatorAmount,
            treasuryAmount,
            revenueAmount,
            address(0)
        );
    }

    /**
     * @dev Get token URI
     */
    function tokenURI(uint256 tokenId) public view override returns (string memory) {
        require(_exists(tokenId), "Token does not exist");
        return _tokenURIs[tokenId];
    }

    /**
     * @dev Get original creator of a token
     */
    function getOriginalCreator(uint256 tokenId) external view returns (address) {
        require(_exists(tokenId), "Token does not exist");
        return _originalCreators[tokenId];
    }

    /**
     * @dev Update default mint price (admin only)
     */
    function setDefaultMintPrice(uint256 newPrice) external onlyRole(ADMIN_ROLE) {
        defaultMintPrice = newPrice;
    }

    /**
     * @dev Update USDC token address (admin only)
     */
    function setUSDCToken(address newUSDC) external onlyRole(ADMIN_ROLE) {
        usdcToken = newUSDC;
    }

    /**
     * @dev Pause contract (emergency stop)
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
     * @dev Required override for AccessControl
     */
    function supportsInterface(bytes4 interfaceId)
        public
        view
        override(ERC721, AccessControl)
        returns (bool)
    {
        return super.supportsInterface(interfaceId);
    }

    /**
     * @dev Check if token exists
     */
    function _exists(uint256 tokenId) internal view returns (bool) {
        return _ownerOf(tokenId) != address(0);
    }
}
