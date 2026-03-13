using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ticketer.Model;

namespace Ticketer.Repository;

public static class Configuration
{
    public static IServiceCollection AddRepository(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<DynamoDbSettings>(config.GetSection("DynamoDb"));
        
        services
            .AddSingleton<EnvironmentClient>(x=> new EnvironmentClient(config))
            // This is configured by eth env vars: AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY
            .AddSingleton<IAmazonDynamoDB>(sp => new AmazonDynamoDBClient(RegionEndpoint.EUWest1))
            
            .AddSingleton<IDynamoDBContext>(sp => {
                var client = CreateDynamoDbClient(sp);
                return new DynamoDBContextBuilder()
                    .WithDynamoDBClient(() => client)
                    .Build();
            })
            
            .AddSingleton<IRepository,Repository>();
        
        return services;
    }

    private static IAmazonDynamoDB CreateDynamoDbClient(IServiceProvider sp)
    {
        if (!sp.GetRequiredService<EnvironmentClient>().IsDevelopment())
            return sp.GetRequiredService<IAmazonDynamoDB>();
        
        // local dev configuration 
        var conf = new AmazonDynamoDBConfig
        {
            ServiceURL = "http://localhost:9000",
            UseHttp = true
        };

        var credentials = new BasicAWSCredentials("dummy", "dummy");

        AWSConfigs.LoggingConfig.LogResponses = ResponseLoggingOption.Always;
        AWSConfigs.LoggingConfig.LogMetrics = true;
        AWSConfigs.LoggingConfig.LogTo = LoggingOptions.Console;

        return new AmazonDynamoDBClient(credentials, conf);
    }
}