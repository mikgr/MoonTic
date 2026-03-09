using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ticketer.Model;

namespace Ticketer.UseCases;

public static class Configuration
{
    public static IServiceCollection AddAllUseCases(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Dictionary<string, StableCoinEntry>>(configuration.GetSection("StableCoins"));
        services.AddSingleton<IStableCoinInfoProvider, StableCoinInfoProvider>();

        services.AddOptions<BlockchainSettings>()
            .Bind(configuration.GetSection("Blockchain"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddSingleton(TimeProvider.System);
        
        return services
            .AddSingleton<TicketContractClient>()
            .AddSingleton<StableCoinContractClient>()
            .AddSingleton<EstimateGasAndEnsureSufficientFundsHandler>()
     
            .AddSingleton<CreateUserHandler>()
            .AddSingleton<BuyTicketHandler>()
            .AddSingleton<CheckInTicketHandler>()
            .AddSingleton<CheckOutTicketHandler>()
            .AddSingleton<DeployContractHandler>()
            .AddSingleton<PublishEventHandler>()
            .AddSingleton<TransferTicketHandler>()
            .AddSingleton<MintTicketHandler>()
            .AddSingleton<UsherTicketHandler>()
            .AddSingleton<CreateAskHandler>()
            .AddSingleton<CancelAskHandler>()
            .AddSingleton<AcceptAskHandler>();
    }
}