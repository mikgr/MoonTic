// SPDX-License-Identifier: UNLICENSED
pragma solidity ^0.8.13;

import { Test } from "forge-std/Test.sol";
import {Ticket} from "../src/Ticket.sol";

contract TicketTest is Test {
    Ticket public ticketContract;
    address public ownerAddr = 0xA2258d67a701e1abB8564db41aDeb5d530203Fc0;
    uint256 public checkOutBlockedTime = 1770927699;    // Thu Feb 12 2026 20:21:39 GMT+0000
    uint256 public venueOpenTime = 1772389800;          // Sun Mar 01 2026 18:30:00 GMT+0000
    uint256 public venueCloseTime = 1772405999;         // Sun Mar 01 2026 22:59:59 GMT+0000
    uint64 public totalTicketCount = 10;
    string public location = "Store VEGA, Enghavevej 40, 1674 Copenhagen V, Denmark";
    address public mockUsdcAddr = 0xf14E7183aaE1A10bae05f862A127F92fc98dda98;

    function setUp() public {
        vm.prank(ownerAddr);
        ticketContract = new Ticket(
            checkOutBlockedTime, 
            venueOpenTime,
            venueCloseTime,
            totalTicketCount,
            location,
            mockUsdcAddr
        );
    }

    
    function test_checkIn_fails_if_not_owner() public {
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
        address bob = 0xcE7c92a6368B445DC8d168a45f35c20644eeC5bf;
        
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);
        
        bytes32 checkInSecretHash = 
            sha256(abi.encodePacked("my super secret secret"));
        
        vm.prank(bob);
        vm.expectRevert();
        ticketContract.checkIn(tokenId, checkInSecretHash);
    }

    
    function test_checkIn_works() public {
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
        
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        bytes32 checkInSecretHash =
            sha256(abi.encodePacked("my super secret secret"));
        
        vm.prank(ada);
        
        // Assert event is emitted 
        vm.expectEmit(true, true, false, false);
        emit Ticket.TokenCheckedIn(tokenId, checkInSecretHash);
        
        // Act
        ticketContract.checkIn(tokenId, checkInSecretHash);
    }
    
    
    function test_checkOutIsBlockedTime_can_be_read() public view {
        uint256 checkOutBlockedTimeRead = ticketContract.checkOutBlockedTime();
        assertEq(checkOutBlockedTimeRead, checkOutBlockedTime);
    }
    
    
    function test_checkInSecretHashFor_fails_if_not_checkedIn() public {
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
        
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);
        
        vm.expectRevert();
        ticketContract.checkInSecretHashFor(tokenId);
    }
    
    function test_checkIn_fails_with_active_ask() public {
        // Arrange
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        vm.prank(ada);
        ticketContract.createAsk(tokenId, 100);

        bytes32 checkInSecretHash = sha256(abi.encodePacked("my super secret secret"));

        // Assert
        vm.prank(ada);
        vm.expectRevert("Cannot check in token with active ask. Cancel ask first.");

        // Act
        ticketContract.checkIn(tokenId, checkInSecretHash);
    }
}
