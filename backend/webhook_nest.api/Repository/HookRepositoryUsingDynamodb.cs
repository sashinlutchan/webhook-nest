using Amazon.DynamoDBv2.DataModel;
using webhook_nest.api.Interfaces;
using webhook_nest.api.Models;
using Microsoft.Extensions.Logging;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using webhook_nest.api.Shared;

namespace webhook_nest.api.Repository;

public class HookRepositoryUsingDynamodb : IHook
{
    private readonly IAmazonDynamoDB _client;
    private readonly IDynamoDBContext _context;
    private readonly ILogger<HookRepositoryUsingDynamodb> _logger;
    private readonly string _tableName;

    public HookRepositoryUsingDynamodb(IAmazonDynamoDB client, IDynamoDBContext context, ILogger<HookRepositoryUsingDynamodb> logger)
    {
        _client = client;
        _context = context;
        _logger = logger;
        _tableName = Environment.GetEnvironmentVariable("TABLE_NAME") ?? "WebHooks";
    }

    public async Task SaveAsync(Payload payload)
    {
      
            _logger.LogInformation("Saving payload to DynamoDB with pk: {Pk}, sk: {Sk}", payload.Pk, payload.Sk);

            var item = new Dictionary<string, AttributeValue>
            {
                { "pk", new AttributeValue { S = payload.Pk } },
                { "sk", new AttributeValue { S = payload.Sk } }
            };

            // Only add url if it's not null or empty
            if (!string.IsNullOrEmpty(payload.Url))
            {
                item["url"] = new AttributeValue { S = payload.Url };
            }

            // Only add method if it's not null or empty
            if (!string.IsNullOrEmpty(payload.Method))
            {
                item["method"] = new AttributeValue { S = payload.Method };
            }

            // Convert headers to DynamoDB map (store as objects)
            if (payload.Headers != null && payload.Headers.Any())
            {
                var headersMap = new Dictionary<string, AttributeValue>();
                foreach (var header in payload.Headers)
                {
                    if (!string.IsNullOrEmpty(header.Value))
                    {
                        headersMap[header.Key] = new AttributeValue { S = header.Value };
                    }
                }
                if (headersMap.Any())
                {
                    item["headers"] = new AttributeValue { M = headersMap };
                }
            }

            // Convert data to DynamoDB map
            if (payload.Data != null && payload.Data.Any())
            {
                var dataMap = Utils.ConvertToDynamoDBMap(payload.Data);
                if (dataMap.Any())
                {
                    item["data"] = new AttributeValue { M = dataMap };
                }
            }

        
            if (payload.ExpiresAt.HasValue)
            {
                item["expiresAt"] = new AttributeValue { N = payload.ExpiresAt.Value.ToString() };
            }

            var request = new PutItemRequest
            {
                TableName = _tableName,
                Item = item
            };

            await _client.PutItemAsync(request);
            _logger.LogInformation($"Successfully saved payload to DynamoDB Request: {request}", request);
      
    }

  

    public async Task<T?> GetByIdAsync<T>(string pk, string sk)
    {
        
            _logger.LogInformation("Loading payload from DynamoDB with pk: {Pk}, sk: {Sk}", pk, sk);

            var request = new GetItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "pk", new AttributeValue { S = pk } },
                    { "sk", new AttributeValue { S = sk } }
                }
            };

            var response = await _client.GetItemAsync(request);

            if (response.Item == null || !response.Item.Any())
            {
                _logger.LogInformation("No item found for pk: {Pk}, sk: {Sk}", pk, sk);
                return default(T);
            }

         
            var convertedData = Utils.ConvertFromDynamoDBMap(response.Item);
            _logger.LogInformation("Converted data from DynamoDB: {Data}", JsonConvert.SerializeObject(convertedData));

            // Serialize and deserialize through JSON for generic type conversion
            var json = JsonConvert.SerializeObject(convertedData);
            return JsonConvert.DeserializeObject<T>(json);
     
    }

    

    public async Task DeleteAsync(string pk, string sk)
    {
        try
        {
            _logger.LogInformation("Deleting payload from DynamoDB with pk: {Pk}, sk: {Sk}", pk, sk);

            var request = new DeleteItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "pk", new AttributeValue { S = pk } },
                    { "sk", new AttributeValue { S = sk } }
                }
            };

            await _client.DeleteItemAsync(request);
            _logger.LogInformation("Successfully deleted payload from DynamoDB");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payload from DynamoDB with pk: {Pk}, sk: {Sk}", pk, sk);
            throw;
        }
    }
}