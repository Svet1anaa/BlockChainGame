
// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";

contract ChainQuestNFT is ERC721 {

    address public contractOwner;

    struct GameItem {
        string name;
        string rarity;
        string description;
    }

    mapping(uint256 => GameItem) private gameItems;

    modifier onlyOwner() {
        require(msg.sender == contractOwner, "Not contract owner");
        _;
    }

    constructor() ERC721("ChainQuest", "CQN") {
        contractOwner = msg.sender;
    }

    function createItem(
        address player,
        uint256 itemId,
        string memory itemName,
        string memory itemRarity,
        string memory itemDescription
    ) public onlyOwner {

        gameItems[itemId] = GameItem(
            itemName,
            itemRarity,
            itemDescription
        );

        _safeMint(player, itemId);
    }

    function getItemDetails(uint256 itemId)
        public
        view
        returns (
            string memory,
            string memory,
            string memory
        )
    {
        require(_ownerOf(itemId) != address(0), "Item does not exist");

        GameItem memory item = gameItems[itemId];

        return (
            item.name,
            item.rarity,
            item.description
        );
    }

    function getItemOwner(uint256 itemId)
        public
        view
        returns (address)
    {
        return ownerOf(itemId);
    }
}
