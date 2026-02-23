using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Ticketer.Model;
using Account = Nethereum.Web3.Accounts.Account;

namespace Ticketer.UseCases;

public static class EstimateGasAndEnsureSufficientFundsHandler
{
    public static async Task<HexBigInteger> Execute(Nethereum.Contracts.Function func, object[] funcInput, string toAddress, Web3 web3)
    {
        Console.WriteLine($"{nameof(EstimateGasAndEnsureSufficientFundsHandler)}");
        // Estimate gas cost
        Console.WriteLine($"Begin {nameof(func.EstimateGasAsync)}");
        var estimatedGas = await func.EstimateGasAsync(
            from: toAddress,
            gas: null,
            value: null,
            functionInput: funcInput
        );
        Console.WriteLine($"End {nameof(func.EstimateGasAsync)}");
        
        Console.WriteLine($"Begin {nameof(EstimatedCostWei)}");
        var estimatedCostWei = await EstimatedCostWei(web3, estimatedGas);
        Console.WriteLine($"End {nameof(EstimatedCostWei)}");
        
        // Check balance in user wallet, if not enough, transfer funds from system wallet
        var userBalance = await web3.Eth.GetBalance.SendRequestAsync(toAddress);
        var requiredBalance = estimatedCostWei * 150 / 100; // 50% buffer for safety
        
        if (userBalance.Value < requiredBalance)
        {
            Console.WriteLine($"Begin {nameof(TransferFundsFromSystemWalletTo)}");
            var amountToTransfer = requiredBalance - userBalance.Value;
            await TransferFundsFromSystemWalletTo(toAddress, amountToTransfer);
            Console.WriteLine($"End {nameof(TransferFundsFromSystemWalletTo)}");
        }
        
        return estimatedGas;
    }
    
    private static async Task<BigInteger> EstimatedCostWei(Web3 web3, HexBigInteger estimatedGas)
    {
        // Get current gas price
        var gasPrice = await web3.Eth.GasPrice.SendRequestAsync();
        
        // Calculate estimated cost in Wei
        var estimatedCostWei = estimatedGas.Value * gasPrice.Value;
        
        // Convert 
        var estimatedCostEth = Web3.Convert.FromWei(estimatedCostWei);
        
        Console.WriteLine($"Estimated gas: {estimatedGas.Value}");
        Console.WriteLine($"Gas price: {Web3.Convert.FromWei(gasPrice.Value, Nethereum.Util.UnitConversion.EthUnit.Gwei)} Gwei");
        Console.WriteLine($"Estimated cost: {estimatedCostEth} ETH");
        return estimatedCostWei;
    }

    private static async Task TransferFundsFromSystemWalletTo(string toAddress, BigInteger amount)
    {
        var systemWalletPrivateKey = TicketerOptions.PrivateKey ?? throw new Exception("PRIVATE_KEY not set");
        var systemAccount = new Account(systemWalletPrivateKey, TicketerOptions.BlockchainId);
        var systemWeb3 = new Web3(systemAccount, TicketerOptions.BlockchainRpcUrl);
            
            
        // todo store this event for the explorer
        var transferReceipt = await systemWeb3.Eth.GetEtherTransferService()
            .TransferEtherAndWaitForReceiptAsync(toAddress, Nethereum.Web3.Web3.Convert.FromWei(amount));
            
        Console.WriteLine($"Transferred {Nethereum.Web3.Web3.Convert.FromWei(amount)} to user wallet. TxHash: {transferReceipt.TransactionHash}");
    }
}