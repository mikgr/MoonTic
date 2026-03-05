using Amazon.DynamoDBv2.DataModel;
using Ticketer.Model;
using Nethereum.Signer;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Ticketer.UseCases;

public class CreateUserHandler(IDynamoDBContext dynamo)
{
    public async Task Execute(string username, string email)
    {
        var userSearch = dynamo.QueryAsync<UserState>(
            username.ToLower(),
            new QueryConfig
            {
                IndexName = "UserNameIndex"
            });
        
        var maybeUser = (await userSearch.GetRemainingAsync()).SingleOrDefault();
        if (maybeUser is not null) throw new DomainInvariant("Username already exists");
        
        if (!System.Text.RegularExpressions.Regex.IsMatch(email, 
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new DomainInvariant("Invalid email format");
    
        var (address, privateKey) = CreateNewWallet();
        
        var newUser = new UserState
        {
            Id = username.ToLower(),
            UserName = username.ToLower(),
            Email = email.ToLower()
        };
        await dynamo.SaveAsync(newUser);

        var userWallet = new UserWallet
        {
            UserId = newUser.Id,
            Address = address.ToLower(),
            PrivateKey = privateKey
        };
        await dynamo.SaveAsync(userWallet);
        
        var userContainerState = new UserTicketContainerState
        {
            UserId = newUser.Id
        };
        
        await dynamo.SaveAsync(userContainerState);
    }
    
    private static (string Address, string PrivateKey) CreateNewWallet()
    {
        var ecKey = EthECKey.GenerateKey();
        var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
        var address = ecKey.GetPublicAddress();
        return (address, privateKey);
    }
}