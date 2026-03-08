using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ticketer.UseCases;

public static class Configuration
{
    public static IServiceCollection AddAllUseCases(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Dictionary<string, StableCoinEntry>>(configuration.GetSection("StableCoins"));
        services.AddSingleton<IStableCoinInfoProvider, StableCoinInfoProvider>();

        return services
            .AddScoped<CreateUserHandler>()
            .AddScoped<BuyTicketHandler>()
            .AddScoped<CheckInTicketHandler>()
            .AddScoped<CheckOutTicketHandler>()
            .AddScoped<DeployContractHandler>()
            .AddScoped<PublishEventHandler>()
            .AddScoped<TicketContractClient>()
            .AddScoped<TransferTicketHandler>()
            .AddScoped<MintTicketHandler>()
            .AddScoped<UsherTicketHandler>()
            .AddScoped<CreateAskHandler>()
            .AddScoped<CancelAskHandler>()
            .AddScoped<AcceptAskHandler>();
    }
}