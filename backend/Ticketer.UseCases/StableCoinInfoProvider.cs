using Microsoft.Extensions.Options;

namespace Ticketer.UseCases;


public class StableCoinEntry
{
    public required string ContractAddress { get; init; }
    public required uint Decimals { get; init; }
    public required string FiatSymbol { get; init; }
}



public interface IStableCoinInfoProvider
{
    public (string contractAddress, uint decimals) GetStableCoinInfo(string symbol);
}



public class StableCoinInfoProvider(IOptions<Dictionary<string, StableCoinEntry>> options) : IStableCoinInfoProvider
{
    public (string contractAddress, uint decimals) GetStableCoinInfo(string symbol)
    {
        return options.Value.TryGetValue(symbol, out var info)
            ? (info.ContractAddress, info.Decimals)
            : throw new NotImplementedException();
    }
}