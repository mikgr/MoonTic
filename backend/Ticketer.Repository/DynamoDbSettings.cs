namespace Ticketer.Repository;

public class DynamoDbSettings
{
    public required string AwsAccessKeyId { get; init; }
    public required string AwsSecretAccessKey { get; init; } 
    public required string RegionEndpoint { get; init; }
}