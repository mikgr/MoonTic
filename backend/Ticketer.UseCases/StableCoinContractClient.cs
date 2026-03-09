using Nethereum.RPC.Eth.DTOs;
using Ticketer.Model;

namespace Ticketer.UseCases;

public class StableCoinContractClient(IStableCoinInfoProvider stableCoinInfoProvider)
{
    public async Task<(TransactionReceipt receipt, DateTime blockTimestamp)> TransferFromLiquidityAccount(
        string symbol, int amountInFiat, string to)
    {
        // todo validate address
        
        var info = stableCoinInfoProvider.GetStableCoinInfo(symbol);
        int amountInStables = amountInFiat * (int)Math.Pow(10, info.decimals);
        
        string abi = """
                     [
                         {
                           "inputs": [
                           {"internalType":"address","name":"to","type":"address"},
                           {"internalType":"uint256","name":"value","type":"uint256"},
                           ],
                           "name":"transfer",
                           "outputs": [],
                           "stateMutability":"nonpayable",
                           "type":"function"
                         }
                     ]
                     """;
        
        // User private key
        var liquidityWalletPrivateKey = BlockchainOptions.SystemPrivateKey;
        var functionName = "transfer";
        // Convert hex string to bytes32
        
        var functionInput = new object[] {to, amountInStables };
        
        return await TicketContractClient.ExecuteContractFunction2(
            info.contractAddress, 
            liquidityWalletPrivateKey, 
            abi, 
            functionName, 
            functionInput);
    }

    public async Task<(TransactionReceipt receipt, DateTime blockTimestamp)> Approve(
        UserWallet buyerWallet, string contractAddress, string stableCoinSymbol, int amountInFiat)
    {
        var stableCoinInfo = stableCoinInfoProvider.GetStableCoinInfo(stableCoinSymbol);
        int amountInStables = amountInFiat * (int)Math.Pow(10, stableCoinInfo.decimals);
        
        string abi = """
                     [
                         {
                           "inputs": [
                           {"internalType":"address","name":"spender","type":"address"},
                           {"internalType":"uint256","name":"value","type":"uint256"},
                           ],
                           "name":"approve",
                           "outputs": [],
                           "stateMutability":"nonpayable",
                           "type":"function"
                         }
                     ]
                     """;
        
        
        var spenderPrivateKey = buyerWallet.PrivateKey;
        var functionName = "approve";
        var functionInput = new object[] {contractAddress, amountInStables };
        
        return await TicketContractClient.ExecuteContractFunction2(
            stableCoinInfo.contractAddress, 
            spenderPrivateKey, 
            abi, 
            functionName, 
            functionInput);
    }
}