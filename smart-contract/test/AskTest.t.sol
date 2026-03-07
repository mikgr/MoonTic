// SPDX-License-Identifier: UNLICENSED
pragma solidity ^0.8.13;

import { Test } from "forge-std/Test.sol";
import {Ticket} from "../src/Ticket.sol";

contract AskTest is Test {
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

    // createAsk
    function test_createAsk_works() public {
        // Arrange
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        vm.prank(ada);
        vm.expectEmit(true, false, false, true);
        emit Ticket.AskCreated(tokenId, 100);

        // Act
        ticketContract.createAsk(tokenId, 100);

        // Assert
        uint256 price = ticketContract.askFor(tokenId);
        assertEq(price, 100);
    }

    function test_createAsk_fails_if_not_owner() public {
        // Arrange
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
        address bob = 0xcE7c92a6368B445DC8d168a45f35c20644eeC5bf;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);
        
        // Assert
        vm.prank(bob);
        vm.expectRevert("Only token owner can create ask");
        
        // Act
        ticketContract.createAsk(tokenId, 100);
    }

    function test_createAsk_fails_if_price_is_zero() public {
        // Arrange
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        // Assert
        vm.prank(ada);
        vm.expectRevert("Price must be greater than 0");
        
        // Act
        ticketContract.createAsk(tokenId, 0);
    }

    function test_createAsk_fails_if_checked_in_before_venue_close() public {
        // Arrange
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        bytes32 checkInSecretHash = sha256(abi.encodePacked("my super secret secret"));

        vm.prank(ada);
        ticketContract.checkIn(tokenId, checkInSecretHash);

        // Assert
        vm.prank(ada);
        vm.expectRevert("Cannot create ask for checked in token before venue close time");

        // Act
        ticketContract.createAsk(tokenId, 100);
    }

    function test_createAsk_works_if_checked_in_after_venue_close() public {
        // Arrange
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        bytes32 checkInSecretHash = sha256(abi.encodePacked("my super secret secret"));

        vm.prank(ada);
        ticketContract.checkIn(tokenId, checkInSecretHash);

        // warp to after venue close time
        vm.warp(venueCloseTime + 1);
        vm.prank(ada);
        
        // Act
        ticketContract.createAsk(tokenId, 100);

        // Assert
        uint256 price = ticketContract.askFor(tokenId);
        assertEq(price, 100);
    }

    function test_createAsk_fails_if_ask_already_exists() public {
        // Arrange
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        vm.prank(ada);
        ticketContract.createAsk(tokenId, 100);

        // Assert
        vm.prank(ada);
        vm.expectRevert("Ask already exists. Cancel existing ask first.");
        
        // Act
        ticketContract.createAsk(tokenId, 200);
    }

    // cancelAsk
    function test_cancelAsk_works() public {
        // Arrange
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        vm.prank(ada);
        ticketContract.createAsk(tokenId, 100);

        vm.prank(ada);
        vm.expectEmit(true, false, false, false);
        emit Ticket.AskCancelled(tokenId);

        // Act
        ticketContract.cancelAsk(tokenId);

        // Assert
        vm.expectRevert("No active ask");
        ticketContract.askFor(tokenId);
    }

    function test_cancelAsk_fails_if_not_owner() public {
        // Arrange
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
        address bob = 0xcE7c92a6368B445DC8d168a45f35c20644eeC5bf;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        vm.prank(ada);
        ticketContract.createAsk(tokenId, 100);

        // Assert
        vm.prank(bob);
        vm.expectRevert("Only token owner can cancel ask");
        
        // Act
        ticketContract.cancelAsk(tokenId);
    }

    function test_cancelAsk_fails_if_no_active_ask() public {
        // Arrange
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        // Assert
        vm.prank(ada);
        vm.expectRevert("No active ask to cancel");
        
        // Act
        ticketContract.cancelAsk(tokenId);
    }

    // askFor

    function test_askFor_fails_if_no_active_ask() public {
        // Arrange
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);
        
        // Assert
        vm.expectRevert("No active ask");
        
        // Act
        ticketContract.askFor(tokenId);
    }

    // maxResellPrice

    function test_createAsk_fails_if_price_exceeds_max_resell_price() public {
        // Arrange
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        uint256 aboveMax = 500 * 1e6 + 1; // 1 unit above maxResellPrice

        // Assert
        vm.prank(ada);
        vm.expectRevert("Price exceeds max resell price");

        // Act
        ticketContract.createAsk(tokenId, aboveMax);
    }

    function test_createAsk_works_at_exact_max_resell_price() public {
        // Arrange
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        uint256 exactMax = 500 * 1e6; // exactly maxResellPrice

        // Act
        vm.prank(ada);
        ticketContract.createAsk(tokenId, exactMax);

        // Assert
        uint256 price = ticketContract.askFor(tokenId);
        assertEq(price, exactMax);
    }

    function test_createAsk_works_below_max_resell_price() public {
        // Arrange
        address ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;

        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        uint256 belowMax = 499 * 1e6;

        // Act
        vm.prank(ada);
        ticketContract.createAsk(tokenId, belowMax);

        // Assert
        uint256 price = ticketContract.askFor(tokenId);
        assertEq(price, belowMax);
    }
}
