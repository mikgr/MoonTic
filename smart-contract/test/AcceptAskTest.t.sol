// SPDX-License-Identifier: UNLICENSED
pragma solidity ^0.8.13;

import {Test} from "forge-std/Test.sol";
import {Ticket} from "../src/Ticket.sol";
import {MockUSDC} from "../src/MockUsdc.sol";

contract AcceptAskTest is Test {
    Ticket public ticketContract;
    MockUSDC public usdc;
    address public ownerAddr = 0xA2258d67a701e1abB8564db41aDeb5d530203Fc0;
    uint256 public checkOutBlockedTime = 1770927699; // Thu Feb 12 2026 20:21:39 GMT+0000
    uint256 public venueOpenTime = 1772389800; // Sun Mar 01 2026 18:30:00 GMT+0000
    uint256 public venueCloseTime = 1772405999; // Sun Mar 01 2026 22:59:59 GMT+0000
    uint64 public totalTicketCount = 10;
    string public location = "Store VEGA, Enghavevej 40, 1674 Copenhagen V, Denmark";

    address public ada = 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B;
    address public bob = 0xcE7c92a6368B445DC8d168a45f35c20644eeC5bf;

    function setUp() public {
        usdc = new MockUSDC();

        vm.prank(ownerAddr);
        ticketContract = new Ticket(
            checkOutBlockedTime,
            venueOpenTime,
            venueCloseTime,
            totalTicketCount,
            location,
            address(usdc)
        );
    }

    function test_acceptAsk_works() public {
        // Arrange: mint ticket to ada, ada creates ask for 100 USDC
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        uint256 price = 100 * 1e6; // 100 USDC
        vm.prank(ada);
        ticketContract.createAsk(tokenId, price);

        // Give bob USDC and approve the ticket contract to spend it
        usdc.mint(bob, price);
        vm.prank(bob);
        usdc.approve(address(ticketContract), price);

        // Expect events
        vm.prank(bob);
        vm.expectEmit(true, true, true, false);
        emit Ticket.TokenTransferred(ada, bob, tokenId);
        
        vm.expectEmit(true, true, false, true);
        emit Ticket.AskAccepted(tokenId, bob, price);

        // Act
        ticketContract.acceptAsk(tokenId);

        // Assert: bob now owns the token
        assertEq(ticketContract.ownerOf(tokenId), bob);

        // Assert: ada received the USDC payment
        assertEq(usdc.balanceOf(ada), price);

        // Assert: bob's USDC balance is now 0
        assertEq(usdc.balanceOf(bob), 0);

        // Assert: ask is cleared
        vm.expectRevert("No active ask");
        ticketContract.askFor(tokenId);
    }

    function test_acceptAsk_fails_if_no_active_ask() public {
        // Arrange: mint ticket to ada, no ask created
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        // Assert
        vm.prank(bob);
        vm.expectRevert("No active ask to fulfill");

        // Act
        ticketContract.acceptAsk(tokenId);
    }

    function test_acceptAsk_fails_if_buyer_is_seller() public {
        // Arrange
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        uint256 price = 100 * 1e6;
        vm.prank(ada);
        ticketContract.createAsk(tokenId, price);

        // Assert
        vm.prank(ada);
        vm.expectRevert("Cannot fulfill ask for your own token");

        // Act
        ticketContract.acceptAsk(tokenId);
    }

    function test_acceptAsk_fails_if_buyer_has_insufficient_balance() public {
        // Arrange
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        uint256 price = 100 * 1e6;
        vm.prank(ada);
        ticketContract.createAsk(tokenId, price);

        // Bob approves but has no USDC balance
        vm.prank(bob);
        usdc.approve(address(ticketContract), price);

        // Assert
        vm.prank(bob);
        vm.expectRevert();

        // Act
        ticketContract.acceptAsk(tokenId);
    }

    function test_acceptAsk_fails_if_buyer_has_insufficient_allowance() public {
        // Arrange
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        uint256 price = 100 * 1e6;
        vm.prank(ada);
        ticketContract.createAsk(tokenId, price);

        // Bob has USDC but does not approve the ticket contract
        usdc.mint(bob, price);

        // Assert
        vm.prank(bob);
        vm.expectRevert();

        // Act
        ticketContract.acceptAsk(tokenId);
    }

    function test_acceptAsk_fails_if_buyer_approves_less_than_price() public {
        // Arrange
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        uint256 price = 100 * 1e6;
        vm.prank(ada);
        ticketContract.createAsk(tokenId, price);

        usdc.mint(bob, price);
        vm.prank(bob);
        usdc.approve(address(ticketContract), price - 1); // approve less than price

        // Assert
        vm.prank(bob);
        vm.expectRevert();

        // Act
        ticketContract.acceptAsk(tokenId);
    }

    function test_acceptAsk_transfers_ownership() public {
        // Arrange
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        uint256 price = 50 * 1e6;
        vm.prank(ada);
        ticketContract.createAsk(tokenId, price);

        usdc.mint(bob, price);
        vm.prank(bob);
        usdc.approve(address(ticketContract), price);

        // Assert before
        assertEq(ticketContract.ownerOf(tokenId), ada);

        // Act
        vm.prank(bob);
        ticketContract.acceptAsk(tokenId);

        // Assert after
        assertEq(ticketContract.ownerOf(tokenId), bob);
    }

    function test_acceptAsk_clears_ask_after_fulfillment() public {
        // Arrange
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        uint256 price = 100 * 1e6;
        vm.prank(ada);
        ticketContract.createAsk(tokenId, price);

        usdc.mint(bob, price);
        vm.prank(bob);
        usdc.approve(address(ticketContract), price);

        // Act
        vm.prank(bob);
        ticketContract.acceptAsk(tokenId);

        // Assert: ask is cleared
        vm.expectRevert("No active ask");
        ticketContract.askFor(tokenId);
    }

    function test_acceptAsk_new_owner_can_create_new_ask() public {
        // Arrange: ada sells to bob, then bob creates a new ask
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        uint256 price = 100 * 1e6;
        vm.prank(ada);
        ticketContract.createAsk(tokenId, price);

        usdc.mint(bob, price);
        vm.prank(bob);
        usdc.approve(address(ticketContract), price);

        vm.prank(bob);
        ticketContract.acceptAsk(tokenId);

        // Act: bob creates a new ask at a higher price
        uint256 newPrice = 150 * 1e6;
        vm.prank(bob);
        ticketContract.createAsk(tokenId, newPrice);

        // Assert
        assertEq(ticketContract.askFor(tokenId), newPrice);
    }

    function test_acceptAsk_new_owner_can_transfer() public {
        // Arrange: ada sells to bob via ask
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        uint256 price = 100 * 1e6;
        vm.prank(ada);
        ticketContract.createAsk(tokenId, price);

        usdc.mint(bob, price);
        vm.prank(bob);
        usdc.approve(address(ticketContract), price);

        vm.prank(bob);
        ticketContract.acceptAsk(tokenId);

        // Act: bob transfers to ada
        vm.prank(bob);
        ticketContract.transfer(ada, tokenId);

        // Assert
        assertEq(ticketContract.ownerOf(tokenId), ada);
    }

    function test_acceptAsk_emits_AskFulfilled_event() public {
        // Arrange
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        uint256 price = 100 * 1e6;
        vm.prank(ada);
        ticketContract.createAsk(tokenId, price);

        usdc.mint(bob, price);
        vm.prank(bob);
        usdc.approve(address(ticketContract), price);

        // Expect AskFulfilled event
        vm.expectEmit(true, true, false, true);
        emit Ticket.AskAccepted(tokenId, bob, price);

        // Act
        vm.prank(bob);
        ticketContract.acceptAsk(tokenId);
    }

    function test_acceptAsk_emits_TokenTransferred_event() public {
        // Arrange
        vm.prank(ownerAddr);
        uint256 tokenId = ticketContract.mint(ada);

        uint256 price = 100 * 1e6;
        vm.prank(ada);
        ticketContract.createAsk(tokenId, price);

        usdc.mint(bob, price);
        vm.prank(bob);
        usdc.approve(address(ticketContract), price);

        // Expect TokenTransferred event
        vm.expectEmit(true, true, true, false);
        emit Ticket.TokenTransferred(ada, bob, tokenId);

        // Act
        vm.prank(bob);
        ticketContract.acceptAsk(tokenId);
    }
}
