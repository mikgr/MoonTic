using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Ticketer;

public class SetUpDynamoTables
{
    public void Execute()
    {
        // var endpoint = "http://localhost:8000";
        // var config = new AmazonDynamoDBConfig
        // {
        //     ServiceURL = endpoint,
        //     AuthenticationRegion = "eu-north-1"
        // };
        // using var client = new AmazonDynamoDBClient("dummy", "dummy", config);
        using var client = new AmazonDynamoDBClient(
            "", "", RegionEndpoint.EUWest1);


        var tables = new List<CreateTableRequest>
        {
            new CreateTableRequest
            {
                TableName = "User",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new("Id", ScalarAttributeType.S),
                    new("UserName", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement> { new("Id", KeyType.HASH) },
                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                {
                    new GlobalSecondaryIndex
                    {
                        IndexName = "UserNameIndex",
                        KeySchema = new List<KeySchemaElement> { new("UserName", KeyType.HASH) },
                        Projection = new Projection { ProjectionType = ProjectionType.ALL }
                    }
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            },
            new CreateTableRequest
            {
                TableName = "UserWallet",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new("UserId", ScalarAttributeType.S),
                    new("Address", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement> { new("UserId", KeyType.HASH) },
                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                {
                    new GlobalSecondaryIndex
                    {
                        IndexName = "AddressIndex",
                        KeySchema = new List<KeySchemaElement> { new("Address", KeyType.HASH) },
                        Projection = new Projection { ProjectionType = ProjectionType.ALL }
                    }
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            },
            new CreateTableRequest
            {
                TableName = "EventInfo",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new("Owner", ScalarAttributeType.S),
                    new("Id", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new("Owner", KeyType.HASH),
                    new("Id", KeyType.RANGE)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            },
            new CreateTableRequest
            {
                TableName = "EventContractState",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new("ContractAddress", ScalarAttributeType.S),
                    new("OwnerId", ScalarAttributeType.S),
                    new("VenueOpenTimeUtc", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new("ContractAddress", KeyType.HASH)
                },
                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                {
                    new GlobalSecondaryIndex
                    {
                        IndexName = "OwnerIdIndex",
                        KeySchema = new List<KeySchemaElement> { new("OwnerId", KeyType.HASH) },
                        Projection = new Projection { ProjectionType = ProjectionType.ALL }
                    },
                    new GlobalSecondaryIndex
                    {
                        IndexName = "OwnerIdVenueOpenTimeIndex",
                        KeySchema = new List<KeySchemaElement>
                        {
                            new("OwnerId", KeyType.HASH),
                            new("VenueOpenTimeUtc", KeyType.RANGE)
                        },
                        Projection = new Projection { ProjectionType = ProjectionType.ALL }
                    }
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            },
            new CreateTableRequest
            {
                TableName = "UserTicketContainerState",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new("UserId", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new("UserId", KeyType.HASH)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            },
            new CreateTableRequest
            {
                TableName = "EventEnteredEvent",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new("ContractAddress", ScalarAttributeType.S),
                    new("TicketId", ScalarAttributeType.N)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new("ContractAddress", KeyType.HASH),
                    new("TicketId", KeyType.RANGE)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            },
            new CreateTableRequest
            {
                TableName = "TicketContractEvent",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new("ContractAddress", ScalarAttributeType.S),
                    new("TimestampUtc", ScalarAttributeType.N)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new("ContractAddress", KeyType.HASH),
                    new("TimestampUtc", KeyType.RANGE)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            }
        };

        foreach (var req in tables)
        {
            try
            {
                client.CreateTableAsync(req).Wait();
                Console.WriteLine($"Created table: {req.TableName}");
            }
            catch (ResourceInUseException)
            {
                Console.WriteLine($"Table already exists: {req.TableName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed {req.TableName}: {ex.Message}");
            }
        }

    }
}