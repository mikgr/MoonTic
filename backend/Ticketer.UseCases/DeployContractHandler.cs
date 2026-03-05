using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;

using Ticketer.Model;
using Nethereum.Util;


namespace Ticketer.UseCases;

public class DeployContractHandler(IDynamoDBContext dynamo)
{
    public async Task Execute(object[] constructorArgs, EventContract eventContract)
    {
        var privateKey = TicketerOptions.PrivateKey
            ?? throw new Exception("PRIVATE_KEY not set");

        var account = new Nethereum.Web3.Accounts.Account(privateKey);
        var web3 = new Nethereum.Web3.Web3(account, TicketerOptions.BlockchainRpcUrl);

        Console.WriteLine("Using account: " + account.Address);
                    
        // 3️⃣ Read the Forge build JSON TODO dont hardcode path
        var jsonText = await File.ReadAllTextAsync("/Users/mikkel/Code/hello_foundry/nft-foundry/out/Ticket.sol/Ticket.json");

        using var doc = JsonDocument.Parse(jsonText);
        var root = doc.RootElement;

        // 4️⃣ Extract ABI and bytecode
        string abi = root.GetProperty("abi").GetRawText(); // ABI array as string
        string byteCode = root.GetProperty("bytecode").GetProperty("object").GetString()
                          ?? throw new Exception("Bytecode not found in Ticket.json");

        // 4️⃣ Deploy contract
        Console.WriteLine("Deploying contract...");

        var deploymentReceipt = await web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(
            abi: abi,
            contractByteCode: byteCode,
            from: account.Address,
            gas: new Nethereum.Hex.HexTypes.HexBigInteger(3000000), // adjust if needed
            values: constructorArgs
        );

        var contractAddress = deploymentReceipt.ContractAddress;
                    
        eventContract.ContractAddress = contractAddress;
        eventContract.DeployTxHash = deploymentReceipt.TransactionHash;

        await dynamo.SaveAsync(eventContract.GetState());
                    
        Console.WriteLine("Contract deployed at: " + contractAddress);
    }
}