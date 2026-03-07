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
            mockUsdcAddr,
            500 * 1e6
        );
    }
    
    // tickets can be transferred again after check venue close time
    function test_tickets_can_be_transferred_after_venue_close_time() public {
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
        address bob = 0xcE7c92a6368B445DC8d168a45f35c20644eeC5bf;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        bytes32 checkInSecretHash =
            sha256(abi.encodePacked("my super secret secret"));

        vm.prank(ada);
        ticketContract.checkIn(tokenId, checkInSecretHash);

        // warp to after venue close time
        uint256 venueIsClosedTime = venueCloseTime + 1;
        vm.warp(venueIsClosedTime);

        vm.prank(ada);
        ticketContract.transfer(bob, tokenId);

        address ownerOfToken = ticketContract.ownerOf(tokenId);
        assertEq(ownerOfToken, bob);
    }
    

    // move to transfer test
    function test_transfer_fails_if_sender_is_not_owner() public {
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;  
        address bob = 0xcE7c92a6368B445DC8d168a45f35c20644eeC5bf;
        
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);
        
        vm.expectRevert();
        vm.prank(bob);
        ticketContract.transfer(ownerAddr, tokenId);
    }

    // move to transfer test
    function test_transfer() public {
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
        address bob = 0xcE7c92a6368B445DC8d168a45f35c20644eeC5bf;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);
        
        vm.prank(ada);
        vm.expectEmit(true, false, false, true);
        emit Ticket.TokenTransferred(ada, bob, tokenId);
        
        
        // Act
        ticketContract.transfer(bob, tokenId);
        
        address ownerOfToken = ticketContract.ownerOf(tokenId);
        assertEq(ownerOfToken, bob);
    }


    // move to transfer test
    function test_transfer_fails_if_checkedIn() public  {
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
        address bob = 0xcE7c92a6368B445DC8d168a45f35c20644eeC5bf;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        bytes32 checkInSecretHash =
            sha256(abi.encodePacked("my super secret secret"));
        
        vm.prank(ada);
        ticketContract.checkIn(tokenId, checkInSecretHash);

        vm.prank(ada);
        vm.expectRevert();
        ticketContract.transfer(bob, tokenId);
    }
    
    function test_transfer_fails_with_active_ask() public {
        // Arrange
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
        address bob = 0xcE7c92a6368B445DC8d168a45f35c20644eeC5bf;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        vm.prank(ada);
        ticketContract.createAsk(tokenId, 100);

        // Assert
        vm.prank(ada);
        vm.expectRevert("Cannot transfer token with active ask. Cancel ask first.");

        // Act
        ticketContract.transfer(bob, tokenId);
    }
}
