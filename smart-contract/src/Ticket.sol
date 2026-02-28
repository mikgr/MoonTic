// SPDX-License-Identifier: UNLICENSED
pragma solidity ^0.8.13;

//import { ERC721ConsecutiveTest } from "../lib/openzeppelin-contracts/test/token/ERC721/extensions/ERC721Consecutive.t.sol";

// todo implement ERC721

contract Ticket {
    
    event TokenMinted(address indexed to, uint256 tokenId);
    event TokenTransferred(address indexed from, address indexed to, uint256 indexed tokenId);
    event TokenCheckedIn(uint256 indexed tokenId, bytes32 indexed secretHash); // todo should i index here?
    event TokenCheckedOut(uint256 indexed tokenId);
    
    uint256 public checkOutBlockedTime;
    uint256 public venueOpenTime;
    uint256 public venueCloseTime;
    uint64 public totalTicketCount;
    string public location;

    address private _owner;
    uint256 private _nextTokenId;
    mapping(uint256 tokenId => address) private _tokenOwners;
    mapping(uint256 tokenId => bytes32) private _checkIns;

    // todo name:string, symbol:MOONTIC, tokenURI
    // todo add uint64 totalTickets, fail if exceed total tickets
    constructor (
        uint256 checkOutBlockedTime_, 
        uint256 venueOpenTime_, 
        uint256 venueCloseTime_, 
        uint64 totalTicketCount_,
        string memory location_
    ) {
        require(checkOutBlockedTime<= venueOpenTime_, "Check out blocked time must be before venue open time");
        require(venueOpenTime_ < venueCloseTime_, "Venue open time must be before venue close time");
        require(bytes(location_).length <= 200, "Location string too long");
        
        checkOutBlockedTime = checkOutBlockedTime_;
        venueOpenTime = venueOpenTime_;
        venueCloseTime = venueCloseTime_;
        totalTicketCount = totalTicketCount_;
        location = location_;
        
        _owner = msg.sender;
        _nextTokenId = 0;
    }
    
    function ownerOf(uint256 tokenId) public view returns(address) {
        return _tokenOwners[tokenId];
    }
    
    function mint(address to) public returns(uint256) {
        require (msg.sender == _owner);
        require (_nextTokenId < totalTicketCount);
        
        uint256 mintedTokenId = _nextTokenId;
        _tokenOwners[mintedTokenId] = to;
        _nextTokenId++;
        
        emit TokenMinted(to, mintedTokenId);
        
        return mintedTokenId;
    }
    
    function transfer(address to, uint256 tokenId) public {
        require(msg.sender == ownerOf(tokenId), "Only token owner can transfer token");
        require(msg.sender != to, "Cannot transfer to self");
        require(_checkIns[tokenId] == bytes32(0) || venueCloseTime < block.timestamp , "Checked in tokens cannot be transferred before venue close time. Check out to transfer.");
        
        emit TokenTransferred(msg.sender, to, tokenId);
        
        _tokenOwners[tokenId]= to;
    }
    
    function checkIn(uint256 tokenId, bytes32 secretHash) public {
        require(msg.sender == ownerOf(tokenId));
        
        emit TokenCheckedIn(tokenId, secretHash);
        
        _checkIns[tokenId] = secretHash;
    }
    
    function checkInSecretHashFor(uint256 tokenId) public view returns(bytes32) {
        bytes32 secretHash = _checkIns[tokenId];
        require(secretHash != bytes32(0), "Not checked in");
        return secretHash; 
    }
    
    function checkOut(uint256 tokenId) public {
        require(msg.sender == ownerOf(tokenId)); 
        require(_checkIns[tokenId] != bytes32(0), "Not checked in"); 
        require(block.timestamp < checkOutBlockedTime, "Check out is blocked");
        
        emit TokenCheckedOut(tokenId);
        
        delete _checkIns[tokenId];
    }
}
