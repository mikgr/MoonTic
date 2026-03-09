using System.Numerics;
using Microsoft.Extensions.Options;
using M = Ticketer.Model;

using Nethereum.Web3.Accounts;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;

namespace Ticketer.UseCases;

public class MintTicketHandler(IOptions<M.BlockchainSettings> blockchainSettings)
{
    public async Task<(int tokenId, string transactionHash)> Execute(string contractAddress, string toAddress)
    {
        // TODO just for POC, PK must not leave HSM - use AWS KMS FROM PROD
        var systemPrivateKey = blockchainSettings.Value.SystemPrivateKey
                               ?? throw new Exception("PRIVATE_KEY not set");
        
        var web3Account = new Account(systemPrivateKey, blockchainSettings.Value.BlockchainId);
        var web3Instance = new Web3(web3Account, blockchainSettings.Value.BlockchainRpcUrl);
        var abi = """
                     [
                         {
                             "inputs": [{"internalType":"address","name":"to","type":"address"}],
                             "name":"mint",
                             "outputs": [{"internalType":"uint256","name":"","type":"uint256"}],
                             "stateMutability":"nonpayable",
                             "type":"function"
                         }
                     ]
                     """;
        
        var contractInstance = web3Instance.Eth.GetContract(abi, contractAddress);
        var mintFunction = contractInstance.GetFunction("mint");
                    

        // Send transaction and wait for receipt
        var txReceipt = await mintFunction.SendTransactionAndWaitForReceiptAsync(
            from: web3Account.Address,
            gas: new Nethereum.Hex.HexTypes.HexBigInteger(100000),
            value: null,
            functionInput: toAddress
        );

        
        var log = txReceipt.Logs.FirstOrDefault();
        if (log is null) throw new M.DomainInvariant("Minting failed: no log returned");
        
        BigInteger tokenId = -1;
        tokenId = new HexBigInteger(log.Data).Value;
        Console.WriteLine($"Minted token ID: {tokenId}");
        
        
        Console.WriteLine("Transaction hash: " + txReceipt.TransactionHash);
        
        return ((int)tokenId, txReceipt.TransactionHash);
    }
}