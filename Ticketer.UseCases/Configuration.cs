using Microsoft.Extensions.DependencyInjection;

namespace Ticketer.UseCases;

public static class Configuration
{
    public static IServiceCollection AddAllUseCases(this IServiceCollection services)
    {
        return services
            .AddScoped<CreateUserHandler>()
            .AddScoped<BuyTicketHandler>()
            .AddScoped<CheckInTicketHandler>()
            .AddScoped<CheckOutTicketHandler>()
            .AddScoped<DeployContractHandler>()
            .AddScoped<PublishEventHandler>()
            .AddScoped<TicketContractClient>()
            .AddScoped<TransferTicketHandler>()
            .AddScoped<MintTicketHandler>();
    }
    
}