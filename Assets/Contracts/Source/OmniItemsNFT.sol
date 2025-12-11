// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC1155/ERC1155.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/utils/Counters.sol";
import "@openzeppelin/contracts/token/ERC1155/extensions/ERC1155Supply.sol";

/**
 * @title OmniItemsNFT
 * @dev ERC-1155 contract for OmniWorld consumables, materials, and items
 */
contract OmniItemsNFT is ERC1155, Ownable, ERC1155Supply {
    using Counters for Counters.Counter;
    Counters.Counter private _tokenIdCounter;

    // Item metadata
    struct ItemData {
        string name;
        string itemType;
        uint256 maxSupply;
        address creator;
        bool isConsumable;
    }

    mapping(uint256 => ItemData) public items;
    mapping(uint256 => string) private _tokenURIs;

    // Events
    event ItemCreated(
        uint256 indexed tokenId,
        string name,
        string itemType,
        uint256 maxSupply,
        address creator
    );

    constructor() ERC1155("https://api.omniworld.io/metadata/{id}.json") {}

    /**
     * @dev Create a new item type
     */
    function createItem(
        string memory name,
        string memory itemType,
        uint256 maxSupply,
        bool isConsumable,
        string memory tokenURI
    ) public returns (uint256) {
        uint256 tokenId = _tokenIdCounter.current();
        _tokenIdCounter.increment();

        items[tokenId] = ItemData({
            name: name,
            itemType: itemType,
            maxSupply: maxSupply,
            creator: msg.sender,
            isConsumable: isConsumable
        });

        _tokenURIs[tokenId] = tokenURI;

        emit ItemCreated(tokenId, name, itemType, maxSupply, msg.sender);

        return tokenId;
    }

    /**
     * @dev Mint items
     */
    function mint(
        address to,
        uint256 tokenId,
        uint256 amount,
        bytes memory data
    ) public {
        require(tokenId < _tokenIdCounter.current(), "Item does not exist");
        
        ItemData memory item = items[tokenId];
        
        // Check max supply
        if (item.maxSupply > 0) {
            require(
                totalSupply(tokenId) + amount <= item.maxSupply,
                "Exceeds max supply"
            );
        }

        _mint(to, tokenId, amount, data);
    }

    /**
     * @dev Batch mint items
     */
    function mintBatch(
        address to,
        uint256[] memory tokenIds,
        uint256[] memory amounts,
        bytes memory data
    ) public {
        // Verify all items exist
        for (uint256 i = 0; i < tokenIds.length; i++) {
            require(tokenIds[i] < _tokenIdCounter.current(), "Item does not exist");
        }

        _mintBatch(to, tokenIds, amounts, data);
    }

    /**
     * @dev Burn consumable items
     */
    function consume(
        address from,
        uint256 tokenId,
        uint256 amount
    ) public {
        require(items[tokenId].isConsumable, "Item is not consumable");
        require(
            from == msg.sender || isApprovedForAll(from, msg.sender),
            "Not authorized"
        );

        _burn(from, tokenId, amount);
    }

    /**
     * @dev Get item data
     */
    function getItemData(uint256 tokenId) public view returns (ItemData memory) {
        require(tokenId < _tokenIdCounter.current(), "Item does not exist");
        return items[tokenId];
    }

    /**
     * @dev Get token URI
     */
    function uri(uint256 tokenId) public view override returns (string memory) {
        require(tokenId < _tokenIdCounter.current(), "Item does not exist");
        
        string memory tokenURI = _tokenURIs[tokenId];
        
        if (bytes(tokenURI).length > 0) {
            return tokenURI;
        }
        
        return super.uri(tokenId);
    }

    /**
     * @dev Set token URI
     */
    function setTokenURI(uint256 tokenId, string memory tokenURI) public onlyOwner {
        require(tokenId < _tokenIdCounter.current(), "Item does not exist");
        _tokenURIs[tokenId] = tokenURI;
    }

    // Override required functions
    function _beforeTokenTransfer(
        address operator,
        address from,
        address to,
        uint256[] memory ids,
        uint256[] memory amounts,
        bytes memory data
    ) internal override(ERC1155, ERC1155Supply) {
        super._beforeTokenTransfer(operator, from, to, ids, amounts, data);
    }
}
