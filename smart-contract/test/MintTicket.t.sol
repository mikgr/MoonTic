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

    function setUp() public {
        vm.prank(ownerAddr);
        ticketContract = new Ticket(
            checkOutBlockedTime, 
            venueOpenTime,
            venueCloseTime,
            totalTicketCount,
            location
        );
    }
    
    
    function test_mint_fails_if_sender_is_not_owner() public {
        address to = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
        vm.prank(to);
        vm.expectRevert();
        ticketContract.mint(to);
    }

    
    function test_mint() public {
        address to = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
        
        vm.prank(ownerAddr);
        vm.expectEmit(true, false, false, true);
        emit Ticket.TokenMinted(to, 0);
        
        // Act
        uint256 tokenId = ticketContract.mint(to);
        
        assertEq(tokenId, 0);
    }
    
    
    function test_mint_total_ticket_count_cannot_be_exceeded() public {

        // Arrange
        address to = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;

        vm.prank(ownerAddr);
        ticketContract = new Ticket(
    checkOutBlockedTime, 
        venueOpenTime, 
        venueCloseTime, 
        1,
            location
                );
        
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(to);
        assertEq(tokenId, 0);
        
        // Act
        vm.expectRevert();
        vm.prank(ownerAddr);
        ticketContract.mint(to);
    }

    
    function test_getOwnerOf() public {
        address to = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
        
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(to);
        
        address owner = ticketContract.ownerOf(tokenId);
        assertEq(owner, to);
    }
    
    
    function test_getOwnerOf_2() public {
        address to = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
        
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(to);
        
        address owner = ticketContract.ownerOf(tokenId);
        assertEq(owner, to);

        vm.prank(ownerAddr);
        uint256 tokenId2 = ticketContract.mint(to);
        
        address owner2 = ticketContract.ownerOf(tokenId2);
        assertEq(owner2, to);
        assertEq(tokenId2, 1);
    }
}
