// SPDX-License-Identifier: UNLICENSED
pragma solidity ^0.8.20;

import {Script} from "forge-std/Script.sol";
import {Nft1} from "../src/nft1.sol";

contract Nft1Script is Script {
    function run() public {
        vm.startBroadcast();

        new Nft1();

        vm.stopBroadcast();
    }
}
