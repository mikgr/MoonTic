
// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import {Script, console} from "forge-std/Script.sol";
import {MockUSDC} from "../src/MockUsdc.sol";

contract DeployMockUSDC is Script {
    function run() external {
        vm.startBroadcast();

        MockUSDC usdc = new MockUSDC();
        console.log("MockUSDC deployed at:", address(usdc));

        vm.stopBroadcast();
    }
}