using Amazon.DynamoDBv2.DataModel;
using Ticketer.Model;
using Nethereum.Signer;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Ticketer.UseCases;

public class CreateUserHandler(IDynamoDBContext dynamo, IRepository repo)
{
    public async Task Execute(string username, string email)
    {
        await EnsureUserNameIsFree(username);
        await EnsureEmailIsFree(email);
        ValidateEmailFormat(email);
    
        // NB: Only for POC!!
        var (address, privateKey) = CreateNewWallet();
        
        var newUser = new UserState
        {
            Id = username.ToLower(),
            UserName = username.ToLower(),
            Email = email.ToLower()
        };
        var writeUser = repo.CreateTransactWrite<UserState>();
        writeUser.AddSaveItem(newUser);

        // TODO THIS IS ONLY FOR DEMO PURPOSES, FOR PROD, CREATE, SAVE PRIVATE KEYS IN AWS-KMS - AND SIGN IN KMS
        var userWallet = new UserWallet
        {
            UserId = newUser.Id,
            Address = address.ToLower(),
            PrivateKey = privateKey
        };
        var writeUserWallet = repo.CreateTransactWrite<UserWallet>();
        writeUserWallet.AddSaveItem(userWallet);
        
        var userContainerState = new UserTicketContainerState
        {
            UserId = newUser.Id
        };
        
        var writeUserContainer = repo.CreateTransactWrite<UserTicketContainerState>();
        writeUserContainer.AddSaveItem(userContainerState);
        
        var transaction = repo.CreateMultiTableTransactWrite(writeUser, writeUserWallet, writeUserContainer);
        
        await transaction.ExecuteAsync();
    }

    private static void ValidateEmailFormat(string email)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(email, 
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new DomainInvariant("Invalid email format");
    }


    private async Task EnsureUserNameIsFree(string username)
    {
        var userSearch = dynamo.QueryAsync<UserState>(
            username.ToLower(),
            new QueryConfig
            {
                IndexName = "UserNameIndex"
            });
        
        var maybeUser = (await userSearch.GetRemainingAsync()).SingleOrDefault();
        if (maybeUser is not null) throw new DomainInvariant("Username already exists");
    }
    private async Task EnsureEmailIsFree(string email)
    {
        var emailSearch = dynamo.QueryAsync<UserState>(
            email.ToLower(),
            new QueryConfig
            {
                IndexName = "EmailIndex"
            });
        
        var maybeEmail = (await emailSearch.GetRemainingAsync()).SingleOrDefault();
        if (maybeEmail is not null) throw new DomainInvariant("Email already exists");
    }

    // NB: Only for POC!!
    private (string Address, string PrivateKey) CreateNewWallet()
    {
        var ecKey = EthECKey.GenerateKey();
        var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
        var address = ecKey.GetPublicAddress();
        return (address, privateKey);
    }
}