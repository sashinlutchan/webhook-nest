using Amazon.DynamoDBv2.DataModel;
using webhook_nest.api.Interfaces;
using webhook_nest.api.Models;
using Microsoft.Extensions.Logging;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        try
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
                var dataMap = ConvertToDynamoDBMap(payload.Data);
                if (dataMap.Any())
                {
                    item["data"] = new AttributeValue { M = dataMap };
                }
            }

            // Only add expiresAt if it has a value
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
            _logger.LogInformation("Successfully saved payload to DynamoDB");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving payload to DynamoDB with pk: {Pk}, sk: {Sk}", payload.Pk, payload.Sk);
            throw;
        }
    }

    private Dictionary<string, AttributeValue> ConvertToDynamoDBMap(Dictionary<string, object> data)
    {
        var map = new Dictionary<string, AttributeValue>();

        foreach (var kvp in data)
        {
            if (kvp.Value == null) continue;

            if (kvp.Value is string strValue)
            {
                if (!string.IsNullOrEmpty(strValue))
                    map[kvp.Key] = new AttributeValue { S = strValue };
            }
            else if (kvp.Value is int intValue)
            {
                map[kvp.Key] = new AttributeValue { N = intValue.ToString() };
            }
            else if (kvp.Value is double doubleValue)
            {
                map[kvp.Key] = new AttributeValue { N = doubleValue.ToString() };
            }
            else if (kvp.Value is bool boolValue)
            {
                map[kvp.Key] = new AttributeValue { BOOL = boolValue };
            }
            else if (kvp.Value is JObject jObject)
            {
                // Convert JObject to DynamoDB map
                var jObjectDict = jObject.ToObject<Dictionary<string, object>>();
                if (jObjectDict != null)
                {
                    var nestedMap = ConvertToDynamoDBMap(jObjectDict);
                    if (nestedMap.Any())
                    {
                        map[kvp.Key] = new AttributeValue { M = nestedMap };
                    }
                }
            }
            else if (kvp.Value is JArray jArray)
            {
                // Convert JArray to DynamoDB list
                var list = new List<AttributeValue>();
                foreach (var item in jArray)
                {
                    if (item.Type == JTokenType.String)
                        list.Add(new AttributeValue { S = item.ToString() });
                    else if (item.Type == JTokenType.Integer)
                        list.Add(new AttributeValue { N = item.ToString() });
                    else if (item.Type == JTokenType.Float)
                        list.Add(new AttributeValue { N = item.ToString() });
                    else if (item.Type == JTokenType.Boolean)
                        list.Add(new AttributeValue { BOOL = item.Value<bool>() });
                    else if (item.Type == JTokenType.Object)
                    {
                        var itemDict = item.ToObject<Dictionary<string, object>>();
                        if (itemDict != null)
                        {
                            var itemMap = ConvertToDynamoDBMap(itemDict);
                            if (itemMap.Any())
                            {
                                list.Add(new AttributeValue { M = itemMap });
                            }
                        }
                    }
                }
                if (list.Any())
                {
                    map[kvp.Key] = new AttributeValue { L = list };
                }
            }
            else
            {
                // Fallback to string for other types
                map[kvp.Key] = new AttributeValue { S = kvp.Value.ToString() };
            }
        }

        return map;
    }

    public async Task<T?> GetByIdAsync<T>(string pk, string sk)
    {
        try
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

            // Use manual conversion for generic types (DynamoDB context only works with concrete types)
            var convertedData = ConvertFromDynamoDBMap(response.Item);
            _logger.LogInformation("Converted data from DynamoDB: {Data}", JsonConvert.SerializeObject(convertedData));

            // Serialize and deserialize through JSON for generic type conversion
            var json = JsonConvert.SerializeObject(convertedData);
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payload from DynamoDB with pk: {Pk}, sk: {Sk}", pk, sk);
            throw;
        }
    }

    private Dictionary<string, object> ConvertFromDynamoDBMap(Dictionary<string, AttributeValue> map)
    {
        var result = new Dictionary<string, object>();

        foreach (var kvp in map)
        {
            var attrValue = kvp.Value;

            if (attrValue.S != null)
            {
                result[kvp.Key] = attrValue.S;
            }
            else if (attrValue.N != null)
            {
                if (long.TryParse(attrValue.N, out var longValue))
                    result[kvp.Key] = longValue;
                else if (double.TryParse(attrValue.N, out var doubleValue))
                    result[kvp.Key] = doubleValue;
            }
            else if (attrValue.BOOL.HasValue)
            {
                result[kvp.Key] = attrValue.BOOL.Value;
            }
            else if (attrValue.M != null)
            {
                result[kvp.Key] = ConvertFromDynamoDBMap(attrValue.M);
            }
            else if (attrValue.L != null)
            {
                var list = new List<object>();
                foreach (var item in attrValue.L)
                {
                    if (item.S != null)
                        list.Add(item.S);
                    else if (item.N != null)
                    {
                        if (long.TryParse(item.N, out var longValue))
                            list.Add(longValue);
                        else if (double.TryParse(item.N, out var doubleValue))
                            list.Add(doubleValue);
                    }
                    else if (item.BOOL.HasValue)
                        list.Add(item.BOOL.Value);
                    else if (item.M != null)
                        list.Add(ConvertFromDynamoDBMap(item.M));
                }
                result[kvp.Key] = list;
            }
        }

        return result;
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