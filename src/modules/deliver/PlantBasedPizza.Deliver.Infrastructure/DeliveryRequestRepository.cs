using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using MongoDB.Driver;
using Newtonsoft.Json;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Infrastructure.Extensions;

namespace PlantBasedPizza.Deliver.Infrastructure
{
    public class DeliveryRequestRepository : IDeliveryRequestRepository
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient;

        public DeliveryRequestRepository(AmazonDynamoDBClient dynamoDbClient)
        {
            this._dynamoDbClient = dynamoDbClient;
        }
        
        public async Task AddNewDeliveryRequest(DeliveryRequest request)
        {
            await this._dynamoDbClient.PutItemAsync(InfrastructureConstants.TableName,
                request.AsAttributeMap());
        }

        public async Task UpdateDeliveryRequest(DeliveryRequest request)
        {
            await this._dynamoDbClient.PutItemAsync(InfrastructureConstants.TableName,
                request.AsAttributeMap());
        }

        public async Task<DeliveryRequest?> GetDeliveryStatusForOrder(string orderIdentifier)
        {
            var getResult = await this._dynamoDbClient.GetItemAsync(InfrastructureConstants.TableName,
                new Dictionary<string, AttributeValue>(2)
                {
                    { "PK", new AttributeValue($"ORDER#{orderIdentifier.ToUpper()}") },
                    { "SK", new AttributeValue($"DELIVERY#{orderIdentifier.ToUpper()}") },
                });

            if (!getResult.IsItemSet)
            {
                return null;
            }
            
            var result = JsonConvert.DeserializeObject<DeliveryRequest>(getResult.Item["Data"].S);

            return result;
        }

        public async Task<List<DeliveryRequest>> GetAwaitingDriver()
        {
            var queryResults = await this._dynamoDbClient.QueryAsync(new QueryRequest()
            {
                TableName = InfrastructureConstants.TableName,
                IndexName = "GSI2",
                KeyConditionExpression = "GSI2PK = :PK and GSI2SK = :SK",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":PK", new AttributeValue { S =  "AWAITINGCOLLECTION" }},
                    {":SK", new AttributeValue { S = "AWAITINGCOLLECTION" }}
                }
            });

            var results = new List<DeliveryRequest>();

            foreach (var queryResult in queryResults.Items)
            {
                results.Add(JsonConvert.DeserializeObject<DeliveryRequest>(queryResult["Data"].S));
            }

            return results;
        }

        public async Task<List<DeliveryRequest>> GetOrdersWithDriver(string driverName)
        {
            var queryResults = await this._dynamoDbClient.QueryAsync(new QueryRequest()
            {
                TableName = InfrastructureConstants.TableName,
                IndexName = "GSI1",
                KeyConditionExpression = "GSI1PK = :PK and GSI1SK = :SK",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":PK", new AttributeValue { S =  $"DRIVER#{driverName.ToUpper()}" }},
                    {":SK", new AttributeValue { S = $"DRIVER#{driverName.ToUpper()}" }}
                }
            });

            var results = new List<DeliveryRequest>();

            foreach (var queryResult in queryResults.Items)
            {
                results.Add(JsonConvert.DeserializeObject<DeliveryRequest>(queryResult["Data"].S));
            }

            return results;
        }
    }
}