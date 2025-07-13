using Amazon.DynamoDBv2.DataModel;
using System.Text.Json;

namespace webhook_nest.api.Models;

[DynamoDBTable("WebHooks")]
public sealed class Payload
{
    // Parameterless constructor required by DynamoDB
    public Payload()
    {
        Pk = string.Empty;
        Sk = string.Empty;
        Url = string.Empty;
    }

    public Payload(string pk, string sk, string url, Dictionary<string, string>? headers = null, string? method = null, object? data = null, long? expiresAt = null)
    {
        Pk = pk;
        Sk = sk;
        Url = url;
        Method = method;
        Headers = headers;
        Data = data != null ? JsonSerializer.Serialize(data) : null;
        ExpiresAt = expiresAt;
    }

    [DynamoDBHashKey("pk")]
    public string Pk { get; set; }

    [DynamoDBRangeKey("sk")]
    public string Sk { get; set; }

    [DynamoDBProperty]
    public string Url { get; set; }

    [DynamoDBProperty]
    public string? Method { get; set; }

    [DynamoDBProperty]
    public Dictionary<string, string>? Headers { get; set; }

    [DynamoDBProperty]
    public string? Data { get; set; }

    [DynamoDBProperty("expiresAt")]
    public long? ExpiresAt { get; set; }

    // Helper method to get data as object
    public T? GetData<T>()
    {
        if (string.IsNullOrEmpty(Data))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(Data);
        }
        catch
        {
            return default;
        }
    }

    // Helper method to set data as object
    public void SetData(object data)
    {
        Data = data != null ? JsonSerializer.Serialize(data) : null;
    }

    public static Payload format(
        string pk,
        string sk,
        string url,
        Dictionary<string, string>? headers = null,
        string method = "",
        object? data = null,
        long? expiresAt = null)
    {
        return new Payload(pk, sk, url, headers, method, data, expiresAt);
    }
}