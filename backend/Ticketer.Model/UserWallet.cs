using SpikeDb;

namespace Ticketer.Model;

public class UserWallet : ISpikeObjIntKey
{
    public required int Id { get; set; }
    public required int UserId { get; init; }
    public required string Address { get; init; }
    public required string PrivateKey { get; init; }
}