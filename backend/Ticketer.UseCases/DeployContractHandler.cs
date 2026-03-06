using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using Ticketer.Model;

namespace Ticketer.UseCases;

public class DeployContractHandler
{
    public record DeployContractResult(DateTime DeployedAtUtc, string ContractAddress, string DeployTxHash);
    
    public async Task<DeployContractResult> Execute(object[] constructorArgs)
    {
        var privateKey = TicketerOptions.SystemPrivateKey
            ?? throw new Exception("PRIVATE_KEY not set");

        var account = new Nethereum.Web3.Accounts.Account(privateKey);
        var web3 = new Nethereum.Web3.Web3(account, TicketerOptions.BlockchainRpcUrl);

        Console.WriteLine("Using account: " + account.Address);
                    
        // Read the Forge build JSON TODO dont hardcode path
        var jsonText = await File.ReadAllTextAsync("/Users/mikkel/Code/hello_foundry/nft-foundry/out/Ticket.sol/Ticket.json");

        using var doc = JsonDocument.Parse(jsonText);
        var root = doc.RootElement;

        // Extract ABI and bytecode
        string abi = root.GetProperty("abi").GetRawText(); // ABI array as string
        string byteCode = root.GetProperty("bytecode").GetProperty("object").GetString()
            ?? throw new Exception("Bytecode not found in Ticket.json");

        //Deploy contract
        Console.WriteLine("Deploying contract...");

        var deploymentReceipt = await web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(
            abi: abi,
            contractByteCode: byteCode,
            from: account.Address,
            gas: new Nethereum.Hex.HexTypes.HexBigInteger(3000000), // adjust if needed
            values: constructorArgs
        );
        
        var block = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber
            .SendRequestAsync(deploymentReceipt.BlockNumber);
        
        var blockTimestamp = DateTimeOffset.FromUnixTimeSeconds((long)block.Timestamp.Value).UtcDateTime;
                    
        Console.WriteLine("Contract deployed at: " + deploymentReceipt.ContractAddress);
        
        return new DeployContractResult(
            blockTimestamp, 
            deploymentReceipt.ContractAddress.ToLower(),
            deploymentReceipt.TransactionHash);
    }
}