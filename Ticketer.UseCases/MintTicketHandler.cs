using System.Numerics;
using Ticketer.Model;

namespace Ticketer.UseCases;

public class MintTicketHandler
{
    public async Task<(int tokenId, string transactionHash)> Execute(
        string contractAddress, string toAddress)
    {
        // Owner private key (must be _owner in your contract)
        var privateKey = TicketerOptions.PrivateKey
            ?? throw new Exception("PRIVATE_KEY not set");
        
        // Create an account object
        Nethereum.Web3.Accounts.Account account = new Nethereum.Web3.Accounts.Account(privateKey, TicketerOptions.BlockchainId);

        // Create a Web3 instance
        Nethereum.Web3.Web3 web3 = new Nethereum.Web3.Web3(account, TicketerOptions.BlockchainRpcUrl);
                    
        // Minimal ABI containing only the mint function
        string abi = """
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

        // Get contract-instance
        Nethereum.Contracts.Contract contract = web3.Eth.GetContract(abi, contractAddress);

        // Get the mint function
        Nethereum.Contracts.Function mintFunction = contract.GetFunction("mint");
                    

        // Send transaction and wait for receipt
        Nethereum.RPC.Eth.DTOs.TransactionReceipt receipt = await mintFunction.SendTransactionAndWaitForReceiptAsync(
            from: account.Address,
            gas: new Nethereum.Hex.HexTypes.HexBigInteger(100000),
            value: null,
            functionInput: toAddress
        );

        BigInteger tokenId = -1;
        
        var log = receipt.Logs.FirstOrDefault();
        if (log is not null)
        {
            //var filterLog = Newtonsoft.Json.JsonConvert.DeserializeObject<Nethereum.RPC.Eth.DTOs.FilterLog>(log.ToString());
            
            tokenId = new Nethereum.Hex.HexTypes.HexBigInteger(log.Data).Value;
            Console.WriteLine($"Minted token ID: {tokenId}");
        }
        
        Console.WriteLine("Transaction hash: " + receipt.TransactionHash);
        
        return ((int)tokenId, receipt.TransactionHash);
        // // 11️⃣ Optionally, decode the return value (minted token ID)
        // BigInteger mintedTokenId = await mintFunction.CallAsync<BigInteger>(toAddress);
        //
        // Console.WriteLine($"Minted token ID: {mintedTokenId} To address: {toAddress}");
    }
}