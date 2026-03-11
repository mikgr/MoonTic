using System.Text.Json;
using Microsoft.Extensions.Options;
using Ticketer.Model;


namespace Ticketer.UseCases;


public class DeployContractHandler(IOptions<BlockchainSettings> blockchainSettings)
{
    public record DeployContractResult(DateTime DeployedAtUtc, string ContractAddress, string DeployTxHash);
    
    public async Task<DeployContractResult> Execute(object[] constructorArgs)
    {
        var privateKey = blockchainSettings.Value.SystemPrivateKey
            ?? throw new Exception("PRIVATE_KEY not set");

        var account = new Nethereum.Web3.Accounts.Account(privateKey);
        var web3 = new Nethereum.Web3.Web3(account, blockchainSettings.Value.BlockchainRpcUrl);

        Console.WriteLine("Using account: " + account.Address);
        
        // NB for prod hash contract and check, before publish 
        var contractPath = Path.Combine(AppContext.BaseDirectory, "Contracts", "Ticket.json");
        var jsonText = await File.ReadAllTextAsync(contractPath); 

        // todo store CompilerVersion, runs, EvmVersion, And Optimized
        // todo - call snowtrac / avascan apis to verify contract, and store verification status in db, for later retrieval in UI
        
        using var doc = JsonDocument.Parse(jsonText);
        var root = doc.RootElement;

        // Extract ABI and bytecode
        string abi = root.GetProperty("abi").GetRawText(); 
        string byteCode = root.GetProperty("bytecode").GetProperty("object").GetString()
            ?? throw new Exception("Bytecode not found in Ticket.json");
        
        Console.WriteLine("Deploying contract...");

        var deploymentReceipt = await web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(
            abi: abi,
            contractByteCode: byteCode,
            from: account.Address,
            gas: new Nethereum.Hex.HexTypes.HexBigInteger(3000000), 
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