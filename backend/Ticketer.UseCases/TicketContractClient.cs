using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Options;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

using Ticketer.Model;

namespace Ticketer.UseCases;

// todo move to own project
public class TicketContractClient(
    IDynamoDBContext dynamo,
    IOptions<BlockchainSettings> blockchainSettings,
    EstimateGasAndEnsureSufficientFundsHandler estimateGasAndEnsureSufficientFundsHandler)
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
        
        var userWallet = await dynamo.LoadAsync<UserWallet>(currentUser.Id);
        var userPrivateKey = userWallet.PrivateKey;
        var functionName = "checkIn";
      
        byte[] secretHashBytes = Convert.FromHexString(checkInSecretHash);
        var functionInput = new object[] {ticketId, secretHashBytes};
        
        return await ExecuteContractFunction2(contract.ContractAddress, userPrivateKey, abi, functionName, functionInput);
    }
    
    
    public async Task<(TransactionReceipt receipt, DateTime blockTimestamp)> OnChainTransferTicket(
        User currentUser, 
        EventContract contract,
        int ticketId,
        string toAddress) // todo create address type
    {
        Console.WriteLine($"{nameof(OnChainTransferTicket)}, Contract: {contract.ContractAddress}, TicketId: {ticketId}, ToAddress: {toAddress}");
        
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
        
        var userWallet = await dynamo.LoadAsync<UserWallet>(currentUser.Id);
        var userPrivateKey = userWallet.PrivateKey;
        var functionName = "transfer";

        var functionInput = new object[] {toAddress, ticketId};
        
        return await ExecuteContractFunction2(contract.ContractAddress, userPrivateKey, abi, functionName, functionInput);
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
        

        var userWallet = await dynamo.LoadAsync<UserWallet>(currentUser.Id);
        var userPrivateKey = userWallet.PrivateKey;
        var functionName = "checkOut";

        var functionInput = new object[] {ticketId};
        
        return await ExecuteContractFunction2(contract.ContractAddress, userPrivateKey, abi, functionName, functionInput);
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
        
        var userWallet = await dynamo.LoadAsync<UserWallet>(currentUser.Id);
        var userPrivateKey = userWallet.PrivateKey;
        var functionName = "createAsk";
        
        var functionInput = new object[] {ticketId, askPrice};
        
        return await ExecuteContractFunction2(contract.ContractAddress, userPrivateKey, abi, functionName, functionInput);
    }
    
    
    public async Task<(TransactionReceipt receipt, DateTime blockTimestamp)> OnChainCancelAsk(
        User user, int ticketId, EventContract contract)
    {
        var abi = """
                  [
                  {
                    "inputs": [
                    {"internalType":"uint256","name":"tokenId","type":"uint256"}
                    ],
                    "name":"cancelAsk",
                    "outputs": [],
                    "stateMutability":"nonpayable",
                    "type":"function"
                  }
                  ]
                  """;
        
        var userWallet = await dynamo.LoadAsync<UserWallet>(user.Id);
        var userPrivateKey = userWallet.PrivateKey;
         
        var functionName = "cancelAsk";
        var functionInput = new object[] {ticketId};
        
        return await ExecuteContractFunction2(contract.ContractAddress, userPrivateKey, abi, functionName, functionInput);
    }
    

    public async Task<(TransactionReceipt receipt, DateTime blockTimestamp)> OnChainAcceptAsk(
        User byUser, int ticketId, EventContract contract)
    {
        var abi = """
                  [
                  {
                    "inputs": [
                    {"internalType":"uint256","name":"tokenId","type":"uint256"}
                    ],
                    "name":"acceptAsk",
                    "outputs": [],
                    "stateMutability":"nonpayable",
                    "type":"function"
                  }
                  ]
                  """;
        
        var byUserWallet = await dynamo.LoadAsync<UserWallet>(byUser.Id);
        var functionInput = new object[] {ticketId};
        
        return await ExecuteContractFunction2(
            contract.ContractAddress, 
            byUserWallet.PrivateKey, 
            abi, 
            functionName: "acceptAsk",
            functionInput);
    }
    
    // todo extract to own class
    internal async Task<(TransactionReceipt receipt, DateTime blockTimestamp)> ExecuteContractFunction2(
        string contractAddress, 
        string userPrivateKey, 
        string abi, 
        string functionName, 
        object[] functionInput)
    {
        var executingAccount = new Nethereum.Web3.Accounts.Account(userPrivateKey, blockchainSettings.Value.BlockchainId); // todo user options
        
        var web3Instance = new Web3(executingAccount, blockchainSettings.Value.BlockchainRpcUrl); // todo user options
        
        var contractInstance = web3Instance.Eth.GetContract(abi, contractAddress);
        
        var contractFunction = contractInstance.GetFunction(functionName);
        
        var estimatedGas = await estimateGasAndEnsureSufficientFundsHandler.Execute(
            contractFunction, functionInput, executingAccount.Address, web3Instance);
        
        
        Console.WriteLine($"Executing function {functionName}");
        
        var receipt = await contractFunction.SendTransactionAndWaitForReceiptAsync(
            from: executingAccount.Address,
            gas: new Nethereum.Hex.HexTypes.HexBigInteger(estimatedGas.Value * 120 / 100), // Add 20% buffer
            value: null,
            functionInput: functionInput
        );
        
        Console.WriteLine($"Executed function {functionName}");
        
        var block = await web3Instance.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(receipt.BlockNumber);
        var blockTimestamp = DateTimeOffset.FromUnixTimeSeconds((long)block.Timestamp.Value).UtcDateTime;
        
        return (receipt, blockTimestamp);
    }


}