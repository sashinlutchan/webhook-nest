using Amazon.DynamoDBv2.DataModel;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace webhook_nest.api.Models;

[DynamoDBTable("WebHooks")]
public sealed class Payload
{

    public Payload()
    {
        Pk = string.Empty;
        Sk = string.Empty;
    }

    public Payload(string pk, string sk, Dictionary<string, string>? headers = null, string? method = null, Dictionary<string, object>? data = null, long? expiresAt = null, string? createdAt = null)
    {
        Pk = pk;
        Sk = sk;
        Method = method;
        Headers = headers;
        Data = data;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }

    [DynamoDBHashKey("pk")]
    public string Pk { get; set; }

    [DynamoDBRangeKey("sk")]
    public string Sk { get; set; }



    [DynamoDBProperty]
    public string? Method { get; set; }

    [DynamoDBProperty]
    public Dictionary<string, string>? Headers { get; set; }

    [DynamoDBProperty]
    public Dictionary<string, object>? Data { get; set; }

    [DynamoDBProperty("expiresAt")]
    public long? ExpiresAt { get; set; }

    [DynamoDBProperty("createdAt")]
    public string? CreatedAt { get; set; }




    public static Payload format(
        string pk,
        string sk,
        Dictionary<string, string>? headers = null,
        string method = "",
        Dictionary<string, object>? data = null,
        long? expiresAt = null,
        string? createdAt = null)
    {
        return new Payload(pk, sk, headers, method, data, expiresAt, createdAt);
    }
}