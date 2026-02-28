using SpikeDb;
using Ticketer.Model;
using Nethereum.Signer;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Ticketer.UseCases;

public class CreateUserHandler
{
    public void Execute(string username, string email)
    {
        var maybeUser = SpikeRepo.ReadFirstOrDefault<User>(x => x.UserName == username);
        if (maybeUser is not null) throw new DomainInvariant("Username already exists");
        
        if (!System.Text.RegularExpressions.Regex.IsMatch(email, 
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new DomainInvariant("Invalid email format");
    
        var (address, privateKey) = CreateNewWallet();
        
        var newUser = new User
        {
            Id = -1,
            UserName = username,
            Email = email
        }.SpikePersistInt();
        
        var userWallet = new UserWallet 
        {
            Id = -1,
            UserId = newUser.Id,
            Address = address,
            PrivateKey = privateKey
        }.SpikePersistInt();
        
        new UserTicketContainer
        {
            Id = -1,
            UserId = newUser.Id
        }.SpikePersistInt();
    }
    
    private static (string Address, string PrivateKey) CreateNewWallet()
    {
        var ecKey = EthECKey.GenerateKey();
        var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
        var address = ecKey.GetPublicAddress();
        return (address, privateKey);
    }
}