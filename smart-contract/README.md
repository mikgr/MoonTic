## Cheat Sheet

### Default Prefunded Account - Avalanche CLI
Address: 0x8db97C7cEcE249c2b98bDC0226Cc4C2A57BF52FC
Private Key: 0x56289e99c94b6912bfc12adc093c9b51124f0dc54ac7a766b2bc5ccf558d8027

### Send NFT
Send nft from owner method.send owner.address contract.method to.address rpc url private key
cast send "0x4ac1d98d9cef99ec6546ded4bd550b0b287aad6d" "mint(address)" "0xcE7c92a6368B445DC8d168a45f35c20644eeC5bf" --rpc-url "$RPC_URL" --private-key "$PRIVATE_KEY"

### Publish MockUsdc on local network (0xf14E7183aaE1A10bae05f862A127F92fc98dda98) (Fuji: 0x1D7A4EDc252a5e1B5B002Fb51Ec3e59d78567a2a)
forge script script/DeployMockUSDC.s.sol:DeployMockUSDC \
--rpc-url http://127.0.0.1:9650/ext/bc/C/rpc \
--private-key 0x56289e99c94b6912bfc12adc093c9b51124f0dc54ac7a766b2bc5ccf558d8027 \
--broadcast

### Use faucet to get 1000 MockUSDC
cast send <MOCKUSDC_ADDRESS> "faucet()" \
--rpc-url http://127.0.0.1:9650/ext/bc/C/rpc \
--private-key 0x56289e99c94b6912bfc12adc093c9b51124f0dc54ac7a766b2bc5ccf558d8027


### Get balance of an address > cast to decimal > format with decimals
cast call <MOCKUSDC_ADDRESS> "balanceOf(address)" 0x8db97C7cEcE249c2b98bDC0226Cc4C2A57BF52FC \
--rpc-url http://127.0.0.1:9650/ext/bc/C/rpc | cast to-dec | python3 -c "print(int(input()) / 1e6)"

### Get balance of USDC MOCK on Fuji testnet
cast call 0x1D7A4EDc252a5e1B5B002Fb51Ec3e59d78567a2a "balanceOf(address)" 0x51d62C5f22EE653ce8dA565Dd5C38e0728ba83CC \
--rpc-url https://api.avax-test.network/ext/bc/C/rpc | cast to-dec | python3 -c "print(int(input()) / 1e6)"


## Foundry

**Foundry is a blazing fast, portable and modular toolkit for Ethereum application development written in Rust.**

Foundry consists of:

- **Forge**: Ethereum testing framework (like Truffle, Hardhat and DappTools).
- **Cast**: Swiss army knife for interacting with EVM smart contracts, sending transactions and getting chain data.
- **Anvil**: Local Ethereum node, akin to Ganache, Hardhat Network.
- **Chisel**: Fast, utilitarian, and verbose solidity REPL.


## Documentation
https://book.getfoundry.sh/
