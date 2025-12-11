// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/token/ERC721/extensions/ERC721URIStorage.sol";
import "@openzeppelin/contracts/token/ERC721/extensions/ERC721Enumerable.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/utils/Counters.sol";
import "@openzeppelin/contracts/interfaces/IERC2981.sol";

/**
 * @title OmniLandNFT
 * @dev ERC-721 contract for OmniWorld land and property NFTs
 * Implements EIP-2981 for perpetual royalties (20%)
 */
contract OmniLandNFT is ERC721, ERC721URIStorage, ERC721Enumerable, Ownable, IERC2981 {
    using Counters for Counters.Counter;
    Counters.Counter private _tokenIdCounter;

    // Royalty configuration
    uint96 public constant ROYALTY_FEE = 2000; // 20% in basis points (10000 = 100%)
    address public royaltyReceiver;

    // Property metadata
    struct PropertyData {
        string city;
        string zoneType;
        uint256 purchasePrice;
        uint256 currentValue;
        address creator;
        uint256 mintedAt;
    }

    mapping(uint256 => PropertyData) public properties;
    mapping(string => uint256[]) public cityProperties;

    // Events
    event PropertyMinted(
        uint256 indexed tokenId,
        address indexed owner,
        string city,
        string zoneType,
        uint256 value
    );
    event PropertyValueUpdated(uint256 indexed tokenId, uint256 newValue);

    constructor(address _royaltyReceiver) ERC721("OmniWorld Land", "OMNILAND") {
        royaltyReceiver = _royaltyReceiver;
    }

    /**
     * @dev Mint a new property NFT
     */
    function mintProperty(
        address to,
        string memory tokenURI,
        string memory city,
        string memory zoneType,
        uint256 initialValue
    ) public onlyOwner returns (uint256) {
        uint256 tokenId = _tokenIdCounter.current();
        _tokenIdCounter.increment();

        _safeMint(to, tokenId);
        _setTokenURI(tokenId, tokenURI);

        properties[tokenId] = PropertyData({
            city: city,
            zoneType: zoneType,
            purchasePrice: initialValue,
            currentValue: initialValue,
            creator: to,
            mintedAt: block.timestamp
        });

        cityProperties[city].push(tokenId);

        emit PropertyMinted(tokenId, to, city, zoneType, initialValue);

        return tokenId;
    }

    /**
     * @dev Update property value (called by oracle or game logic)
     */
    function updatePropertyValue(uint256 tokenId, uint256 newValue) public onlyOwner {
        require(_exists(tokenId), "Property does not exist");
        
        properties[tokenId].currentValue = newValue;
        
        emit PropertyValueUpdated(tokenId, newValue);
    }

    /**
     * @dev Get properties in a city
     */
    function getCityProperties(string memory city) public view returns (uint256[] memory) {
        return cityProperties[city];
    }

    /**
     * @dev Get property data
     */
    function getPropertyData(uint256 tokenId) public view returns (PropertyData memory) {
        require(_exists(tokenId), "Property does not exist");
        return properties[tokenId];
    }

    /**
     * @dev EIP-2981 royalty info
     */
    function royaltyInfo(uint256 tokenId, uint256 salePrice)
        public
        view
        override
        returns (address, uint256)
    {
        require(_exists(tokenId), "Property does not exist");
        
        // Calculate royalty amount
        uint256 royaltyAmount = (salePrice * ROYALTY_FEE) / 10000;
        
        // Return creator address for this specific property
        return (properties[tokenId].creator, royaltyAmount);
    }

    /**
     * @dev Update royalty receiver (treasury address)
     */
    function setRoyaltyReceiver(address _royaltyReceiver) public onlyOwner {
        royaltyReceiver = _royaltyReceiver;
    }

    // Override required functions
    function _beforeTokenTransfer(
        address from,
        address to,
        uint256 tokenId,
        uint256 batchSize
    ) internal override(ERC721, ERC721Enumerable) {
        super._beforeTokenTransfer(from, to, tokenId, batchSize);
    }

    function _burn(uint256 tokenId) internal override(ERC721, ERC721URIStorage) {
        super._burn(tokenId);
    }

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
        override(ERC721, ERC721Enumerable, IERC2981)
        returns (bool)
    {
        return super.supportsInterface(interfaceId);
    }
}
