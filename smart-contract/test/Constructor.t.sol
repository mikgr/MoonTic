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

    
    function test_venueOpenTime_can_be_read() public view {
        uint256 venueOpenTimeRead = ticketContract.venueOpenTime();
        assertEq(venueOpenTimeRead, venueOpenTime);
    }
    
    
    function test_location_can_be_read() public view {
        string memory locationRead = ticketContract.location();
        assertEq(locationRead, location);
    }
    
    
    function test_venueCloseTime_can_be_read() public view {
        uint256 venueCloseTimeRead = ticketContract.venueCloseTime();
        assertEq(venueCloseTimeRead, venueCloseTime);
    }


    // todo accept ask
    
    
    
    
    
    
    
    
    
    
}
