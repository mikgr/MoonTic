// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import {ERC721} from "openzeppelin-contracts/contracts/token/ERC721/ERC721.sol";
import {Ownable} from "openzeppelin-contracts/contracts/access/Ownable.sol";

contract Nft1 is ERC721, Ownable {
    uint256 private _nextTokenId;

    constructor() ERC721("My NFT", "MNFT") Ownable(msg.sender) {}

    function mint(address to) external onlyOwner {
        uint256 tokenId = _nextTokenId;
        _nextTokenId++;

        _safeMint(to, tokenId);
    }


    // todo : Override tokenURI to return metadata for each token

}