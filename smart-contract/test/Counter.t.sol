// SPDX-License-Identifier: UNLICENSED
pragma solidity ^0.8.13;

import { Test } from "forge-std/Test.sol";
import {Ticket} from "../src/Ticket.sol";

contract CounterTest is Test {
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

    // move to constructor test
    function test_venueOpenTime_can_be_read() public view {
        uint256 venueOpenTimeRead = ticketContract.venueOpenTime();
        assertEq(venueOpenTimeRead, venueOpenTime);
    }
    
    function test_location_can_be_read() public view {
        string memory locationRead = ticketContract.location();
        assertEq(locationRead, location);
    }
    
    // move to constructor test
    function test_venueCloseTime_can_be_read() public view {
        uint256 venueCloseTimeRead = ticketContract.venueCloseTime();
        assertEq(venueCloseTimeRead, venueCloseTime);
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
    
//    function testFuzz_SetNumber(uint256 x) public {
//        counter.setNumber(x);
//        assertEq(counter.number(), x);
//    }

    // move to mint test
    function test_mint_fails_if_sender_is_not_owner() public {
        address to = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
        vm.prank(to);
        vm.expectRevert();
        ticketContract.mint(to);
    }

    // move to mint test 
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

    // move to checkIn test
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

    // move to checkIn test
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
    
    // move to checkIn test
    function test_checkOutIsBlockedTime_can_be_read() public view {
        uint256 checkOutBlockedTimeRead = ticketContract.checkOutBlockedTime();
        assertEq(checkOutBlockedTimeRead, checkOutBlockedTime);
    }
    
    // move to checkIn test
    function test_checkInSecretHashFor_fails_if_not_checkedIn() public {
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
        
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);
        
        vm.expectRevert();
        ticketContract.checkInSecretHashFor(tokenId);
    }
    
    // move to checkout test
    function test_checkOut_fails_if_not_token_holder() public {
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
        address bob = 0xcE7c92a6368B445DC8d168a45f35c20644eeC5bf;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        bytes32 checkInSecretHash =
            sha256(abi.encodePacked("my super secret secret"));

        vm.prank(ada);
        ticketContract.checkIn(tokenId, checkInSecretHash);

        // Assert Bob cannot check out Adas ticket
        vm.prank(bob);
        vm.expectRevert();
        ticketContract.checkOut(tokenId);
    }

    // move to checkOut test
    function test_check_out_works() public {
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        bytes32 checkInSecretHash =
            sha256(abi.encodePacked("my super secret secret"));

        vm.prank(ada);
        ticketContract.checkIn(tokenId, checkInSecretHash);

        bytes32 readCheckInSecretHash = ticketContract.checkInSecretHashFor(tokenId);
        assertEq(readCheckInSecretHash, checkInSecretHash);

        vm.prank(ada);
        
        // Assert event is emitted 
        vm.expectEmit(true, true, false, false);
        emit Ticket.TokenCheckedOut(tokenId);
        
        // Act
        ticketContract.checkOut(tokenId);
        
        vm.expectRevert();
        ticketContract.checkInSecretHashFor(tokenId);
    }

    // move to checkOut test
    function test_checkOutBlockedTime_can_be_read() public view {
        uint256 checkOutBlockedTimeRead = ticketContract.checkOutBlockedTime();
        assertEq(checkOutBlockedTimeRead, checkOutBlockedTime);
    }
    
    // move to checkOut test
    function test_blocktime_must_be_before_checkOutIsBlockTime_or_checkout_fails() public {
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
        
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        bytes32 checkInSecretHash =
            sha256(abi.encodePacked("my super secret secret"));
        
        vm.prank(ada);
        ticketContract.checkIn(tokenId, checkInSecretHash);

        uint256 futureTime = checkOutBlockedTime + 1;
        vm.warp(futureTime);

        vm.prank(ada);
        vm.expectRevert();
        ticketContract.checkOut(tokenId);
    }
}
