using Amazon.DynamoDBv2.DataModel;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

using Ticketer.Model;

namespace Ticketer.UseCases;

// todo move to own project
public class TicketContractClient(IDynamoDBContext dynamo)
{
    public async Task<(TransactionReceipt receipt, DateTime blockTimestamp)> OnChainCheckIn(
        User currentUser, 
        int ticketId, 
        EventContract contract, 
        string checkInSecretHash)
    {
        string abi = """
         [
             {
               "inputs": [
               {"internalType":"uint256","name":"tokenId","type":"uint256"},
               {"internalType":"bytes32","name":"secretHash","type":"bytes32"}
               ],
               "name":"checkIn",
               "outputs": [],
               "stateMutability":"nonpayable",
               "type":"function"
             }
         ]
         """;
        
        // User private key
        var userWallet = await dynamo.LoadAsync<UserWallet>(currentUser.Id);
        var userPrivateKey = userWallet.PrivateKey;
        var functionName = "checkIn";
        // Convert hex string to bytes32
        byte[] secretHashBytes = Convert.FromHexString(checkInSecretHash);
        var functionInput = new object[] {ticketId, secretHashBytes};
        
        return await ExecuteContractFunction(contract, userPrivateKey, abi, functionName, functionInput);
    }
    
    
    public async Task<(TransactionReceipt receipt, DateTime blockTimestamp)> OnChainTransferTicket(
        User currentUser, 
        EventContract contract,
        int ticketId,
        string toAddress) // todo create address type
    {
        Console.WriteLine($"{nameof(OnChainTransferTicket)}, Contract: {contract.ContractAddress}, TicketId: {ticketId}, ToAddress: {toAddress}");
        
        
        // address / token id
        string abi = """
                     [
                     {
                       "inputs": [
                            {"internalType":"address","name":"to","type":"address"},
                            {"internalType":"uint256","name":"tokenId","type":"uint256"}
                       ],
                       "name":"transfer",
                       "outputs": [],
                       "stateMutability":"nonpayable",
                       "type":"function"
                     }
                     ]
                     """;
        
        // User private key
        var userWallet = await dynamo.LoadAsync<UserWallet>(currentUser.Id);
        var userPrivateKey = userWallet.PrivateKey;
        var functionName = "transfer";
        // Convert hex string to bytes32
 
        var functionInput = new object[] {toAddress, ticketId};
        
        return await ExecuteContractFunction(contract, userPrivateKey, abi, functionName, functionInput);
    }
    
    
    public async Task<(TransactionReceipt receipt, DateTime blockTimestamp)> OnChainCheckOut(
        User currentUser, 
        int ticketId, 
        EventContract contract)
    {
        string abi = """
                     [
                     {
                       "inputs": [
                       {"internalType":"uint256","name":"tokenId","type":"uint256"}
                       ],
                       "name":"checkOut",
                       "outputs": [],
                       "stateMutability":"nonpayable",
                       "type":"function"
                     }
                     ]
                     """;
        
        // User private key
        var userWallet = await dynamo.LoadAsync<UserWallet>(currentUser.Id);
        var userPrivateKey = userWallet.PrivateKey;
        var functionName = "checkOut";
        // Convert hex string to bytes32
        var functionInput = new object[] {ticketId};
        
        return await ExecuteContractFunction(contract, userPrivateKey, abi, functionName, functionInput);
    }

    
    public async Task<(TransactionReceipt receipt, DateTime blockTimestamp)> OnChainCreateAsk(
        User currentUser, 
        int ticketId, 
        int askPrice, 
        EventContract contract)
    {
        var abi = """
                     [
                     {
                       "inputs": [
                       {"internalType":"uint256","name":"tokenId","type":"uint256"},
                       {"internalType":"uint256","name":"price","type":"uint256"}
                       ],
                       "name":"createAsk",
                       "outputs": [],
                       "stateMutability":"nonpayable",
                       "type":"function"
                     }
                     ]
                     """;
        
        // User private key
        var userWallet = await dynamo.LoadAsync<UserWallet>(currentUser.Id);
        var userPrivateKey = userWallet.PrivateKey;
        var functionName = "createAsk";
        
        var functionInput = new object[] {ticketId, askPrice};
        
        return await ExecuteContractFunction(contract, userPrivateKey, abi, functionName, functionInput);
    }
    
    
    // todo use kms client, dont store private key in settings 
    // var kmsClient = new AmazonKeyManagementServiceClient();
    // var kmsKeyId = TicketerOptions.KmsKeyId ?? throw new Exception("KMS_KEY_ID not set");
    private static async Task<(TransactionReceipt receipt, DateTime blockTimestamp)> ExecuteContractFunction(
        EventContract contract, 
        string userPrivateKey, 
        string abi, 
        string functionName, 
        object[] functionInput)
    {
        // Create an account object
        var userAccount = new Nethereum.Web3.Accounts.Account(userPrivateKey, TicketerOptions.BlockchainId);

        // Create a Web3 instance
        var web3 = new Web3(userAccount, TicketerOptions.BlockchainRpcUrl);
                    
        
        // Get contract-instance
        Nethereum.Contracts.Contract contractInstance = web3.Eth.GetContract(abi, contract.ContractAddress);

        // Get the mint function
        Nethereum.Contracts.Function contractFunction = contractInstance.GetFunction(functionName);
        
        
        var estimatedGas = await EstimateGasAndEnsureSufficientFundsHandler.Execute(
            contractFunction, functionInput, userAccount.Address, web3);
        
        // Send transaction and wait for receipt
        Console.WriteLine($"Executing function {functionName}");
        TransactionReceipt receipt = await contractFunction.SendTransactionAndWaitForReceiptAsync(
            from: userAccount.Address,
            gas: new Nethereum.Hex.HexTypes.HexBigInteger(estimatedGas.Value * 120 / 100), // Add 20% buffer
            value: null,
            functionInput: functionInput
        );
        Console.WriteLine($"Executed function {functionName}");
        
        // Get block timestamp
        var block = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(receipt.BlockNumber);
        var blockTimestamp = DateTimeOffset.FromUnixTimeSeconds((long)block.Timestamp.Value).UtcDateTime;
        
        return (receipt, blockTimestamp);
    }

 
}