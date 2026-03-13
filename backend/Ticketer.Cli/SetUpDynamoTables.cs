using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ticketer.Repository;

namespace Ticketer;

public class SetUpDynamoTables
{
    public void Execute(IServiceProvider serviceProvider)
    {
        //NB: AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY are set from env-vars in the AmazonDynamoDBClient by convention
        
        var client = serviceProvider.GetRequiredService<IAmazonDynamoDB>();
        
        var tables = new List<CreateTableRequest>
        {
            new CreateTableRequest
            {
                TableName = "User",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new("Id", ScalarAttributeType.S),
                    new("UserName", ScalarAttributeType.S),
                    new("Email", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement> { new("Id", KeyType.HASH) },
                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                {
                    new GlobalSecondaryIndex
                    {
                        IndexName = "UserNameIndex",
                        KeySchema = new List<KeySchemaElement> { new("UserName", KeyType.HASH) },
                        Projection = new Projection { ProjectionType = ProjectionType.ALL }
                    },
                    new GlobalSecondaryIndex
                    {
                        IndexName = "EmailIndex",
                        KeySchema = new List<KeySchemaElement> { new("Email", KeyType.HASH) },
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
                    new("VenueOpenTimeUtc", ScalarAttributeType.S),
                    new("SellerId", ScalarAttributeType.S)
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
                        IndexName = "SellerIdIndex",
                        KeySchema = new List<KeySchemaElement> { new("SellerId", KeyType.HASH) },
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
                TableName = "TicketPurchasedEvent",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new("ContractAddress", ScalarAttributeType.S),
                    new("TimestampUtc", ScalarAttributeType.S),
                    new("OwnerId", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new("ContractAddress", KeyType.HASH),
                    new("TimestampUtc", KeyType.RANGE)
                },
                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                {
                    new GlobalSecondaryIndex
                    {
                        IndexName = "OwnerIdIndex",
                        KeySchema = new List<KeySchemaElement>
                        {
                            new("OwnerId", KeyType.HASH),
                        },
                        Projection = new Projection { ProjectionType = ProjectionType.ALL }
                    }
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            },
            new CreateTableRequest
            {
                TableName = "TicketCheckedOutEvent",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new("ContractAddress", ScalarAttributeType.S),
                    new("TimestampUtc", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new("ContractAddress", KeyType.HASH),
                    new("TimestampUtc", KeyType.RANGE)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            },
            new CreateTableRequest
            {
                TableName = "TicketCheckedInEvent",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new("ContractAddress", ScalarAttributeType.S),
                    new("TimestampUtc", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new("ContractAddress", KeyType.HASH),
                    new("TimestampUtc", KeyType.RANGE)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            },
            new CreateTableRequest
            {
                TableName = "TicketTransferredEvent",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new("ContractAddress", ScalarAttributeType.S),
                    new("TimestampUtc", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new("ContractAddress", KeyType.HASH),
                    new("TimestampUtc", KeyType.RANGE)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            },
            new CreateTableRequest
            {
                TableName = "AskCreatedEvent",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new("ContractAddress", ScalarAttributeType.S),
                    new("TimestampUtc", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new("ContractAddress", KeyType.HASH),
                    new("TimestampUtc", KeyType.RANGE)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            },
            new CreateTableRequest
            {
                TableName = "AskCanceledEvent",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new("ContractAddress", ScalarAttributeType.S),
                    new("TimestampUtc", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new("ContractAddress", KeyType.HASH),
                    new("TimestampUtc", KeyType.RANGE)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            },
            new CreateTableRequest
            {
                TableName = "TicketAsk",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new("ContractAddress", ScalarAttributeType.S),
                    new("TimestampUtc", ScalarAttributeType.S),
                    new("UserId", ScalarAttributeType.S),
                    new("Price", ScalarAttributeType.N)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new("ContractAddress", KeyType.HASH),
                    new("TimestampUtc", KeyType.RANGE)
                },
                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                {
                    new GlobalSecondaryIndex
                    {
                        IndexName = "UserIdIndex",
                        KeySchema = new List<KeySchemaElement>
                        {
                            new("UserId", KeyType.HASH),
                            new("Price", KeyType.RANGE)
                        },
                        Projection = new Projection { ProjectionType = ProjectionType.ALL }
                    },
                    new GlobalSecondaryIndex
                    {
                        IndexName = "ContractAddressPriceIndex",
                        KeySchema = new List<KeySchemaElement>
                        {
                            new("ContractAddress", KeyType.HASH),
                            new("Price", KeyType.RANGE)
                        },
                        Projection = new Projection { ProjectionType = ProjectionType.ALL }
                    }
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