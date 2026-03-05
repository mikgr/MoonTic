
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ticketer.Model;

namespace Ticketer.Repository;

public static class Configuration
{
    public static IServiceCollection AddRepository(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<DynamoDbSettings>(config.GetSection("DynamoDb"));
        
        services.AddSingleton<IAmazonDynamoDB>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<DynamoDbSettings>>().Value;
            
            if (string.IsNullOrWhiteSpace(settings.AwsAccessKeyId)) throw new Exception("AWS access key not set");
            if (string.IsNullOrWhiteSpace(settings.AwsSecretAccessKey)) throw new Exception("AWS secret access key not set");
            if (string.IsNullOrWhiteSpace(settings.RegionEndpoint)) throw new Exception("AWS region not set");
            
            var regionEndpoint = settings.RegionEndpoint == "EUWest1" ? RegionEndpoint.EUWest1 : throw new Exception("Unknown region");
            
            return new AmazonDynamoDBClient(settings.AwsAccessKeyId, settings.AwsSecretAccessKey, regionEndpoint);
            
        }).AddSingleton<IDynamoDBContext>(sp =>
        {
            var client = sp.GetRequiredService<IAmazonDynamoDB>();

            return new DynamoDBContextBuilder()
                .WithDynamoDBClient(() => client)
                .Build();
        })
        .AddSingleton<IRepository,Repository>();
        
        return services;
    }
}