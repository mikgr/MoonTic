// SPDX-License-Identifier: UNLICENSED
pragma solidity ^0.8.13;

import {Script} from "forge-std/Script.sol";
import {Ticket} from "../src/Ticket.sol";

contract CounterScript is Script {
    Ticket public ticketContract;

    function setUp() public {}

    function run() public {
        vm.startBroadcast();

        uint256 fakeCheckOutBlockedTime = 1707321600;
        uint256 venueOpenTime = 1772389800;          // Sun Mar 01 2026 18:30:00 GMT+0000
        uint256 venueCloseTime = 1772405999;         // Sun Mar 01 2026 22:59:59 GMT+0000
        uint64 totalTicketCount = 10;
        string memory location = "Store VEGA, Enghavevej 40, 1674 Copenhagen V, Denmark";
        
        ticketContract = new Ticket(
            fakeCheckOutBlockedTime,
            venueOpenTime,
            venueCloseTime,
            totalTicketCount,
            location,
            0xf14E7183aaE1A10bae05f862A127F92fc98dda98
        );
    
        vm.stopBroadcast();
    }
}
